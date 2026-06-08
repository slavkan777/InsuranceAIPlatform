using InsuranceAIPlatform.Api.Contracts.Common;
using InsuranceAIPlatform.Api.Rag;
using InsuranceAIPlatform.Api.Services;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Retrieval;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceAIPlatform.Api.Controllers;

/// <summary>Body for POST /api/claims/{claimId}/advanced-ai-review.</summary>
public sealed record AdvancedAiReviewRequestDto(string? Question);

/// <summary>
/// Optional "Advanced AI Review" — calls the LangChain analytics sidecar over the SAME claim-scoped
/// EvidenceChunks the core RAG uses, and returns a structured advisory manager review. Feature-flagged
/// (AdvancedAiReview:Enabled, default false). When the flag is off OR the sidecar is unreachable, the
/// endpoint returns a SAFE fallback (advisory-only, no fabricated citations) — the core RAG flow is
/// never replaced or broken. Advisory only: never a final payout/fraud/legal decision. Claim-scoped:
/// only the claim's own evidence is sent, and citations are re-scoped to that evidence on return.
/// </summary>
[ApiController]
[Route("api/claims")]
[Tags("Advanced AI Review (LangChain sidecar)")]
public sealed class AdvancedAiReviewController : ClaimsControllerBase
{
    private readonly IClaimReadService _claimRead;
    private readonly IRagChunkSource _chunks;
    private readonly IAdvancedClaimAnalyticsClient _client;
    private readonly AdvancedAiReviewOptions _options;

    public AdvancedAiReviewController(
        IClaimReadService claimRead,
        IRagChunkSource chunks,
        IAdvancedClaimAnalyticsClient client,
        AdvancedAiReviewOptions options)
    {
        _claimRead = claimRead;
        _chunks = chunks;
        _client = client;
        _options = options;
    }

    [HttpPost("{claimId}/advanced-ai-review")]
    [ProducesResponseType(typeof(AdvancedReviewResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdvancedReviewResult>> Review(
        string claimId, [FromBody] AdvancedAiReviewRequestDto? body, CancellationToken ct)
    {
        if (ValidateClaimId(claimId) is { } bad) return bad;
        if (_claimRead.GetClaim(claimId) is null) return ClaimNotFound(claimId);

        // Feature off → safe fallback, no sidecar call.
        if (!_options.Enabled)
            return Ok(Fallback(claimId, "Disabled",
                "Розширений AI-огляд вимкнено. Доступний базовий RAG-аналіз з цитатами."));

        var claim = _claimRead.GetClaim(claimId);
        var chunks = await _chunks.GetClaimChunksAsync(claimId, ct);

        var evidence = chunks
            .Take(_options.MaxEvidenceChunks)
            .Select(c => new AdvancedEvidenceItem(c.ChunkId, c.Kind, c.Text))
            .ToList();

        var request = new AdvancedReviewRequest(
            ClaimId: claimId,
            CustomerName: claim?.Customer,
            Vehicle: claim?.Vehicle,
            EventType: claim?.EventType,
            Description: claim?.Description,
            Question: body?.Question?.Trim(),
            Evidence: evidence);

        var result = await _client.ReviewAsync(request, ct);

        // Sidecar unreachable/error → safe fallback (no fabricated content).
        if (result is null)
            return Ok(Fallback(claimId, "Unavailable",
                "Сервіс розширеного аналізу недоступний. Скористайтеся базовим RAG-аналізом з цитатами."));

        // Defense-in-depth: re-scope citations to THIS claim's own evidence ids only (no cross-claim leakage).
        var allowed = evidence.Select(e => e.ChunkId).ToHashSet();
        var safeCitations = result.Citations.Where(c => allowed.Contains(c.ChunkId)).ToList();

        return Ok(result with { ClaimId = claimId, AdvisoryOnly = true, Citations = safeCitations });
    }

    private static AdvancedReviewResult Fallback(string claimId, string providerMode, string summary) =>
        new(ClaimId: claimId, Summary: summary,
            CoverageAssessment: "—", EvidenceStrength: "none",
            Anomalies: Array.Empty<string>(), MissingItems: Array.Empty<string>(),
            RecommendedNextAction: "Скористайтеся базовим RAG-аналізом; фінальне рішення приймає людина-адʼюстер.",
            Citations: Array.Empty<AdvancedReviewCitation>(), Confidence: 0, AdvisoryOnly: true,
            ProviderMode: providerMode, Framework: "langchain");
}
