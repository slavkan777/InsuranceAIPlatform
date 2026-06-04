using InsuranceAIPlatform.Api.Contracts.Common;
using InsuranceAIPlatform.Api.Contracts.Rag;
using InsuranceAIPlatform.Api.Middleware;
using InsuranceAIPlatform.Api.Services;
using InsuranceAIPlatform.Services.AiAnalysis.Rag;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceAIPlatform.Api.Controllers;

/// <summary>
/// Local RAG "Claim Evidence Intelligence" endpoints. Advisory only — AI cannot approve, reject,
/// change status, or accuse fraud; the human makes the final decision. All evidence is synthetic
/// and strictly claim-scoped (no cross-claim leakage). No external/cloud call.
/// </summary>
[ApiController]
[Route("api/claims")]
[Tags("RAG Evidence Intelligence")]
public sealed class RagController : ClaimsControllerBase
{
    private readonly IRagService _rag;
    private readonly IClaimReadService _claimRead;

    public RagController(IRagService rag, IClaimReadService claimRead)
    {
        _rag = rag;
        _claimRead = claimRead;
    }

    /// <summary>Retrieve evidence and return a grounded advisory answer; persists an audit trace.</summary>
    [HttpPost("{claimId}/rag/ask")]
    [ProducesResponseType(typeof(RagAnswerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RagAnswerDto>> Ask(string claimId, [FromBody] RagAskRequestDto? body, CancellationToken ct)
    {
        if (ValidateClaimId(claimId) is { } bad) return bad;
        if (NotFoundIfClaimMissing(claimId) is { } missing) return missing;

        var question = body?.Question?.Trim();
        if (string.IsNullOrWhiteSpace(question))
        {
            return BadRequest(new ApiErrorResponse("INVALID_QUESTION", "Question must not be empty.", GetCorrelationId()));
        }

        var answer = await _rag.AskAsync(claimId, question, body?.UseCase, GetCorrelationId(), ct);
        return Ok(MapAnswer(answer, GetCorrelationId()));
    }

    /// <summary>Raw semantic evidence search within a claim (no generated answer).</summary>
    [HttpGet("{claimId}/rag/evidence-search")]
    [ProducesResponseType(typeof(RagEvidenceSearchResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RagEvidenceSearchResponseDto>> EvidenceSearch(
        string claimId, [FromQuery] string? q, [FromQuery] int topK, CancellationToken ct)
    {
        if (ValidateClaimId(claimId) is { } bad) return bad;
        if (NotFoundIfClaimMissing(claimId) is { } missing) return missing;
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest(new ApiErrorResponse("INVALID_QUERY", "Query 'q' must not be empty.", GetCorrelationId()));
        }

        var hits = await _rag.SearchEvidenceAsync(claimId, q.Trim(), topK, ct);
        var dto = new RagEvidenceSearchResponseDto(
            claimId, q.Trim(),
            hits.Select(h => new RagEvidenceHitDto(h.ChunkId, h.DocumentId, h.Kind, h.Snippet, h.Score)).ToList(),
            GetCorrelationId());
        return Ok(dto);
    }

    /// <summary>The gold evaluation questions seeded for a claim.</summary>
    [HttpGet("{claimId}/rag/evaluation-questions")]
    [ProducesResponseType(typeof(IReadOnlyList<RagEvaluationQuestionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<RagEvaluationQuestionDto>>> EvaluationQuestions(string claimId, CancellationToken ct)
    {
        if (ValidateClaimId(claimId) is { } bad) return bad;
        if (NotFoundIfClaimMissing(claimId) is { } missing) return missing;

        var qs = await _rag.GetEvaluationQuestionsAsync(claimId, ct);
        return Ok(qs.Select(q => new RagEvaluationQuestionDto(q.QuestionId, q.ClaimId, q.UseCase, q.Text, q.Language)).ToList());
    }

    /// <summary>Recent persisted RAG audit traces for a claim (newest first).</summary>
    [HttpGet("{claimId}/rag/audit")]
    [ProducesResponseType(typeof(IReadOnlyList<RagAuditTraceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<RagAuditTraceDto>>> Audit(string claimId, [FromQuery] int limit, CancellationToken ct)
    {
        if (ValidateClaimId(claimId) is { } bad) return bad;
        if (NotFoundIfClaimMissing(claimId) is { } missing) return missing;

        var rows = await _rag.GetAuditAsync(claimId, limit, ct);
        return Ok(rows.Select(t => new RagAuditTraceDto(
            t.TraceId, t.ClaimId, t.UseCase, t.QueryText, t.AnswerText, t.Confidence,
            t.RetrievedChunkIds, t.CostMicros, t.CreatedAtUtc)).ToList());
    }

    /// <summary>Cross-claim similar-claims search (claim-level cards only — no other claim's evidence text).</summary>
    [HttpGet("{claimId}/rag/similar-claims")]
    [ProducesResponseType(typeof(SimilarClaimsResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SimilarClaimsResponseDto>> SimilarClaims(string claimId, [FromQuery] int topK, CancellationToken ct)
    {
        if (ValidateClaimId(claimId) is { } bad) return bad;
        if (NotFoundIfClaimMissing(claimId) is { } missing) return missing;

        var similar = await _rag.FindSimilarClaimsAsync(claimId, topK, ct);
        var dto = new SimilarClaimsResponseDto(
            claimId,
            similar.Select(s => new SimilarClaimDto(s.ClaimId, s.Score, s.Reason, s.MatchingCategories)).ToList(),
            GetCorrelationId());
        return Ok(dto);
    }

    /// <summary>RAG infrastructure health snapshot: SQL counts, embedding-cache completeness, LocalLlama runtime.</summary>
    [HttpGet("{claimId}/rag/infrastructure")]
    [ProducesResponseType(typeof(RagInfrastructureStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RagInfrastructureStatusDto>> GetInfrastructureStatus(string claimId, CancellationToken ct)
    {
        if (ValidateClaimId(claimId) is { } bad) return bad;
        if (NotFoundIfClaimMissing(claimId) is { } missing) return missing;

        var status = await _rag.GetInfrastructureStatusAsync(claimId, GetCorrelationId(), ct);
        return Ok(MapInfrastructureStatus(status));
    }

    /// <summary>
    /// Re-embed all EvidenceChunks for the claim (deterministic, idempotent) then return refreshed
    /// infrastructure status. No schema change — updates EmbeddingJson on existing rows only.
    /// </summary>
    [HttpPost("{claimId}/rag/infrastructure/reindex")]
    [ProducesResponseType(typeof(RagInfrastructureStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RagInfrastructureStatusDto>> Reindex(string claimId, CancellationToken ct)
    {
        if (ValidateClaimId(claimId) is { } bad) return bad;
        if (NotFoundIfClaimMissing(claimId) is { } missing) return missing;

        var status = await _rag.ReindexClaimAsync(claimId, GetCorrelationId(), ct);
        return Ok(MapInfrastructureStatus(status));
    }

    // ---- helpers ----

    private ActionResult? NotFoundIfClaimMissing(string claimId) =>
        _claimRead.GetClaim(claimId) is null
            ? NotFound(new ApiErrorResponse("CLAIM_NOT_FOUND", $"Claim '{claimId}' was not found.", GetCorrelationId()))
            : null;

    private string GetCorrelationId() =>
        HttpContext.Items[CorrelationIdMiddleware.CorrelationIdKey]?.ToString() ?? HttpContext.TraceIdentifier;

    private static RagAnswerDto MapAnswer(RagAnswer a, string correlationId) => new(
        a.TraceId, a.ClaimId, a.UseCase, a.Question, a.AnswerText, a.Confidence,
        a.Citations.Select(c => new RagCitationDto(c.ChunkId, c.DocumentId, c.Kind, c.Snippet, c.Score)).ToList(),
        a.RetrievedChunkIds, a.ProviderMode, a.PromptTokens, a.CompletionTokens, a.CostMicros, a.RetrievalMs,
        a.AdvisoryOnly, correlationId, a.CreatedAtUtc);

    private static RagInfrastructureStatusDto MapInfrastructureStatus(RagInfrastructureStatus s) => new(
        s.ClaimId,
        new RagSqlStatusDto(s.SqlSourceOfTruth.Status, s.SqlSourceOfTruth.PolicyClauses, s.SqlSourceOfTruth.EvidenceChunks, s.SqlSourceOfTruth.EvaluationQuestions, s.SqlSourceOfTruth.AuditTraces),
        new RagIndexStatusDto(s.EvidenceMemoryIndex.Status, s.EvidenceMemoryIndex.EmbeddedChunks, s.EvidenceMemoryIndex.TotalChunks, s.EvidenceMemoryIndex.EmbeddingModel, s.EvidenceMemoryIndex.Dimensions),
        new RagVectorRuntimeStatusDto(s.VectorRuntime.Status, s.VectorRuntime.Enabled, s.VectorRuntime.Backend, s.VectorRuntime.EndpointConfigured, s.VectorRuntime.Reachable),
        new RagRuntimeStatusDto(s.LocalReasoningRuntime.Status, s.LocalReasoningRuntime.Enabled, s.LocalReasoningRuntime.Model, s.LocalReasoningRuntime.EndpointConfigured, s.LocalReasoningRuntime.Reachable),
        s.GeneratedAtUtc,
        s.CorrelationId);
}
