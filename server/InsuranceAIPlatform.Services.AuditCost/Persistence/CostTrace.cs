namespace InsuranceAIPlatform.Services.AuditCost.Persistence;

/// <summary>Cost distribution entry for a claim pipeline run.</summary>
public sealed class CostTrace
{
    public int Id { get; set; }
    public string ClaimId { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
