namespace InsuranceAIPlatform.Services.AiAnalysis.Guardrails;

/// <summary>
/// Immutable guardrail flags. All "Can*" flags are HARDCODED false — there is no constructor path,
/// property setter, or reflection path that can flip any of them to true. AI is advisory only.
///
/// These flags are returned as part of every AiAnalysisResult and stored in GuardrailFlagsJson.
/// </summary>
public sealed record GuardrailFlags
{
    /// <summary>AI output is advisory — human review required before any action.</summary>
    public bool AdvisoryOnly { get; } = true;

    /// <summary>Human review is always required before acting on AI output.</summary>
    public bool RequiresHumanReview { get; } = true;

    /// <summary>AI CANNOT approve payout — hardcoded false.</summary>
    public bool CanApprovePayout { get; } = false;

    /// <summary>AI CANNOT reject a claim as a final decision — hardcoded false.</summary>
    public bool CanRejectClaim { get; } = false;

    /// <summary>AI CANNOT accuse fraud as a final, actionable fact — hardcoded false.</summary>
    public bool CanAccuseFraudFinal { get; } = false;

    /// <summary>AI CANNOT send customer messages — hardcoded false.</summary>
    public bool CanSendCustomerMessage { get; } = false;

    /// <summary>AI CANNOT change claim status — hardcoded false.</summary>
    public bool CanChangeClaimStatus { get; } = false;

    // Private parameterless constructor forces use of the static singleton instance.
    private GuardrailFlags() { }

    /// <summary>The single immutable instance. No other instance can be constructed.</summary>
    public static GuardrailFlags Advisory { get; } = new GuardrailFlags();
}
