using InsuranceAIPlatform.BuildingBlocks;

namespace InsuranceAIPlatform.Services.AuditCost;

/// <summary>
/// Audit &amp; Cost service boundary (skeleton). Future owner of the append-only audit trail,
/// token/cost traces, and cross-service governance correlation. No persistence, no outbox in the skeleton.
/// </summary>
public interface IAuditCostService : IServiceHealthContributor
{
    /// <summary>Canonical service name (see <see cref="ServiceNames.AuditCost"/>).</summary>
    string ServiceName { get; }
}
