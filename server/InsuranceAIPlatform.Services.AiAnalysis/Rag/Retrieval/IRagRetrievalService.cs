using InsuranceAIPlatform.Services.AiAnalysis.Rag.Contracts;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Persistence;

namespace InsuranceAIPlatform.Services.AiAnalysis.Rag.Retrieval;

/// <summary>
/// Pure similarity ranking over a candidate set. Claim-scoping is the caller's responsibility
/// (the chunk source filters by claimId) — this keeps ranking deterministic and unit-testable.
/// </summary>
public interface IRagRetrievalService
{
    /// <summary>Embeds the query and returns the top-k candidates by cosine similarity (deterministic tie-break by ChunkId).</summary>
    IReadOnlyList<ScoredChunk> Rank(string query, IReadOnlyList<EvidenceChunk> candidates, int topK);
}
