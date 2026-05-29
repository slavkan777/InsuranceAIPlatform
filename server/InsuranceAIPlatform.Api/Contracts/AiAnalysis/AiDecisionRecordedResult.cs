namespace InsuranceAIPlatform.Api.Contracts.AiAnalysis;

/// <summary>
/// Result of <c>POST /api/claims/{claimId}/ai-decision</c> — extends the canonical
/// command result with AI-derived metadata about the run the decision is based on.
/// Audit + outbox only. Always advisory; never authorises payout / customer messages
/// / claim status mutation / external side effect.
/// </summary>
public sealed record AiDecisionRecordedResult(
    bool Success,
    string CommandId,
    string ClaimId,
    string? Status,
    int? AuditEventId,
    int? OutboxMessageId,
    string CorrelationId,
    string Message,
    IReadOnlyList<string> Warnings,
    string AiRunId,
    string ProviderMode,
    string ModelName,
    string RecommendedAction,
    string RiskLevel,
    int ConfidenceScore,
    bool IsAdvisoryOnly,
    string Source);
