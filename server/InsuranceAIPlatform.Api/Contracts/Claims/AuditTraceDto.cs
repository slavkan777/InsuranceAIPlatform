namespace InsuranceAIPlatform.Api.Contracts.Claims;

/// <summary>Complete audit trace for a claim's AI processing run.</summary>
public record AuditTraceDto(
    string RunId,
    string TraceId,
    string Model,
    int Tokens,
    decimal Cost,
    double DurationSec,
    AuditEventDto[] Events,
    CostDistributionItemDto[] CostDistribution);

/// <summary>One event in the audit trail.</summary>
public record AuditEventDto(
    string Time,
    string Actor,
    string Action,
    string Result);

/// <summary>Per-stage AI cost allocation.</summary>
public record CostDistributionItemDto(
    string Stage,
    decimal Cost);
