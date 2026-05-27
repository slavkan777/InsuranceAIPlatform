namespace InsuranceAIPlatform.Services.Claims.Persistence;

/// <summary>
/// Claim aggregate root. Mirrors ClaimDetailsDto field set from InMemoryClaimReadService.
/// CustomerId, PolicyId are id-only cross-context string references (no EF navigation to other contexts).
/// </summary>
public sealed class Claim
{
    public string ClaimId { get; set; } = string.Empty;     // e.g. "CLM-1006"
    public string CustomerId { get; set; } = string.Empty;  // cross-context ref by id
    public string PolicyId { get; set; } = string.Empty;    // cross-context ref by id
    public string Customer { get; set; } = string.Empty;    // denormalized display name
    public string Vehicle { get; set; } = string.Empty;     // denormalized display string
    public string VehicleVin { get; set; } = string.Empty;
    public string Policy { get; set; } = string.Empty;      // product name display
    public string EventType { get; set; } = string.Empty;
    public DateOnly EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Risk { get; set; } = string.Empty;
    public int RiskScore { get; set; }
    public int Confidence { get; set; }
    public DateTimeOffset SlaDeadline { get; set; }
    public int DocumentsReceived { get; set; }
    public int DocumentsTotal { get; set; }
    public string? MissingDocument { get; set; }
    public decimal Estimate { get; set; }
    public decimal ExpectedBenchmark { get; set; }
    public decimal Deductible { get; set; }
    public decimal RecommendedPayout { get; set; }
    public string TraceId { get; set; } = string.Empty;
    public string RunId { get; set; } = string.Empty;
    public int Tokens { get; set; }
    public decimal Cost { get; set; }
    public double DurationSec { get; set; }

    public ICollection<ClaimStatusHistory> StatusHistory { get; set; } = new List<ClaimStatusHistory>();
}
