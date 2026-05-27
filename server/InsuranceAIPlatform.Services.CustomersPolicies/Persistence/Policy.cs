namespace InsuranceAIPlatform.Services.CustomersPolicies.Persistence;

/// <summary>
/// Insurance policy owned by a synthetic customer.
/// PolicyId format: POL-YYYY-AC-NNNN (e.g. "POL-2025-AC-4421").
/// CustomerId is an id-only cross-service reference — no EF navigation to other contexts.
/// </summary>
public sealed class Policy
{
    public string PolicyId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;  // FK by id — same context
    public string ProductName { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal Premium { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation within same context
    public SyntheticCustomer? Customer { get; set; }
}
