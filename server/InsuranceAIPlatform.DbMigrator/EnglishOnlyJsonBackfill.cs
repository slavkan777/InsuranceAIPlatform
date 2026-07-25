using System.Text.Json;
using System.Text.Json.Nodes;
using InsuranceAIPlatform.Services.AiAnalysis.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAIPlatform.DbMigrator;

/// <summary>
/// Lease 10 JSON backfill — a FOURTH, separately recorded version. v1/v2/v3 recorded
/// contracts are untouched.
///
/// Why this exists: two columns store JSON with non-ASCII written as `\u04XX` escape
/// sequences. Every earlier census searched for actual Cyrillic CHARACTERS, so escaped
/// content was structurally invisible to it — while the browser decodes the escapes on
/// parse and renders Ukrainian. This pass works on the decoded JSON.
///
/// Rules:
///   - Parse, mutate, re-serialize. The object/array SHAPE is preserved: only the
///     specific offending string values change. Unrelated fields (rationale,
///     confidence, ChunkId, DocumentId, Kind, Score) and array ORDER are untouched.
///   - JSON is validated before AND after. If a value does not parse, or the result
///     would not parse, the row is left untouched and reported.
///   - UPDATE only, per-column. No delete/truncate/drop, no whole-row replacement.
///     Ids, relationships and timestamps are never modified.
///   - Idempotent: replacements contain no Cyrillic, so a second pass finds nothing.
/// </summary>
public static class EnglishOnlyJsonBackfill
{
    public const string Version = "english-only-json-v4";

    private const string ActionReplacement =
        "Request the missing bumper photo. Review the repair invoice. The decision rests solely with the human adjuster.";

    private const string SnippetFallback =
        "Historical citation excerpt migrated to English-only mode. Re-run analysis to refresh the excerpt.";

    private static bool HasCyrillic(string? s) =>
        !string.IsNullOrEmpty(s) && s.Any(c => c >= 'Ѐ' && c <= 'ӿ');

    private static readonly JsonSerializerOptions Opts = new() { WriteIndented = false };

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

    private static bool IsValidJson(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return false;
        try { using var _ = JsonDocument.Parse(s); return true; }
        catch (JsonException) { return false; }
    }

    public static async Task<BackfillReport> RunAsync(
        AiAnalysisDbContext ai, DbContext checkpointDb, CancellationToken ct)
    {
        var r = new BackfillReport { Version = Version };

        // Current English chunk text, keyed by ChunkId — the preferred snippet source.
        var chunkText = await ai.EvidenceChunks
            .ToDictionaryAsync(c => c.ChunkId, c => c.Text, StringComparer.Ordinal, ct);

        // ---- AiAnalysisRuns.RecommendedActionJson ---------------------------
        foreach (var run in await ai.AiAnalysisRuns.ToListAsync(ct))
        {
            var json = run.RecommendedActionJson;
            if (string.IsNullOrWhiteSpace(json)) { r.AlreadyEnglish++; continue; }
            if (!IsValidJson(json)) { r.Skip($"AiAnalysisRun[{run.RunId}].RecommendedActionJson: not valid JSON"); continue; }

            JsonNode? node;
            try { node = JsonNode.Parse(json); }
            catch (JsonException) { r.Skip($"AiAnalysisRun[{run.RunId}].RecommendedActionJson: parse failed"); continue; }
            if (node is not JsonObject obj) { r.AlreadyEnglish++; continue; }

            var changed = false;
            foreach (var key in obj.Select(kv => kv.Key).ToList())
            {
                // Only the action field is rewritten; rationale/confidence/etc. are preserved.
                if (!key.Equals("action", StringComparison.OrdinalIgnoreCase)) continue;
                if (obj[key] is JsonValue v && v.TryGetValue<string>(out var s) && HasCyrillic(s))
                {
                    obj[key] = ActionReplacement;
                    changed = true;
                }
            }

            if (!changed) { r.AlreadyEnglish++; continue; }

            var outJson = obj.ToJsonString(Opts);
            if (!IsValidJson(outJson)) { r.Skip($"AiAnalysisRun[{run.RunId}].RecommendedActionJson: result invalid, not written"); continue; }
            run.RecommendedActionJson = outJson;
            r.Updated++;
        }

        // ---- RagAuditTraces.CitationsJson ------------------------------------
        foreach (var tr in await ai.RagAuditTraces.ToListAsync(ct))
        {
            var json = tr.CitationsJson;
            if (string.IsNullOrWhiteSpace(json)) { r.AlreadyEnglish++; continue; }
            if (!IsValidJson(json)) { r.Skip($"RagAuditTrace[{tr.TraceId}].CitationsJson: not valid JSON"); continue; }

            JsonNode? node;
            try { node = JsonNode.Parse(json); }
            catch (JsonException) { r.Skip($"RagAuditTrace[{tr.TraceId}].CitationsJson: parse failed"); continue; }
            if (node is not JsonArray arr) { r.AlreadyEnglish++; continue; }

            var changed = false;
            // Array order is preserved: entries are mutated in place, never reordered.
            foreach (var item in arr)
            {
                if (item is not JsonObject c) continue;
                var snippetKey = c.Select(kv => kv.Key)
                    .FirstOrDefault(k => k.Equals("snippet", StringComparison.OrdinalIgnoreCase));
                if (snippetKey is null) continue;
                if (c[snippetKey] is not JsonValue sv || !sv.TryGetValue<string>(out var snippet) || !HasCyrillic(snippet))
                    continue;

                var idKey = c.Select(kv => kv.Key)
                    .FirstOrDefault(k => k.Equals("chunkId", StringComparison.OrdinalIgnoreCase));
                var chunkId = idKey is not null && c[idKey] is JsonValue iv && iv.TryGetValue<string>(out var id) ? id : null;

                // Prefer the live English chunk text for the same ChunkId; otherwise the
                // explicit neutral fallback. Never a guessed translation.
                c[snippetKey] = chunkId is not null
                                && chunkText.TryGetValue(chunkId, out var text)
                                && !HasCyrillic(text)
                    ? text
                    : SnippetFallback;
                changed = true;
            }

            if (!changed) { r.AlreadyEnglish++; continue; }

            var outJson = arr.ToJsonString(Opts);
            if (!IsValidJson(outJson)) { r.Skip($"RagAuditTrace[{tr.TraceId}].CitationsJson: result invalid, not written"); continue; }
            tr.CitationsJson = outJson;
            r.Updated++;
        }

        // ---- stale language tags on already-English chunks -------------------
        // Tag-only correction: text is unchanged, so the embedding is still valid and
        // is deliberately NOT recomputed.
        foreach (var ch in await ai.EvidenceChunks.ToListAsync(ct))
        {
            if (string.Equals(ch.Language, "en", StringComparison.OrdinalIgnoreCase)) { r.AlreadyEnglish++; continue; }
            if (HasCyrillic(ch.Text)) { r.Skip($"EvidenceChunk[{ch.ChunkId}]: text still Cyrillic, tag left as-is"); continue; }
            ch.Language = "en";
            r.Updated++;
        }

        await ai.SaveChangesAsync(ct);
        await RecordAsync(checkpointDb, r, ct);
        return r;
    }
}
