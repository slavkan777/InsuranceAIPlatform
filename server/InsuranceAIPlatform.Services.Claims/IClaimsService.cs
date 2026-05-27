using InsuranceAIPlatform.BuildingBlocks;

namespace InsuranceAIPlatform.Services.Claims;

/// <summary>
/// Claims service boundary (skeleton). Future owner of the claim queue, claim detail,
/// claim lifecycle, and deterministic status rules. No data, no writes, no AI in the skeleton.
/// </summary>
public interface IClaimsService : IServiceHealthContributor
{
    /// <summary>Canonical service name (see <see cref="ServiceNames.Claims"/>).</summary>
    string ServiceName { get; }
}
