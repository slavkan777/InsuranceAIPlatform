namespace InsuranceAIPlatform.Services.AiAnalysis.Rag.Persistence;

/// <summary>
/// A policy clause — the citable unit for "policy coverage check". Synthetic only.
/// Lives in the ai_analysis schema (RAG foundation) and references policy/product by string id
/// (cross-context ref; no Policy entity is shared — preserves the bounded-context boundary).
/// SQL is the source of truth; clauses are also chunked into <see cref="EvidenceChunk"/> for retrieval.
/// </summary>
public sealed class PolicyClause
{
    public string ClauseId { get; set; } = string.Empty;       // e.g. "CLA-AC-COVER-001"
    public string ProductCode { get; set; } = string.Empty;    // e.g. "AUTO-COMPREHENSIVE"
    public string? PolicyId { get; set; }                       // optional specific policy, e.g. "POL-2025-AC-4421"
    public string ClauseType { get; set; } = string.Empty;     // "coverage" | "exclusion" | "limit" | "deductible"
    public string Title { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public int Ordinal { get; set; }
}
