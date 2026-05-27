using System.Text.Json;
using InsuranceAIPlatform.Api.Contracts.Common;
using InsuranceAIPlatform.Api.Middleware;
using InsuranceAIPlatform.Api.Services;
using InsuranceAIPlatform.BuildingBlocks;
using InsuranceAIPlatform.Services.Approval;
using InsuranceAIPlatform.Services.AuditCost;
using InsuranceAIPlatform.Services.Documents;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceAIPlatform.Api.Controllers;

// -----------------------------------------------------------------------
// Request body records — all fields are human-controlled inputs only.
// No payout, no customer message, no binary upload, no AI call.
// -----------------------------------------------------------------------

/// <summary>Body for POST /api/claims/{claimId}/approval-draft</summary>
public sealed record SaveApprovalDraftRequest(string? CurrentDecision, string? Notes);

/// <summary>Body for POST /api/claims/{claimId}/human-decision</summary>
public sealed record SubmitHumanDecisionRequest(string Decision, string? Notes);

/// <summary>Body for POST /api/claims/{claimId}/missing-document-requests</summary>
public sealed record RequestMissingDocumentRequest(string DocumentTitle, string? Reason);

/// <summary>Body for POST /api/claims/{claimId}/document-metadata</summary>
public sealed record CreateDocumentMetadataRequest(string Kind, string Title, string? DocType);

// -----------------------------------------------------------------------
// Controller
// -----------------------------------------------------------------------

/// <summary>
/// BFF command endpoints — human-controlled write operations only.
/// All commands are advisory/audit-trail only; no payout, no customer messaging,
/// no binary upload, no AI provider call, no Azure, no external HTTP.
/// Each command: validate → call owning service → append audit → write outbox → return CommandResult.
/// Service-write and audit/outbox-write are two separate local transactions (acceptable for demo;
/// true atomic outbox is a future gate).
/// </summary>
[ApiController]
[Route("api/claims")]
[Tags("Claim Commands")]
public sealed class ClaimCommandsController : ClaimsControllerBase
{
    private readonly IApprovalService _approval;
    private readonly IDocumentsService _documents;
    private readonly IAuditCostService _audit;
    private readonly IClaimReadService _claimRead;

    public ClaimCommandsController(
        IApprovalService approval,
        IDocumentsService documents,
        IAuditCostService audit,
        IClaimReadService claimRead)
    {
        _approval  = approval;
        _documents = documents;
        _audit     = audit;
        _claimRead = claimRead;
    }

    // -----------------------------------------------------------------------
    // Shared helpers
    // -----------------------------------------------------------------------

    private string GetCorrelationId() =>
        HttpContext.Items[CorrelationIdMiddleware.CorrelationIdKey]?.ToString()
        ?? HttpContext.TraceIdentifier;

    private static ActorContext BuildActor() => CommandActors.SyntheticAdjuster();

    private static string? IdempotencyKey(HttpContext ctx) =>
        ctx.Request.Headers.TryGetValue("Idempotency-Key", out var v) ? v.ToString() : null;

    private bool ClaimExists(string claimId) =>
        _claimRead.GetClaim(claimId) is not null;

    // -----------------------------------------------------------------------
    // 1. POST /api/claims/{claimId}/approval-draft
    // -----------------------------------------------------------------------

    /// <summary>
    /// SaveApprovalDraft — upserts the human adjuster's draft decision for a claim.
    /// Audit action: ApprovalDraftSaved. Outbox event: ApprovalDraftSaved.
    /// No payout, no customer message, no submit.
    /// </summary>
    [HttpPost("{claimId}/approval-draft")]
    [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommandResult>> SaveApprovalDraft(
        string claimId,
        [FromBody] SaveApprovalDraftRequest body,
        CancellationToken ct)
    {
        var validationError = ValidateClaimId(claimId);
        if (validationError is not null) return validationError;

        if (!ClaimExists(claimId)) return ClaimNotFound(claimId);

        var correlationId = GetCorrelationId();
        var actor         = BuildActor();
        var idempKey      = IdempotencyKey(HttpContext);
        var commandId     = $"cmd-{Guid.NewGuid():N}";
        var warnings      = new List<string>();

        // (a) Service write — Approval owns its DbContext
        await _approval.SaveDraftAsync(claimId, body.CurrentDecision, body.Notes, actor, ct);

        // (b) Audit write
        var meta = JsonSerializer.Serialize(new { CurrentDecision = body.CurrentDecision, HasNotes = body.Notes is not null });
        var auditId = await _audit.AppendAuditAsync(
            claimId, "ApprovalDraftSaved", actor, correlationId,
            "OK", $"Approval draft saved for claim {claimId}.", meta, ct);

        // (c) Outbox write
        var payload = JsonSerializer.Serialize(new { ClaimId = claimId, ActionType = "ApprovalDraftSaved", CommandId = commandId });
        var (outboxId, outboxWarning) = await _audit.WriteOutboxAsync(
            "ApprovalDraftSaved", claimId, correlationId, payload, idempKey, ct);
        if (outboxWarning is not null) warnings.Add(outboxWarning);

        return Ok(new CommandResult(
            Success: true,
            CommandId: commandId,
            ClaimId: claimId,
            Status: "DraftSaved",
            AuditEventId: auditId < 0 ? null : auditId,
            OutboxMessageId: outboxId < 0 ? null : outboxId,
            CorrelationId: correlationId,
            Message: "Approval draft saved.",
            Warnings: warnings));
    }

    // -----------------------------------------------------------------------
    // 2. POST /api/claims/{claimId}/human-decision
    // -----------------------------------------------------------------------

    /// <summary>
    /// SubmitHumanDecision — sets Submitted=true on the approval draft.
    /// ONLY allowed values: ApproveForReview, RejectForReview, NeedsMoreInformation, RequestDocuments.
    /// Any other value → 400. No payout, no customer message.
    /// Audit: HumanDecisionSubmitted. Outbox: HumanDecisionSubmitted + ClaimStatusTransitionRequested.
    /// The Claim row is NOT mutated — outbox-only per design (safer).
    /// </summary>
    [HttpPost("{claimId}/human-decision")]
    [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommandResult>> SubmitHumanDecision(
        string claimId,
        [FromBody] SubmitHumanDecisionRequest body,
        CancellationToken ct)
    {
        var validationError = ValidateClaimId(claimId);
        if (validationError is not null) return validationError;

        if (!ClaimExists(claimId)) return ClaimNotFound(claimId);

        // Validate decision value — human-only set; reject anything else with safe 400
        if (!HumanDecisions.IsAllowed(body.Decision))
        {
            return BadRequest(new ApiErrorResponse(
                Code: "INVALID_DECISION",
                Message: $"Decision '{body.Decision}' is not allowed. " +
                         "Allowed values: ApproveForReview, RejectForReview, NeedsMoreInformation, RequestDocuments.",
                TraceId: HttpContext.TraceIdentifier));
        }

        var correlationId = GetCorrelationId();
        var actor         = BuildActor();
        var idempKey      = IdempotencyKey(HttpContext);
        var commandId     = $"cmd-{Guid.NewGuid():N}";
        var warnings      = new List<string>();

        // (a) Service write — sets Submitted=true, no Claim row mutation
        await _approval.SubmitDecisionAsync(claimId, body.Decision, body.Notes, actor, ct);

        // (b) Audit write
        var meta = JsonSerializer.Serialize(new { Decision = body.Decision, HasNotes = body.Notes is not null });
        var auditId = await _audit.AppendAuditAsync(
            claimId, "HumanDecisionSubmitted", actor, correlationId,
            "OK", $"Human decision '{body.Decision}' submitted for claim {claimId}.", meta, ct);

        // (c) Outbox: HumanDecisionSubmitted
        var payload = JsonSerializer.Serialize(new { ClaimId = claimId, Decision = body.Decision, ActionType = "HumanDecisionSubmitted", CommandId = commandId });
        var (outboxId, outboxWarning) = await _audit.WriteOutboxAsync(
            "HumanDecisionSubmitted", claimId, correlationId, payload, idempKey, ct);
        if (outboxWarning is not null) warnings.Add(outboxWarning);

        // (d) Additional outbox: ClaimStatusTransitionRequested — DO NOT mutate Claim row
        var requestedStatus = HumanDecisions.ToRequestedStatus(body.Decision);
        var transitionPayload = JsonSerializer.Serialize(new { ClaimId = claimId, RequestedStatus = requestedStatus, Decision = body.Decision, CommandId = commandId });
        var (transOutboxId, transWarning) = await _audit.WriteOutboxAsync(
            "ClaimStatusTransitionRequested", claimId, correlationId, transitionPayload, null, ct);
        if (transWarning is not null) warnings.Add(transWarning);

        return Ok(new CommandResult(
            Success: true,
            CommandId: commandId,
            ClaimId: claimId,
            Status: requestedStatus,
            AuditEventId: auditId < 0 ? null : auditId,
            OutboxMessageId: outboxId < 0 ? null : outboxId,
            CorrelationId: correlationId,
            Message: $"Human decision '{body.Decision}' submitted. Status transition to '{requestedStatus}' queued (outbox-only).",
            Warnings: warnings));
    }

    // -----------------------------------------------------------------------
    // 3. POST /api/claims/{claimId}/missing-document-requests
    // -----------------------------------------------------------------------

    /// <summary>
    /// RequestMissingDocument — persists an internal missing-document request.
    /// NO customer message is sent.
    /// Audit: MissingDocumentRequested. Outbox: MissingDocumentRequested.
    /// </summary>
    [HttpPost("{claimId}/missing-document-requests")]
    [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommandResult>> RequestMissingDocument(
        string claimId,
        [FromBody] RequestMissingDocumentRequest body,
        CancellationToken ct)
    {
        var validationError = ValidateClaimId(claimId);
        if (validationError is not null) return validationError;

        if (!ClaimExists(claimId)) return ClaimNotFound(claimId);

        if (string.IsNullOrWhiteSpace(body.DocumentTitle))
        {
            return BadRequest(new ApiErrorResponse(
                Code: "MISSING_DOCUMENT_TITLE",
                Message: "documentTitle is required.",
                TraceId: HttpContext.TraceIdentifier));
        }

        var correlationId = GetCorrelationId();
        var actor         = BuildActor();
        var idempKey      = IdempotencyKey(HttpContext);
        var commandId     = $"cmd-{Guid.NewGuid():N}";
        var warnings      = new List<string>();

        // (a) Service write — Documents owns its DbContext
        var requestId = await _documents.RequestMissingDocumentAsync(
            claimId, body.DocumentTitle, body.Reason, actor, ct);

        // (b) Audit write
        var meta = JsonSerializer.Serialize(new { DocumentTitle = body.DocumentTitle, HasReason = body.Reason is not null });
        var auditId = await _audit.AppendAuditAsync(
            claimId, "MissingDocumentRequested", actor, correlationId,
            "OK", $"Missing document '{body.DocumentTitle}' requested for claim {claimId}.", meta, ct);

        // (c) Outbox write
        var payload = JsonSerializer.Serialize(new { ClaimId = claimId, DocumentTitle = body.DocumentTitle, ActionType = "MissingDocumentRequested", CommandId = commandId, RequestId = requestId });
        var (outboxId, outboxWarning) = await _audit.WriteOutboxAsync(
            "MissingDocumentRequested", claimId, correlationId, payload, idempKey, ct);
        if (outboxWarning is not null) warnings.Add(outboxWarning);

        return Ok(new CommandResult(
            Success: true,
            CommandId: commandId,
            ClaimId: claimId,
            Status: "MissingDocumentRequested",
            AuditEventId: auditId < 0 ? null : auditId,
            OutboxMessageId: outboxId < 0 ? null : outboxId,
            CorrelationId: correlationId,
            Message: $"Missing document request recorded for '{body.DocumentTitle}'. No customer message sent.",
            Warnings: warnings));
    }

    // -----------------------------------------------------------------------
    // 4. POST /api/claims/{claimId}/document-metadata
    // -----------------------------------------------------------------------

    /// <summary>
    /// CreateDocumentMetadataPlaceholder — persists a ClaimDocument metadata row.
    /// NO binary upload, NO blob storage, NO OCR, NO customer messaging.
    /// Audit: DocumentMetadataCreated. Outbox: DocumentMetadataCreated.
    /// </summary>
    [HttpPost("{claimId}/document-metadata")]
    [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommandResult>> CreateDocumentMetadataPlaceholder(
        string claimId,
        [FromBody] CreateDocumentMetadataRequest body,
        CancellationToken ct)
    {
        var validationError = ValidateClaimId(claimId);
        if (validationError is not null) return validationError;

        if (!ClaimExists(claimId)) return ClaimNotFound(claimId);

        if (string.IsNullOrWhiteSpace(body.Kind) || string.IsNullOrWhiteSpace(body.Title))
        {
            return BadRequest(new ApiErrorResponse(
                Code: "MISSING_REQUIRED_FIELDS",
                Message: "kind and title are required.",
                TraceId: HttpContext.TraceIdentifier));
        }

        var correlationId = GetCorrelationId();
        var actor         = BuildActor();
        var idempKey      = IdempotencyKey(HttpContext);
        var commandId     = $"cmd-{Guid.NewGuid():N}";
        var warnings      = new List<string>();

        // (a) Service write — Documents owns its DbContext
        var docId = await _documents.CreateMetadataPlaceholderAsync(
            claimId, body.Kind, body.Title, body.DocType, actor, ct);

        // (b) Audit write
        var meta = JsonSerializer.Serialize(new { Kind = body.Kind, Title = body.Title, DocType = body.DocType, DocId = docId });
        var auditId = await _audit.AppendAuditAsync(
            claimId, "DocumentMetadataCreated", actor, correlationId,
            "OK", $"Document metadata placeholder created: '{body.Title}' (kind={body.Kind}) for claim {claimId}.", meta, ct);

        // (c) Outbox write
        var payload = JsonSerializer.Serialize(new { ClaimId = claimId, DocId = docId, Kind = body.Kind, Title = body.Title, ActionType = "DocumentMetadataCreated", CommandId = commandId });
        var (outboxId, outboxWarning) = await _audit.WriteOutboxAsync(
            "DocumentMetadataCreated", claimId, correlationId, payload, idempKey, ct);
        if (outboxWarning is not null) warnings.Add(outboxWarning);

        return Ok(new CommandResult(
            Success: true,
            CommandId: commandId,
            ClaimId: claimId,
            Status: "MetadataCreated",
            AuditEventId: auditId < 0 ? null : auditId,
            OutboxMessageId: outboxId < 0 ? null : outboxId,
            CorrelationId: correlationId,
            Message: $"Document metadata placeholder created (id={docId}). No binary upload.",
            Warnings: warnings));
    }
}
