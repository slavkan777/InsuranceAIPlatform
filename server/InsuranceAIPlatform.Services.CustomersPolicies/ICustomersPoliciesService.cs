using InsuranceAIPlatform.BuildingBlocks;

namespace InsuranceAIPlatform.Services.CustomersPolicies;

/// <summary>
/// Customers &amp; Policies service boundary (skeleton). Future owner of customers, vehicles,
/// policies, coverage validation, and the 200 synthetic test users. No data, no writes, no AI
/// in the skeleton; does not own claims.
/// </summary>
public interface ICustomersPoliciesService : IServiceHealthContributor
{
    /// <summary>Canonical service name (see <see cref="ServiceNames.CustomersPolicies"/>).</summary>
    string ServiceName { get; }
}
