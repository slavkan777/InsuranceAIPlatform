# InsuranceAIPlatform · Real DeepSeek Rotated-Key Short Smoke Retry V0.1 — Report

**Gate:** `REAL_DEEPSEEK_ROTATED_KEY_SHORT_SMOKE_RETRY_V0.1` · **Date:** 2026-05-28
**Type:** short provider-recovery smoke / no implementation / no commit / no push / no Azure.
**Verdict:** **ACCEPT** (independent inspector CLEARED 13/13 ✅, 0 ⚠️, 0 ❌).

## Why this gate existed

The previous security-hygiene gate (`SECURITY_ROTATE_AND_FIX_DEEPSEEK_LOCAL_SECRET_SETUP_V0.1`) verified the rotated-key setup end-to-end but could not capture a successful 200 response because DeepSeek upstream returned 503 / 30s timeout on all 4 attempts in that window — classified as external provider availability, not implementation failure. Before committing the real DeepSeek opt-in integration, GPT asked for a short follow-up smoke once the provider recovered to obtain fresh body-shape evidence with the rotated key specifically.

## Current state of the source repo (unchanged this gate)

- Branch `dev` @ `918e8a3aec82462bde9239aeec91d3a46b1082b4` (HEAD = `origin/dev`).
- `origin/main` `69e67312a10cc9bcf28c4e387a126b48c91fb9c5` — untouched.
- No commit / push / amend / rebase / merge in this gate.
- Working tree: 6 modified tracked files + 20 untracked items (10 architecture docs + 6 report dirs + 4 new source/test files). The +1 untracked vs the previous gate is the prior gate's source-side mirror dir `docs/reports/security-rotate-and-fix-deepseek-local-secret-setup-v0.1/`, carried forward.
- Zero new source code changes in this gate.

## Phase 1 — Safe secret path check (booleans only — no values disclosed)

| Field | Value |
|---|---|
| `key_present` | true |
| `key_source_kind` | user-scope-env-registry |
| `key_shape_plausible` | true |
| `auth_result` | accepted (zero 401s) |

Value never echoed / written to disk / assigned to durable variable; `[System.GC]::Collect()` invoked after spawn.

## Phase 2 — Repo / log leakage scan

| Check | Result |
|---|---|
| `git grep -nE 'sk-[A-Za-z0-9_-]{16,}'` (tracked) | **PASS** — 0 hits |
| `sk-` shape across 20 untracked items | **PASS** — 0 hits |
| `DEEPSEEK_API_KEY\s*=` literal in any tracked file | **PASS** — 0 hits |
| `appsettings.Development.json` key literal | **PASS** — only Mode/RealCallsEnabled/Model/Endpoint/TimeoutSeconds |
| `ApiKey` property anywhere in `server/**/*.cs` | **PASS** — 0 hits |
| `PackageReference` scan across 10 csproj files | **PASS** — only `Microsoft.Extensions.*` / `Microsoft.EntityFrameworkCore.*` / `Microsoft.AspNetCore.Mvc.Testing` / `Microsoft.NET.Test.Sdk` / `xunit` / `coverlet` / `Swashbuckle`. Zero provider SDK |
| API stdout log `api-real-deepseek-retry.stdout.log` for `sk-` shape | **PASS** — 0 hits |
| Same log for `DEEPSEEK_API_KEY` literal | **PASS** — 0 hits |
| Real PII | **PASS** — synthetic CLM-1006 only |
| Azure resources / SDKs | **PASS** — none |

## Phase 3 — Build / test / frontend (reused from prior-gate evidence)

| Step | Result | Source |
|---|---|---|
| Backend build (`dotnet build -c Release --nologo`) | **PASS** — 0 warnings / 0 errors | reused from prior gate ~45 min ago; source identical (HEAD unchanged), binaries intact |
| Backend tests (`dotnet test --no-build`) | **PASS** — 113 / 113 / 0 | reused, same justification |
| Frontend build (`npm run build`) | **PASS** — 107 modules | reused, same justification |

## Phase 4 — Mock default regression smoke (port 5284)

Default config (`appsettings.Development.json`: `Mode=Mock`, `RealCallsEnabled=false`). Health = 200 within 1s.

- `POST /api/claims/CLM-1006/ai-analysis/run` → **200**.
- DB row `run_5687085a` persisted at 2026-05-28 11:51:52 with `ProviderMode=Mock`, `ModelName=local-mock-v0.1`, `Tokens=4261`, `Cost=0.0187`, `ModelConfidence=78`, `RiskLevel=high`, summary in Ukrainian (Mock fingerprint), all guardrails advisory-only, every `Can*` flag false.
- API stopped cleanly.

## Phase 5 — Real DeepSeek rotated-key retry smoke (port 5285)

Started API with explicit opt-in (`--AiProvider:Mode=DeepSeek --AiProvider:RealCallsEnabled=true`). Process env at spawn time refreshed from User-scope. Startup log emitted **`AI provider resolved to: RealDeepSeek (opt-in)`** (3-condition gate satisfied).

**Single attempt — HTTP 200 in ~8s.** Per gate rules ("do not run a second call unless needed for confirmation"), no further attempts executed.

### Response fields (sanitized)

| Field | Value |
|---|---|
| `runId` | `run_817e340b` |
| `claimId` | `CLM-1006` |
| `providerMode` | **DeepSeek** (not Mock) |
| `modelName` returned | **deepseek-v4-flash** (server alias of configured `deepseek-chat`) |
| `status` | `succeeded` |
| `confidenceScore` | **75** (Mock = 78) |
| `costTrace.tokens` | **688** (Mock = 4261) |
| `costTrace.estimatedCost` | **0.00018576** (Mock = 0.0187) |
| `costTrace.currencyCode` | `USD` |
| `riskLevel` | `high` |
| `findings` count | 2 |
| `evidence` count | 2 |
| `risks` count | 2 |
| `summaryText` language | **English** (Mock is Ukrainian) — `Claim CLM-1006 involves a 2021 Toyota Camry in a road traffic accident near Boryspil. The recommended payout ($2720) is...` |
| `recommendedAction` | object `{action, rationale, confidenceScore}` — `"Request the missing rear bumper photo before proceeding..."` |
| `policyCoverageExplanation` | present, 159 chars — `"Auto Comprehensive Policy POL-2025-AC-4421 covers accident damage subject to a $500 deductible..."` |
| `isAdvisoryOnly` | true |
| `notice` | present (advisory disclaimer) |
| `correlationId` | `corr-retry-real-001` (echoed) |
| `guardrails.advisoryOnly` | **true** |
| `guardrails.canApprovePayout` | **false** |
| `guardrails.canRejectClaim` | **false** |
| `guardrails.canAccuseFraudFinal` | **false** |
| `guardrails.canSendCustomerMessage` | **false** |
| `guardrails.canChangeClaimStatus` | **false** |

API stopped cleanly. Log scan: 0× `sk-` shape, 0× `DEEPSEEK_API_KEY` literal, 0× any key material.

## G2 external-call evidence triple

| Part | Status | Source |
|---|---|---|
| (a) process log host | **PASS** | `RealDeepSeekAiProvider: DeepSeek AI provider sending request. Host: api.deepseek.com Path: /v1/chat/completions` + `HttpClient.deepseek.ClientHandler[101] Received HTTP response headers after 724.3794ms - 200` in `api-real-deepseek-retry.stdout.log` |
| (b) DB row fingerprint | **PASS** | `run_817e340b`: ProviderMode=`DeepSeek`, ModelName=`deepseek-v4-flash`, Tokens=`688`, Cost=`0.0002`, ModelConfidence=`75` — all distinct from Mock fingerprint values |
| (c) shape evidence | **PASS** | English summary (Mock is Ukrainian); structured 2/2/2 finding/evidence/risk shape (Mock fixed 3/2/4); ~8s round-trip (real LLM call, not synchronous mock); `estimatedCost=0.00018576` (real provider pricing) |

## G5 stub-fallback fingerprint absence

**PASS** — none of the Mock fingerprint values (`modelName=local-mock-v0.1`, `Tokens=4261`, `Cost=0.0187`, `ConfidenceScore=78`, Ukrainian summary text, 3/2/4 finding/evidence/risk shape) appear in `run_817e340b`.

## Phase 6 — DB / audit / outbox check

| Query | Result |
|---|---|
| `SELECT COUNT(*) FROM ai_analysis.AiAnalysisRuns` | 10 total (+2 this gate: `run_5687085a` Mock + `run_817e340b` DeepSeek) |
| `SELECT ProviderMode, COUNT(*) ... GROUP BY ProviderMode` | DeepSeek=3, Disabled=1, Mock=6 |
| `SELECT COUNT(*) FROM audit_cost.AuditEvents WHERE ActionType='AiAnalysisCompleted'` | 9 (+2 this gate) |
| `SELECT COUNT(*) FROM audit_cost.OutboxMessages WHERE EventType='AiAnalysisCompleted'` | 9 (+2 this gate) |
| AuditEvent for `run_817e340b` | Id=25, ActionType=`AiAnalysisCompleted`, ClaimId=CLM-1006, CorrelationId=`corr-retry-real-001`, OccurredAtUtc=2026-05-28 11:52:45.6125887 |
| OutboxMessage for `run_817e340b` | Id=20, EventType=`AiAnalysisCompleted`, ClaimId=CLM-1006, CorrelationId=`corr-retry-real-001`, OccurredAtUtc=2026-05-28 11:52:45.9339045 |

## Provider classification

**`provider_recovered_200_success`** — DeepSeek upstream returned valid 200 with structured advisory content; previous gate's `provider_availability` limitation is now removed.

## Independent inspector review

| Check | Result |
|---|---|
| Claims verified | **13/13** |
| Warnings | 0/13 |
| Failures | 0/13 |
| Key value in any output | **NO** |
| Key value in any tracked file | **NO** |
| Key value in any log | **NO** |
| Repository leakage | **NO** |
| Any `401` returned to authenticated request | **NO** |
| Successful `200` response captured | **YES** |
| **Final inspector verdict** | **CLEARED** |

### Inspector non-binding recommendations

- Stage the 4 untracked source/test files (`RealDeepSeekAiProvider.cs`, `DeepSeekChatModels.cs`, `AiProviderSelectionTests.cs`, `RealDeepSeekProviderTests.cs`) in the next implementation commit gate so `origin/dev` reflects the real provider source under version control.
- Persisted log files helped this audit; consider archiving them alongside the per-run report dir for future replay.
- The ~6.5s delta between `AiAnalysisRuns.CreatedAtUtc` and `AuditEvents.OccurredAtUtc` is informational; not a defect at the current SLA.

## Limitations removed this gate

- Previous gate's limitation `"no successful 200 in window due to DeepSeek upstream 503/timeouts"` is now **RESOLVED** by this gate's 200 success on `run_817e340b`.

## Known limitations (carried forward — non-blocking)

- `AuditEvent.Actor` formatted as `"Synthetic Adjuster (human)"` by existing AuditCost service `Name + " (" + Type + ")"` formatting; still synthetic, no real person.
- Legacy seed run `run_8f3d2a7e` retains `ProviderMode='Disabled'` + NULL on the 9 new nullable columns; expected behaviour for the additive migration.

## Blockers

**None.** Implementation untouched and correct; setup verified end-to-end via opt-in path; real provider returned 200 with full structured output; all G2/G5 evidence captured; inspector cleared 13/13.

## Next safe step

`COMMIT_AND_PUSH_DEV_REAL_DEEPSEEK_OPT_IN_LOCAL_INTEGRATION_ONLY` — commit exactly the DeepSeek-opt-in scope to `dev`, FF-push `dev` only, `main` untouched, no force.

- **Suggested commit message:** `feat: add real DeepSeek opt-in provider`
- **Accepted scope (5 new + 6 modified):**
  - new: `server/InsuranceAIPlatform.Services.AiAnalysis/Providers/RealDeepSeekAiProvider.cs`
  - new: `server/InsuranceAIPlatform.Services.AiAnalysis/Providers/DeepSeekChatModels.cs`
  - new: `server/InsuranceAIPlatform.Tests/RealDeepSeekProviderTests.cs`
  - new: `server/InsuranceAIPlatform.Tests/AiProviderSelectionTests.cs`
  - new: `docs/architecture/REAL_DEEPSEEK_OPT_IN_LOCAL_INTEGRATION_V0.1.md`
  - modified: `server/InsuranceAIPlatform.Services.AiAnalysis/AiProviderMode.cs` (DeepSeek enum value)
  - modified: `server/InsuranceAIPlatform.Services.AiAnalysis/Configuration/AiProviderOptions.cs` (Endpoint + TimeoutSeconds; no ApiKey)
  - modified: `server/InsuranceAIPlatform.Services.AiAnalysis/InsuranceAIPlatform.Services.AiAnalysis.csproj` (Microsoft.Extensions.* only)
  - modified: `server/InsuranceAIPlatform.Services.AiAnalysis/Orchestration/PersistenceAiAnalysisOrchestrator.cs` (provider-mode propagation; previously cosmetic limitation, now correct)
  - modified: `server/InsuranceAIPlatform.Api/Program.cs` (three-condition provider selection)
  - modified: `server/InsuranceAIPlatform.Api/appsettings.Development.json` (Mode + RealCallsEnabled + DeepSeek section; no key value)
- Per-run report dirs and other planning docs remain untracked (carry-forward pattern).

## What was NOT touched

- `main` branch — no merge / push / force.
- DevDept folder — untouched.
- Local secrets files — file existence checked in prior gate; **contents never read**.
- Tracked production code — zero changes this gate.
- No Azure / no provider SDK / no real PII / no payout / no customer messaging / no binary upload.
