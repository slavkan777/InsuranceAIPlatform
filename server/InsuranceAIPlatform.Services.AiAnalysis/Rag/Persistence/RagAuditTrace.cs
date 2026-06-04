namespace InsuranceAIPlatform.Services.AiAnalysis.Rag.Persistence;

/// <summary>
/// One persisted, reproducible RAG answer event (audit + cost trace).
///
/// Merge note (§3): the proposed <c>RetrievalQuery</c>, <c>RetrievedEvidence</c>,
/// <c>GroundedAnswer</c>, and <c>CitationReference</c> entities are folded into this single
/// audit row for v1: the query, the retrieved chunk ids, the grounded answer, and the citations
/// (as JSON) are all captured here so an answer can be replayed/audited. Full normalization into
/// separate tables is the documented expansion path. SQL remains the source of truth.
/// ProviderMode is ALWAYS "Mock" or "LocalLlama" — never a real cloud provider in this gate.
/// </summary>
public sealed class RagAuditTrace
{
    public string TraceId { get; set; } = string.Empty;          // e.g. "ragtrc_3f9c1a"
    public string ClaimId { get; set; } = string.Empty;
    public string UseCase { get; set; } = string.Empty;
    public string QueryText { get; set; } = string.Empty;
    public string RetrievedChunkIdsCsv { get; set; } = string.Empty;
    public string CitationsJson { get; set; } = string.Empty;    // JSON array of {chunkId, documentId, snippet}
    public string AnswerText { get; set; } = string.Empty;
    public int Confidence { get; set; }
    public string ProviderMode { get; set; } = "Mock";          // "Mock" | "LocalLlama" — never real cloud
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public long CostMicros { get; set; }                          // cost in millionths of a currency unit (0 for local/mock)
    public long RetrievalMs { get; set; }
    public bool AdvisoryOnly { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }
}
