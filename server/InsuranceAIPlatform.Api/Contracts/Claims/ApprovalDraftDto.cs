namespace InsuranceAIPlatform.Api.Contracts.Claims;

/// <summary>Current approval state for a claim, plus available decision options. Advisory only — human must decide.</summary>
public record ApprovalDraftDto(
    string ClaimId,
    string? CurrentDecision,
    string? Notes,
    DateTimeOffset? SavedAt,
    bool Submitted,
    DateTimeOffset? SubmittedAt,
    HumanDecisionOptionDto[] AvailableOptions,
    string? AiRecommendation,
    decimal RecommendedPayout);

/// <summary>One decision option available to the human adjuster.</summary>
public record HumanDecisionOptionDto(
    string Value,
    string Label,
    bool Recommended,
    string? Description);
