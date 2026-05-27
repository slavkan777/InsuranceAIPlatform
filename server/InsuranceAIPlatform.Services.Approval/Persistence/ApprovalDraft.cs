namespace InsuranceAIPlatform.Services.Approval.Persistence;

/// <summary>
/// Approval draft for a claim. ClaimId is PK (one draft per claim).
/// Submitted=false always in seed — no submit behavior wired.
/// </summary>
public sealed class ApprovalDraft
{
    public string ClaimId { get; set; } = string.Empty;     // PK + cross-context ref
    public string? CurrentDecision { get; set; }
    public string? Notes { get; set; }
    public bool Submitted { get; set; } = false;
    public DateTimeOffset? SubmittedAt { get; set; }
    public DateTimeOffset? SavedAt { get; set; }
    public string AiRecommendation { get; set; } = string.Empty;
    public decimal RecommendedPayout { get; set; }

    public ICollection<ApprovalDecisionOption> Options { get; set; } = new List<ApprovalDecisionOption>();
}
