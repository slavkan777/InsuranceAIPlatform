using InsuranceAIPlatform.BuildingBlocks;

namespace InsuranceAIPlatform.Services.CustomersPolicies;

/// <summary>
/// Skeleton implementation. The DB-backed
/// <see cref="Persistence.PersistenceCustomersPoliciesService"/> handles real reads.
/// </summary>
public sealed class CustomersPoliciesService : ICustomersPoliciesService
{
    public string ServiceName => ServiceNames.CustomersPolicies;

    public ServiceHealthSnapshot GetHealth() => new(
        ServiceNames.CustomersPolicies,
        ServiceReadinessStatus.Stub,
        "skeleton-v0.1",
        new[] { "customers", "vehicles", "policies", "coverage-validation" });

    public Task<int> CountSyntheticCustomersAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(0);

    public Task<CustomerListResult> ListCustomersAsync(
        string? search, int page, int pageSize, CancellationToken ct = default)
        => Task.FromResult(new CustomerListResult(0, page, pageSize, Array.Empty<CustomerSummary>()));

    public Task<CustomerSummary?> GetCustomerByIdAsync(string customerId, CancellationToken ct = default)
        => Task.FromResult<CustomerSummary?>(null);

    public Task<CustomerSummary> CreateSyntheticCustomerAsync(
        NewSyntheticCustomer input, CancellationToken ct = default)
        => throw new NotSupportedException(
            "Skeleton CustomersPoliciesService cannot create rows; use the persistence-backed registration.");
}
