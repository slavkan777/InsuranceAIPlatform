# AI Analysis Service — Architecture

## One-line goal

Advisory-only AI claim analysis with guardrail enforcement, audit/outbox integration, and a disabled-but-present DeepSeek adapter — no real provider call, no secret read.

## Bounded context

`InsuranceAIPlatform.Services.AiAnalysis` — isolated service assembly.  
No direct references to sibling service assemblies (`AuditCost`, `Claims`, etc.).  
Cross-service integration via injected delegate types only.

## Provider abstraction

```
IAiProvider
  ├── MockAiProvider          (Mode=Mock)       — deterministic golden output for CLM-1006
  └── DisabledDeepSeekAiProvider  (Mode=DeepSeekDisabled)  — throws InvalidOperationException, no HttpClient
```

Provider selected at startup via `AiProviderOptions.Mode` (appsettings).  
`DEEPSEEK_API_KEY` is never read at any point in any code path.

## Guardrail system

`IGuardrailEvaluator` → `AdvisoryOnlyGuardrailEvaluator`

- Scans all text fields: SummaryText, RecommendedActionText, PolicyExplanationText, finding texts, risk descriptions.
- Blocks on forbidden authority phrases (case-insensitive): "approve payout", "payout approved", "reject claim", "claim rejected", "fraud confirmed", "final decision", "case closed", "send to customer", "email customer", "sms customer", "status changed", "set status".
- Returns `GuardrailAssessment { Blocked, ReasonCode, OffendingPhrase }`.

`GuardrailFlags.Advisory` — singleton with private constructor. All `Can*` properties are hardcoded `false` and have no public setters. There is no code path to flip them.

## Orchestrator

`PersistenceAiAnalysisOrchestrator : IAiAnalysisOrchestrator`

Dependencies injected:
- `IDbContextFactory<AiAnalysisDbContext>` (singleton-safe factory)
- `IAiProvider`
- `IGuardrailEvaluator`
- `AppendAuditDelegate` (typed delegate — wraps AuditCost service without importing its assembly)
- `WriteOutboxDelegate` (typed delegate — same pattern)
- `IClock`
- `Func<string, bool>` claimExists

### RunAsync flow

```
1. claimExists check → return claim_not_found if false
2. provider.AnalyzeAsync → raw output
3. guardrail.Evaluate(raw)
4a. if blocked: persist run (status=blocked_unsafe), writeOutbox(AiAnalysisBlocked), return
4b. if passed: persist run + findings + evidence + risks (status=succeeded)
5. appendAudit(AiAnalysisCompleted)
6. writeOutbox(AiAnalysisCompleted)
7. return AiAnalysisResult
```

Child entity IDs are prefixed with `{runId}_` to prevent duplicate keys when the same provider returns fixed IDs across multiple runs against the same InMemory DB.

## Persistence schema

Schema: `ai_analysis`

- `AiAnalysisRuns` — 9 structured nullable fields added in migration `20260528084917_AddAiAnalysisRunStructuredFields`
- `AiFindings`
- `AiEvidenceReferences`
- `AiRiskSignals`

DbContext registered as `IDbContextFactory<AiAnalysisDbContext>` with `ServiceLifetime.Singleton`.

## BFF endpoints

`AiAnalysisController` — no DbContext, no sibling-service reference.

```
GET  /api/claims/{claimId}/ai-analysis       → 200 AiAnalysisDto | 404 ApiErrorResponse
POST /api/claims/{claimId}/ai-analysis/run   → 200 AiAnalysisDto | 404 ApiErrorResponse(CLAIM_NOT_FOUND)
```

Response always includes `IsAdvisoryOnly: true` and hardcoded `Notice`.  
`X-Correlation-Id` echoed in response header and `dto.CorrelationId`.

## Cross-service integration pattern (delegate injection)

```csharp
// Program.cs wires AuditCost service into delegates — AiAnalysis assembly stays clean
services.AddSingleton<AppendAuditDelegate>(sp => {
    var svc = sp.GetRequiredService<IAuditCostService>();
    return (claimId, actionType, actor, correlationId, severity, message, meta, ct)
        => svc.AppendAuditAsync(claimId, actionType, actor, correlationId, severity, message, meta, ct);
});
```

## Security constraints (hardcoded)

- No `DEEPSEEK_API_KEY` read anywhere.
- No `HttpClient` in `AiAnalysis` assembly.
- No `IFormFile`, `SendGrid`, `Twilio`, `SmtpClient` references.
- `GuardrailFlags.Advisory` cannot be mutated — private constructor + no public setters.
- `AiProviderOptions` has no key property.
