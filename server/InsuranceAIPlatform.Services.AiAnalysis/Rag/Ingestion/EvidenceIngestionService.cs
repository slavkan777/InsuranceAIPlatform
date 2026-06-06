using System.Security.Cryptography;
using System.Text;
using InsuranceAIPlatform.Services.AiAnalysis.Persistence;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Embedding;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAIPlatform.Services.AiAnalysis.Rag.Ingestion;

/// <summary>
/// Default evidence ingestion: splits a synthetic claim document's text into paragraph-sized,
/// bounded chunks, embeds each with the local deterministic embedder, and stores them as
/// claim-scoped <c>EvidenceChunk</c> rows. The chunk shape mirrors <c>RagSeeder.BuildChunk</c> so
/// uploaded evidence is retrieved and cited identically to seeded evidence.
///
/// Safety properties (see <see cref="IEvidenceIngestionService"/>): strictly additive, per-key
/// idempotent (skips <c>ChunkId</c>s already present for the claim), and ClaimId is set to the
/// target claim only — combined with <c>DbRagChunkSource</c>'s <c>ClaimId == claimId</c> filter this
/// guarantees no cross-claim leakage. Bounded for the demo: at most <see cref="MaxChunks"/> chunks
/// of at most <see cref="MaxChunkChars"/> characters each. No external service, no API key, no PII handling.
/// </summary>
public sealed class EvidenceIngestionService : IEvidenceIngestionService
{
    private const int MaxChunks = 24;
    private const int MaxChunkChars = 800;

    private readonly IDbContextFactory<AiAnalysisDbContext> _factory;
    private readonly IEmbeddingProvider _embed;

    public EvidenceIngestionService(IDbContextFactory<AiAnalysisDbContext> factory, IEmbeddingProvider embed)
    {
        _factory = factory;
        _embed = embed;
    }

    public async Task<int> IngestDocumentTextAsync(
        string claimId, string documentId, string kind, string title, string text, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(claimId) || string.IsNullOrWhiteSpace(text))
            return 0;

        var pieces = Chunk(text);
        if (pieces.Count == 0) return 0;

        var slug = Slug(title);
        var docTail = SafeTail(documentId, 8); // keeps ChunkId unique per source document

        await using var db = await _factory.CreateDbContextAsync(ct);

        // Per-key idempotency: only consider ids not already present for THIS claim.
        var existingIds = (await db.EvidenceChunks
                .Where(c => c.ClaimId == claimId)
                .Select(c => c.ChunkId)
                .ToListAsync(ct))
            .ToHashSet();

        var toAdd = new List<EvidenceChunk>();
        for (var i = 0; i < pieces.Count; i++)
        {
            var chunkId = $"{claimId}-uploaded-{slug}-{docTail}-{i}";
            if (existingIds.Contains(chunkId)) continue;

            var pieceText = pieces[i];
            var vector = _embed.Embed(pieceText);
            toAdd.Add(new EvidenceChunk
            {
                ChunkId = chunkId,
                ClaimId = claimId,                 // <-- target claim only (leakage guard relies on this)
                DocumentId = documentId,
                Kind = string.IsNullOrWhiteSpace(kind) ? "uploaded" : kind,
                Ordinal = i,
                Text = pieceText,
                TokenCount = Math.Max(1, pieceText.Length / 4),
                ChunkHash = Hash(pieceText),
                Language = "uk",
                SourceVersion = "v0.1",
                EmbeddingModel = _embed.ModelName,
                EmbeddingDim = _embed.Dimensions,
                EmbeddingJson = EmbeddingCodec.ToJson(vector),
            });
        }

        if (toAdd.Count == 0) return 0;
        await db.EvidenceChunks.AddRangeAsync(toAdd, ct);
        await db.SaveChangesAsync(ct);
        return toAdd.Count;
    }

    /// <summary>Split on blank lines / newlines into bounded pieces; cap total for demo safety.</summary>
    private static List<string> Chunk(string text)
    {
        var normalized = text.Replace("\r\n", "\n").Replace('\r', '\n');
        var raw = normalized.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var pieces = new List<string>();
        foreach (var part in raw)
        {
            var p = part;
            while (p.Length > MaxChunkChars)
            {
                pieces.Add(p[..MaxChunkChars]);
                if (pieces.Count >= MaxChunks) return pieces;
                p = p[MaxChunkChars..];
            }
            if (p.Length > 0) pieces.Add(p);
            if (pieces.Count >= MaxChunks) return pieces;
        }
        if (pieces.Count == 0)
        {
            var whole = text.Trim();
            if (whole.Length > 0)
                pieces.Add(whole.Length > MaxChunkChars ? whole[..MaxChunkChars] : whole);
        }
        return pieces;
    }

    /// <summary>ASCII-only slug from the document title (falls back to "doc"); keeps ChunkIds clean.</summary>
    private static string Slug(string title)
    {
        if (string.IsNullOrWhiteSpace(title)) return "doc";
        var sb = new StringBuilder();
        foreach (var ch in title.Trim().ToLowerInvariant())
        {
            if (ch is >= 'a' and <= 'z' or >= '0' and <= '9') sb.Append(ch);
            else if (ch is ' ' or '-' or '_') sb.Append('-');
            if (sb.Length >= 32) break;
        }
        var s = sb.ToString().Trim('-');
        return s.Length == 0 ? "doc" : s;
    }

    /// <summary>Last up-to-n alphanumeric characters of an id (stable per source document).</summary>
    private static string SafeTail(string id, int n)
    {
        if (string.IsNullOrWhiteSpace(id)) return "src";
        var alnum = new string(id.Where(char.IsLetterOrDigit).ToArray());
        if (alnum.Length == 0) return "src";
        return alnum.Length <= n ? alnum.ToLowerInvariant() : alnum[^n..].ToLowerInvariant();
    }

    private static string Hash(string text) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)))[..16];
}
