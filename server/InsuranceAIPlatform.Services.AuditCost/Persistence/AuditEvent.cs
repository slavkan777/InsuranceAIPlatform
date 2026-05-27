namespace InsuranceAIPlatform.Services.AuditCost.Persistence;

/// <summary>Audit trail event for a claim pipeline run.</summary>
public sealed class AuditEvent
{
    public int Id { get; set; }
    public string ClaimId { get; set; } = string.Empty;     // cross-context ref
    public string At { get; set; } = string.Empty;          // e.g. "14:05:12"
    public string Source { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;    // "OK" | "WARN" | "BLOCK"
}
