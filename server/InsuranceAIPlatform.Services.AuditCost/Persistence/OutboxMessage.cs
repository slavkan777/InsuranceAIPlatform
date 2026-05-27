namespace InsuranceAIPlatform.Services.AuditCost.Persistence;

/// <summary>
/// Transactional outbox record. Written at command time; a future relay/processor marks Processed=true.
/// PayloadJson is sanitized — no secrets, no real PII.
/// </summary>
public sealed class OutboxMessage
{
    public int Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string ClaimId { get; set; } = string.Empty;     // cross-context ref (AggregateId)
    public DateTimeOffset OccurredAtUtc { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    /// <summary>Sanitized payload JSON. No secrets, no real PII.</summary>
    public string PayloadJson { get; set; } = string.Empty;
    public bool Processed { get; set; } = false;
    public DateTimeOffset? ProcessedAtUtc { get; set; }
    public string? Error { get; set; }
    /// <summary>Optional idempotency key. Prevents duplicate outbox rows for the same command.</summary>
    public string? IdempotencyKey { get; set; }
}
