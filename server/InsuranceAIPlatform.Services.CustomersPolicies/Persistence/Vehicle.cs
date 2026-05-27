namespace InsuranceAIPlatform.Services.CustomersPolicies.Persistence;

/// <summary>
/// Vehicle associated with a synthetic customer.
/// VIN uses synthetic format: SYNVIN0000000001 .. SYNVIN0000000200.
/// CustomerId is an id-only reference within the same bounded context.
/// </summary>
public sealed class Vehicle
{
    public string Id { get; set; } = string.Empty;          // e.g. "VEH-T0001"
    public string CustomerId { get; set; } = string.Empty;  // FK by id — same context
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Vin { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int Mileage { get; set; }

    // Navigation within same context
    public SyntheticCustomer? Customer { get; set; }
}
