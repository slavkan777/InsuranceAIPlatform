namespace InsuranceAIPlatform.BuildingBlocks;

/// <summary>
/// Identifies who performed a command (human adjuster, system, AI-advisory-only).
/// Always synthetic in demo mode — never carries real PII.
/// </summary>
public sealed record ActorContext(
    string ActorId,
    string ActorName,
    string ActorType);

/// <summary>
/// Well-known synthetic actor identities for demo/test use.
/// </summary>
public static class CommandActors
{
    public const string SyntheticAdjusterId   = "demo.adjuster@insuranceai.local";
    public const string SyntheticAdjusterName = "Synthetic Adjuster";
    public const string ActorTypeHuman        = "human";

    /// <summary>Returns the canonical synthetic adjuster actor used in all command endpoints.</summary>
    public static ActorContext SyntheticAdjuster() =>
        new(SyntheticAdjusterId, SyntheticAdjusterName, ActorTypeHuman);
}
