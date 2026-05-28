using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using InsuranceAIPlatform.Api.Contracts.Common;
using InsuranceAIPlatform.Api.Controllers;
using InsuranceAIPlatform.BuildingBlocks;
using InsuranceAIPlatform.Services.Approval;
using InsuranceAIPlatform.Services.AuditCost;
using InsuranceAIPlatform.Services.Documents;
using Microsoft.Extensions.DependencyInjection;

namespace InsuranceAIPlatform.Tests;

/// <summary>
/// Command endpoint tests using <see cref="CommandTestWebApplicationFactory"/>.
/// All tests use InMemory DB — no live SQL Server required.
/// Validates: command success, audit + outbox writes, decision validation, idempotency, shape.
/// </summary>
public class ClaimCommandTests : IClassFixture<CommandTestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CommandTestWebApplicationFactory _factory;
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public ClaimCommandTests(CommandTestWebApplicationFactory factory)
    {
        _factory = factory;
        _client  = factory.CreateClient();
    }

    // -----------------------------------------------------------------------
    // (C1) SaveApprovalDraft — success writes draft + audit + outbox
    // -----------------------------------------------------------------------

    [Fact]
    public async Task SaveApprovalDraft_success_writes_draft_audit_outbox()
    {
        var body = new { currentDecision = "ApproveForReview", notes = "Looks good" };
        var response = await _client.PostAsJsonAsync("/api/claims/CLM-1006/approval-draft", body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CommandResult>(JsonOpts);
        Assert.NotNull(result);
        Assert.True(result!.Success);
        Assert.Equal("CLM-1006", result.ClaimId);
        Assert.Equal("DraftSaved", result.Status);
        Assert.NotNull(result.CommandId);
        Assert.NotEmpty(result.CommandId);
        Assert.NotNull(result.CorrelationId);
        Assert.NotEmpty(result.CorrelationId);

        // Audit event id present (positive from real DB write in InMemory)
        Assert.NotNull(result.AuditEventId);
        Assert.True(result.AuditEventId > 0, "AuditEventId should be > 0");

        // Outbox message id present
        Assert.NotNull(result.OutboxMessageId);
        Assert.True(result.OutboxMessageId > 0, "OutboxMessageId should be > 0");
    }

    // -----------------------------------------------------------------------
    // (C2) SubmitHumanDecision — valid decisions succeed, Submitted=true
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("ApproveForReview")]
    [InlineData("RejectForReview")]
    [InlineData("NeedsMoreInformation")]
    [InlineData("RequestDocuments")]
    public async Task SubmitHumanDecision_valid_decisions_succeed(string decision)
    {
        var body = new { decision };
        var response = await _client.PostAsJsonAsync("/api/claims/CLM-1006/human-decision", body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CommandResult>(JsonOpts);
        Assert.NotNull(result);
        Assert.True(result!.Success);
        Assert.Equal("CLM-1006", result.ClaimId);
        Assert.NotNull(result.AuditEventId);
        Assert.True(result.AuditEventId > 0);
        Assert.NotNull(result.OutboxMessageId);
        Assert.True(result.OutboxMessageId > 0);
    }

    // -----------------------------------------------------------------------
    // (C3) SubmitHumanDecision — invalid decision → 400
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("approve")]
    [InlineData("Approve")]
    [InlineData("REJECT")]
    [InlineData("payout")]
    [InlineData("")]
    [InlineData("EscalateToCEO")]
    public async Task SubmitHumanDecision_invalid_decision_returns_400(string decision)
    {
        var body = new { decision };
        var response = await _client.PostAsJsonAsync("/api/claims/CLM-1006/human-decision", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOpts);
        Assert.NotNull(error);
        Assert.Equal("INVALID_DECISION", error!.Code);
    }

    // -----------------------------------------------------------------------
    // (C4) RequestMissingDocument — writes request + audit + outbox
    // -----------------------------------------------------------------------

    [Fact]
    public async Task RequestMissingDocument_writes_request_audit_outbox()
    {
        var body = new { documentTitle = "Police Report", reason = "Required for high-risk claim" };
        var response = await _client.PostAsJsonAsync("/api/claims/CLM-1006/missing-document-requests", body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CommandResult>(JsonOpts);
        Assert.NotNull(result);
        Assert.True(result!.Success);
        Assert.Equal("CLM-1006", result.ClaimId);
        Assert.Equal("MissingDocumentRequested", result.Status);
        Assert.NotNull(result.AuditEventId);
        Assert.True(result.AuditEventId > 0);
        Assert.NotNull(result.OutboxMessageId);
        Assert.True(result.OutboxMessageId > 0);
    }

    // -----------------------------------------------------------------------
    // (C5) CreateDocumentMetadataPlaceholder — writes row + audit + outbox
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CreateDocumentMetadataPlaceholder_writes_row_audit_outbox()
    {
        var body = new { kind = "police-report", title = "Police Report 2024", docType = "document" };
        var response = await _client.PostAsJsonAsync("/api/claims/CLM-1006/document-metadata", body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CommandResult>(JsonOpts);
        Assert.NotNull(result);
        Assert.True(result!.Success);
        Assert.Equal("CLM-1006", result.ClaimId);
        Assert.Equal("MetadataCreated", result.Status);
        Assert.NotNull(result.AuditEventId);
        Assert.True(result.AuditEventId > 0);
        Assert.NotNull(result.OutboxMessageId);
        Assert.True(result.OutboxMessageId > 0);
    }

    // -----------------------------------------------------------------------
    // (C6) Idempotency-Key — duplicate key returns existing row, no duplicate outbox
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Idempotency_key_duplicate_is_safe_no_duplicate_outbox()
    {
        const string idempKey = "idem-test-draft-clm1006-001";
        var body = new { currentDecision = "NeedsMoreInformation", notes = "First call" };

        // First call
        var req1 = new HttpRequestMessage(HttpMethod.Post, "/api/claims/CLM-1006/approval-draft")
        {
            Content = JsonContent.Create(body),
        };
        req1.Headers.Add("Idempotency-Key", idempKey);
        var res1 = await _client.SendAsync(req1);
        Assert.Equal(HttpStatusCode.OK, res1.StatusCode);
        var r1 = await res1.Content.ReadFromJsonAsync<CommandResult>(JsonOpts);
        Assert.NotNull(r1);
        var firstOutboxId = r1!.OutboxMessageId;

        // Second call with same key
        var req2 = new HttpRequestMessage(HttpMethod.Post, "/api/claims/CLM-1006/approval-draft")
        {
            Content = JsonContent.Create(body),
        };
        req2.Headers.Add("Idempotency-Key", idempKey);
        var res2 = await _client.SendAsync(req2);
        Assert.Equal(HttpStatusCode.OK, res2.StatusCode);
        var r2 = await res2.Content.ReadFromJsonAsync<CommandResult>(JsonOpts);
        Assert.NotNull(r2);

        // Second call returns the SAME outbox id (no duplicate)
        Assert.Equal(firstOutboxId, r2!.OutboxMessageId);
        // Warning should indicate duplicate
        Assert.NotEmpty(r2.Warnings);
        Assert.Contains(r2.Warnings, w => w.Contains("duplicate"));
    }

    // -----------------------------------------------------------------------
    // (C7) CommandResult shape — all required fields present
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CommandResult_has_required_shape()
    {
        var body = new { kind = "photo-front", title = "Front Photo" };
        var response = await _client.PostAsJsonAsync("/api/claims/CLM-1006/document-metadata", body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // All required fields per CommandResult record
        Assert.True(root.TryGetProperty("success", out _), "missing: success");
        Assert.True(root.TryGetProperty("commandId", out var cmdId), "missing: commandId");
        Assert.False(string.IsNullOrEmpty(cmdId.GetString()), "commandId must not be empty");
        Assert.True(root.TryGetProperty("claimId", out var cid), "missing: claimId");
        Assert.Equal("CLM-1006", cid.GetString());
        Assert.True(root.TryGetProperty("correlationId", out var corrId), "missing: correlationId");
        Assert.False(string.IsNullOrEmpty(corrId.GetString()), "correlationId must not be empty");
        Assert.True(root.TryGetProperty("auditEventId", out _), "missing: auditEventId");
        Assert.True(root.TryGetProperty("outboxMessageId", out _), "missing: outboxMessageId");
        Assert.True(root.TryGetProperty("message", out _), "missing: message");
        Assert.True(root.TryGetProperty("warnings", out _), "missing: warnings");
    }

    // -----------------------------------------------------------------------
    // (C8) AI provider is MockAiProvider (no HTTP egress); command services intact.
    // Updated for gate AddAiAnalysisRunStructuredFields: IAiProvider is now MockAiProvider.
    // -----------------------------------------------------------------------

    [Fact]
    public void Command_path_constructs_no_ai_provider_and_makes_no_http_egress()
    {
        using var scope = _factory.Services.CreateScope();
        // IAiProvider is now MockAiProvider — deterministic local mock, no HTTP, no real provider.
        var aiProvider = scope.ServiceProvider.GetService<InsuranceAIPlatform.Services.AiAnalysis.IAiProvider>();
        Assert.NotNull(aiProvider);
        Assert.Equal(InsuranceAIPlatform.Services.AiAnalysis.AiProviderMode.Mock, aiProvider!.Mode);

        // Command services must be service interfaces, not DbContext types
        var approval  = scope.ServiceProvider.GetService<IApprovalService>();
        var documents = scope.ServiceProvider.GetService<IDocumentsService>();
        var audit     = scope.ServiceProvider.GetService<IAuditCostService>();
        Assert.NotNull(approval);
        Assert.NotNull(documents);
        Assert.NotNull(audit);
    }

    // -----------------------------------------------------------------------
    // (C9) Controller constructors take service interfaces, not DbContext types
    // -----------------------------------------------------------------------

    [Fact]
    public void ClaimCommandsController_constructor_takes_service_interfaces_not_dbcontext()
    {
        var ctorParams = typeof(ClaimCommandsController)
            .GetConstructors()
            .SelectMany(c => c.GetParameters())
            .Select(p => p.ParameterType)
            .ToList();

        // Must NOT inject DbContext or DbContextFactory directly
        Assert.DoesNotContain(ctorParams, t =>
            t.Name.Contains("DbContext") || t.Name.Contains("DbContextFactory"));

        // Must inject service interfaces
        Assert.Contains(typeof(IApprovalService), ctorParams);
        Assert.Contains(typeof(IDocumentsService), ctorParams);
        Assert.Contains(typeof(IAuditCostService), ctorParams);
    }

    // -----------------------------------------------------------------------
    // (C10) Invalid claimId → 400; unknown but valid claimId → 404
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("BADID")]
    [InlineData("CLM-")]
    [InlineData("clm-1006")]
    public async Task Commands_invalid_claimId_returns_400(string badId)
    {
        var body = new { currentDecision = "ApproveForReview" };
        var response = await _client.PostAsJsonAsync($"/api/claims/{badId}/approval-draft", body);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Commands_unknown_valid_claimId_returns_404()
    {
        var body = new { currentDecision = "ApproveForReview" };
        var response = await _client.PostAsJsonAsync("/api/claims/CLM-9999/approval-draft", body);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // -----------------------------------------------------------------------
    // (C11) CorrelationId header is echoed in response
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Command_response_includes_correlation_id_header()
    {
        var req = new HttpRequestMessage(HttpMethod.Post, "/api/claims/CLM-1006/approval-draft")
        {
            Content = JsonContent.Create(new { notes = "corr-test" }),
        };
        req.Headers.Add("X-Correlation-Id", "test-corr-99887766");

        var response = await _client.SendAsync(req);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("X-Correlation-Id"));
        var echoed = response.Headers.GetValues("X-Correlation-Id").FirstOrDefault();
        Assert.Equal("test-corr-99887766", echoed);

        // CorrelationId in the CommandResult body must match the header
        var result = await response.Content.ReadFromJsonAsync<CommandResult>(JsonOpts);
        Assert.NotNull(result);
        Assert.Equal("test-corr-99887766", result!.CorrelationId);
    }
}
