namespace InsuranceAIPlatform.Api.Contracts.Claims;

/// <summary>Full policy coverage detail with all coverage blocks and validation results.</summary>
public record PolicyDto(
    string PolicyId,
    string ProductName,
    PolicyCoverageDto[] CoverageBlocks,
    PolicyCheckResultDto Validation);

/// <summary>One coverage line: limit, deductible, applicability to current event.</summary>
public record PolicyCoverageDto(
    string Id,
    string Label,
    string Limit,
    string Deductible,
    bool Applicable,
    string? Note);

/// <summary>Aggregate policy validation result for the event.</summary>
public record PolicyCheckResultDto(
    bool Covered,
    string CoverageType,
    string[] ValidationNotes,
    bool ExclusionTriggered);
