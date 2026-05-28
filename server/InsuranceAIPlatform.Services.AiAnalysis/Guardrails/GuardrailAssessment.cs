namespace InsuranceAIPlatform.Services.AiAnalysis.Guardrails;

/// <summary>
/// Result of guardrail evaluation. If Blocked=true, the run should be persisted
/// with Status="blocked_unsafe" and no child collections inserted.
/// GuardrailFlags is always AdvisoryOnly regardless of the blocked state.
/// </summary>
public sealed record GuardrailAssessment(
    bool Blocked,
    string? ReasonCode,
    string? OffendingPhrase,
    GuardrailFlags Flags);
