namespace InsuranceAIPlatform.Api.Contracts.Claims;

/// <summary>One row in the claims queue table. Returned as array by GET /api/claims.</summary>
public record ClaimListItemDto(
    string Id,
    string Customer,
    string Vehicle,
    string EventType,
    string Status,
    string DocumentsCount,
    string AiStatus,
    string Risk,
    string Sla,
    string NextAction,
    DateTimeOffset Updated);
