namespace InsuranceAIPlatform.Services.AiAnalysis;

/// <summary>
/// Selects AI provider behaviour. Future default is <see cref="Mock"/>; DeepSeek is opt-in and
/// disabled by default; the skeleton itself runs as <see cref="Disabled"/> (no provider wired, no call).
/// </summary>
public enum AiProviderMode
{
    /// <summary>Deterministic mock provider (future default). No external call.</summary>
    Mock,

    /// <summary>DeepSeek adapter present but disabled — opt-in only, isolated to a later gate.</summary>
    DeepSeekDisabled,

    /// <summary>No provider configured or wired — the current skeleton state.</summary>
    Disabled
}
