using InsuranceAIPlatform.Services.AiAnalysis.Rag.Persistence;

namespace InsuranceAIPlatform.Services.AiAnalysis.Rag.Retrieval;

/// <summary>
/// Supplies the candidate chunks for a claim. The implementation MUST scope strictly to the
/// requested claimId — this is the cross-claim leakage guard (NEGATIVE_PASS): no other claim's
/// evidence can ever enter retrieval for this claim.
/// </summary>
public interface IRagChunkSource
{
    Task<IReadOnlyList<EvidenceChunk>> GetClaimChunksAsync(string claimId, CancellationToken ct = default);

    /// <summary>
    /// All chunks across all claims — used ONLY to build claim-level centroids for cross-claim
    /// similarity. Callers must never surface another claim's raw text from this set; only
    /// claim-level metadata (id/score/categories) may leave the similarity boundary.
    /// </summary>
    Task<IReadOnlyList<EvidenceChunk>> GetAllChunksAsync(CancellationToken ct = default);
}
