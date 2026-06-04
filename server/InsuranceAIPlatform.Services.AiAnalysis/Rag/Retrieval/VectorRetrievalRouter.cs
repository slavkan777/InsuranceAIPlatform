using InsuranceAIPlatform.Services.AiAnalysis.Rag.Contracts;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Embedding;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Persistence;

namespace InsuranceAIPlatform.Services.AiAnalysis.Rag.Retrieval;

/// <summary>
/// Default <see cref="IVectorRetrievalRouter"/>. Tries Qdrant first when the seam is enabled and a
/// client is wired; on ANY failure (disabled, unreachable, error) it falls back to the in-process
/// deterministic index (<see cref="IRagRetrievalService"/>). This is the seam that makes
/// <c>backend=qdrant</c> semantically honest: it is reported only after a real Qdrant round-trip.
/// </summary>
public sealed class VectorRetrievalRouter : IVectorRetrievalRouter
{
    private readonly IRagRetrievalService _inMemory;
    private readonly IEmbeddingProvider _embed;
    private readonly RagOptions _options;
    private readonly IQdrantVectorClient? _qdrant;

    public VectorRetrievalRouter(
        IRagRetrievalService inMemory,
        IEmbeddingProvider embed,
        RagOptions options,
        IQdrantVectorClient? qdrant = null)
    {
        _inMemory = inMemory;
        _embed = embed;
        _options = options;
        _qdrant = qdrant;
    }

    public async Task<RetrievalOutcome> RankAsync(
        string claimId, string query, IReadOnlyList<EvidenceChunk> candidates, int topK, CancellationToken ct = default)
    {
        var viaQdrant = await TryQdrantRankAsync(claimId, query, candidates, topK, ct);
        if (viaQdrant is not null) return viaQdrant;

        // Safe fallback — the in-process deterministic index always serves.
        return new RetrievalOutcome(_inMemory.Rank(query, candidates, topK), VectorBackends.InMemoryHash);
    }

    public async Task<string> ResolveServingBackendAsync(
        string claimId, IReadOnlyList<EvidenceChunk> candidates, CancellationToken ct = default)
    {
        // Exercise the REAL retrieval path (ensure + upsert + search). "qdrant" only if it actually served.
        int k = _options.DefaultTopK > 0 ? _options.DefaultTopK : 4;
        var viaQdrant = await TryQdrantRankAsync(claimId, claimId, candidates, k, ct);
        return viaQdrant is not null ? VectorBackends.Qdrant : VectorBackends.InMemoryHash;
    }

    /// <summary>
    /// Attempt a full Qdrant retrieval round-trip. Returns null (→ caller falls back) when the seam is
    /// disabled / no client, or when any step throws (Qdrant unreachable or erroring).
    /// </summary>
    private async Task<RetrievalOutcome?> TryQdrantRankAsync(
        string claimId, string query, IReadOnlyList<EvidenceChunk> candidates, int topK, CancellationToken ct)
    {
        if (!_options.QdrantEnabled || _qdrant is null || topK <= 0 || candidates is null)
            return null;

        try
        {
            await _qdrant.EnsureCollectionAsync(_embed.Dimensions, ct);

            // Upsert this claim's chunks only. Leakage guard #1: every point carries claimId so the
            // search-time filter can scope to this claim. Re-upsert is idempotent (deterministic ids).
            if (candidates.Count > 0)
            {
                var points = new List<QdrantUpsertPoint>(candidates.Count);
                foreach (var c in candidates)
                {
                    float[] vec = EmbeddingCodec.FromJson(c.EmbeddingJson) ?? _embed.Embed(c.Text);
                    points.Add(new QdrantUpsertPoint(c.ChunkId, c.ClaimId, vec));
                }
                await _qdrant.UpsertAsync(points, ct);
            }

            float[] q = _embed.Embed(query ?? string.Empty);
            IReadOnlyList<QdrantSearchHit> hits = await _qdrant.SearchAsync(claimId, q, topK, ct);

            // Map hits back to THIS claim's candidates. Leakage guard #2: a hit that is not in the
            // claim-scoped candidate set (e.g. a stale foreign point) is dropped, never surfaced.
            var byId = candidates.ToDictionary(c => c.ChunkId, StringComparer.Ordinal);
            var scored = new List<ScoredChunk>(hits.Count);
            foreach (var h in hits)
            {
                if (byId.TryGetValue(h.ChunkId, out var chunk))
                    scored.Add(new ScoredChunk(chunk, h.Score));
            }

            // Deterministic ordering identical to the in-memory path (score desc, ChunkId tie-break).
            var ordered = scored
                .OrderByDescending(s => s.Score)
                .ThenBy(s => s.Chunk.ChunkId, StringComparer.Ordinal)
                .Take(topK)
                .ToList();

            return new RetrievalOutcome(ordered, VectorBackends.Qdrant);
        }
        catch
        {
            // Qdrant unreachable or erroring — degrade silently. The honest signal surfaces via the
            // backend label (in-memory-hash) on the infrastructure-status endpoint.
            return null;
        }
    }
}
