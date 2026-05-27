using InsuranceAIPlatform.BuildingBlocks;

namespace InsuranceAIPlatform.Services.AiAnalysis;

/// <summary>
/// AI Analysis service boundary (skeleton). Future home of advisory AI workflows behind
/// <see cref="IAiProvider"/> (mock default; DeepSeek opt-in / disabled). AI output is advisory only
/// and never the final authority. No provider, no HTTP, no <c>DEEPSEEK_API_KEY</c>, no call in the skeleton.
/// </summary>
public interface IAiAnalysisService : IServiceHealthContributor
{
    /// <summary>Canonical service name (see <see cref="ServiceNames.AiAnalysis"/>).</summary>
    string ServiceName { get; }

    /// <summary>Provider mode the service is wired with. Skeleton = <see cref="AiProviderMode.Disabled"/> (no provider).</summary>
    AiProviderMode ProviderMode { get; }

    /// <summary>AI output is advisory only; human approval is always final.</summary>
    bool AdvisoryOnly { get; }
}
