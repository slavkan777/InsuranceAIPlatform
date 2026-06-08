using InsuranceAIPlatform.Api.Contracts.Claims;
using InsuranceAIPlatform.Api.Controllers;
using InsuranceAIPlatform.Api.Rag;
using InsuranceAIPlatform.Api.Services;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Persistence;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Retrieval;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace InsuranceAIPlatform.Tests;

/// <summary>
/// Unit tests for the optional Advanced AI Review endpoint. Verifies: (1) flag OFF returns a safe
/// fallback and never calls the sidecar; (2) flag ON returns the sidecar review BUT re-scopes
/// citations to the claim's own evidence (drops any foreign chunkId — leakage guard); (3) sidecar
/// failure (null) degrades to a safe fallback.
/// </summary>
public class AdvancedAiReviewControllerTests
{
    private static ClaimDetailsDto Claim(string id) => new(
        id, "E2E Customer", "CUST-T1", "Toyota Corolla", "VIN-1", "Auto Comprehensive", "POL-1",
        "ДТП", new DateOnly(2026, 6, 1), "Київ", "Зіткнення", "Open", "low", 10, 50,
        DateTimeOffset.UnixEpoch, 1, 3, null, 100m, 90m, 5m, 80m, "tr", "run", 10, 0.01m, 1.0);

    private sealed class FakeClaimRead : IClaimReadService
    {
        public ClaimDetailsDto? GetClaim(string claimId) => Claim(claimId);
        public ClaimSummaryDto GetSummary() => throw new NotImplementedException();
        public IReadOnlyList<ClaimListItemDto> GetClaims() => throw new NotImplementedException();
        public IReadOnlyList<ClaimDocumentDto>? GetDocuments(string claimId) => throw new NotImplementedException();
        public AiEvidenceDto? GetAiEvidence(string claimId) => throw new NotImplementedException();
        public RiskAssessmentDto? GetRisks(string claimId) => throw new NotImplementedException();
        public PolicyDto? GetPolicy(string claimId) => throw new NotImplementedException();
        public CustomerVehicleContextDto? GetCustomerVehicle(string claimId) => throw new NotImplementedException();
        public ApprovalDraftDto? GetApproval(string claimId) => throw new NotImplementedException();
        public AuditTraceDto? GetAudit(string claimId) => throw new NotImplementedException();
        public DemoScenarioDto GetDemoScenario() => throw new NotImplementedException();
    }

    private sealed class FakeChunks : IRagChunkSource
    {
        public Task<IReadOnlyList<EvidenceChunk>> GetClaimChunksAsync(string claimId, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<EvidenceChunk>>(new List<EvidenceChunk>
            {
                new() { ChunkId = $"{claimId}-uploaded-doc-0", ClaimId = claimId, Kind = "statement", Text = "докази" },
            });
        public Task<IReadOnlyList<EvidenceChunk>> GetAllChunksAsync(CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<EvidenceChunk>>(new List<EvidenceChunk>());
    }

    private sealed class FakeClient : IAdvancedClaimAnalyticsClient
    {
        public int Calls;
        public AdvancedReviewResult? Result;
        public Task<AdvancedReviewResult?> ReviewAsync(AdvancedReviewRequest request, CancellationToken ct = default)
        { Calls++; return Task.FromResult(Result); }
    }

    private static AdvancedAiReviewController Build(bool enabled, FakeClient client) =>
        new(new FakeClaimRead(), new FakeChunks(), client, new AdvancedAiReviewOptions { Enabled = enabled });

    [Fact]
    public async Task Disabled_returns_fallback_and_does_not_call_sidecar()
    {
        var client = new FakeClient();
        var ctrl = Build(enabled: false, client);
        var res = (await ctrl.Review("CLM-1006", null, default)).Result as OkObjectResult;
        var review = Assert.IsType<AdvancedReviewResult>(res!.Value);
        Assert.Equal("Disabled", review.ProviderMode);
        Assert.True(review.AdvisoryOnly);
        Assert.Empty(review.Citations);
        Assert.Equal(0, client.Calls); // sidecar never contacted when off
    }

    [Fact]
    public async Task Enabled_returns_review_but_rescopes_citations_to_claim()
    {
        var client = new FakeClient
        {
            Result = new AdvancedReviewResult(
                "CLM-1006", "summary", "coverage", "moderate",
                new[] { "anomaly" }, new[] { "missing" }, "human review",
                new[]
                {
                    new AdvancedReviewCitation("CLM-1006-uploaded-doc-0", "statement"), // belongs to claim
                    new AdvancedReviewCitation("CLM-1007-police#0", "police"),           // FOREIGN — must be dropped
                },
                42, true, "Deterministic", "langchain"),
        };
        var ctrl = Build(enabled: true, client);
        var res = (await ctrl.Review("CLM-1006", new AdvancedAiReviewRequestDto("coverage?"), default)).Result as OkObjectResult;
        var review = Assert.IsType<AdvancedReviewResult>(res!.Value);

        Assert.Equal(1, client.Calls);
        Assert.True(review.AdvisoryOnly);
        Assert.Single(review.Citations);
        Assert.Equal("CLM-1006-uploaded-doc-0", review.Citations[0].ChunkId);
        Assert.DoesNotContain(review.Citations, c => c.ChunkId.Contains("CLM-1007"));
    }

    [Fact]
    public async Task Enabled_but_sidecar_unavailable_returns_fallback()
    {
        var client = new FakeClient { Result = null }; // sidecar unreachable
        var ctrl = Build(enabled: true, client);
        var res = (await ctrl.Review("CLM-1006", null, default)).Result as OkObjectResult;
        var review = Assert.IsType<AdvancedReviewResult>(res!.Value);
        Assert.Equal("Unavailable", review.ProviderMode);
        Assert.True(review.AdvisoryOnly);
        Assert.Empty(review.Citations);
    }
}
