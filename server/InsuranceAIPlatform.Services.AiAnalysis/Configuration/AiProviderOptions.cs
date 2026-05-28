namespace InsuranceAIPlatform.Services.AiAnalysis.Configuration;

/// <summary>
/// Configuration for the AI provider. Bound from appsettings section "AiProvider".
/// NO key property — DEEPSEEK_API_KEY must never appear here.
/// Default mode: Mock (no external call).
/// </summary>
public sealed class AiProviderOptions
{
    public string Mode { get; set; } = "Mock";

    /// <summary>
    /// Defense-in-depth: even if Mode="DeepSeek", real calls are blocked unless this is true.
    /// Default: false. This gate always forces false regardless of config value.
    /// </summary>
    public bool RealCallsEnabled { get; set; } = false;

    public DeepSeekOptions DeepSeek { get; set; } = new();
}

/// <summary>
/// DeepSeek model config. NO key property — DEEPSEEK_API_KEY is never stored here.
/// </summary>
public sealed class DeepSeekOptions
{
    public string Model { get; set; } = "deepseek-v4-flash";
}
