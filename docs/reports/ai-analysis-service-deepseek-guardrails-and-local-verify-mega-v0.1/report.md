# AI Analysis Service · DeepSeek Guardrails · Local Verify (Mega) V0.1 — Report

**Gate:** `AI_ANALYSIS_SERVICE_DEEPSEEK_GUARDRAILS_AND_LOCAL_VERIFY_MEGA_V0.1` · **Date:** 2026-05-28
**Type:** bounded implementation — AI Analysis provider abstraction + deterministic Mock provider + disabled-by-default DeepSeek adapter + structured outputs + advisory-only guardrails + persistence integration + BFF AI endpoints + audit/outbox + tests. **No real provider call, no `DEEPSEEK_API_KEY` read, no Azure, no payout/messaging/upload, no source commit/push.**
**Status:** implemented + independently verified (Opus inspector **CLEARED**, 12/12 verified, 0 ⚠️, 0 ❌).

## Current state
- Source: branch `dev` @ `54b6e9c2c9ee9c4bfbe49521126d66229c55dff0`; `origin/main` `69e67312a10cc9bcf28c4e387a126b48c91fb9c5` untouched. **No commit this gate** — HEAD still `54b6e9c`; changes uncommitted.
- DB `InsuranceAIPlatform` (LocalDB): 1 new additive migration applied to `ai_analysis`; 200 synthetic test users intact (`CUST-T%` = 200, total customers = 201 incl. golden); `CLM-1006` intact.
- DevDept untouched.

## Provider abstraction
- `IAiProvider` extended with `Task<AiProviderRawOutput> AnalyzeAsync(AiAnalysisRequest, CancellationToken)` plus the existing `Mode` getter.
- `MockAiProvider` (Mode=`Mock`, default) — deterministic; for `CLM-1006` returns the seeded shape exactly (3 findings, 2 evidence refs, 4 risk signals, confidence 78, model `local-mock-v0.1`); generic safe stub for any other claim id.
- `DisabledDeepSeekAiProvider` (Mode=`DeepSeekDisabled`) — unconditionally throws `InvalidOperationException("DeepSeek provider is disabled … never reads DEEPSEEK_API_KEY …")`; no `HttpClient`, no SDK, no key read.
- `Program.cs` selects provider by `AiProvider:Mode` (default `Mock`) and applies defense-in-depth: even if config sets `Mode="DeepSeek" + RealCallsEnabled=true`, the wiring forces `Mock` — there is no live `DeepSeek` branch in this gate.

## Structured outputs (advisory-only)
`Contracts/AiAnalysisRequest`, `AiProviderRawOutput`, `AiAnalysisResult` (+ inner `FindingOut`/`EvidenceOut`/`RiskOut`/`RecommendedAction`/`CostTrace`). `AiAnalysisResult` carries `RunId`, `ClaimId`, `ProviderMode`, `ModelName`, `Status`, `SummaryText`, `RecommendedAction`, `PolicyCoverageExplanation`, `RiskLevel`, `ConfidenceScore`, child collections, `Guardrails`, `CostTrace`, `CorrelationId`, `CreatedAtUtc`, and a hardcoded `AdvisoryOnlyWarning` string.

## Guardrails
`GuardrailFlags` is a hardcoded record with private constructor and an `Advisory` singleton — every instance has `AdvisoryOnly=true`, `RequiresHumanReview=true`, `CanApprovePayout=false`, `CanRejectClaim=false`, `CanAccuseFraudFinal=false`, `CanSendCustomerMessage=false`, `CanChangeClaimStatus=false`. There is **no public setter, no `init`, no constructor path** that flips any `Can*` to `true`. `AdvisoryOnlyGuardrailEvaluator` scans the raw output for forbidden authority phrases (approve payout / reject claim / claim approved / fraud confirmed / final decision / status change + 8 more); if any matches, the run is persisted with `Status="blocked_unsafe"` and emits `EventType="AiAnalysisBlocked"` in the outbox instead.

## Persistence (additive migration)
Migration `20260528084917_AddAiAnalysisRunStructuredFields` adds **9 NULLABLE** columns to `ai_analysis.AiAnalysisRuns`: `ModelName`, `Status`, `SummaryText`, `RecommendedActionJson`, `PolicyExplanationText`, `GuardrailFlagsJson`, `RiskLevel`, `CreatedAtUtc`, `CorrelationId`. No other table touched, no data-loss op. `__EFMigrationsHistory(ai_analysis)` = 2 rows. The orchestrator uses `IDbContextFactory<AiAnalysisDbContext>` (singleton-safe).

## BFF endpoints (exactly 1 GET + 1 POST)
- `GET /api/claims/{claimId}/ai-analysis` — returns latest run as `AiAnalysisDto` (200) or `404` (`no_ai_analysis_run`).
- `POST /api/claims/{claimId}/ai-analysis/run` — triggers `IAiAnalysisOrchestrator.RunAsync`, returns the structured DTO (`200`); `404` for unknown claim id.

`AiAnalysisController` injects **only** `IAiAnalysisOrchestrator` + `IClaimReadService` — **no `DbContext`**. Correlation header echoed; synthetic actor `CommandActors.SyntheticAdjuster` used for audit.

Write-verb count in the entire codebase: **5 `[HttpPost]`** (4 existing commands + the new `/ai-analysis/run`), **0** `[HttpPut]`/`[HttpPatch]`/`[HttpDelete]`.

## Frontend (additive only)
`src/api/insuranceApi.types.ts` and `src/api/backendInsuranceApi.ts` gained AI types + 2 functions (`getClaimAiAnalysis`, `runClaimAiAnalysis`). No `src/features/**`, `src/pages/**`, `src/components/**` touched — UI wiring honestly deferred.

## Audit / cost integration
Every successful AI run appends an `AuditEvent` (`ActionType="AiAnalysisCompleted"`, synthetic actor, correlation id, metadata JSON `{RunId, ProviderMode, Status}`) and writes an `OutboxMessage` (`EventType="AiAnalysisCompleted"`, sanitized payload `{ClaimId, RunId, Status}`); blocked runs emit `AiAnalysisBlocked` instead. Token + cost trace fields persisted on `AiAnalysisRun` itself.

## Tests
**28 new tests added; 75 prior + 28 = 103 passing (0 failed, 0 skipped).** New test files:
- `AiAnalysisModeTests` (M1–M3): default mode = Mock; DeepSeekDisabled throws; `DEEPSEEK_API_KEY` sentinel never read.
- `MockAiProviderTests` (P1–P3): CLM-1006 deterministic golden shape; generic stub for other claim ids; idempotent across calls.
- `AiGuardrailTests` (G1–G4): unsafe authority language blocked; safe output passes; `GuardrailFlags` all-false-`Can-*` enforced.
- `AiOrchestratorPersistenceTests` (O1–O4): `RunAsync` persists run + findings + evidence + risks + audit + outbox; `blocked_unsafe` path; `GetLatestAsync` returns most recent.
- `BffAiAnalysisEndpointTests` (B1–B5): POST + GET happy paths; unknown claim → 404; correlation echoed; advisory-only guardrails returned.

Two existing tests were **tightened** (not weakened): `ServiceSkeletonTests.cs` test 4 and `ClaimCommandTests.cs` C8 — changed from `Assert.Null(provider)` to `Assert.NotNull(provider); Assert.Equal(AiProviderMode.Mock, provider.Mode)` (test names unchanged). The change is required by the new contract that the BFF now wires Mock as default.

## Verification at this gate
| Check | Result |
|---|---|
| Backend build | **PASS** — 9 projects, 0 warnings, 0 errors |
| Backend tests | **PASS** — 103 / 103 / 0 (75 prior + 28 new) |
| Frontend build | **PASS** — 107 modules, built 7.95s |
| DB invariants | 200 `CUST-T%` users; CLM-1006 present; `ai_analysis.AiAnalysisRuns` = 2 (1 legacy + 1 mock-smoke); 9 new nullable columns present; 2 ai_analysis migrations; `AuditEvents AiAnalysisCompleted` = 1; `OutboxMessages AiAnalysisCompleted` = 1 |
| Live HTTP smoke | POST /run → 200 with `IsAdvisoryOnly=true`, `ProviderMode=Mock`, 3 findings; GET → 200; unknown claim GET/POST → 404; correlation echoed |
| Write verbs | POST:5 (= 4 existing + 1 new `/ai-analysis/run`), PUT:0, PATCH:0, DELETE:0 |
| `DEEPSEEK_API_KEY` | name-only in comments / exception-message string / test assertion / configuration class doc-comment; **zero** `GetEnvironmentVariable("DEEPSEEK_API_KEY")` or `Configuration["DEEPSEEK_API_KEY"]` anywhere |
| `HttpClient` in `Services.AiAnalysis/**` | only in doc-comments stating "no HttpClient"; **zero** usage |
| BFF AI controller injection | `IAiAnalysisOrchestrator` + `IClaimReadService` only; **no `DbContext`** |
| `Services.AiAnalysis.csproj` packages | EF SqlServer + EF Design + DI Abstractions — **no provider SDK** (no Azure.AI, no OpenAI, no SemanticKernel, no DeepSeek-anything) |
| Source commit/push | **none**; `main` untouched; HEAD still `54b6e9c` |

## Issues / disclosed notes (non-blocking)
1. `PersistenceAiAnalysisOrchestrator.BuildResult()` hardcodes `ProviderMode:"Mock"` in the returned DTO. **Persistence is correct** (uses `_provider.Mode.ToString()`); since Mock is the only mode that completes successfully in this gate, DTO and DB agree. Cosmetic fix for any future non-Mock completed mode.
2. The audit `Actor` column persists as `"Synthetic Adjuster (human)"` (the `AuditCost` service's existing `Name + " (" + Type + ")"` formatting) rather than the email-form synthetic id `demo.adjuster@insuranceai.local`. The id is correctly defined and passed by the controller; this is a presentation choice of the existing AuditCost service. Still synthetic, no real person.
3. The legacy seeded run `run_8f3d2a7e` has `ProviderMode="Disabled"` and `NULL` values for the 9 new columns — expected for an additive nullable migration.

## Deferred / limitations
- True aggregate-atomic outbox is still a future gate (orchestrator's persist + audit + outbox are two local transactions like the existing command flow).
- Real DeepSeek provider call is **explicitly deferred** to a later opt-in gate; the disabled adapter exists only to make DI reachable for the `DeepSeekDisabled` mode.
- AI Analysis BFF endpoints have **no UI wiring** yet — additive frontend client only; pages/components untouched.

## Next safe step
`COMMIT_AND_PUSH_DEV_AI_ANALYSIS_SERVICE_GUARDRAILS_ONLY` — commit exactly this AI/guardrails scope to `dev`, fast-forward push `dev` only; `main` untouched; no force.
