using InsuranceAIPlatform.BuildingBlocks;

namespace InsuranceAIPlatform.Services.AuditCost;

/// <summary>
/// Skeleton/health implementation of <see cref="IAuditCostService"/>.
/// Write methods are no-ops here; the DB-backed <see cref="PersistenceAuditCostService"/> handles real writes.
/// This class exists so the BFF health endpoint always resolves a contributor.
/// </summary>
public sealed class AuditCostService : IAuditCostService
{
    public string ServiceName => ServiceNames.AuditCost;

    public ServiceHealthSnapshot GetHealth() => new(
        ServiceNames.AuditCost,
        ServiceReadinessStatus.Stub,
        "skeleton-v0.1",
        new[] { "audit-trace-read", "cost-trace", "governance-correlation" });

    public Task<int> AppendAuditAsync(
        string claimId, string actionType, ActorContext actor,
        string correlationId, string severity, string message,
        string? metadataJson, CancellationToken ct = default)
        => Task.FromResult(-1); // no-op in skeleton

    public Task<(int Id, string? Warning)> WriteOutboxAsync(
        string eventType, string claimId, string correlationId,
        string payloadJson, string? idempotencyKey, CancellationToken ct = default)
        => Task.FromResult((-1, (string?)"skeleton-stub: no persistence")); // no-op
}
