namespace InsuranceAIPlatform.Services.Approval.Persistence;

/// <summary>
/// Available human decision option for an approval draft.
/// </summary>
public sealed class ApprovalDecisionOption
{
    public int Id { get; set; }
    public string ClaimId { get; set; } = string.Empty;     // FK to ApprovalDraft
    public string Key { get; set; } = string.Empty;         // e.g. "request", "approve", "reject"
    public string Label { get; set; } = string.Empty;
    public bool Recommended { get; set; }
    public string Rationale { get; set; } = string.Empty;

    public ApprovalDraft? Draft { get; set; }
}
