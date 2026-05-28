using InsuranceAIPlatform.Api.Contracts.AiAnalysis;
using InsuranceAIPlatform.Api.Contracts.Common;
using InsuranceAIPlatform.Api.Middleware;
using InsuranceAIPlatform.Api.Services;
using InsuranceAIPlatform.BuildingBlocks;
using InsuranceAIPlatform.Services.AiAnalysis.Contracts;
using InsuranceAIPlatform.Services.AiAnalysis.Orchestration;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceAIPlatform.Api.Controllers;

/// <summary>
/// BFF AI Analysis endpoints. Advisory only — AI cannot approve payout, reject claim,
/// change status, accuse fraud, or send customer messages. No DbContext injected here.
/// Correlation ID is echoed from middleware on every response.
/// </summary>
[ApiController]
[Route("api/claims")]
[Tags("AI Analysis")]
public sealed class AiAnalysisController : ClaimsControllerBase
{
    private readonly IAiAnalysisOrchestrator _orchestrator;
    private readonly IClaimReadService _claimRead;

    public AiAnalysisController(
        IAiAnalysisOrchestrator orchestrator,
        IClaimReadService claimRead)
    {
        _orchestrator = orchestrator;
        _claimRead    = claimRead;
    }

    // -----------------------------------------------------------------------
    // GET /api/claims/{claimId}/ai-analysis
    // -----------------------------------------------------------------------

    /// <summary>
    /// Returns the latest AI analysis run for a claim, or 404 if none exists.
    /// AI output is advisory only — human decision is always final.
    /// </summary>
    [HttpGet("{claimId}/ai-analysis")]
    [ProducesResponseType(typeof(AiAnalysisDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AiAnalysisDto>> GetLatestAiAnalysis(string claimId, CancellationToken ct)
    {
        if (ValidateClaimId(claimId) is { } bad) return bad;

        var result = await _orchestrator.GetLatestAsync(claimId, ct);
        if (result is null)
        {
            return NotFound(new ApiErrorResponse(
                Code: "no_ai_analysis_run",
                Message: $"No AI analysis run found for claim '{claimId}'.",
                TraceId: GetCorrelationId()));
        }

        return Ok(MapToDto(result, GetCorrelationId()));
    }

    // -----------------------------------------------------------------------
    // POST /api/claims/{claimId}/ai-analysis/run
    // -----------------------------------------------------------------------

    /// <summary>
    /// Triggers a new AI analysis run for a claim. Returns 200 with the result DTO.
    /// If claim is not found, returns 404. AI output is advisory only.
    /// Synthetic actor: demo.adjuster@insuranceai.local.
    /// </summary>
    [HttpPost("{claimId}/ai-analysis/run")]
    [ProducesResponseType(typeof(AiAnalysisDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AiAnalysisDto>> RunAiAnalysis(
        string claimId,
        [FromBody] AiAnalysisRunRequestDto? _body,
        CancellationToken ct)
    {
        if (ValidateClaimId(claimId) is { } bad) return bad;

        // Claim existence check before triggering run
        if (_claimRead.GetClaim(claimId) is null)
        {
            return NotFound(new ApiErrorResponse(
                Code: "CLAIM_NOT_FOUND",
                Message: $"Claim '{claimId}' was not found.",
                TraceId: GetCorrelationId()));
        }

        var correlationId = GetCorrelationId();
        var actor = CommandActors.SyntheticAdjusterId;

        var result = await _orchestrator.RunAsync(claimId, correlationId, actor, ct);

        // claim_not_found status means claim was concurrently removed — treat as 404
        if (result.Status == "claim_not_found")
        {
            return NotFound(new ApiErrorResponse(
                Code: "CLAIM_NOT_FOUND",
                Message: $"Claim '{claimId}' was not found during analysis.",
                TraceId: correlationId));
        }

        return Ok(MapToDto(result, correlationId));
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private string GetCorrelationId() =>
        HttpContext.Items[CorrelationIdMiddleware.CorrelationIdKey]?.ToString()
        ?? HttpContext.TraceIdentifier;

    private static AiAnalysisDto MapToDto(AiAnalysisResult r, string correlationId) =>
        new(
            RunId: r.RunId,
            ClaimId: r.ClaimId,
            ProviderMode: r.ProviderMode,
            ModelName: r.ModelName,
            Status: r.Status,
            SummaryText: r.SummaryText,
            RecommendedAction: new AiAnalysisRecommendedActionDto(
                r.RecommendedAction.Action,
                r.RecommendedAction.Rationale,
                r.RecommendedAction.ConfidenceScore),
            PolicyCoverageExplanation: r.PolicyCoverageExplanation,
            RiskLevel: r.RiskLevel,
            ConfidenceScore: r.ConfidenceScore,
            Findings: r.Findings.Select(f => new AiAnalysisFindingDto(f.Id, f.Category, f.Text, f.Severity)).ToList(),
            Evidence: r.Evidence.Select(e => new AiAnalysisEvidenceDto(e.Id, e.Source, e.Note, e.Confidence)).ToList(),
            Risks: r.Risks.Select(risk => new AiAnalysisRiskDto(risk.Id, risk.Label, risk.Weight)).ToList(),
            Guardrails: new AiAnalysisGuardrailsDto(
                AdvisoryOnly:          r.Guardrails.AdvisoryOnly,
                RequiresHumanReview:   r.Guardrails.RequiresHumanReview,
                CanApprovePayout:      r.Guardrails.CanApprovePayout,
                CanRejectClaim:        r.Guardrails.CanRejectClaim,
                CanAccuseFraudFinal:   r.Guardrails.CanAccuseFraudFinal,
                CanSendCustomerMessage: r.Guardrails.CanSendCustomerMessage,
                CanChangeClaimStatus:  r.Guardrails.CanChangeClaimStatus),
            CostTrace: new AiAnalysisCostTraceDto(r.CostTrace.Tokens, r.CostTrace.EstimatedCost, r.CostTrace.CurrencyCode),
            CorrelationId: correlationId,
            CreatedAtUtc: r.CreatedAtUtc);
}
