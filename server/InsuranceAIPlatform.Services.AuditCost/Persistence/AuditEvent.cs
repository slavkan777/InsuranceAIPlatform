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

    // --- Command audit extensions (nullable/additive; existing seed rows unaffected) ---
    public string? CorrelationId { get; set; }
    public string? Actor { get; set; }
    public string? ActionType { get; set; }
    public DateTimeOffset? OccurredAtUtc { get; set; }
    /// <summary>Sanitized metadata JSON. No secrets, no PII.</summary>
    public string? MetadataJson { get; set; }
}
