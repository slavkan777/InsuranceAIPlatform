using System.Text.Json;
using System.Text.Json.Nodes;
using InsuranceAIPlatform.Services.AuditCost.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAIPlatform.DbMigrator;

/// <summary>
/// Lease 11 audit/outbox JSON backfill — a FIFTH, separately recorded step.
/// The recorded v1/v2/v3/v4 contracts are NOT edited.
///
/// Target: the two remaining local demo/history JSON columns
/// (<c>audit_cost.AuditEvents.MetadataJson</c>, <c>audit_cost.OutboxMessages.PayloadJson</c>).
/// Their Cyrillic is stored as literal <c>\u04XX</c> escapes, which decode on parse.
///
/// Contract:
///   - VALUE-LEVEL mutation only. The document is parsed, only offending string values
///     are rewritten, and the document is re-serialized. Object/array shape, property
///     names, array order, numbers, booleans, nulls, ids and timestamps are preserved.
///     A whole document is never replaced.
///   - Known semantics map to the established English contract value (e.g. the event
///     type "ДТП" -> "RoadAccident", the known recommended-action text -> its English
///     equivalent already used by MockAiProvider).
///   - Anything else becomes a truthful neutral historical placeholder. No business
///     fact is invented and no translation is guessed.
///   - JSON is validated BEFORE and AFTER. A row that does not parse, or whose result
///     would not parse, is left completely unchanged and reported as a blocker.
///   - UPDATE only. No delete, truncate, drop, id change or destructive reseed.
///   - Idempotent: replacements contain no Cyrillic, so a second pass changes nothing.
/// </summary>
public static class EnglishOnlyAuditJsonBackfill
{
    public const string Version = "english-only-audit-json-v5";

    /// <summary>Neutral, truthful placeholder — states the row is migrated history.</summary>
    public const string NeutralPlaceholder =
        "Historical synthetic value migrated to English-only mode.";

    /// <summary>Known values with an established English contract equivalent.</summary>
    private static readonly Dictionary<string, string> KnownValues = new(StringComparer.Ordinal)
    {
        // Claim contract codes (same values the product emits today).
        ["ДТП"] = "RoadAccident",
        ["Паркування"] = "Parking",
        ["Зіткнення"] = "Collision",
        ["Пошкодження"] = "Damage",
        ["Скло"] = "Glass",
        ["Угон"] = "Theft",
        ["Викрадення"] = "Theft",
        ["Новий"] = "New",
        ["В роботі"] = "InProgress",
        ["Збір документів"] = "CollectingDocuments",
        ["AI-обробка"] = "AiProcessing",
        ["Високий ризик"] = "HighRisk",
        ["Готова"] = "Ready",
        ["Завершено"] = "Completed",
        ["Невизначений"] = "Undetermined",
        ["Низький"] = "Low",
        ["Середній"] = "Medium",
        ["Високий"] = "High",
        // Known AI recommended-action text -> the established MockAiProvider English text.
        ["Запросіть відсутнє фото бампера. Перевірте рахунок СТО. Рішення лише за людиною-ад'ютантом."] =
            "Request the missing bumper photo. Review the repair invoice. The decision rests solely with the human adjuster.",
        ["Запросіть додаткові документи. Рішення лише за людиною-ад'ютантом."] =
            "Request additional documents. The decision rests solely with the human adjuster.",
        // Known document / claim demo values already migrated elsewhere.
        ["Поліцейський звіт"] = "Police report",
        ["Рахунок СТО"] = "Repair invoice",
        ["Заява клієнта"] = "Customer statement",
        ["Фото пошкодження заднього бампера"] = "Rear bumper damage photo",
        ["Київ, проспект Перемоги 50"] = "Springfield, Lake Drive 50",
        ["Бориспіль, вул. Київська 24"] = "Springfield, Main Street 24",
    };

    public static bool HasCyrillic(string? s) =>
        !string.IsNullOrEmpty(s) && s.Any(c => c >= 'Ѐ' && c <= 'ӿ');

    public static bool IsValidJson(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return false;
        try { using var _ = JsonDocument.Parse(s); return true; }
        catch (JsonException) { return false; }
    }

    /// <summary>Maps one offending value. Known semantics win; otherwise neutral placeholder.</summary>
    public static string MapValue(string current) =>
        KnownValues.TryGetValue(current.Trim(), out var mapped) ? mapped : NeutralPlaceholder;

    /// <summary>
    /// Recursively rewrites every Cyrillic-bearing string value in place.
    /// Returns true when anything changed. Shape, keys and order are untouched.
    /// </summary>
    public static bool MutateInPlace(JsonNode? node)
    {
        var changed = false;
        switch (node)
        {
            case JsonObject obj:
                foreach (var key in obj.Select(kv => kv.Key).ToList())
                {
                    var child = obj[key];
                    if (child is JsonValue v && v.TryGetValue<string>(out var s))
                    {
                        if (HasCyrillic(s)) { obj[key] = MapValue(s); changed = true; }
                    }
                    else if (MutateInPlace(child)) changed = true;
                }
                break;

            case JsonArray arr:
                for (var i = 0; i < arr.Count; i++)   // index-based: order preserved
                {
                    var child = arr[i];
                    if (child is JsonValue av && av.TryGetValue<string>(out var s2))
                    {
                        if (HasCyrillic(s2)) { arr[i] = MapValue(s2); changed = true; }
                    }
                    else if (MutateInPlace(child)) changed = true;
                }
                break;
        }
        return changed;
    }

    /// <summary>
    /// Returns the migrated JSON, or null when the row must be left untouched
    /// (unparseable input, nothing to change, or an unsafe result).
    /// </summary>
    public static string? TryMigrate(string? json, out string? blocker)
    {
        blocker = null;
        if (string.IsNullOrWhiteSpace(json)) return null;
        if (!IsValidJson(json)) { blocker = "input is not valid JSON"; return null; }

        JsonNode? node;
        try { node = JsonNode.Parse(json); }
        catch (JsonException) { blocker = "input failed to parse"; return null; }

        if (!MutateInPlace(node)) return null;          // nothing offending

        var outJson = node!.ToJsonString();
        if (!IsValidJson(outJson)) { blocker = "result would not be valid JSON"; return null; }
        if (HasCyrillic(outJson)) { blocker = "result still contains Cyrillic"; return null; }
        return outJson;
    }

    private const string EnsureHistorySql = """
        IF OBJECT_ID(N'[dbo].[DataBackfillHistory]', N'U') IS NULL
        CREATE TABLE [dbo].[DataBackfillHistory](
            [Version]       NVARCHAR(100)     NOT NULL PRIMARY KEY,
            [AppliedAtUtc]  DATETIMEOFFSET    NOT NULL,
            [Updated]       INT               NOT NULL,
            [Skipped]       INT               NOT NULL,
            [RunCount]      INT               NOT NULL
        );
        """;

    public static async Task<bool> WasAppliedAsync(DbContext db, CancellationToken ct)
    {
        await db.Database.ExecuteSqlRawAsync(EnsureHistorySql, ct);
        var rows = await db.Database
            .SqlQueryRaw<int>("SELECT COUNT(*) AS Value FROM [dbo].[DataBackfillHistory] WHERE [Version] = {0}", Version)
            .ToListAsync(ct);
        return rows.FirstOrDefault() > 0;
    }

    private static async Task RecordAsync(DbContext db, BackfillReport r, CancellationToken ct)
    {
        const string sql = """
            IF EXISTS (SELECT 1 FROM [dbo].[DataBackfillHistory] WHERE [Version] = {0})
                UPDATE [dbo].[DataBackfillHistory]
                   SET [AppliedAtUtc] = SYSDATETIMEOFFSET(),
                       [Updated] = [Updated] + {1}, [Skipped] = {2}, [RunCount] = [RunCount] + 1
                 WHERE [Version] = {0};
            ELSE
                INSERT INTO [dbo].[DataBackfillHistory]([Version],[AppliedAtUtc],[Updated],[Skipped],[RunCount])
                VALUES ({0}, SYSDATETIMEOFFSET(), {1}, {2}, 1);
            """;
        await db.Database.ExecuteSqlRawAsync(sql, new object[] { Version, r.Updated, r.Skipped }, ct);
    }

    public static async Task<BackfillReport> RunAsync(
        AuditCostDbContext audit, DbContext checkpointDb, CancellationToken ct)
    {
        var r = new BackfillReport { Version = Version };

        foreach (var e in await audit.AuditEvents.ToListAsync(ct))
        {
            if (!HasCyrillicOrEscape(e.MetadataJson)) { r.AlreadyEnglish++; continue; }
            var migrated = TryMigrate(e.MetadataJson, out var blocker);
            if (migrated is null)
            {
                if (blocker is null) r.AlreadyEnglish++;
                else r.Skip($"AuditEvent[{e.Id}].MetadataJson: {blocker} — left unchanged");
                continue;
            }
            e.MetadataJson = migrated;
            r.Updated++;
        }

        foreach (var o in await audit.OutboxMessages.ToListAsync(ct))
        {
            if (!HasCyrillicOrEscape(o.PayloadJson)) { r.AlreadyEnglish++; continue; }
            var migrated = TryMigrate(o.PayloadJson, out var blocker);
            if (migrated is null)
            {
                if (blocker is null) r.AlreadyEnglish++;
                else r.Skip($"OutboxMessage[{o.Id}].PayloadJson: {blocker} — left unchanged");
                continue;
            }
            o.PayloadJson = migrated;
            r.Updated++;
        }

        await audit.SaveChangesAsync(ct);
        await RecordAsync(checkpointDb, r, ct);
        return r;
    }

    /// <summary>True when the raw text holds Cyrillic characters OR literal \u04XX escapes.</summary>
    public static bool HasCyrillicOrEscape(string? s) =>
        HasCyrillic(s) || (s is not null && s.Contains("\\u04", StringComparison.OrdinalIgnoreCase));
}
