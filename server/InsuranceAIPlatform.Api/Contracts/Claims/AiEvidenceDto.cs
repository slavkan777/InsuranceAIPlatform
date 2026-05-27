namespace InsuranceAIPlatform.Api.Contracts.Claims;

/// <summary>Full AI advisory analysis result for the claim. Advisory only — not a binding decision.</summary>
public record AiEvidenceDto(
    string RunId,
    int ModelConfidence,
    AiFindingDto[] Findings,
    EvidenceSourceDto[] Evidence,
    ExtractedEntityDto[] ExtractedEntities,
    ConfidenceBreakdownItemDto[] ModelConfidenceBreakdown);

/// <summary>One AI finding entry — category + text + severity.</summary>
public record AiFindingDto(
    string Id,
    string Category,
    string Text,
    string Severity);

/// <summary>One supporting evidence source referenced by AI.</summary>
public record EvidenceSourceDto(
    string Id,
    string Source,
    string Text,
    int Confidence);

/// <summary>One structured field extracted by the AI field extractor.</summary>
public record ExtractedEntityDto(
    string Field,
    string Value,
    string Source,
    int Confidence);

/// <summary>Per-stage AI model confidence score.</summary>
public record ConfidenceBreakdownItemDto(
    string Stage,
    int Confidence);
