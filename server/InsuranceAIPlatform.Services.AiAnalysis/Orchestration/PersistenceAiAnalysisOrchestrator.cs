using System.Text.Json;
using InsuranceAIPlatform.BuildingBlocks;
using InsuranceAIPlatform.Services.AiAnalysis.Contracts;
using InsuranceAIPlatform.Services.AiAnalysis.Guardrails;
using InsuranceAIPlatform.Services.AiAnalysis.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InsuranceAIPlatform.Services.AiAnalysis.Orchestration;

/// <summary>
/// Delegates for audit and outbox operations — injected from Program.cs to avoid
/// a cross-service assembly reference (Services.AiAnalysis must not reference Services.AuditCost).
/// These are thin wrappers over IAuditCostService.AppendAuditAsync / WriteOutboxAsync.
/// </summary>
public delegate Task<int> AppendAuditDelegate(
    string claimId, string actionType, ActorContext actor, string correlationId,
    string severity, string message, string? metadataJson, CancellationToken ct);

public delegate Task WriteOutboxDelegate(
    string eventType, string claimId, string correlationId, string payloadJson,
    string? idempotencyKey, CancellationToken ct);

/// <summary>
/// Singleton-safe orchestrator using IDbContextFactory for database access.
/// RunAsync flow:
///   1. Validate claim via claimExistsFunc delegate (no cross-service project reference).
///   2. Generate RunId.
///   3. Call provider.AnalyzeAsync.
///   4. Evaluate guardrails.
///   5. Persist run + children (or blocked run with no children).
///   6. Append audit + outbox via delegate (no direct AuditCost service reference).
///   7. Return structured AiAnalysisResult.
///
/// AI is advisory only — no code path here approves payout, rejects claim,
/// accuses fraud, sends messages, changes status, or uploads binary.
/// DEEPSEEK_API_KEY is never read anywhere in this class.
/// </summary>
public sealed class PersistenceAiAnalysisOrchestrator : IAiAnalysisOrchestrator
{
    private readonly IDbContextFactory<AiAnalysisDbContext> _factory;
    private readonly IAiProvider _provider;
    private readonly IGuardrailEvaluator _guardrails;
    private readonly AppendAuditDelegate _appendAudit;
    private readonly WriteOutboxDelegate _writeOutbox;
    private readonly IClock _clock;
    private readonly Func<string, bool> _claimExistsFunc;
    private readonly ILogger<PersistenceAiAnalysisOrchestrator> _logger;

    public PersistenceAiAnalysisOrchestrator(
        IDbContextFactory<AiAnalysisDbContext> factory,
        IAiProvider provider,
        IGuardrailEvaluator guardrails,
        AppendAuditDelegate appendAudit,
        WriteOutboxDelegate writeOutbox,
        IClock clock,
        Func<string, bool> claimExistsFunc,
        ILogger<PersistenceAiAnalysisOrchestrator> logger)
    {
        _factory        = factory;
        _provider       = provider;
        _guardrails     = guardrails;
        _appendAudit    = appendAudit;
        _writeOutbox    = writeOutbox;
        _clock          = clock;
        _claimExistsFunc = claimExistsFunc;
        _logger         = logger;
    }

    // -----------------------------------------------------------------------
    // RunAsync
    // -----------------------------------------------------------------------

    public async Task<AiAnalysisResult> RunAsync(
        string claimId,
        string correlationId,
        string actor,
        CancellationToken ct = default)
    {
        // Step 1: Validate claim existence
        if (!_claimExistsFunc(claimId))
        {
            _logger.LogWarning("AI analysis requested for unknown claimId {ClaimId}", claimId);
            return BuildClaimNotFoundResult(claimId, correlationId);
        }

        // Step 2: Generate RunId
        var runId = "run_" + Guid.NewGuid().ToString("N")[..8];
        var now   = _clock.UtcNow.UtcDateTime;

        // Step 3: Call provider
        var request = new AiAnalysisRequest(claimId, correlationId, actor);
        var rawOutput = await _provider.AnalyzeAsync(request, ct);

        // Step 4: Evaluate guardrails
        var assessment = _guardrails.Evaluate(rawOutput);

        // Step 5: Persist
        await using var db = await _factory.CreateDbContextAsync(ct);

        string status;
        AiAnalysisResult result;
        string eventType;

        if (assessment.Blocked)
        {
            status    = "blocked_unsafe";
            eventType = "AiAnalysisBlocked";

            var blockedRun = new AiAnalysisRun
            {
                RunId          = runId,
                ClaimId        = claimId,
                ProviderMode   = _provider.Mode.ToString(),
                ModelConfidence= rawOutput.ConfidenceScore,
                Tokens         = rawOutput.Tokens,
                Cost           = rawOutput.Cost,
                ModelName      = rawOutput.ModelName,
                Status         = status,
                SummaryText    = rawOutput.SummaryText,
                RiskLevel      = "unknown",
                CreatedAtUtc   = now,
                CorrelationId  = correlationId,
                GuardrailFlagsJson    = JsonSerializer.Serialize(assessment.Flags),
                RecommendedActionJson = null,
                PolicyExplanationText = null,
            };
            db.AiAnalysisRuns.Add(blockedRun);
            await db.SaveChangesAsync(ct);

            result = BuildResult(runId, claimId, _provider.Mode.ToString(), status, rawOutput, assessment, now, correlationId,
                findings: Array.Empty<FindingOut>(),
                evidence: Array.Empty<EvidenceOut>(),
                risks: Array.Empty<RiskOut>(),
                riskLevel: "unknown");
        }
        else
        {
            status    = "succeeded";
            eventType = "AiAnalysisCompleted";

            var riskLevel = ComputeRiskLevel(rawOutput.Risks);

            var recommendedAction = new { Action = rawOutput.RecommendedActionText, Rationale = "AI advisory recommendation", ConfidenceScore = rawOutput.ConfidenceScore };

            var run = new AiAnalysisRun
            {
                RunId          = runId,
                ClaimId        = claimId,
                ProviderMode   = _provider.Mode.ToString(),
                ModelConfidence= rawOutput.ConfidenceScore,
                Tokens         = rawOutput.Tokens,
                Cost           = rawOutput.Cost,
                ModelName      = rawOutput.ModelName,
                Status         = status,
                SummaryText    = rawOutput.SummaryText,
                RiskLevel      = riskLevel,
                CreatedAtUtc   = now,
                CorrelationId  = correlationId,
                GuardrailFlagsJson    = JsonSerializer.Serialize(assessment.Flags),
                RecommendedActionJson = JsonSerializer.Serialize(recommendedAction),
                PolicyExplanationText = rawOutput.PolicyExplanationText,
                // IDs are prefixed with runId suffix to ensure uniqueness across runs.
                // The draft Id (f1, e1, rs1 etc.) is preserved as a suffix for traceability.
                Findings = rawOutput.Findings.Select(f => new AiFinding
                {
                    Id       = $"{runId}_{f.Id}",
                    RunId    = runId,
                    Category = f.Category,
                    Text     = f.Text,
                    Severity = f.Severity,
                }).ToList(),
                EvidenceReferences = rawOutput.Evidence.Select(e => new AiEvidenceReference
                {
                    Id         = $"{runId}_{e.Id}",
                    RunId      = runId,
                    Source     = e.Source,
                    Note       = e.Note,
                    Confidence = e.Confidence,
                }).ToList(),
                RiskSignals = rawOutput.Risks.Select(r => new AiRiskSignal
                {
                    Id     = $"{runId}_{r.Id}",
                    RunId  = runId,
                    Label  = r.Label,
                    Weight = r.Weight,
                }).ToList(),
            };
            db.AiAnalysisRuns.Add(run);
            await db.SaveChangesAsync(ct);

            var findingsOut = rawOutput.Findings.Select(f => new FindingOut(f.Id, f.Category, f.Text, f.Severity)).ToList();
            var evidenceOut = rawOutput.Evidence.Select(e => new EvidenceOut(e.Id, e.Source, e.Note, e.Confidence)).ToList();
            var risksOut    = rawOutput.Risks.Select(r => new RiskOut(r.Id, r.Label, r.Weight)).ToList();

            result = BuildResult(runId, claimId, _provider.Mode.ToString(), status, rawOutput, assessment, now, correlationId,
                findings: findingsOut, evidence: evidenceOut, risks: risksOut, riskLevel: riskLevel);
        }

        // Step 6: Audit + outbox (via delegates — no direct AuditCost service reference)
        var actorContext = CommandActors.SyntheticAdjuster();
        var meta = JsonSerializer.Serialize(new { RunId = runId, ProviderMode = _provider.Mode.ToString(), Status = status });

        await _appendAudit(
            claimId, "AiAnalysisCompleted", actorContext, correlationId,
            assessment.Blocked ? "WARN" : "OK",
            $"AI analysis run {runId} {status} for claim {claimId}.", meta, ct);

        var payload = JsonSerializer.Serialize(new { ClaimId = claimId, RunId = runId, Status = status });
        await _writeOutbox(eventType, claimId, correlationId, payload, null, ct);

        return result;
    }

    // -----------------------------------------------------------------------
    // GetLatestAsync
    // -----------------------------------------------------------------------

    public async Task<AiAnalysisResult?> GetLatestAsync(string claimId, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var run = await db.AiAnalysisRuns
            .Include(r => r.Findings)
            .Include(r => r.EvidenceReferences)
            .Include(r => r.RiskSignals)
            .Where(r => r.ClaimId == claimId)
            .OrderByDescending(r => r.CreatedAtUtc ?? DateTime.MinValue)
            .FirstOrDefaultAsync(ct);

        if (run is null) return null;

        return MapRunToResult(run, claimId);
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static string ComputeRiskLevel(IReadOnlyList<AiRiskDraft> risks)
    {
        var sum = risks.Sum(r => r.Weight);
        return sum < 30 ? "low" : sum <= 60 ? "moderate" : "high";
    }

    private static AiAnalysisResult BuildResult(
        string runId,
        string claimId,
        string providerMode,
        string status,
        AiProviderRawOutput raw,
        GuardrailAssessment assessment,
        DateTime now,
        string correlationId,
        IReadOnlyList<FindingOut> findings,
        IReadOnlyList<EvidenceOut> evidence,
        IReadOnlyList<RiskOut> risks,
        string riskLevel)
    {
        return new AiAnalysisResult(
            RunId: runId,
            ClaimId: claimId,
            ProviderMode: providerMode,
            ModelName: raw.ModelName,
            Status: status,
            SummaryText: raw.SummaryText,
            RecommendedAction: new RecommendedActionOut(raw.RecommendedActionText, "AI advisory recommendation", raw.ConfidenceScore),
            PolicyCoverageExplanation: raw.PolicyExplanationText,
            RiskLevel: riskLevel,
            ConfidenceScore: raw.ConfidenceScore,
            Findings: findings,
            Evidence: evidence,
            Risks: risks,
            Guardrails: assessment.Flags,
            CostTrace: new CostTraceOut(raw.Tokens, raw.Cost),
            CorrelationId: correlationId,
            CreatedAtUtc: now);
    }

    private static AiAnalysisResult BuildClaimNotFoundResult(string claimId, string correlationId) =>
        new(
            RunId: "run_not_found",
            ClaimId: claimId,
            ProviderMode: "unknown",
            ModelName: "n/a",
            Status: "claim_not_found",
            SummaryText: string.Empty,
            RecommendedAction: new RecommendedActionOut(string.Empty, string.Empty, 0),
            PolicyCoverageExplanation: string.Empty,
            RiskLevel: "unknown",
            ConfidenceScore: 0,
            Findings: Array.Empty<FindingOut>(),
            Evidence: Array.Empty<EvidenceOut>(),
            Risks: Array.Empty<RiskOut>(),
            Guardrails: GuardrailFlags.Advisory,
            CostTrace: new CostTraceOut(0, 0m),
            CorrelationId: correlationId,
            CreatedAtUtc: DateTime.UtcNow);

    private static AiAnalysisResult MapRunToResult(AiAnalysisRun run, string claimId)
    {
        // Strip runId prefix from child IDs (e.g. "run_abc123_f1" → "f1")
        string StripPrefix(string id) => id.Contains('_') ? id[(id.IndexOf('_', id.IndexOf('_') + 1) + 1)..] : id;

        var findings = run.Findings.Select(f => new FindingOut(StripPrefix(f.Id), f.Category, f.Text, f.Severity)).ToList();
        var evidence = run.EvidenceReferences.Select(e => new EvidenceOut(StripPrefix(e.Id), e.Source, e.Note, e.Confidence)).ToList();
        var risks    = run.RiskSignals.Select(r => new RiskOut(StripPrefix(r.Id), r.Label, r.Weight)).ToList();

        RecommendedActionOut recommendedAction;
        if (!string.IsNullOrEmpty(run.RecommendedActionJson))
        {
            try
            {
                var ra = JsonSerializer.Deserialize<JsonElement>(run.RecommendedActionJson);
                recommendedAction = new RecommendedActionOut(
                    ra.GetProperty("Action").GetString() ?? string.Empty,
                    ra.GetProperty("Rationale").GetString() ?? string.Empty,
                    ra.TryGetProperty("ConfidenceScore", out var cs) ? cs.GetInt32() : run.ModelConfidence);
            }
            catch
            {
                recommendedAction = new RecommendedActionOut(string.Empty, string.Empty, run.ModelConfidence);
            }
        }
        else
        {
            recommendedAction = new RecommendedActionOut(string.Empty, string.Empty, run.ModelConfidence);
        }

        return new AiAnalysisResult(
            RunId: run.RunId,
            ClaimId: run.ClaimId,
            ProviderMode: run.ProviderMode,
            ModelName: run.ModelName ?? "n/a",
            Status: run.Status ?? "unknown",
            SummaryText: run.SummaryText ?? string.Empty,
            RecommendedAction: recommendedAction,
            PolicyCoverageExplanation: run.PolicyExplanationText ?? string.Empty,
            RiskLevel: run.RiskLevel ?? "unknown",
            ConfidenceScore: run.ModelConfidence,
            Findings: findings,
            Evidence: evidence,
            Risks: risks,
            Guardrails: GuardrailFlags.Advisory,
            CostTrace: new CostTraceOut(run.Tokens, run.Cost),
            CorrelationId: run.CorrelationId ?? string.Empty,
            CreatedAtUtc: run.CreatedAtUtc ?? DateTime.UtcNow);
    }
}
