namespace InsuranceAIPlatform.Services.AiAnalysis.Rag.Runtime;

/// <summary>
/// Mechanically checks whether a local runtime (Ollama, Qdrant) is actually reachable, so the RAG
/// infrastructure status is honest instead of guessed from a config flag. It NEVER blocks the RAG
/// pipeline — the pipeline always has a deterministic local fallback (in-process index + mock
/// generation). No secret, no cloud call: local endpoints only.
/// </summary>
public interface IRagRuntimeProbe
{
    /// <summary>
    /// Returns true if a cheap, short-timeout local probe of <paramref name="endpoint"/>
    /// (optionally joined with <paramref name="healthPath"/>) gets any HTTP response.
    /// Connection refused / timeout / DNS failure returns false. Never throws.
    /// </summary>
    Task<bool> IsReachableAsync(string endpoint, string? healthPath, CancellationToken ct = default);
}
