using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using InsuranceAIPlatform.Api.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;

namespace InsuranceAIPlatform.Tests;

/// <summary>
/// Integration tests for the Stage-1 BFF / API Gateway skeleton.
/// Verifies: BFF identity health endpoint, correlation-id middleware behaviour,
/// and that all pre-existing routes remain intact.
/// Uses WebApplicationFactory for in-process integration testing — no DB, no network.
/// </summary>
public class BffSkeletonTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    // -----------------------------------------------------------------------
    // (a) GET /api/bff/health → 200 + identity fields
    // -----------------------------------------------------------------------

    [Fact]
    public async Task BffHealth_returns_200_with_identity_fields()
    {
        var response = await _client.GetAsync("/api/bff/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<BffHealthResponse>(JsonOpts);
        Assert.NotNull(body);
        Assert.Equal("bff-api-gateway", body!.Service);
        Assert.Equal("healthy", body.Status);
        Assert.Equal("skeleton-v0.1", body.Stage);
        Assert.Equal("in-memory-read-service", body.Upstream);
        Assert.NotNull(body.CorrelationId);
        Assert.NotEmpty(body.CorrelationId);
    }

    // -----------------------------------------------------------------------
    // (b) Any response carries X-Correlation-Id header
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Response_carries_XCorrelationId_header()
    {
        var response = await _client.GetAsync("/api/bff/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(
            response.Headers.Contains("X-Correlation-Id"),
            "Response must include X-Correlation-Id header from correlation middleware.");
        var correlationId = response.Headers.GetValues("X-Correlation-Id").FirstOrDefault();
        Assert.NotNull(correlationId);
        Assert.NotEmpty(correlationId!);
    }

    // -----------------------------------------------------------------------
    // (c) Incoming X-Correlation-Id is echoed back unchanged
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Incoming_XCorrelationId_is_echoed_back()
    {
        const string incomingId = "test-abc-123456";

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/bff/health");
        request.Headers.Add("X-Correlation-Id", incomingId);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(
            response.Headers.Contains("X-Correlation-Id"),
            "Response must echo X-Correlation-Id header.");
        var echoed = response.Headers.GetValues("X-Correlation-Id").FirstOrDefault();
        Assert.Equal(incomingId, echoed);
    }

    // -----------------------------------------------------------------------
    // (d) Preserved routes still return 200
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Health_route_still_returns_200()
    {
        var response = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Claims_list_route_still_returns_200()
    {
        var response = await _client.GetAsync("/api/claims");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Claims_golden_claim_route_still_returns_200()
    {
        var response = await _client.GetAsync("/api/claims/CLM-1006");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Demo_scenario_route_still_returns_200()
    {
        var response = await _client.GetAsync("/api/demo/scenario");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // -----------------------------------------------------------------------
    // Additional: X-Bff identity header is present
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Response_carries_XBff_identity_header()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(
            response.Headers.Contains("X-Bff"),
            "Response must include X-Bff identity header from correlation middleware.");
        var bffValue = response.Headers.GetValues("X-Bff").FirstOrDefault();
        Assert.Equal("api-gateway", bffValue);
    }
}
