using InsuranceAIPlatform.Services.AiAnalysis.Guardrails;

namespace InsuranceAIPlatform.Services.AiAnalysis.Contracts;

// -----------------------------------------------------------------------
// Output DTO records returned by the orchestrator and BFF.
// AI is advisory only — no field here authorizes a payout, status change,
// customer message, fraud accusation, or claim rejection as a final fact.
// -----------------------------------------------------------------------

public sealed record FindingOut(string Id, string Category, string Text, string Severity);
public sealed record EvidenceOut(string Id, string Source, string Note, int Confidence);
public sealed record RiskOut(string Id, string Label, int Weight);

public sealed record RecommendedActionOut(
    string Action,
    string Rationale,
    int ConfidenceScore);

public sealed record CostTraceOut(
    int Tokens,
    decimal EstimatedCost,
    string CurrencyCode = "USD");

/// <summary>
/// Structured output from a completed AI analysis run.
/// AdvisoryOnlyWarning is hardcoded — there is no constructor path that removes it.
/// </summary>
public sealed record AiAnalysisResult(
    string RunId,
    string ClaimId,
    string ProviderMode,
    string ModelName,
    string Status,
    string SummaryText,
    RecommendedActionOut RecommendedAction,
    string PolicyCoverageExplanation,
    /// <summary>"low" | "moderate" | "high" — derived from sum of risk signal weights.</summary>
    string RiskLevel,
    int ConfidenceScore,
    IReadOnlyList<FindingOut> Findings,
    IReadOnlyList<EvidenceOut> Evidence,
    IReadOnlyList<RiskOut> Risks,
    GuardrailFlags Guardrails,
    CostTraceOut CostTrace,
    string CorrelationId,
    DateTime CreatedAtUtc,
    string AdvisoryOnlyWarning = "AI output is advisory only — human decision is final.");
