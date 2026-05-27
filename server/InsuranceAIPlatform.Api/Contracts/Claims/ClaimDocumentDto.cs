namespace InsuranceAIPlatform.Api.Contracts.Claims;

/// <summary>
/// Single item in the merged document checklist (covers both uploaded documents and damage photos).
/// Consumer screen: /documents.
/// </summary>
public record ClaimDocumentDto(
    string Id,
    string Label,
    string? Detail,
    string Status,
    string Type,
    int? Confidence);
