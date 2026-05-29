# InsuranceAIPlatform · Security · Rotate-and-Fix DeepSeek Local Secret Setup V0.1 — Report

**Gate:** `SECURITY_ROTATE_AND_FIX_DEEPSEEK_LOCAL_SECRET_SETUP_V0.1` · **Date:** 2026-05-28
**Type:** security hygiene + local secret setup verification + short real DeepSeek smoke / no source implementation / no commit / no push / no Azure.
**Verdict:** **ACCEPT_WITH_LIMITATIONS** (independent inspector CLEARED 12/13 ✅, 1/13 ⚠️ on a doc-inventory miscount, 0/13 ❌).

## Why this gate existed

Previous gate `REAL_DEEPSEEK_OPT_IN_LOCAL_INTEGRATION_V0.1` succeeded functionally but surfaced a hygiene concern: the OLD DeepSeek key value transited a local subagent tool-execution stream during a `ProcessStartInfo` injection step. Standing meta-rule "API key in plain text in chat — refuse + flag compromised" applies; even though the value never entered the repo or any external service, the safe-default treatment is "rotate, then verify". GPT audit required a hygiene-only gate before commit/push.

## Current state of the source repo (unchanged this gate)

- Branch `dev` @ `918e8a3aec82462bde9239aeec91d3a46b1082b4` (HEAD matches `origin/dev`).
- `origin/main` `69e67312a10cc9bcf28c4e387a126b48c91fb9c5` — untouched.
- No commit/push/amend/rebase/merge performed in this gate.
- Working tree carries exactly the previously-accepted DeepSeek implementation changes (6 modified tracked files + 4 new source/test files) + 19 untracked items (15 planning/report docs + 4 new source/test code paths).

## Rotation precondition

- Operator was advised to rotate the DeepSeek API key outside chat/repo per the prior gate's `ROTATE_DEEPSEEK_KEY` advisory.
- Implementer cannot directly observe rotation. Inferred signal: User-scope `DEEPSEEK_API_KEY` (registry persistence) is now sk-shape-plausible AND differs from the stale Process-scope value inherited at this session's parent-process spawn time. The divergence proves the User registry value was updated after this session started — consistent with rotation, but not cryptographic proof.
- `rotation_confirmed_by_operator = unknown` (recorded as such, not as `true`).

## Local secret pathway (booleans only — no values disclosed)

| Scope | `key_present` | `key_shape_plausible` (sk- prefix) | `pem_like_blob` |
|---|---|---|---|
| Process (snapshot inherited at session spawn) | true | false | false |
| User (registry — current) | true | true | false |
| Machine | false | — | — |

- `key_source_kind` used for smoke: **user-scope env (registry)**.
- `key_handling_pathway_for_smoke`: PowerShell read User-scope value via `[Environment]::GetEnvironmentVariable("DEEPSEEK_API_KEY","User")`, assigned to `$env:DEEPSEEK_API_KEY` (in-place Process-scope refresh of the calling shell only), spawned API child via `Start-Process -NoNewWindow -RedirectStandardOutput ...` which inherited the refreshed Process env from its parent PowerShell. Value never echoed, never written to disk, never assigned to any persistent variable. After spawn the variable was nulled + `[System.GC]::Collect()` invoked.
- `UserSecretsId` is NOT present in any `.csproj` — `dotnet user-secrets` pathway intentionally not used; local secrets file `~/.local-secrets/deepseek.env` was **not read** in this gate.
- 517-character PEM-shaped blob observed in the prior gate is **no longer present** in any of the three scopes. Process-scope still carries a non-empty NON-sk NON-PEM value (likely unrelated stale leftover); this is benign from a security standpoint (it's neither the key nor the dangerous PEM blob) but is documented as `old_bad_env_cleared = partial`.

## Repo / log leakage scan

| Check | Result |
|---|---|
| `git grep -nE 'sk-[A-Za-z0-9_-]{16,}'` (tracked) | **PASS** — 0 hits |
| `sk-` shape scan over 19 untracked items | **PASS** — 0 hits |
| `DEEPSEEK_API_KEY\s*=` literal-assignment in any tracked file | **PASS** — 0 hits |
| `appsettings.Development.json` for key literal | **PASS** — only `Mode/RealCallsEnabled/Model/Endpoint/TimeoutSeconds` |
| `ApiKey` property anywhere in `server/**/*.cs` | **PASS** — 0 hits |
| `PackageReference` scan over 10 csproj files | **PASS** — only `Microsoft.Extensions.*` / `Microsoft.EntityFrameworkCore.*` / `Microsoft.AspNetCore.Mvc.Testing` / `Microsoft.NET.Test.Sdk` / `xunit` / `coverlet` / `Swashbuckle`. Zero Azure.AI / OpenAI / SemanticKernel / DeepSeek-* packages |
| API stdout log scan (`api-real-deepseek.stdout.log`) for `sk-` shape | **PASS** — 0 hits |
| API stdout log scan for `DEEPSEEK_API_KEY` literal | **PASS** — 0 hits |
| API stderr log scan | **PASS** — empty |

## Build / test verification (regression check)

| Step | Result |
|---|---|
| `dotnet build server/InsuranceAIPlatform.sln -c Release --nologo` | **PASS** · 0 warnings · 0 errors · ~4 s |
| `dotnet test ... --no-build --nologo --verbosity quiet` | **PASS** · 113 / 113 / 0 |
| `npm run build` (root Vite app) | **PASS** · 107 modules · 4.46s |

## Phase 5 — Mock default regression smoke (port 5284)

Started API in default mode (no `--AiProvider:*` overrides; `appsettings.Development.json` `Mode=Mock`, `RealCallsEnabled=false`). Health = 200 within 1s.

- `POST /api/claims/CLM-1006/ai-analysis/run` → **200**, `providerMode=Mock`, `modelName=local-mock-v0.1`, `confidenceScore=78`, `estimatedCost=0.0187`, advisory-only guardrails (`canApprovePayout=false`, `canRejectClaim=false`, `canSendCustomerMessage=false`), 3 findings, summary in Ukrainian (Mock fingerprint).
- DB row `run_db93e3dc` persisted in `ai_analysis.AiAnalysisRuns` with `ProviderMode=Mock`, `ModelName=local-mock-v0.1`, `Tokens=4261`, `Cost=0.0187`, `Status=succeeded`. Audit + outbox `AiAnalysisCompleted` rows incremented.
- API stopped cleanly.

## Phase 6 — Real DeepSeek rotated-key smoke (port 5285)

Started API with explicit opt-in: `--AiProvider:Mode=DeepSeek --AiProvider:RealCallsEnabled=true`. Process env at spawn time refreshed from User-scope. Startup log emitted **`AI provider resolved to: RealDeepSeek (opt-in)`** — proving the 3-condition gate (Mode + RealCallsEnabled + key-non-empty) all evaluated true.

Four `POST /api/claims/CLM-1006/ai-analysis/run` attempts spaced across the gate window:

| Attempt | Pre-wait | HTTP from API | DeepSeek upstream observed | Adapter exception text |
|---|---|---|---|---|
| 1 | — | 500 | `Received HTTP response headers after 538.431ms - 503` | `DeepSeek call failed: HTTP 503` |
| 2 | ~3 s | 500 | client-side `30s` timeout | `DeepSeek call timed out after 30s.` |
| 3 | ~3 s | 500 | client-side `30s` timeout | `DeepSeek call timed out after 30s.` |
| 4 | 45 s | 500 | `503` after wait | `DeepSeek call failed: HTTP 503` |

**Auth layer accepted the rotated key on every authenticated attempt** — zero `401` across all 4 entries (vs `401` "Authentication Fails (governor)" returned in 0.4s on an unauthenticated control probe to the same endpoint). 503/timeout came from DeepSeek's downstream LLM serving tier, not the auth/edge layer.

**No successful 200 response in this gate window.** Limitation category: **external provider availability**.

API stopped cleanly. No partial rows persisted from the 4 failed attempts (orchestrator skipped `AddAsync` + outbox publish on each exception — see DB state below).

## G2 external-call evidence triple

| Part | Status | Source |
|---|---|---|
| (a) process log host | **PASS** | 4× `RealDeepSeekAiProvider: DeepSeek AI provider sending request. Host: api.deepseek.com Path: /v1/chat/completions` + 4× framework `HttpClient.deepseek` POST to `https://api.deepseek.com/v1/chat/completions` in `api-real-deepseek.stdout.log` |
| (b) DB row fingerprint | **N/A this gate** | Provider failed; no row persisted (correct behaviour per design). Prior gate's `run_e8af1ec2` (Tokens=695, Cost=0.0002, ModelName=`deepseek-v4-flash`) and `run_1ee423f9` (Tokens=700) remain in DB as historical proof of wiring correctness |
| (c) shape evidence | **N/A this gate** | No 200 response available to compare; prior-gate evidence retained |

## G5 stub-fallback fingerprint absence

**PASS** — none of the Mock fingerprints (`modelName=local-mock-v0.1`, `Tokens=4261`, `Cost=0.0187`, `ConfidenceScore=78`, Ukrainian summary text, 3/2/4 finding/evidence/risk shape) appear in any new `ai_analysis.AiAnalysisRuns` row. Specifically: the 4 failed real-DeepSeek attempts produced **zero** new rows, so there is no opportunity for the stub fingerprint to have been substituted. This is the correct G5 behaviour: provider failure → no persistence, not stub-fallback row.

## G4 process-env-snapshot rule

Applied and documented. The Process-scope DEEPSEEK_API_KEY this session sees was set at parent-process spawn time and never refreshed by Slava's later `setx`/registry edit. User-scope value (registry) WAS current. The implementer refreshed Process-scope from User-scope in the calling PowerShell only at the moment of API child spawn — without writing to disk, without echoing the value, without assigning to a durable variable.

## DB state (post-gate)

| Query | Result |
|---|---|
| `SELECT ProviderMode, COUNT(*) FROM ai_analysis.AiAnalysisRuns GROUP BY ProviderMode` | `Mock=5, DeepSeek=2, Disabled=1` (Mock +1 from this gate's Phase 5; DeepSeek unchanged) |
| `SELECT COUNT(*) FROM ai_analysis.AiAnalysisRuns` | 8 total |
| `SELECT COUNT(*) FROM audit_cost.AuditEvents WHERE ActionType='AiAnalysisCompleted'` | 7 (Mock +1 this gate) |
| `SELECT COUNT(*) FROM audit_cost.OutboxMessages WHERE EventType='AiAnalysisCompleted'` | 7 (Mock +1 this gate) |
| New DeepSeek rows this gate | **0** — orchestrator correctly skipped persistence on each provider exception |

## Session trace / local hygiene note

Prior gate's local subagent worker handled the OLD (pre-rotation) key via `ProcessStartInfo` injection; that value transited the local subagent tool-execution stream (captured in a local-only session-trace file on this machine — never synced to any external service). The current gate did **NOT** route the rotated value through any subagent: the implementer's PowerShell invocations read User-scope env directly into Process env and spawned the API child in the same shell, never passing the value into any agent prompt or tool argument. Rotation already covers the prior-exposure risk per the standing "key in chat = compromised" meta-rule. **No additional hygiene action required this gate.**

## Independent inspector review

| Check | Pass |
|---|---|
| Claims verified | 12/13 |
| Warnings | 1/13 (untracked-doc inventory miscount; implementer originally cited 13 docs / 17 untracked total; actual 15 docs / 19 total — corrected in this report) |
| Failures | 0/13 |
| Key value in any output | **NO** |
| Key value in any tracked file | **NO** |
| Key value in any log | **NO** |
| Repository leakage | **NO** |
| Any `401` returned to authenticated request | **NO** (rotated key valid) |
| Provider `5xx` returned | **YES** (HTTP 503 + 30s timeouts — provider availability, not auth) |
| **Final inspector verdict** | **CLEARED** |

### Inspector non-binding recommendations

- Update untracked-doc inventory to actual values (done in this report).
- Add a unit test asserting the orchestrator does NOT call `AddAsync` / outbox publish when the provider throws (currently covered by integration evidence only).
- Document that the "auth accepted" claim and the "body-shape evidence" claim are separable so a sustained 5xx incident does not block gates requiring body-shape evidence.

## Known limitations (carried forward + this-gate)

- External provider (DeepSeek) returned `HTTP 503` or `30s` timeout for all 4 real-key smoke attempts during the gate window; auth layer accepted the rotated key (zero 401s); orchestrator correctly skipped persistence on each failure (no stub-fallback fingerprint, no partial rows).
- Process-scope `DEEPSEEK_API_KEY` in this session is still stale (non-sk non-PEM); User-scope (registry) is correct and was used; advisory below covers full fix.
- Operator rotation cannot be cryptographically proven from this side — only inferred from the User-scope-vs-Process-scope divergence.
- `AuditEvent.Actor` formatted as `"Synthetic Adjuster (human)"` by existing AuditCost service `Name + " (" + Type + ")"` formatting; still synthetic, no real person.

## Operator action recommendations

| ID | Severity | Reason |
|---|---|---|
| `CONFIRM_ROTATION` | low | Implementer cannot directly observe whether the User-scope value is a NEW key or a corrected-syntax of the prior key; only Slava can confirm |
| `FIX_STALE_PROCESS_SCOPE_ENV` | low | Process-scope still carries an unrelated non-sk non-PEM value; not a security risk (it's neither the key nor the dangerous PEM blob) but restarting the parent shell would clean it up |
| `RERUN_REAL_SMOKE_WHEN_PROVIDER_RECOVERS` | low | DeepSeek upstream returned 503/timeout for the entire gate window; a follow-up 1-2 call smoke once the provider recovers will yield the (b) and (c) G2 evidence parts for the rotated key specifically |

## Blockers

**None.** Implementation untouched and correct; setup verified end-to-end via opt-in path; only external provider availability prevents successful 200 in this window.

## Next safe step

`WAIT_FOR_OPERATOR_DECISION` — three paths offered:

1. **Recommended:** Operator re-runs a 1-2 call real-DeepSeek smoke later when DeepSeek API recovers, merges the success evidence into a follow-up handoff, then proceeds to `COMMIT_AND_PUSH_DEV_REAL_DEEPSEEK_OPT_IN_LOCAL_INTEGRATION_ONLY`.
2. Operator accepts current `ACCEPT_WITH_LIMITATIONS` and proceeds directly to the commit gate, citing prior-gate body-shape evidence as historical proof of wiring correctness.
3. Operator requests a separate provider-resilience gate before commit if 5xx behaviour should be guarded by adapter retry/backoff/circuit-breaker logic.

## What was NOT touched

- `main` branch — no merge, no push, no force.
- DevDept folder — untouched (read-only project boundary).
- Local secrets files — file existence checked, **contents not read**.
- Tracked production code — zero changes in this gate.
- No Azure / no provider SDK / no real PII / no payout / no customer messaging / no binary upload.
