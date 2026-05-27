namespace InsuranceAIPlatform.Services.Claims.Persistence;

/// <summary>
/// Optional status history entry for a claim.
/// </summary>
public sealed class ClaimStatusHistory
{
    public int Id { get; set; }
    public string ClaimId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset ChangedAt { get; set; }
    public string? Note { get; set; }

    public Claim? Claim { get; set; }
}
