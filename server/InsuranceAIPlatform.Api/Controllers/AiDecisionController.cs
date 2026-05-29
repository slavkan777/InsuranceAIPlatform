using System.Text.Json;
using InsuranceAIPlatform.Api.Contracts.AiAnalysis;
using InsuranceAIPlatform.Api.Contracts.Common;
using InsuranceAIPlatform.Api.Middleware;
using InsuranceAIPlatform.Api.Services;
using InsuranceAIPlatform.BuildingBlocks;
using InsuranceAIPlatform.Services.AiAnalysis.Orchestration;
using InsuranceAIPlatform.Services.AuditCost;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceAIPlatform.Api.Controllers;

/// <summary>Body for POST /api/claims/{claimId}/ai-decision</summary>
public sealed record RecordAiDecisionRequest(string? Notes);

/// <summary>
/// Local/demo AI Decision endpoint. Records an auditable AI decision derived from
/// the latest AI analysis run for a claim.
///
/// SAFETY MODEL (sacred):
///   - AI is advisory only. This endpoint NEVER authorises payout, claim rejection,
///     fraud accusations, customer messages, or claim status mutation.
///   - Side effects: append-only audit row + outbox message (DB-only). No external
///     send, no payout transfer, no PII, no binary upload.
///   - Source attribution: every audit row written here carries an AI actor identity
///     (<c>ActorType = "ai-system"</c>) so the audit history makes the AI origin
///     unambiguous to a reviewer.
///   - Requires an AI analysis run to already exist; returns 400 if none.
/// </summary>
[ApiController]
[Route("api/claims")]
[Tags("AI Decision")]
public sealed class AiDecisionController : ClaimsControllerBase
{
    private readonly IAiAnalysisOrchestrator _orchestrator;
    private readonly IAuditCostService _audit;
    private readonly IClaimReadService _claimRead;

    /// <summary>Synthetic AI actor identity — never carries real PII.</summary>
    public const string AiActorId   = "ai-system@insuranceai.local";
    public const string AiActorName = "AI System (advisory)";
    public const string AiActorType = "ai-system";

    public AiDecisionController(
        IAiAnalysisOrchestrator orchestrator,
        IAuditCostService audit,
        IClaimReadService claimRead)
    {
        _orchestrator = orchestrator;
        _audit        = audit;
        _claimRead    = claimRead;
    }

    private string GetCorrelationId() =>
        HttpContext.Items[CorrelationIdMiddleware.CorrelationIdKey]?.ToString()
        ?? HttpContext.TraceIdentifier;

    private static string? IdempotencyKey(HttpContext ctx) =>
        ctx.Request.Headers.TryGetValue("Idempotency-Key", out var v) ? v.ToString() : null;

    private bool ClaimExists(string claimId) => _claimRead.GetClaim(claimId) is not null;

    // -----------------------------------------------------------------------
    // POST /api/claims/{claimId}/ai-decision
    // -----------------------------------------------------------------------

    /// <summary>
    /// Records an AI decision based on the latest AI analysis run for the claim.
    /// Audit event: <c>AiDecisionRecorded</c> (severity=OK, actor=ai-system).
    /// Outbox event: <c>AiDecisionRecorded</c>.
    /// No payout, no customer message, no status mutation.
    /// </summary>
    [HttpPost("{claimId}/ai-decision")]
    [ProducesResponseType(typeof(AiDecisionRecordedResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AiDecisionRecordedResult>> RecordAiDecision(
        string claimId,
        [FromBody] RecordAiDecisionRequest? body,
        CancellationToken ct)
    {
        if (ValidateClaimId(claimId) is { } bad) return bad;

        if (!ClaimExists(claimId)) return ClaimNotFound(claimId);

        var correlationId = GetCorrelationId();

        // Require an existing AI run; never invent one here.
        var run = await _orchestrator.GetLatestAsync(claimId, ct);
        if (run is null)
        {
            return BadRequest(new ApiErrorResponse(
                Code: "no_ai_analysis_run",
                Message: $"No AI analysis run found for claim '{claimId}'. " +
                         "Run AI analysis before recording an AI decision.",
                TraceId: correlationId));
        }

        var idempKey  = IdempotencyKey(HttpContext);
        var commandId = $"cmd-{Guid.NewGuid():N}";
        var warnings  = new List<string>();
        var notes     = body?.Notes;

        var aiActor = new ActorContext(AiActorId, AiActorName, AiActorType);

        // Audit metadata — explicit Source attribution so the audit history
        // shows unambiguously that this came from the AI advisor and not a human.
        var auditMeta = JsonSerializer.Serialize(new
        {
            Source            = "AI",
            AiRunId           = run.RunId,
            ProviderMode      = run.ProviderMode,
            ModelName         = run.ModelName,
            RecommendedAction = run.RecommendedAction.Action,
            Rationale         = run.RecommendedAction.Rationale,
            ConfidenceScore   = run.RecommendedAction.ConfidenceScore,
            RiskLevel         = run.RiskLevel,
            Advisory          = true,
            IsAdvisoryOnly    = true,
            HasOperatorNotes  = notes is not null,
            Notes             = notes,
        });

        var auditId = await _audit.AppendAuditAsync(
            claimId, "AiDecisionRecorded", aiActor, correlationId,
            "OK",
            $"AI decision recorded for claim {claimId} (advisory; no payout, no customer message). " +
            $"Based on run {run.RunId} via {run.ProviderMode}/{run.ModelName}.",
            auditMeta, ct);

        var outboxPayload = JsonSerializer.Serialize(new
        {
            ClaimId           = claimId,
            AiRunId           = run.RunId,
            ProviderMode      = run.ProviderMode,
            ModelName         = run.ModelName,
            RecommendedAction = run.RecommendedAction.Action,
            ConfidenceScore   = run.RecommendedAction.ConfidenceScore,
            RiskLevel         = run.RiskLevel,
            Source            = "AI",
            IsAdvisoryOnly    = true,
            ActionType        = "AiDecisionRecorded",
            CommandId         = commandId,
        });

        var (outboxId, outboxWarning) = await _audit.WriteOutboxAsync(
            "AiDecisionRecorded", claimId, correlationId, outboxPayload, idempKey, ct);
        if (outboxWarning is not null) warnings.Add(outboxWarning);

        return Ok(new AiDecisionRecordedResult(
            Success:          true,
            CommandId:        commandId,
            ClaimId:          claimId,
            Status:           "AiDecisionRecorded",
            AuditEventId:     auditId < 0 ? null : auditId,
            OutboxMessageId:  outboxId < 0 ? null : outboxId,
            CorrelationId:    correlationId,
            Message:
                $"AI decision recorded (advisory only; no payout, no customer message). " +
                $"Based on run {run.RunId} via {run.ProviderMode}/{run.ModelName}.",
            Warnings:         warnings,
            AiRunId:          run.RunId,
            ProviderMode:     run.ProviderMode,
            ModelName:        run.ModelName,
            RecommendedAction:run.RecommendedAction.Action,
            RiskLevel:        run.RiskLevel,
            ConfidenceScore:  run.RecommendedAction.ConfidenceScore,
            IsAdvisoryOnly:   true,
            Source:           "AI"));
    }
}
