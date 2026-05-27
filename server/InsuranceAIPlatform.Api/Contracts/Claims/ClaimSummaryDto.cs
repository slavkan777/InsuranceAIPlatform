namespace InsuranceAIPlatform.Api.Contracts.Claims;

/// <summary>Dashboard aggregate counters. Single object for GET /api/claims/summary.</summary>
public record ClaimSummaryDto(
    int TotalActive,
    int PendingReview,
    int HighRisk,
    double AvgSlaRemainingHours,
    int ProcessedToday,
    int AiAnalysisRunning);
