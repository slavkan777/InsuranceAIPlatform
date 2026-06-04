using InsuranceAIPlatform.Services.AiAnalysis.Rag.Contracts;

namespace InsuranceAIPlatform.Services.AiAnalysis.Rag.Generation;

/// <summary>
/// Disabled-by-default LocalLlama / Ollama generation seam (LOCAL_LLAMA_PROVIDER_V0.1 is a future gate).
///
/// In this gate it performs NO network call: it always delegates to the deterministic
/// <see cref="MockGroundedAnswerGenerator"/> — a safe, honest fallback. It is wired into DI ONLY
/// when <c>Rag:LocalLlamaEnabled = true</c>; the default registration is the mock generator.
/// When the real Ollama integration lands, replace the delegation below with an HTTP call to
/// <c>RagOptions.LocalLlamaEndpoint</c>, keeping the same fallback-on-failure contract.
/// No API key / secret is involved (local model).
/// </summary>
public sealed class LocalLlamaGroundedAnswerGenerator : IGroundedAnswerGenerator
{
    private readonly RagOptions _options;
    private readonly MockGroundedAnswerGenerator _fallback;

    public LocalLlamaGroundedAnswerGenerator(RagOptions options, MockGroundedAnswerGenerator fallback)
    {
        _options = options;
        _fallback = fallback;
    }

    // Honest: while the real local call is not implemented, the effective provider is the mock.
    public string ProviderMode => "Mock";

    public GroundedDraft Generate(GroundedRequest request)
    {
        // Future: if (_options.LocalLlamaEnabled) -> POST to _options.LocalLlamaEndpoint (Ollama),
        // grounding the prompt on request.Retrieved, with fallback to _fallback on any failure.
        // For this gate we never call out — deterministic fallback only.
        return _fallback.Generate(request);
    }
}
