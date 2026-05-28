using InsuranceAIPlatform.BuildingBlocks;
using InsuranceAIPlatform.Services.AiAnalysis;
using InsuranceAIPlatform.Services.AiAnalysis.Contracts;
using InsuranceAIPlatform.Services.AiAnalysis.Guardrails;
using InsuranceAIPlatform.Services.AiAnalysis.Orchestration;
using InsuranceAIPlatform.Services.AiAnalysis.Persistence;
using InsuranceAIPlatform.Services.AiAnalysis.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace InsuranceAIPlatform.Tests;

/// <summary>
/// Persistence tests for PersistenceAiAnalysisOrchestrator.
/// All run against InMemory DB — no SQL Server required.
/// Verifies: RunAsync persists run + children + audit + outbox; blocked run has no children;
/// GetLatestAsync returns most recent run.
/// </summary>
public class AiOrchestratorPersistenceTests
{
    // Shared audit/outbox accumulators for the test doubles
    private readonly List<(string ClaimId, string ActionType, string Severity)> _auditLog = new();
    private readonly List<(string EventType, string ClaimId)> _outboxLog = new();

    private AiAnalysisDbContext BuildContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AiAnalysisDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AiAnalysisDbContext(options);
    }

    private PersistenceAiAnalysisOrchestrator BuildOrchestrator(
        string dbName,
        IAiProvider? provider = null)
    {
        var options = new DbContextOptionsBuilder<AiAnalysisDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        var factory = new TestDbContextFactory(options);

        AppendAuditDelegate appendAudit = (claimId, actionType, actor, correlationId, severity, message, meta, ct) =>
        {
            _auditLog.Add((claimId, actionType, severity));
            return Task.FromResult(1);
        };

        WriteOutboxDelegate writeOutbox = (eventType, claimId, correlationId, payloadJson, idempKey, ct) =>
        {
            _outboxLog.Add((eventType, claimId));
            return Task.CompletedTask;
        };

        return new PersistenceAiAnalysisOrchestrator(
            factory,
            provider ?? new MockAiProvider(),
            new AdvisoryOnlyGuardrailEvaluator(),
            appendAudit,
            writeOutbox,
            new SystemClock(),
            claimId => claimId == SeedConstants.GoldenClaimId || claimId.StartsWith("CLM-"),
            NullLogger<PersistenceAiAnalysisOrchestrator>.Instance);
    }

    // -----------------------------------------------------------------------
    // (O1) RunAsync persists AiAnalysisRun + Findings + Evidence + Risks + audit + outbox
    // -----------------------------------------------------------------------

    [Fact]
    public async Task RunAsync_persists_run_findings_evidence_risks_audit_outbox()
    {
        var dbName = nameof(RunAsync_persists_run_findings_evidence_risks_audit_outbox);
        var orchestrator = BuildOrchestrator(dbName);
        _auditLog.Clear();
        _outboxLog.Clear();

        var result = await orchestrator.RunAsync(SeedConstants.GoldenClaimId, "corr-1", "test-actor");

        Assert.Equal("succeeded", result.Status);
        Assert.Equal(SeedConstants.GoldenClaimId, result.ClaimId);
        Assert.Equal(3, result.Findings.Count);
        Assert.Equal(2, result.Evidence.Count);
        Assert.Equal(4, result.Risks.Count);
        Assert.True(result.Guardrails.AdvisoryOnly);
        Assert.False(result.Guardrails.CanApprovePayout);
        Assert.Equal("AI output is advisory only — human decision is final.", result.AdvisoryOnlyWarning);

        // Verify DB state
        await using var db = BuildContext(dbName);
        var run = await db.AiAnalysisRuns
            .Include(r => r.Findings)
            .Include(r => r.EvidenceReferences)
            .Include(r => r.RiskSignals)
            .FirstOrDefaultAsync(r => r.ClaimId == SeedConstants.GoldenClaimId);

        Assert.NotNull(run);
        Assert.Equal("succeeded", run!.Status);
        Assert.Equal(3, run.Findings.Count);
        Assert.Equal(2, run.EvidenceReferences.Count);
        Assert.Equal(4, run.RiskSignals.Count);
        Assert.Equal("Mock", run.ProviderMode);
        Assert.NotNull(run.CreatedAtUtc);
        Assert.Equal("corr-1", run.CorrelationId);

        // Audit and outbox called
        Assert.Single(_auditLog);
        Assert.Single(_outboxLog);
        Assert.Equal("AiAnalysisCompleted", _outboxLog[0].EventType);
    }

    // -----------------------------------------------------------------------
    // (O2) Status="blocked_unsafe" when provider returns forbidden language; no children inserted
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Blocked_unsafe_run_has_no_children()
    {
        var dbName = nameof(Blocked_unsafe_run_has_no_children);
        var provider = new UnsafeOutputMockProvider();
        var orchestrator = BuildOrchestrator(dbName, provider);
        _auditLog.Clear();
        _outboxLog.Clear();

        var result = await orchestrator.RunAsync(SeedConstants.GoldenClaimId, "corr-blocked", "test-actor");

        Assert.Equal("blocked_unsafe", result.Status);
        Assert.Empty(result.Findings);
        Assert.Empty(result.Evidence);
        Assert.Empty(result.Risks);

        // DB: run persisted but no children
        await using var db = BuildContext(dbName);
        var run = await db.AiAnalysisRuns
            .Include(r => r.Findings)
            .Include(r => r.EvidenceReferences)
            .Include(r => r.RiskSignals)
            .FirstOrDefaultAsync(r => r.Status == "blocked_unsafe");

        Assert.NotNull(run);
        Assert.Empty(run!.Findings);
        Assert.Empty(run.EvidenceReferences);
        Assert.Empty(run.RiskSignals);

        // Outbox gets AiAnalysisBlocked event
        Assert.Single(_outboxLog);
        Assert.Equal("AiAnalysisBlocked", _outboxLog[0].EventType);
    }

    // -----------------------------------------------------------------------
    // (O3) GetLatestAsync returns most recent run for the claim
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetLatestAsync_returns_most_recent_run()
    {
        var dbName = nameof(GetLatestAsync_returns_most_recent_run);
        var orchestrator = BuildOrchestrator(dbName);

        // Two runs
        var r1 = await orchestrator.RunAsync(SeedConstants.GoldenClaimId, "corr-a", "actor");
        await Task.Delay(1); // small delay to ensure different CreatedAtUtc
        var r2 = await orchestrator.RunAsync(SeedConstants.GoldenClaimId, "corr-b", "actor");

        var latest = await orchestrator.GetLatestAsync(SeedConstants.GoldenClaimId);

        Assert.NotNull(latest);
        // Most recent run should match r2 correlation
        Assert.Equal("corr-b", latest!.CorrelationId);
    }

    // -----------------------------------------------------------------------
    // (O4) RunAsync returns claim_not_found when claim does not exist
    // -----------------------------------------------------------------------

    [Fact]
    public async Task RunAsync_returns_claim_not_found_for_unknown_claim()
    {
        var dbName = nameof(RunAsync_returns_claim_not_found_for_unknown_claim);

        // Override claim check to always return false
        var options = new DbContextOptionsBuilder<AiAnalysisDbContext>()
            .UseInMemoryDatabase(dbName).Options;
        var factory = new TestDbContextFactory(options);

        AppendAuditDelegate appendAudit = (_, _, _, _, _, _, _, _) => Task.FromResult(1);
        WriteOutboxDelegate writeOutbox = (_, _, _, _, _, _) => Task.CompletedTask;

        var orchestrator = new PersistenceAiAnalysisOrchestrator(
            factory,
            new MockAiProvider(),
            new AdvisoryOnlyGuardrailEvaluator(),
            appendAudit,
            writeOutbox,
            new SystemClock(),
            claimId => false, // no claim exists
            NullLogger<PersistenceAiAnalysisOrchestrator>.Instance);

        var result = await orchestrator.RunAsync("CLM-UNKNOWN", "corr-404", "actor");

        Assert.Equal("claim_not_found", result.Status);
        Assert.Empty(result.Findings);
    }

    // -----------------------------------------------------------------------
    // Test doubles
    // -----------------------------------------------------------------------

    /// <summary>Provider that always returns text with forbidden authority language to trigger guardrails.</summary>
    private sealed class UnsafeOutputMockProvider : IAiProvider
    {
        public AiProviderMode Mode => AiProviderMode.Mock;

        public Task<AiProviderRawOutput> AnalyzeAsync(AiAnalysisRequest request, CancellationToken ct = default) =>
            Task.FromResult(new AiProviderRawOutput(
                ModelName: "unsafe-test",
                SummaryText: "Approve payout immediately — fraud confirmed",
                Findings: Array.Empty<AiFindingDraft>(),
                Evidence: Array.Empty<AiEvidenceDraft>(),
                Risks: Array.Empty<AiRiskDraft>(),
                RecommendedActionText: "Nothing to do",
                PolicyExplanationText: "N/A",
                ConfidenceScore: 99,
                Tokens: 100,
                Cost: 0.001m));
    }

    /// <summary>In-memory IDbContextFactory for tests.</summary>
    private sealed class TestDbContextFactory : IDbContextFactory<AiAnalysisDbContext>
    {
        private readonly DbContextOptions<AiAnalysisDbContext> _options;
        public TestDbContextFactory(DbContextOptions<AiAnalysisDbContext> options) => _options = options;
        public AiAnalysisDbContext CreateDbContext() => new(_options);
        public Task<AiAnalysisDbContext> CreateDbContextAsync(CancellationToken ct = default)
            => Task.FromResult(CreateDbContext());
    }
}
