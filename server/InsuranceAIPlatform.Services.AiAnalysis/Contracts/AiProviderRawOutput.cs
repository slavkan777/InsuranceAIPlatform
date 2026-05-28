namespace InsuranceAIPlatform.Services.AiAnalysis.Contracts;

// -----------------------------------------------------------------------
// Draft records — simple value objects used internally before mapping to
// the structured AiAnalysisResult returned by the orchestrator.
// -----------------------------------------------------------------------

/// <summary>A raw finding draft from the AI provider before normalization.</summary>
public sealed record AiFindingDraft(string Id, string Category, string Text, string Severity);

/// <summary>A raw evidence reference draft from the AI provider.</summary>
public sealed record AiEvidenceDraft(string Id, string Source, string Note, int Confidence);

/// <summary>A raw risk signal draft from the AI provider.</summary>
public sealed record AiRiskDraft(string Id, string Label, int Weight);

/// <summary>
/// Raw output from an AI provider. Contains all fields needed for guardrail evaluation
/// and structured persistence. No payout amounts, no status changes, no customer data.
/// </summary>
public sealed record AiProviderRawOutput(
    string ModelName,
    string SummaryText,
    IReadOnlyList<AiFindingDraft> Findings,
    IReadOnlyList<AiEvidenceDraft> Evidence,
    IReadOnlyList<AiRiskDraft> Risks,
    string RecommendedActionText,
    string PolicyExplanationText,
    int ConfidenceScore,
    int Tokens,
    decimal Cost);
