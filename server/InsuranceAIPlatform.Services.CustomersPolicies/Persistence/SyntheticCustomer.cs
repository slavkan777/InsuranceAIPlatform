namespace InsuranceAIPlatform.Services.CustomersPolicies.Persistence;

/// <summary>
/// Synthetic customer — portfolio test data. All rows have IsSynthetic=true.
/// CustomerId format: CUST-T0001 .. CUST-T0200 (synthetic), CUST-4421 (golden claim customer).
/// </summary>
public sealed class SyntheticCustomer
{
    public string Id { get; set; } = string.Empty;          // e.g. "CUST-T0001"
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string AddressLine { get; set; } = string.Empty;
    public DateOnly CustomerSince { get; set; }
    public int PreviousClaimsCount { get; set; }
    public bool IsSynthetic { get; set; } = true;

    // Navigation
    public ICollection<Policy> Policies { get; set; } = new List<Policy>();
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
