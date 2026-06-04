namespace InsuranceAIPlatform.Services.AiAnalysis.Rag.Retrieval;

/// <summary>
/// A point to upsert into the Qdrant collection. <see cref="ClaimId"/> is stored as a payload field
/// so search can be filtered to a single claim (the cross-claim leakage guard at the vector layer).
/// </summary>
public sealed record QdrantUpsertPoint(string ChunkId, string ClaimId, IReadOnlyList<float> Vector);

/// <summary>A single Qdrant search result mapped back to the originating chunk id and its score.</summary>
public sealed record QdrantSearchHit(string ChunkId, double Score);

/// <summary>
/// Minimal local Qdrant vector-store boundary used by <see cref="VectorRetrievalRouter"/> when the
/// Qdrant seam is enabled and the local runtime is reachable. Deliberately tiny: ensure-collection,
/// upsert, and a claim-filtered search — exactly what claim-scoped evidence retrieval needs.
///
/// Safety posture (QDRANT_RETRIEVAL_ADAPTER_V0.1):
///  - local-only HTTP, no secret, no cloud call;
///  - any method may throw if the local runtime is unavailable — the router catches and falls back
///    to the in-process deterministic index, so retrieval never fails because Qdrant is down;
///  - search MUST be filtered by claimId so no other claim's vectors can enter a claim's retrieval.
/// </summary>
public interface IQdrantVectorClient
{
    /// <summary>Create the collection if missing (Cosine distance, fixed <paramref name="dimensions"/>). Idempotent.</summary>
    Task EnsureCollectionAsync(int dimensions, CancellationToken ct = default);

    /// <summary>Upsert points (deterministic ids → idempotent re-upsert). No-op for an empty list.</summary>
    Task UpsertAsync(IReadOnlyList<QdrantUpsertPoint> points, CancellationToken ct = default);

    /// <summary>Top-k vector search filtered to a single claim. Returns (chunkId, score), highest score first.</summary>
    Task<IReadOnlyList<QdrantSearchHit>> SearchAsync(string claimId, IReadOnlyList<float> queryVector, int topK, CancellationToken ct = default);
}
