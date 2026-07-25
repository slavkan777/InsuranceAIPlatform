using System.Text.Json;
using System.Text.Json.Nodes;
using InsuranceAIPlatform.DbMigrator;

namespace InsuranceAIPlatform.Tests;

/// <summary>
/// Direct tests for the Lease 11 audit/outbox JSON migration.
///
/// The value it protects: this migration rewrites recorded audit and outbox history,
/// so it must change ONLY offending string values and must never damage the document —
/// shape, property names, array order, numbers, booleans, nulls, ids and timestamps
/// all have to survive byte-for-byte in meaning.
/// </summary>
public class AuditJsonBackfillTests
{
    // ---- parsing / validation ---------------------------------------------

    [Theory]
    [InlineData("{\"a\":1}")]
    [InlineData("[]")]
    [InlineData("{\"a\":{\"b\":[1,2,{\"c\":null}]}}")]
    public void IsValidJson_accepts_well_formed_documents(string json) =>
        Assert.True(EnglishOnlyAuditJsonBackfill.IsValidJson(json));

    [Theory]
    [InlineData("{not json")]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValidJson_rejects_malformed_or_blank(string json) =>
        Assert.False(EnglishOnlyAuditJsonBackfill.IsValidJson(json));

    [Fact]
    public void Invalid_json_is_left_unchanged_and_reported_as_a_blocker()
    {
        var result = EnglishOnlyAuditJsonBackfill.TryMigrate("{\"EventType\":\"ДТП\"", out var blocker);
        Assert.Null(result);                      // nothing to write
        Assert.False(string.IsNullOrEmpty(blocker));
    }

    // ---- escaped input decodes and is detected -----------------------------

    [Fact]
    public void Literal_unicode_escapes_are_detected_and_migrated()
    {
        // "ДТП" is the ASCII-escaped form of the Ukrainian event type.
        const string json = "{\"EventType\":\"\\u0414\\u0422\\u041F\",\"ClaimId\":\"CLM-1024\"}";
        Assert.True(EnglishOnlyAuditJsonBackfill.HasCyrillicOrEscape(json));

        var migrated = EnglishOnlyAuditJsonBackfill.TryMigrate(json, out var blocker);
        Assert.Null(blocker);
        Assert.NotNull(migrated);

        var obj = JsonNode.Parse(migrated!)!.AsObject();
        Assert.Equal("RoadAccident", (string?)obj["EventType"]);
        Assert.Equal("CLM-1024", (string?)obj["ClaimId"]);   // id preserved
    }

    // ---- known-value mapping ----------------------------------------------

    [Theory]
    [InlineData("ДТП", "RoadAccident")]
    [InlineData("Готова", "Ready")]
    [InlineData("Високий", "High")]
    [InlineData("Поліцейський звіт", "Police report")]
    public void Known_values_map_to_the_established_english_contract_value(string ua, string expected) =>
        Assert.Equal(expected, EnglishOnlyAuditJsonBackfill.MapValue(ua));

    [Fact]
    public void Unknown_historical_text_gets_the_neutral_placeholder_not_a_guess()
    {
        var mapped = EnglishOnlyAuditJsonBackfill.MapValue("Якийсь невідомий історичний текст");
        Assert.Equal(EnglishOnlyAuditJsonBackfill.NeutralPlaceholder, mapped);
        Assert.DoesNotContain("невідом", mapped);
    }

    // ---- structural preservation ------------------------------------------

    [Fact]
    public void Shape_keys_numbers_booleans_nulls_and_ids_are_preserved()
    {
        const string json = """
            {"ClaimId":"CLM-1024","AuditId":42,"Ok":true,"Note":null,
             "EventType":"ДТП","At":"2026-05-28T10:00:00Z",
             "Nested":{"Score":0.75,"Kind":"police"}}
            """;
        var migrated = EnglishOnlyAuditJsonBackfill.TryMigrate(json, out var blocker);
        Assert.Null(blocker);
        var o = JsonNode.Parse(migrated!)!.AsObject();

        Assert.Equal("CLM-1024", (string?)o["ClaimId"]);
        Assert.Equal(42, (int?)o["AuditId"]);
        Assert.True((bool?)o["Ok"]);
        Assert.Null(o["Note"]);
        Assert.Equal("2026-05-28T10:00:00Z", (string?)o["At"]);
        Assert.Equal(0.75, (double?)o["Nested"]!["Score"]);
        Assert.Equal("police", (string?)o["Nested"]!["Kind"]);
        Assert.Equal("RoadAccident", (string?)o["EventType"]);
        // Property set is unchanged — nothing added or dropped.
        Assert.Equal(new[] { "ClaimId", "AuditId", "Ok", "Note", "EventType", "At", "Nested" },
                     o.Select(kv => kv.Key).ToArray());
    }

    [Fact]
    public void Array_order_is_preserved_and_only_offending_entries_change()
    {
        const string json = """
            {"Items":["first","ДТП","third","Готова"]}
            """;
        var migrated = EnglishOnlyAuditJsonBackfill.TryMigrate(json, out _);
        var arr = JsonNode.Parse(migrated!)!["Items"]!.AsArray();

        Assert.Equal(4, arr.Count);
        Assert.Equal("first", (string?)arr[0]);
        Assert.Equal("RoadAccident", (string?)arr[1]);
        Assert.Equal("third", (string?)arr[2]);
        Assert.Equal("Ready", (string?)arr[3]);
    }

    [Fact]
    public void Whole_document_is_never_replaced_when_a_value_level_edit_is_possible()
    {
        const string json = """{"Keep":"unchanged english","EventType":"ДТП"}""";
        var migrated = EnglishOnlyAuditJsonBackfill.TryMigrate(json, out _);
        var o = JsonNode.Parse(migrated!)!.AsObject();
        Assert.Equal("unchanged english", (string?)o["Keep"]);
        Assert.Equal(2, o.Count);
    }

    // ---- idempotency -------------------------------------------------------

    [Fact]
    public void Second_pass_makes_no_further_change()
    {
        const string json = """{"EventType":"ДТП","ClaimId":"CLM-1"}""";
        var first = EnglishOnlyAuditJsonBackfill.TryMigrate(json, out _);
        Assert.NotNull(first);

        var second = EnglishOnlyAuditJsonBackfill.TryMigrate(first, out var blocker2);
        Assert.Null(blocker2);
        Assert.Null(second);   // nothing offending remains
    }

    [Fact]
    public void Clean_english_document_is_not_touched()
    {
        const string json = """{"EventType":"RoadAccident","ClaimId":"CLM-1"}""";
        Assert.False(EnglishOnlyAuditJsonBackfill.HasCyrillicOrEscape(json));
        Assert.Null(EnglishOnlyAuditJsonBackfill.TryMigrate(json, out var blocker));
        Assert.Null(blocker);
    }

    // ---- output safety -----------------------------------------------------

    [Fact]
    public void Migrated_output_contains_no_cyrillic_in_either_representation()
    {
        const string json = """
            {"RecommendedAction":"Запросіть","EventType":"ДТП"}
            """;
        var migrated = EnglishOnlyAuditJsonBackfill.TryMigrate(json, out _)!;
        Assert.False(EnglishOnlyAuditJsonBackfill.HasCyrillic(migrated));
        Assert.DoesNotContain("\\u04", migrated, StringComparison.OrdinalIgnoreCase);
        Assert.True(EnglishOnlyAuditJsonBackfill.IsValidJson(migrated));
    }
}
