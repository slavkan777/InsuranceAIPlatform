namespace InsuranceAIPlatform.Services.AiAnalysis.Rag.Ingestion;

/// <summary>
/// Turns a synthetic claim document's text into claim-scoped <c>EvidenceChunk</c> rows so a
/// newly-created claim can receive cited RAG analysis (closing the create-from-zero gap).
///
/// Contract guarantees:
/// <list type="bullet">
///   <item>Deterministic — same text yields the same chunks/embeddings (local hash embedder).</item>
///   <item>Additive + idempotent — never deletes or updates existing chunks; re-ingesting the same
///         document text produces no duplicates.</item>
///   <item>Claim-scoped — every created chunk's <c>ClaimId</c> is the target claim only, so the
///         retrieval filter (<c>ClaimId == claimId</c>) keeps evidence isolated; seeded claims
///         (CLM-1006/1007/1012) are never touched or referenced.</item>
/// </list>
/// </summary>
public interface IEvidenceIngestionService
{
    /// <summary>
    /// Splits <paramref name="text"/> into bounded chunks and persists them as claim-scoped
    /// <c>EvidenceChunk</c> rows (with cached deterministic embeddings) for <paramref name="claimId"/>.
    /// Returns the number of NEW chunks created (0 if the text was empty or all chunks already existed).
    /// </summary>
    Task<int> IngestDocumentTextAsync(
        string claimId,
        string documentId,
        string kind,
        string title,
        string text,
        CancellationToken ct = default);
}
