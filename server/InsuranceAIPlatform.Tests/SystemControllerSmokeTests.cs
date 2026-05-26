using System.Net;
using System.Net.Http.Json;
using InsuranceAIPlatform.Api.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;

namespace InsuranceAIPlatform.Tests;

public class SystemControllerSmokeTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory = factory;

    [Fact]
    public async Task Health_returns_200_and_healthy()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<HealthResponse>();
        Assert.NotNull(body);
        Assert.Equal("Healthy", body!.Status);
        Assert.Equal("InsuranceAIPlatform.Api", body.Service);
    }

    [Fact]
    public async Task DemoStatus_returns_200_and_synthetic_skeleton_payload()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/system/demo-status");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<DemoStatusResponse>();
        Assert.NotNull(body);
        Assert.Equal("InsuranceAIPlatform", body!.Project);
        Assert.Equal("Synthetic", body.Data);
        Assert.Equal("Skeleton", body.Backend);
        Assert.Equal("NotConnected", body.Database);
        Assert.Equal("NotConnected", body.AiProvider);
        Assert.True(body.HumanApprovalRequired);
    }
}
