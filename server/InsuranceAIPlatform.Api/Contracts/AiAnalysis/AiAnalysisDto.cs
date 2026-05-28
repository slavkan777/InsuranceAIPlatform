namespace InsuranceAIPlatform.Api.Contracts.AiAnalysis;

// -----------------------------------------------------------------------
// BFF-shaped DTOs for the AI Analysis endpoints.
// AI is advisory only — IsAdvisoryOnly is hardcoded true, Notice is always present.
// -----------------------------------------------------------------------

public sealed record AiAnalysisFindingDto(
    string Id,
    string Category,
    string Text,
    string Severity);

public sealed record AiAnalysisEvidenceDto(
    string Id,
    string Source,
    string Note,
    int Confidence);

public sealed record AiAnalysisRiskDto(
    string Id,
    string Label,
    int Weight);

public sealed record AiAnalysisGuardrailsDto(
    bool AdvisoryOnly,
    bool RequiresHumanReview,
    bool CanApprovePayout,
    bool CanRejectClaim,
    bool CanAccuseFraudFinal,
    bool CanSendCustomerMessage,
    bool CanChangeClaimStatus);

public sealed record AiAnalysisCostTraceDto(
    int Tokens,
    decimal EstimatedCost,
    string CurrencyCode);

public sealed record AiAnalysisRecommendedActionDto(
    string Action,
    string Rationale,
    int ConfidenceScore);

/// <summary>
/// BFF DTO wrapping AiAnalysisResult. IsAdvisoryOnly is hardcoded true.
/// Notice is hardcoded advisory warning — no constructor path removes it.
/// </summary>
public sealed record AiAnalysisDto(
    string RunId,
    string ClaimId,
    string ProviderMode,
    string ModelName,
    string Status,
    string SummaryText,
    AiAnalysisRecommendedActionDto RecommendedAction,
    string PolicyCoverageExplanation,
    string RiskLevel,
    int ConfidenceScore,
    IReadOnlyList<AiAnalysisFindingDto> Findings,
    IReadOnlyList<AiAnalysisEvidenceDto> Evidence,
    IReadOnlyList<AiAnalysisRiskDto> Risks,
    AiAnalysisGuardrailsDto Guardrails,
    AiAnalysisCostTraceDto CostTrace,
    string CorrelationId,
    DateTime CreatedAtUtc,
    bool IsAdvisoryOnly = true,
    string Notice = "AI output is advisory only — human decision is final.");

/// <summary>Empty body for POST /api/claims/{claimId}/ai-analysis/run</summary>
public sealed record AiAnalysisRunRequestDto;
