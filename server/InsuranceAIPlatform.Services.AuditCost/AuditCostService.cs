using InsuranceAIPlatform.BuildingBlocks;

namespace InsuranceAIPlatform.Services.AuditCost;

/// <summary>
/// Skeleton implementation of <see cref="IAuditCostService"/>. Reports readiness only; persists
/// nothing yet. Append-only audit and cost/token traces are added in a later gate.
/// </summary>
public sealed class AuditCostService : IAuditCostService
{
    public string ServiceName => ServiceNames.AuditCost;

    public ServiceHealthSnapshot GetHealth() => new(
        ServiceNames.AuditCost,
        ServiceReadinessStatus.Stub,
        "skeleton-v0.1",
        new[] { "audit-trace-read", "cost-trace", "governance-correlation" });
}
