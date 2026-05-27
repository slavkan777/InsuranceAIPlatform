using InsuranceAIPlatform.BuildingBlocks;

namespace InsuranceAIPlatform.Services.AiAnalysis;

/// <summary>
/// Skeleton implementation of <see cref="IAiAnalysisService"/>. Reports readiness only. Wires no
/// <see cref="IAiProvider"/>, so nothing here can call DeepSeek/OpenAI/Azure. Readiness is
/// <see cref="ServiceReadinessStatus.Deferred"/> because the provider integration is a later gate.
/// </summary>
public sealed class AiAnalysisService : IAiAnalysisService
{
    public string ServiceName => ServiceNames.AiAnalysis;

    // Skeleton wires no provider implementation — no external AI call is possible from here.
    public AiProviderMode ProviderMode => AiProviderMode.Disabled;

    public bool AdvisoryOnly => true;

    public ServiceHealthSnapshot GetHealth() => new(
        ServiceNames.AiAnalysis,
        ServiceReadinessStatus.Deferred,
        "skeleton-v0.1",
        new[] { "advisory-analysis(deferred)", "structured-findings(deferred)", "provider:disabled" });
}
