using InsuranceAIPlatform.DbMigrator;

namespace InsuranceAIPlatform.Tests;

/// <summary>
/// Direct tests for the Lease 15 demo-residue migration (v6).
///
/// The value it protects: v6 touches free-typed demo rows, so it must translate ONLY
/// exact whole values with an unambiguous dictionary equivalent and must never guess.
/// Its replacement text must itself be clean in both Cyrillic representations, so the
/// step stays idempotent.
/// </summary>
public class DemoResidueBackfillTests
{
    // ---- whole-value mapping ----------------------------------------------

    [Theory]
    [InlineData("Бампер", "Bumper")]
    [InlineData("Тойота", "Toyota")]
    [InlineData("  Бампер  ", "Bumper")]   // trimmed exact match still counts
    public void Known_whole_values_map_to_their_dictionary_equivalent(string ua, string expected) =>
        Assert.Equal(expected, EnglishOnlyDemoResidueBackfill.TryMapWholeValue(ua));

    [Theory]
    [InlineData("Невідомий Клієнт")]            // unknown Cyrillic → never guessed
    [InlineData("Бампер пошкоджений")]          // known word inside a longer value → not a whole-value match
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Everything_else_returns_null_rather_than_a_guess(string? value) =>
        Assert.Null(EnglishOnlyDemoResidueBackfill.TryMapWholeValue(value));

    [Fact]
    public void English_values_pass_through_untranslated()
    {
        // An already-English value is not in the map — RunAsync counts it AlreadyEnglish
        // because HasCyrillic is false; TryMapWholeValue must not invent anything.
        Assert.Null(EnglishOnlyDemoResidueBackfill.TryMapWholeValue("Bumper"));
        Assert.False(EnglishOnlyDemoResidueBackfill.HasCyrillic("Bumper"));
    }

    // ---- detection ---------------------------------------------------------

    [Theory]
    [InlineData("Бампер", true)]
    [InlineData("Robert Johnson", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void HasCyrillic_detects_only_actual_cyrillic(string? s, bool expected) =>
        Assert.Equal(expected, EnglishOnlyDemoResidueBackfill.HasCyrillic(s));

    // ---- replacement safety (idempotency precondition) ---------------------

    [Fact]
    public void Neutral_chunk_text_is_clean_in_both_representations()
    {
        var text = EnglishOnlyDemoResidueBackfill.NeutralChunkText;
        Assert.False(EnglishOnlyDemoResidueBackfill.HasCyrillic(text));
        Assert.DoesNotContain("\\u04", text, StringComparison.OrdinalIgnoreCase);
        Assert.False(string.IsNullOrWhiteSpace(text));
    }

    [Fact]
    public void Mapped_values_are_clean_so_a_second_pass_changes_nothing()
    {
        foreach (var ua in new[] { "Бампер", "Тойота" })
        {
            var mapped = EnglishOnlyDemoResidueBackfill.TryMapWholeValue(ua)!;
            Assert.False(EnglishOnlyDemoResidueBackfill.HasCyrillic(mapped));
            Assert.Null(EnglishOnlyDemoResidueBackfill.TryMapWholeValue(mapped)); // fixpoint
        }
    }
}
