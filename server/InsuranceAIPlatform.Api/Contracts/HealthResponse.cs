namespace InsuranceAIPlatform.Api.Contracts;

/// <summary>Liveness response for the backend skeleton.</summary>
public record HealthResponse(
    string Status,
    string Service,
    string Environment,
    DateTimeOffset TimestampUtc);
