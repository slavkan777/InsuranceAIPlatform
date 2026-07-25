using System.Security.Cryptography;
using System.Text;
using InsuranceAIPlatform.Services.AiAnalysis.Persistence;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Embedding;
using InsuranceAIPlatform.Services.Claims.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAIPlatform.DbMigrator;

/// <summary>
/// Lease 15 demo-residue backfill — a SIXTH, separately recorded step.
/// The recorded v1..v5 contracts are NOT edited.
///
/// Target: rows that exist only in interactively-used demo databases (created through
/// the running product by a human), which therefore never appeared in any seed corpus:
///
///   1. <c>claims.Claims</c> rows whose <c>Customer</c>/<c>Vehicle</c> hold free-typed
///      Ukrainian words with an unambiguous dictionary equivalent
///      (e.g. "Бампер" -> "Bumper", "Тойота" -> "Toyota"). Only exact whole-value
///      matches are translated; anything else Cyrillic is left unchanged and reported.
///   2. <c>ai_analysis.EvidenceChunks</c> rows whose <c>Text</c> is still Cyrillic
///      (chunks ingested from user-uploaded demo documents; earlier backfills
///      deliberately skip them because they have no corpus counterpart). Their text
///      becomes a truthful neutral placeholder and the embedding/hash/token count are
///      recomputed so no stale vector or hash survives — the same in-place pattern v1
///      uses for corpus chunks.
///
/// Contract: UPDATE only; no delete/truncate/drop/id change; idempotent (replacements
/// contain no Cyrillic); unknown values are never guessed — skip-and-report.
/// </summary>
public static class EnglishOnlyDemoResidueBackfill
{
    public const string Version = "english-only-demo-residue-v6";

    /// <summary>Truthful placeholder for uploaded-demo chunk text with no corpus counterpart.</summary>
    public const string NeutralChunkText =
        "Historical synthetic uploaded-document content migrated to English-only mode.";

    /// <summary>
    /// Free-typed demo values with an unambiguous, dictionary-literal English equivalent.
    /// Deliberately tiny: only values actually observed in a demo database belong here.
    /// </summary>
    private static readonly Dictionary<string, string> WholeValueMap = new(StringComparer.Ordinal)
    {
        ["Бампер"] = "Bumper",
        ["Тойота"] = "Toyota",
    };

    public static bool HasCyrillic(string? s) =>
        !string.IsNullOrEmpty(s) && s.Any(c => c >= 'Ѐ' && c <= 'ӿ');

    /// <summary>Exact whole-value translate; null when the value is not in the map.</summary>
    public static string? TryMapWholeValue(string? current)
    {
        if (string.IsNullOrWhiteSpace(current)) return null;
        return WholeValueMap.TryGetValue(current.Trim(), out var mapped) ? mapped : null;
    }

    private static string Hash(string text) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)))[..16];

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
        ClaimsDbContext claims,
        AiAnalysisDbContext ai,
        IEmbeddingProvider embed,
        DbContext checkpointDb,
        CancellationToken ct)
    {
        var r = new BackfillReport { Version = Version };

        // ---- 1. free-typed claim display fields (exact whole-value only) ----
        foreach (var c in await claims.Claims.ToListAsync(ct))
        {
            var touched = false;

            if (HasCyrillic(c.Customer))
            {
                var mapped = TryMapWholeValue(c.Customer);
                if (mapped is not null) { c.Customer = mapped; touched = true; }
                else r.Skip($"Claim[{c.ClaimId}].Customer: no unambiguous equivalent — left unchanged");
            }

            if (HasCyrillic(c.Vehicle))
            {
                var mapped = TryMapWholeValue(c.Vehicle);
                if (mapped is not null) { c.Vehicle = mapped; touched = true; }
                else r.Skip($"Claim[{c.ClaimId}].Vehicle: no unambiguous equivalent — left unchanged");
            }

            if (touched) r.Updated++;
            else if (!HasCyrillic(c.Customer) && !HasCyrillic(c.Vehicle)) r.AlreadyEnglish++;
        }
        await claims.SaveChangesAsync(ct);

        // ---- 2. uploaded-demo evidence chunks still holding Cyrillic text ----
        foreach (var ch in await ai.EvidenceChunks.ToListAsync(ct))
        {
            if (!HasCyrillic(ch.Text)) { r.AlreadyEnglish++; continue; }

            // Same in-place pattern as v1's corpus rewrite: same ChunkId (no duplicate
            // retrievable chunk), fresh hash + embedding so nothing stale survives.
            ch.Text = NeutralChunkText;
            ch.TokenCount = Math.Max(1, NeutralChunkText.Length / 4);
            ch.ChunkHash = Hash(NeutralChunkText);
            ch.Language = "en";
            ch.EmbeddingModel = embed.ModelName;
            ch.EmbeddingDim = embed.Dimensions;
            ch.EmbeddingJson = EmbeddingCodec.ToJson(embed.Embed(NeutralChunkText));
            r.Updated++;
            r.ChunksReEmbedded++;
        }
        await ai.SaveChangesAsync(ct);

        await RecordAsync(checkpointDb, r, ct);
        return r;
    }
}
