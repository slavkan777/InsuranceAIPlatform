using InsuranceAIPlatform.BuildingBlocks;

namespace InsuranceAIPlatform.Services.CustomersPolicies;

/// <summary>
/// Skeleton implementation of <see cref="ICustomersPoliciesService"/>. Reports readiness only;
/// owns no data and seeds no users yet. Synthetic users / coverage rules arrive in a later gate.
/// </summary>
public sealed class CustomersPoliciesService : ICustomersPoliciesService
{
    public string ServiceName => ServiceNames.CustomersPolicies;

    public ServiceHealthSnapshot GetHealth() => new(
        ServiceNames.CustomersPolicies,
        ServiceReadinessStatus.Stub,
        "skeleton-v0.1",
        new[] { "customers", "vehicles", "policies", "coverage-validation" });

    /// <summary>Skeleton returns 0 — DB not wired in-process. Use the seeder + migrator for real counts.</summary>
    public Task<int> CountSyntheticCustomersAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(0);
}
