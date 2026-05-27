namespace InsuranceAIPlatform.Api.Contracts.Claims;

/// <summary>Combined customer + vehicle context for the customer-vehicle screen.</summary>
public record CustomerVehicleContextDto(
    CustomerDto Customer,
    VehicleDto Vehicle);

/// <summary>Customer identity and history summary.</summary>
public record CustomerDto(
    string CustomerId,
    string FullName,
    int PreviousClaimsCount,
    DateOnly CustomerSince,
    CommunicationEntryDto[] CommunicationHistory);

/// <summary>One entry in customer communication history.</summary>
public record CommunicationEntryDto(
    DateOnly Date,
    string Channel,
    string Summary);

/// <summary>Vehicle details for the insured asset.</summary>
public record VehicleDto(
    string Make,
    string Model,
    int Year,
    string Vin,
    string? Color,
    int? Mileage);
