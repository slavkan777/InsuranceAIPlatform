namespace InsuranceAIPlatform.Services.AiAnalysis;

/// <summary>
/// Placeholder for a future AI provider adapter. NOT implemented and NOT called in the skeleton.
/// The real DeepSeek adapter is isolated to a later AI-provider gate (mock default,
/// opt-in / disabled-by-default). The skeleton adds no HTTP client, no SDK, and never reads
/// <c>DEEPSEEK_API_KEY</c>. No concrete implementation is registered in DI.
/// </summary>
public interface IAiProvider
{
    /// <summary>Configured provider mode. The skeleton wires no implementation, so no provider runs.</summary>
    AiProviderMode Mode { get; }
}
