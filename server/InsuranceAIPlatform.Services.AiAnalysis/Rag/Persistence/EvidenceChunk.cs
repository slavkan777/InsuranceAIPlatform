namespace InsuranceAIPlatform.Services.AiAnalysis.Rag.Persistence;

/// <summary>
/// The retrievable unit of evidence (source of truth in SQL). Each chunk carries synthetic
/// text and a cached deterministic embedding.
///
/// Merge note (RAG_LOCAL_FOUNDATION_MEGA_V0.1 §3): the proposed separate <c>ChunkEmbedding</c>
/// table is folded into these nullable cache columns. Justification: embeddings here are
/// DETERMINISTIC and rebuildable from <see cref="Text"/>, so a separate table buys nothing for
/// v1. <see cref="ChunkHash"/> is the re-embed gate; <see cref="EmbeddingModel"/> records which
/// model produced the cache. The vector cache can be dropped and rebuilt without touching source rows.
///
/// <see cref="DocumentId"/> / <see cref="ClaimId"/> are cross-context string references
/// (to Documents' ClaimDocument and the claim) — no foreign entity is shared across contexts.
/// </summary>
public sealed class EvidenceChunk
{
    public string ChunkId { get; set; } = string.Empty;        // e.g. "CLM-1006-police#0"
    public string ClaimId { get; set; } = string.Empty;        // retrieval filter key (cross-context ref)
    public string DocumentId { get; set; } = string.Empty;     // ClaimDocument.Id (cross-context ref)
    public string Kind { get; set; } = string.Empty;           // police | invoice | application | policy-clause | statement | photo-caption
    public int Ordinal { get; set; }
    public string Text { get; set; } = string.Empty;           // synthetic evidence text — SOURCE OF TRUTH
    public int TokenCount { get; set; }
    public string ChunkHash { get; set; } = string.Empty;      // deterministic hash of Text (re-embed gate)
    public string Language { get; set; } = "uk";               // "uk" | "en"
    public string SourceVersion { get; set; } = "v0.1";

    // ---- embedding cache (merged ChunkEmbedding; rebuildable, never the source of truth) ----
    public string? EmbeddingModel { get; set; }
    public int EmbeddingDim { get; set; }
    public string? EmbeddingJson { get; set; }                 // JSON float[]; null until embedded
}
