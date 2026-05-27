using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using InsuranceAIPlatform.Api.Contracts.Claims;
using InsuranceAIPlatform.Api.Contracts.Common;
using Microsoft.AspNetCore.Mvc.Testing;

namespace InsuranceAIPlatform.Tests;

/// <summary>
/// Smoke + contract tests for the P0 claims API slice.
/// Uses WebApplicationFactory for in-process integration testing — no DB, no network.
/// </summary>
public class ClaimsApiTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    // -----------------------------------------------------------------------
    // /api/claims/summary
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Summary_returns_200_with_expected_shape()
    {
        var response = await _client.GetAsync("/api/claims/summary");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ClaimSummaryDto>(JsonOpts);
        Assert.NotNull(body);
        Assert.True(body!.TotalActive > 0, "totalActive must be positive");
        Assert.True(body.HighRisk > 0, "highRisk must be positive");
    }

    // -----------------------------------------------------------------------
    // /api/claims
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetClaims_returns_200_containing_CLM1006()
    {
        var response = await _client.GetAsync("/api/claims");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ClaimListItemDto[]>(JsonOpts);
        Assert.NotNull(body);
        Assert.Contains(body!, c => c.Id == "CLM-1006");
    }

    // -----------------------------------------------------------------------
    // /api/claims/CLM-1006
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetClaim_CLM1006_returns_200_with_correct_values()
    {
        var response = await _client.GetAsync("/api/claims/CLM-1006");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ClaimDetailsDto>(JsonOpts);
        Assert.NotNull(body);
        Assert.Equal("CLM-1006", body!.Id);
        Assert.Equal(82, body.RiskScore);
        Assert.Equal(78, body.Confidence);
        Assert.Equal(4261, body.Tokens);
        Assert.Equal(0.0187m, body.Cost);
        Assert.Equal(18.9, body.DurationSec);
        Assert.Equal("trc_8f3d2a7e", body.TraceId);
        Assert.Equal("run_8f3d2a7e", body.RunId);
        Assert.Equal("POL-2025-AC-4421", body.PolicyId);
        Assert.Contains("Джонсон", body.Customer);
    }

    // -----------------------------------------------------------------------
    // /api/claims/CLM-1006/ai-evidence
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetAiEvidence_CLM1006_returns_200_with_advisory_data()
    {
        var response = await _client.GetAsync("/api/claims/CLM-1006/ai-evidence");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AiEvidenceDto>(JsonOpts);
        Assert.NotNull(body);
        Assert.Equal("run_8f3d2a7e", body!.RunId);
        Assert.Equal(78, body.ModelConfidence);
        Assert.NotEmpty(body.Findings);
        Assert.NotEmpty(body.ExtractedEntities);
    }

    // -----------------------------------------------------------------------
    // /api/claims/CLM-1006/risks
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetRisks_CLM1006_returns_200_with_score_82()
    {
        var response = await _client.GetAsync("/api/claims/CLM-1006/risks");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<RiskAssessmentDto>(JsonOpts);
        Assert.NotNull(body);
        Assert.Equal(82, body!.Score);
        Assert.Equal(60, body.Threshold);
        Assert.Equal("Високий", body.Level);
        Assert.NotEmpty(body.Factors);
    }

    // -----------------------------------------------------------------------
    // /api/claims/CLM-1006/audit
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetAudit_CLM1006_returns_200_with_trace_and_governance_block()
    {
        var response = await _client.GetAsync("/api/claims/CLM-1006/audit");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AuditTraceDto>(JsonOpts);
        Assert.NotNull(body);
        Assert.Equal("trc_8f3d2a7e", body!.TraceId);
        Assert.Equal("run_8f3d2a7e", body.RunId);
        Assert.Equal(4261, body.Tokens);
        Assert.Equal(0.0187m, body.Cost);
        Assert.Contains(body.Events, e => e.Result == "BLOCK");
    }

    // -----------------------------------------------------------------------
    // Validation — invalid claimId format → 400
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("BADID")]
    [InlineData("CLM-")]
    [InlineData("clm-1006")]
    [InlineData("CLM-10060")]
    public async Task GetClaim_invalid_format_returns_400(string badId)
    {
        var response = await _client.GetAsync($"/api/claims/{badId}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOpts);
        Assert.NotNull(body);
        Assert.Equal("INVALID_CLAIM_ID", body!.Code);
    }

    // -----------------------------------------------------------------------
    // Validation — well-formed but unknown claimId → 404
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetClaim_unknown_id_returns_404()
    {
        var response = await _client.GetAsync("/api/claims/CLM-9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOpts);
        Assert.NotNull(body);
        Assert.Equal("CLAIM_NOT_FOUND", body!.Code);
    }

    // -----------------------------------------------------------------------
    // /api/demo/scenario
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetDemoScenario_returns_200_with_steps()
    {
        var response = await _client.GetAsync("/api/demo/scenario");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<DemoScenarioDto>(JsonOpts);
        Assert.NotNull(body);
        Assert.Equal("CLM-1006", body!.GoldenClaimId);
        Assert.NotEmpty(body.Steps);
    }
}
