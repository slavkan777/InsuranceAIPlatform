using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using InsuranceAIPlatform.Api.Contracts.AiAnalysis;
using InsuranceAIPlatform.Api.Contracts.Common;
using InsuranceAIPlatform.Services.AiAnalysis.Orchestration;
using InsuranceAIPlatform.Services.AiAnalysis.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InsuranceAIPlatform.Tests;

/// <summary>
/// BFF AI Analysis endpoint tests. All use InMemory DB — no SQL Server required.
/// Verifies: POST run → 200; GET latest → 200; GET unknown → 404; POST unknown → 404;
/// correlation header echoed; advisory-only guardrails.
/// </summary>
public class BffAiAnalysisEndpointTests : IClassFixture<AiAnalysisTestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public BffAiAnalysisEndpointTests(AiAnalysisTestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // -----------------------------------------------------------------------
    // (B1) POST /api/claims/CLM-1006/ai-analysis/run → 200 with advisory-only guardrails
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Post_run_CLM1006_returns_200_with_advisory_only_guardrails()
    {
        var response = await _client.PostAsJsonAsync("/api/claims/CLM-1006/ai-analysis/run", new { });
        var responseBody = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, $"Response {(int)response.StatusCode}: {responseBody}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dto = await response.Content.ReadFromJsonAsync<AiAnalysisDto>(JsonOpts);
        Assert.NotNull(dto);
        Assert.Equal("CLM-1006", dto!.ClaimId);
        Assert.Equal("succeeded", dto.Status);
        Assert.True(dto.IsAdvisoryOnly);
        Assert.Equal("AI output is advisory only — human decision is final.", dto.Notice);
        Assert.True(dto.Guardrails.AdvisoryOnly);
        Assert.True(dto.Guardrails.RequiresHumanReview);
        Assert.False(dto.Guardrails.CanApprovePayout);
        Assert.False(dto.Guardrails.CanRejectClaim);
        Assert.False(dto.Guardrails.CanAccuseFraudFinal);
        Assert.False(dto.Guardrails.CanSendCustomerMessage);
        Assert.False(dto.Guardrails.CanChangeClaimStatus);
        Assert.Equal("Mock", dto.ProviderMode);
        Assert.NotEmpty(dto.RunId);
        Assert.Equal(3, dto.Findings.Count);
    }

    // -----------------------------------------------------------------------
    // (B2) GET /api/claims/CLM-1006/ai-analysis → 200 (after run)
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Get_ai_analysis_CLM1006_after_run_returns_200()
    {
        // First run to ensure a result exists
        await _client.PostAsJsonAsync("/api/claims/CLM-1006/ai-analysis/run", new { });

        var response = await _client.GetAsync("/api/claims/CLM-1006/ai-analysis");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<AiAnalysisDto>(JsonOpts);
        Assert.NotNull(dto);
        Assert.Equal("CLM-1006", dto!.ClaimId);
        Assert.True(dto.IsAdvisoryOnly);
    }

    // -----------------------------------------------------------------------
    // (B3) GET /api/claims/UNKNOWN/ai-analysis → 404 (no run + unknown)
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Get_ai_analysis_unknown_claim_returns_404()
    {
        // CLM-9999 is a valid pattern but doesn't exist in InMemory claim service
        var response = await _client.GetAsync("/api/claims/CLM-9999/ai-analysis");

        // 404 — no run exists for this claim
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // -----------------------------------------------------------------------
    // (B4) POST /api/claims/CLM-9999/ai-analysis/run → 404 (claim not found)
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Post_run_unknown_claim_returns_404()
    {
        var response = await _client.PostAsJsonAsync("/api/claims/CLM-9999/ai-analysis/run", new { });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOpts);
        Assert.NotNull(error);
        Assert.Equal("CLAIM_NOT_FOUND", error!.Code);
    }

    // -----------------------------------------------------------------------
    // (B5) Correlation header is echoed
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Correlation_header_is_echoed_on_run_response()
    {
        var req = new HttpRequestMessage(HttpMethod.Post, "/api/claims/CLM-1006/ai-analysis/run")
        {
            Content = JsonContent.Create(new { }),
        };
        req.Headers.Add("X-Correlation-Id", "ai-test-corr-99887766");

        var response = await _client.SendAsync(req);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("X-Correlation-Id"));
        var echoed = response.Headers.GetValues("X-Correlation-Id").FirstOrDefault();
        Assert.Equal("ai-test-corr-99887766", echoed);

        var dto = await response.Content.ReadFromJsonAsync<AiAnalysisDto>(JsonOpts);
        Assert.NotNull(dto);
        Assert.Equal("ai-test-corr-99887766", dto!.CorrelationId);
    }
}

/// <summary>
/// WebApplicationFactory that replaces all command DbContexts + AiAnalysisDbContext with InMemory.
/// Extends CommandTestWebApplicationFactory pattern.
/// </summary>
public sealed class AiAnalysisTestWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string SharedDbName = "AiAnalysisTest_InMemory_Shared";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            ReplaceWithInMemory<InsuranceAIPlatform.Services.Approval.Persistence.ApprovalDbContext>(services);
            ReplaceWithInMemory<InsuranceAIPlatform.Services.AuditCost.Persistence.AuditCostDbContext>(services);
            ReplaceWithInMemory<InsuranceAIPlatform.Services.Documents.Persistence.DocumentsDbContext>(services);
            ReplaceWithInMemory<AiAnalysisDbContext>(services);

            // Replace audit/outbox delegates with no-op test doubles to avoid DB dependency
            // on the AuditCost context during AI Analysis endpoint tests.
            // The audit/outbox behavior is covered by AiOrchestratorPersistenceTests.
            ReplaceDescriptor<AppendAuditDelegate>(services,
                _ => (_, _, _, _, _, _, _, _) => Task.FromResult(1));
            ReplaceDescriptor<WriteOutboxDelegate>(services,
                _ => (_, _, _, _, _, _) => Task.CompletedTask);
        });
    }

    private static void ReplaceDescriptor<TService>(IServiceCollection services, Func<IServiceProvider, TService> factory)
        where TService : class
    {
        var toRemove = services.Where(d => d.ServiceType == typeof(TService)).ToList();
        foreach (var d in toRemove) services.Remove(d);
        services.AddSingleton(factory);
    }

    private static void ReplaceWithInMemory<TContext>(IServiceCollection services)
        where TContext : DbContext
    {
        var toRemove = services
            .Where(d =>
                (d.ServiceType.IsGenericType &&
                 d.ServiceType.GetGenericArguments().Any(a => a == typeof(TContext))) ||
                (d.ImplementationType is not null && d.ImplementationType == typeof(TContext)) ||
                (d.ServiceType == typeof(DbContextOptions<TContext>)) ||
                (d.ServiceType == typeof(DbContextOptions) && d.ImplementationType == typeof(DbContextOptions<TContext>)))
            .ToList();

        foreach (var d in toRemove)
            services.Remove(d);

        services.AddDbContextFactory<TContext>(options =>
            options.UseInMemoryDatabase(SharedDbName), ServiceLifetime.Singleton);
    }
}
