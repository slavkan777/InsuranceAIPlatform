namespace InsuranceAIPlatform.Services.AiAnalysis.Contracts;

/// <summary>
/// Input to the AI analysis pipeline. ClaimId identifies the claim; Actor is the synthetic adjuster.
/// </summary>
public sealed record AiAnalysisRequest(
    string ClaimId,
    string CorrelationId,
    string Actor);
