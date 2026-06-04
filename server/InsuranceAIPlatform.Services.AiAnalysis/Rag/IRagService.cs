using InsuranceAIPlatform.Services.AiAnalysis.Rag.Contracts;

namespace InsuranceAIPlatform.Services.AiAnalysis.Rag;

/// <summary>
/// Local RAG facade: retrieval-before-generation grounded answering, raw evidence search,
/// the gold evaluation question set, and the persisted audit trail. All synthetic, advisory-only,
/// strictly claim-scoped. No external/cloud call.
/// </summary>
public interface IRagService
{
    /// <summary>Retrieve top-k evidence for the claim, generate a grounded advisory answer, persist an audit trace.</summary>
    Task<RagAnswer> AskAsync(string claimId, string question, string? useCase, string correlationId, CancellationToken ct = default);

    /// <summary>Raw semantic evidence search within a claim (no generated answer).</summary>
    Task<IReadOnlyList<RagEvidenceHit>> SearchEvidenceAsync(string claimId, string query, int topK, CancellationToken ct = default);

    /// <summary>The gold evaluation questions seeded for a claim.</summary>
    Task<IReadOnlyList<RagEvalQuestionView>> GetEvaluationQuestionsAsync(string claimId, CancellationToken ct = default);

    /// <summary>Recent persisted RAG audit traces for a claim (newest first).</summary>
    Task<IReadOnlyList<RagAuditView>> GetAuditAsync(string claimId, int limit, CancellationToken ct = default);

    /// <summary>
    /// Cross-claim similar-claims search. Returns claim-level results only (id/score/reason/categories) —
    /// never another claim's raw evidence text. The target claim is excluded from its own results.
    /// </summary>
    Task<IReadOnlyList<SimilarClaim>> FindSimilarClaimsAsync(string claimId, int topK, CancellationToken ct = default);

    /// <summary>
    /// Snapshot of RAG infrastructure health for a claim: SQL counts, embedding-cache completeness,
    /// and LocalLlama runtime reachability. Read-only; no side effects.
    /// </summary>
    Task<RagInfrastructureStatus> GetInfrastructureStatusAsync(string claimId, string correlationId, CancellationToken ct = default);

    /// <summary>
    /// Deterministically re-embeds all EvidenceChunks for the specified claim using the configured
    /// IEmbeddingProvider, persists updated EmbeddingJson/EmbeddingModel/EmbeddingDim, then returns
    /// refreshed infrastructure status. Idempotent: safe to call multiple times. No deletes.
    /// </summary>
    Task<RagInfrastructureStatus> ReindexClaimAsync(string claimId, string correlationId, CancellationToken ct = default);
}
