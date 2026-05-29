using InsuranceAIPlatform.BuildingBlocks;

namespace InsuranceAIPlatform.Services.CustomersPolicies;

/// <summary>
/// Customers &amp; Policies service boundary. Owner of customers, vehicles, policies,
/// coverage validation, and the 200 synthetic test users. Read-only in this gate;
/// no real PII.
/// </summary>
public interface ICustomersPoliciesService : IServiceHealthContributor
{
    /// <summary>Canonical service name (see <see cref="ServiceNames.CustomersPolicies"/>).</summary>
    string ServiceName { get; }

    /// <summary>Returns the count of seeded synthetic customers (IsSynthetic=true rows). DB-optional: returns 0 if DB not wired.</summary>
    Task<int> CountSyntheticCustomersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists synthetic customers with optional substring search across FullName / Email / Id.
    /// Result is paginated. Search is case-insensitive. Empty/null search returns all rows.
    /// </summary>
    Task<CustomerListResult> ListCustomersAsync(
        string? search,
        int page,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>
    /// Looks up a single synthetic customer by id. Returns null when not found.
    /// </summary>
    Task<CustomerSummary?> GetCustomerByIdAsync(string customerId, CancellationToken ct = default);

    /// <summary>
    /// Creates a new synthetic customer in the local sandbox. ID is allocated as the
    /// next free <c>CUST-T0XXX</c> after the current maximum (so it sorts naturally with
    /// the seed). Row is always <c>IsSynthetic=true</c> — production / real-PII rows
    /// cannot be created through this method. Returns the created summary.
    /// </summary>
    Task<CustomerSummary> CreateSyntheticCustomerAsync(
        NewSyntheticCustomer input,
        CancellationToken ct = default);
}

/// <summary>Paginated customer-list result.</summary>
public sealed record CustomerListResult(
    int Total,
    int Page,
    int PageSize,
    IReadOnlyList<CustomerSummary> Items);

/// <summary>Lightweight customer projection for the UI/directory.</summary>
public sealed record CustomerSummary(
    string Id,
    string FullName,
    string Email,
    string Phone,
    string AddressLine,
    DateOnly CustomerSince,
    int PreviousClaimsCount,
    bool IsSynthetic);

/// <summary>
/// Input for <see cref="ICustomersPoliciesService.CreateSyntheticCustomerAsync"/>.
/// All fields are synthetic / sandbox — no real PII expected. Email/phone are
/// generated from a synthetic domain when missing.
/// </summary>
public sealed record NewSyntheticCustomer(
    string FullName,
    string? Email,
    string? Phone,
    string? AddressLine,
    DateOnly? CustomerSince);
