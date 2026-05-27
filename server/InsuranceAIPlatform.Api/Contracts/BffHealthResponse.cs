namespace InsuranceAIPlatform.Api.Contracts;

/// <summary>
/// Identity response returned by GET /api/bff/health.
/// Identifies this service as the BFF / API Gateway layer (Stage-1 skeleton).
/// Synthetic only — no operational or PII data.
/// </summary>
public record BffHealthResponse(
    string Service,
    string Status,
    string Stage,
    string Upstream,
    string Environment,
    string CorrelationId,
    DateTimeOffset TimestampUtc);
