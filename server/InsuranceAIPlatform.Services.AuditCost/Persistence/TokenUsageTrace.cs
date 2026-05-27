namespace InsuranceAIPlatform.Services.AuditCost.Persistence;

/// <summary>Token usage trace for a claim pipeline run. Cost may be 0/mock.</summary>
public sealed class TokenUsageTrace
{
    public int Id { get; set; }
    public string ClaimId { get; set; } = string.Empty;
    public int Tokens { get; set; }
    public decimal Cost { get; set; }
}
