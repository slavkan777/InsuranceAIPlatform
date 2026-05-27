using InsuranceAIPlatform.BuildingBlocks;

namespace InsuranceAIPlatform.Services.AuditCost;

/// <summary>
/// Audit &amp; Cost service boundary. Owner of the append-only audit trail,
/// token/cost traces, cross-service governance correlation, and the transactional outbox.
/// </summary>
public interface IAuditCostService : IServiceHealthContributor
{
    /// <summary>Canonical service name (see <see cref="ServiceNames.AuditCost"/>).</summary>
    string ServiceName { get; }

    /// <summary>
    /// Appends an audit event to the append-only audit trail.
    /// Returns the new AuditEvent.Id.
    /// </summary>
    Task<int> AppendAuditAsync(
        string claimId,
        string actionType,
        ActorContext actor,
        string correlationId,
        string severity,
        string message,
        string? metadataJson,
        CancellationToken ct = default);

    /// <summary>
    /// Writes an outbox message.
    /// If <paramref name="idempotencyKey"/> is non-null and already present, returns the existing id + warning (no duplicate).
    /// Returns (id, warning?) where warning is non-null on duplicate.
    /// </summary>
    Task<(int Id, string? Warning)> WriteOutboxAsync(
        string eventType,
        string claimId,
        string correlationId,
        string payloadJson,
        string? idempotencyKey,
        CancellationToken ct = default);
}
