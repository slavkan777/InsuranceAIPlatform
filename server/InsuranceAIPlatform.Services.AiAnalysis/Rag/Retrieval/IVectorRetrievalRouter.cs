using InsuranceAIPlatform.Services.AiAnalysis.Rag.Contracts;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Persistence;

namespace InsuranceAIPlatform.Services.AiAnalysis.Rag.Retrieval;

/// <summary>Canonical names of the vector backend that actually served a retrieval.</summary>
public static class VectorBackends
{
    /// <summary>The local Qdrant vector store served the retrieval (real round-trip succeeded).</summary>
    public const string Qdrant = "qdrant";

    /// <summary>The in-process deterministic embedding index served the retrieval (default / safe fallback).</summary>
    public const string InMemoryHash = "in-memory-hash";
}

/// <summary>
/// The result of a retrieval: the ranked chunks plus the backend that ACTUALLY served them.
/// <see cref="Backend"/> is <see cref="VectorBackends.Qdrant"/> only when a real Qdrant round-trip
/// (ensure + upsert + search) succeeded — never merely because Qdrant was probe-reachable.
/// </summary>
public sealed record RetrievalOutcome(IReadOnlyList<ScoredChunk> Hits, string Backend);

/// <summary>
/// Routes claim-scoped retrieval to Qdrant when the seam is enabled and the local runtime serves the
/// request, otherwise to the in-process deterministic index. The fallback is total: any Qdrant failure
/// (disabled, unreachable, error) degrades silently to the in-memory index, so retrieval always works.
/// </summary>
public interface IVectorRetrievalRouter
{
    /// <summary>
    /// Rank <paramref name="candidates"/> (already claim-scoped by the caller) for <paramref name="query"/>,
    /// returning the top-k hits and the backend that served them.
    /// </summary>
    Task<RetrievalOutcome> RankAsync(string claimId, string query, IReadOnlyList<EvidenceChunk> candidates, int topK, CancellationToken ct = default);

    /// <summary>
    /// Resolve which backend would actually serve retrieval for this claim RIGHT NOW by exercising the
    /// real Qdrant path (ensure + upsert + search). Returns <see cref="VectorBackends.Qdrant"/> only on a
    /// successful round-trip; otherwise <see cref="VectorBackends.InMemoryHash"/>. Used for honest status.
    /// </summary>
    Task<string> ResolveServingBackendAsync(string claimId, IReadOnlyList<EvidenceChunk> candidates, CancellationToken ct = default);
}
