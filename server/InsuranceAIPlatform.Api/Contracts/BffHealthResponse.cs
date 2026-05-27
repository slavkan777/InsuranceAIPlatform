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
    DateTimeOffset TimestampUtc,
    IReadOnlyList<ServiceReadinessInfo> Services);

/// <summary>
/// Synthetic readiness line for one internal service skeleton, surfaced (additively) by
/// GET /api/bff/health. BFF-owned DTO mapped from each service's health snapshot —
/// internal service types never leak to the frontend.
/// </summary>
public record ServiceReadinessInfo(
    string Service,
    string Status,
    string Stage,
    IReadOnlyList<string> Capabilities);
