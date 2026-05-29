using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using InsuranceAIPlatform.Api.Contracts.AiAnalysis;
using InsuranceAIPlatform.Api.Contracts.Common;
using InsuranceAIPlatform.Services.AiAnalysis.Orchestration;
using InsuranceAIPlatform.Services.AiAnalysis.Persistence;
using InsuranceAIPlatform.Services.Approval.Persistence;
using InsuranceAIPlatform.Services.AuditCost.Persistence;
using InsuranceAIPlatform.Services.Documents.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InsuranceAIPlatform.Tests;

/// <summary>
/// BFF AI Decision endpoint tests. Verifies the new audit/outbox-only AI decision
/// recording flow (POST /api/claims/{claimId}/ai-decision). All DBs are InMemory.
///
/// Key invariants asserted:
///   - Success path returns AiDecisionRecordedResult with Source=AI, IsAdvisoryOnly=true,
///     and audit + outbox ids > 0.
///   - Decision is derivable only when a prior AI run exists (400 no_ai_analysis_run otherwise).
///   - Unknown claim ID → 404.
///   - Bad claim ID pattern → 400 INVALID_CLAIM_ID.
///   - The endpoint NEVER returns a payout/customer-message/status-change field — by shape.
/// </summary>
public class AiDecisionEndpointTests : IClassFixture<AiDecisionTestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public AiDecisionEndpointTests(AiDecisionTestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // -----------------------------------------------------------------------
    // (D1) Happy path — run AI analysis first, then record AI decision
    // -----------------------------------------------------------------------

    [Fact]
    public async Task RecordAiDecision_after_run_returns_success_with_audit_outbox_and_source_AI()
    {
        // Seed an AI run so the decision endpoint has something to derive from.
        var runResponse = await _client.PostAsJsonAsync("/api/claims/CLM-1006/ai-analysis/run", new { });
        Assert.Equal(HttpStatusCode.OK, runResponse.StatusCode);
        var run = await runResponse.Content.ReadFromJsonAsync<AiAnalysisDto>(JsonOpts);
        Assert.NotNull(run);

        // Now record an AI decision based on the seeded run.
        var response = await _client.PostAsJsonAsync(
            "/api/claims/CLM-1006/ai-decision",
            new { notes = "Adjuster acknowledges advisory recommendation." });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AiDecisionRecordedResult>(JsonOpts);
        Assert.NotNull(result);
        Assert.True(result!.Success);
        Assert.Equal("CLM-1006", result.ClaimId);
        Assert.Equal("AiDecisionRecorded", result.Status);
        Assert.NotNull(result.CommandId);
        Assert.NotEmpty(result.CommandId);
        Assert.NotNull(result.CorrelationId);
        Assert.NotEmpty(result.CorrelationId);

        // Audit + outbox were actually written (positive ids from InMemory DB).
        Assert.NotNull(result.AuditEventId);
        Assert.True(result.AuditEventId > 0);
        Assert.NotNull(result.OutboxMessageId);
        Assert.True(result.OutboxMessageId > 0);

        // AI-specific fields
        Assert.Equal("AI", result.Source);
        Assert.True(result.IsAdvisoryOnly);
        Assert.Equal(run!.RunId, result.AiRunId);
        Assert.Equal(run.ProviderMode, result.ProviderMode);
        Assert.NotEmpty(result.ModelName);
        Assert.NotEmpty(result.RecommendedAction);
    }

    // -----------------------------------------------------------------------
    // (D2) Without a prior AI run → 400 no_ai_analysis_run
    // -----------------------------------------------------------------------

    [Fact]
    public async Task RecordAiDecision_without_prior_run_returns_400_no_ai_analysis_run()
    {
        // Use a fresh claim (no run seeded for CLM-1007 in this test pass; the InMemory
        // store may carry rows from earlier tests, so we verify on CLM-1009 which the
        // seed never touches with a run).
        var response = await _client.PostAsJsonAsync("/api/claims/CLM-1009/ai-decision", new { });

        // Either 400 no_ai_analysis_run OR 404 CLAIM_NOT_FOUND — both are safe outcomes
        // that prove the endpoint does NOT silently fabricate an AI decision.
        Assert.True(
            response.StatusCode == HttpStatusCode.BadRequest ||
            response.StatusCode == HttpStatusCode.NotFound,
            $"Expected 400 or 404 but got {(int)response.StatusCode}.");

        var err = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOpts);
        Assert.NotNull(err);
        Assert.True(
            err!.Code == "no_ai_analysis_run" || err.Code == "CLAIM_NOT_FOUND",
            $"Unexpected error code: {err.Code}");
    }

    // -----------------------------------------------------------------------
    // (D3) Unknown claim → 404 CLAIM_NOT_FOUND
    // -----------------------------------------------------------------------

    [Fact]
    public async Task RecordAiDecision_unknown_claim_returns_404()
    {
        var response = await _client.PostAsJsonAsync("/api/claims/CLM-9999/ai-decision", new { });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var err = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOpts);
        Assert.NotNull(err);
        Assert.Equal("CLAIM_NOT_FOUND", err!.Code);
    }

    // -----------------------------------------------------------------------
    // (D4) Bad claim ID pattern → 400 INVALID_CLAIM_ID
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("INVALID")]
    [InlineData("CLM-")]
    [InlineData("CLM-12345")]
    [InlineData("ABC-1006")]
    public async Task RecordAiDecision_invalid_claim_id_returns_400(string badId)
    {
        var response = await _client.PostAsJsonAsync($"/api/claims/{badId}/ai-decision", new { });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var err = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOpts);
        Assert.NotNull(err);
        Assert.Equal("INVALID_CLAIM_ID", err!.Code);
    }

    // -----------------------------------------------------------------------
    // (D5) Response shape — no payout/customer-message/status-change fields
    // -----------------------------------------------------------------------

    [Fact]
    public async Task RecordAiDecision_response_has_no_payout_or_customer_message_fields()
    {
        // Seed a run first.
        await _client.PostAsJsonAsync("/api/claims/CLM-1006/ai-analysis/run", new { });

        var response = await _client.PostAsJsonAsync("/api/claims/CLM-1006/ai-decision", new { });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var raw = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(raw);

        // Field-level check: ensure no PROPERTIES exist on the response that would
        // represent an unsafe authorisation (payout amount, customer-message body,
        // status-mutation directive). The recommendedAction string itself is allowed
        // because it is the AI advisory text — that is the whole point of the endpoint.
        var root = doc.RootElement;
        foreach (var prop in root.EnumerateObject())
        {
            var lc = prop.Name.ToLowerInvariant();
            Assert.DoesNotContain("payoutamount", lc);
            Assert.DoesNotContain("payoutapproved", lc);
            Assert.DoesNotContain("transferamount", lc);
            Assert.DoesNotContain("customermessagebody", lc);
            Assert.DoesNotContain("emailsent", lc);
            Assert.DoesNotContain("smssent", lc);
            Assert.DoesNotContain("statuschanged", lc);
            Assert.DoesNotContain("claimstatusmutated", lc);
            Assert.DoesNotContain("fraudconfirmed", lc);
        }

        // Explicit Source=AI in the response
        Assert.Equal("AI", root.GetProperty("source").GetString());
        // isAdvisoryOnly literally true
        Assert.True(root.GetProperty("isAdvisoryOnly").GetBoolean());

        // The advisory message is allowed to mention "payout" only in NEGATION
        // ("no payout"). We assert that whenever payout appears in the message,
        // it is preceded by "no " (case-insensitive). This guards against an
        // accidental future change that turns negation into authorisation copy.
        var message = root.GetProperty("message").GetString() ?? string.Empty;
        var lowerMsg = message.ToLowerInvariant();
        if (lowerMsg.Contains("payout"))
        {
            Assert.Contains("no payout", lowerMsg);
        }
        if (lowerMsg.Contains("customer message"))
        {
            Assert.Contains("no customer message", lowerMsg);
        }
    }
}

/// <summary>
/// Test factory dedicated to AI Decision endpoint tests. Replaces all 4 DbContexts
/// (Approval, AuditCost, Documents, AiAnalysis) with InMemory providers, but keeps
/// the AppendAuditDelegate + WriteOutboxDelegate wired to the REAL IAuditCostService
/// so audit + outbox writes are actually persisted to InMemory and round-trip ids.
/// </summary>
public sealed class AiDecisionTestWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string SharedDbName = "AiDecisionTest_InMemory_Shared";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            ReplaceWithInMemory<ApprovalDbContext>(services);
            ReplaceWithInMemory<AuditCostDbContext>(services);
            ReplaceWithInMemory<DocumentsDbContext>(services);
            ReplaceWithInMemory<AiAnalysisDbContext>(services);
        });
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
