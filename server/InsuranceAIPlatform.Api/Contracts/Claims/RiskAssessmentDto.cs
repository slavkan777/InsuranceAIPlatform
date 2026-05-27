namespace InsuranceAIPlatform.Api.Contracts.Claims;

/// <summary>Full risk assessment including score, factors, and pipeline status.</summary>
public record RiskAssessmentDto(
    int Score,
    int Threshold,
    string Level,
    RiskFactorDto[] Factors,
    PipelineStageDto[] Pipeline);

/// <summary>One contributing factor to the risk score.</summary>
public record RiskFactorDto(
    string Id,
    string Label,
    int Contribution);

/// <summary>One stage in the AI processing pipeline with its run status.</summary>
public record PipelineStageDto(
    string Stage,
    string Status);
