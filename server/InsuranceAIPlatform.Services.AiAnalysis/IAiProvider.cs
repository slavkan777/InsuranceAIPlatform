using InsuranceAIPlatform.Services.AiAnalysis.Contracts;

namespace InsuranceAIPlatform.Services.AiAnalysis;

/// <summary>
/// AI provider adapter boundary. Implementations: MockAiProvider (default) and
/// DisabledDeepSeekAiProvider (disabled, throws). No HttpClient, no SDK, no external call.
/// DEEPSEEK_API_KEY is never read — not even in the disabled adapter.
/// </summary>
public interface IAiProvider
{
    /// <summary>Configured provider mode.</summary>
    AiProviderMode Mode { get; }

    /// <summary>
    /// Runs AI analysis for the given request. MockAiProvider returns deterministic output.
    /// DisabledDeepSeekAiProvider throws InvalidOperationException unconditionally.
    /// No real provider call is made in this gate.
    /// </summary>
    Task<AiProviderRawOutput> AnalyzeAsync(AiAnalysisRequest request, CancellationToken ct = default);
}
