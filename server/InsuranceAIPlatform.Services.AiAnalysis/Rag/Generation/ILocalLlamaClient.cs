namespace InsuranceAIPlatform.Services.AiAnalysis.Rag.Generation;

/// <summary>
/// A completion returned by the local Ollama runtime. Token counts come from the Ollama response
/// (<c>prompt_eval_count</c> / <c>eval_count</c>) when present, otherwise 0 (the caller estimates).
/// <paramref name="Model"/> is the model id Ollama reports it actually ran.
/// </summary>
public sealed record LocalLlamaCompletion(string Text, int PromptTokens, int CompletionTokens, string Model);

/// <summary>
/// Thin client over a LOCAL Ollama endpoint (no API key, no cloud). The concrete HTTP implementation
/// lives in the Api layer (mirrors <c>IQdrantVectorClient</c> / <c>HttpQdrantVectorClient</c>) so this
/// project takes no <c>Microsoft.Extensions.Http</c> dependency.
///
/// Contract: <see cref="TryComplete"/> NEVER throws and returns <c>null</c> on ANY failure
/// (endpoint unreachable, timeout, non-2xx, empty body). A <c>null</c> result tells the generator to
/// fall back to the deterministic mock — so a slow/absent Ollama can never break the business flow
/// and can never be mislabelled as a live local answer.
/// </summary>
public interface ILocalLlamaClient
{
    /// <summary>
    /// Synchronously POST a grounded chat prompt to the local Ollama endpoint and return the completion,
    /// or <c>null</c> on any failure/timeout. Synchronous to match the synchronous
    /// <see cref="IGroundedAnswerGenerator.Generate"/> seam without an interface refactor.
    /// </summary>
    LocalLlamaCompletion? TryComplete(string systemPrompt, string userPrompt, CancellationToken ct = default);
}
