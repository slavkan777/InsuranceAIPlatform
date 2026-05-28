using InsuranceAIPlatform.Services.AiAnalysis.Contracts;

namespace InsuranceAIPlatform.Services.AiAnalysis.Providers;

/// <summary>
/// Adapter exists only to make the disabled mode reachable through DI;
/// it must never read DEEPSEEK_API_KEY or open an HTTP connection.
///
/// Mode=DeepSeekDisabled. AnalyzeAsync always throws InvalidOperationException.
/// No HttpClient, no IHttpClientFactory, no SDK reference, no key read.
/// </summary>
public sealed class DisabledDeepSeekAiProvider : IAiProvider
{
    // Adapter exists only to make the disabled mode reachable through DI;
    // it must never read DEEPSEEK_API_KEY or open an HTTP connection.

    public AiProviderMode Mode => AiProviderMode.DeepSeekDisabled;

    public Task<AiProviderRawOutput> AnalyzeAsync(AiAnalysisRequest request, CancellationToken ct = default)
    {
        throw new InvalidOperationException(
            "DeepSeek provider is disabled by configuration and never reads DEEPSEEK_API_KEY; " +
            "real provider calls require an explicit later gate.");
    }
}
