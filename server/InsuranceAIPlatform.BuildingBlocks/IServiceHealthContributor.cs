namespace InsuranceAIPlatform.BuildingBlocks;

/// <summary>
/// Implemented by every internal service skeleton so the BFF can aggregate readiness
/// without depending on any service's concrete type. Health-contributor abstraction of
/// the thin shared kernel — no domain, no DTOs, no persistence.
/// </summary>
public interface IServiceHealthContributor
{
    /// <summary>Returns the current synthetic readiness snapshot for this service.</summary>
    ServiceHealthSnapshot GetHealth();
}
