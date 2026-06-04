namespace InsuranceAIPlatform.Services.AiAnalysis.Rag;

/// <summary>
/// Configuration for the local RAG foundation. Bound from appsettings section "Rag".
///
/// Safety posture (RAG_LOCAL_FOUNDATION_MEGA_V0.1):
///  - LocalLlamaEnabled defaults to FALSE — the LocalLlama/Ollama generator seam is present
///    but never called unless explicitly enabled. The deterministic mock generator is the default.
///  - No API key / secret property exists here (same rule as <c>AiProviderOptions</c>).
///  - Embeddings are deterministic and local — no external embedding service.
/// </summary>
public sealed class RagOptions
{
    public const string SectionName = "Rag";

    /// <summary>Dimension of the deterministic feature-hashing embedding. 256 is ample for a ~150-400 chunk corpus.</summary>
    public int EmbeddingDimensions { get; set; } = 256;

    /// <summary>Default number of chunks returned by retrieval.</summary>
    public int DefaultTopK { get; set; } = 4;

    /// <summary>
    /// Disabled-by-default LocalLlama/Ollama generation seam. When false (default) the deterministic
    /// <c>MockGroundedAnswerGenerator</c> is used and NO local model is contacted. No real LLaMA call
    /// is ever required for tests.
    /// </summary>
    public bool LocalLlamaEnabled { get; set; } = false;

    /// <summary>Ollama-style endpoint, used ONLY when <see cref="LocalLlamaEnabled"/> is true. Never called otherwise.</summary>
    public string LocalLlamaEndpoint { get; set; } = "http://localhost:11434";

    /// <summary>Local model id, used ONLY when <see cref="LocalLlamaEnabled"/> is true.</summary>
    public string LocalLlamaModel { get; set; } = "llama3.1:8b";

    // ---- Vector runtime (Qdrant) seam — disabled-by-default, fallback to the in-process index ----

    /// <summary>
    /// Disabled-by-default Qdrant vector-runtime seam. When false (default) the in-process deterministic
    /// embedding index is the vector layer and Qdrant is never contacted. When true, Qdrant is probed for
    /// status; the in-process index always remains the safe fallback if Qdrant is unreachable.
    /// </summary>
    public bool QdrantEnabled { get; set; } = false;

    /// <summary>Local Qdrant HTTP endpoint. Probed for status only; never carries a secret (local-only).</summary>
    public string QdrantEndpoint { get; set; } = "http://localhost:6333";

    /// <summary>Local Qdrant collection name, used ONLY when <see cref="QdrantEnabled"/> is true.</summary>
    public string QdrantCollection { get; set; } = "insurance_evidence";

    /// <summary>
    /// Timeout (ms) for a local runtime reachability probe. Kept short so a missing local runtime
    /// (Ollama/Qdrant not started) never stalls the infrastructure-status endpoint.
    /// </summary>
    public int RuntimeProbeTimeoutMs { get; set; } = 1500;
}
