# Microservice Service Skeletons — Implementation V0.1

**Gate:** `MICROSERVICE_SERVICE_SKELETONS_IMPLEMENTATION_MEGA_V0.1` · **Branch:** `dev` @ `9f494a1` (no commit in this gate) · **Date:** 2026-05-27
**Type:** bounded implementation — class-library skeletons only. No DB, no EF, no migrations, no write endpoints, no AI provider call, no Azure, no `src/**` change, no source commit/push.
**Status:** implemented; build + tests + frontend + smoke all PASS; forbidden scope confirmed clean.

## Status / purpose
Implement the accepted Option C plan: turn the six service boundaries behind the already-committed BFF / API Gateway (`9f494a1`) into **real compile-time boundaries** — per-service class libraries with interface + contracts + DI extension + health metadata, plus a thin `BuildingBlocks` shared kernel — registered in the BFF, in-process, with **no data, no persistence, no writes, no AI calls**. This makes later gates (read-ownership → persistence → write/events → AI) attach to a real owner rather than a monolith, without web-host or DB sprawl.

## Source state before implementation
- Branch `dev` @ `9f494a1` (`feat: add BFF API gateway skeleton`); `origin/dev` `9f494a1`; `origin/main` `69e6731` (untouched). Working tree clean except prior-gate planning docs (untracked).
- Solution: `server/InsuranceAIPlatform.Api` (BFF / API Gateway) + `server/InsuranceAIPlatform.Tests`. 13 preserved read routes + 2 additive read-only BFF endpoints; correlation middleware (`X-Correlation-Id`/`X-Trace-Id`/`X-Bff: api-gateway`); `IClaimReadService` → `InMemoryClaimReadService` (singleton). Build PASS, 22 tests PASS, frontend PASS.

## Chosen strategy implemented
**Option C — Hybrid: per-service class-library skeletons, in-process behind the BFF.** Six `Services.*` class libraries (each: public interface + forward-declaration contract marker(s) + DI registration extension + health metadata via a shared `IServiceHealthContributor`) + one thin `BuildingBlocks` primitives library. The BFF references all six service libraries and `BuildingBlocks`, registers each via its `Add<Service>ServiceSkeleton()` extension, resolves all six, and **additively** surfaces their readiness on `GET /api/bff/health`. No web hosts, no DB, no writes, no AI calls. **Read logic was deliberately NOT moved** into the services this gate (default per the gate); existing controllers keep delegating to `InMemoryClaimReadService`, so every preserved route is response-identical by construction.

## Projects created (7 new)
| Project | Kind | References |
|---|---|---|
| `server/InsuranceAIPlatform.BuildingBlocks` | class lib | none (internal) |
| `server/InsuranceAIPlatform.Services.Claims` | class lib | BuildingBlocks |
| `server/InsuranceAIPlatform.Services.CustomersPolicies` | class lib | BuildingBlocks |
| `server/InsuranceAIPlatform.Services.Documents` | class lib | BuildingBlocks |
| `server/InsuranceAIPlatform.Services.AiAnalysis` | class lib | BuildingBlocks |
| `server/InsuranceAIPlatform.Services.Approval` | class lib | BuildingBlocks |
| `server/InsuranceAIPlatform.Services.AuditCost` | class lib | BuildingBlocks |

All target `net9.0` (nullable + implicit usings). The six `Services.*` projects each reference one framework-abstraction package — `Microsoft.Extensions.DependencyInjection.Abstractions` 9.0.0 — required only so each library can ship its own `IServiceCollection` DI extension. No EF, no provider SDK, no HTTP client package. `BuildingBlocks` references no package.

## Dependency direction (enforced)
`InsuranceAIPlatform.Api` (BFF) → `Services.*` → `BuildingBlocks`.
- BFF references all six services + BuildingBlocks.
- Each service references **only** BuildingBlocks.
- Services do **not** reference one another (asserted by a reflection test).
- BuildingBlocks references no service and no API (asserted by a reflection test).
- No service references the BFF/API project.

## BuildingBlocks content (thin shared kernel — domain-free)
- `ServiceReadinessStatus` enum — `Ready` / `Stub` / `Deferred`.
- `ServiceHealthSnapshot` record — `(ServiceName, Status, Stage, Capabilities)`; synthetic, no PII.
- `ServiceNames` static — canonical service-name constants.
- `IServiceHealthContributor` — `ServiceHealthSnapshot GetHealth()`; the health-contributor abstraction every service implements.
No domain entities, no business DTOs, no DbContext, no Result/error envelope (omitted as unused) — kept intentionally minimal.

## Service skeleton contents
Each service ships: a public marker interface `I<Name>Service : IServiceHealthContributor` (with `ServiceName`), a sealed skeleton implementation returning a `ServiceHealthSnapshot` (readiness + capability tags only — no data, no behaviour), one or two id-only forward-declaration contract markers under `Contracts/` (e.g. `ClaimRef`, `PolicyRef`/`CustomerRef`, `DocumentRef`, `AiRunRef`, `ApprovalDraftRef`, `AuditTraceRef` — no business fields, no PII), and an `Add<Name>ServiceSkeleton()` DI extension.
- **Claims / CustomersPolicies / Documents / Approval / AuditCost** → readiness `Stub`.
- **AiAnalysis** → readiness `Deferred`; additionally ships `AiProviderMode` enum (`Mock` / `DeepSeekDisabled` / `Disabled`) and a non-implemented `IAiProvider` placeholder. The service reports `ProviderMode = Disabled` and `AdvisoryOnly = true`. **No `IAiProvider` implementation is registered**, no HTTP client, no SDK — so no AI/DeepSeek call is structurally possible; `DEEPSEEK_API_KEY` is never read.

## BFF DI wiring
`Program.cs` composition root calls the six `Add<Name>ServiceSkeleton()` extensions after the existing read-service registration. Each extension registers the service as its interface (singleton) and exposes the same instance as `IServiceHealthContributor`, so `IEnumerable<IServiceHealthContributor>` yields exactly six. Existing read routes and `InMemoryClaimReadService` are unchanged.

## Health / metadata behavior
`GET /api/bff/health` is **additively** extended: `BffHealthResponse` gains a trailing `Services` array (BFF-owned `ServiceReadinessInfo` DTO mapped from each service's snapshot — internal service types never leak to the frontend). All pre-existing identity fields (`service`, `status`, `stage`, `upstream`, `environment`, `correlationId`, `timestampUtc`) are unchanged, so the existing contract and tests are not broken. Live verification returned all six services (`ai-analysis-service`=Deferred, other five=Stub) with correlation headers intact.

## Tests added / updated
New `ServiceSkeletonTests.cs` (8 methods → 13 executed cases): (1) each of the six service interfaces resolves from BFF DI as an `IServiceHealthContributor` (Theory ×6); (2) exactly six distinct contributors registered; (3) readiness/capabilities are as designed; (4) AiAnalysis is `Disabled`+advisory-only and no `IAiProvider` is registered; (5) `/api/bff/health` additively lists six services without breaking identity fields; (6) no `Services.*` assembly references another (reflection); (7) BuildingBlocks references no service/API (reflection); (8) no EF referenced anywhere (reflection). Pre-existing suites unchanged and still green.

## Verification results
- **Backend build:** `dotnet build` solution — 9 projects, **0 warnings, 0 errors**.
- **Backend tests:** `dotnet test` — **35 passed / 0 failed / 0 skipped** (was 22; +13 executed cases).
- **Frontend build:** `npm run build` — **107 modules**, built ✓ (unchanged; no `src/**` edit).
- **Live smoke** (`http://localhost:5284`): `/health`, `/api/bff/health`, `/api/bff/demo-status`, `/api/claims`, `/api/claims/CLM-1006`, `/api/claims/summary`, `/api/claims/CLM-1006/{ai-evidence,approval,audit}`, `/api/demo/scenario`, `/api/system/demo-status` → all **200**; `X-Correlation-Id`/`X-Trace-Id`/`X-Bff: api-gateway` present; incoming correlation-id echoed; health body lists all six skeletons.

## Forbidden scope confirmation
No DB; no EF/`DbContext`/SqlClient; no migrations; no `.env`/`.db`/`.mdf`; **zero** write endpoints (`[HttpPost/Put/Patch/Delete]` = 0 matches); no AI/DeepSeek/OpenAI/Azure call; no HTTP client added to any service; `DEEPSEEK_API_KEY` never read/printed/logged (only named in "we never read this" doc-comments); no secrets; synthetic data only (`CLM-1006`); no Azure resources; no `src/**` change; **no source commit/push**; `main` untouched (`69e6731`); source HEAD still `9f494a1`.

## Deferred work (later gates, in order)
Stage 3: move read ownership into Claims/CustomersPolicies/Documents (response-identical). Stage 4: per-service persistence (schema-per-service DbContext + migrations). Stage 5: write/command endpoints + transactional outbox/events + audit append. Stage 6: AI Analysis provider (mock default; DeepSeek opt-in/disabled-by-default, isolated to that service). Azure mapping (Container Apps + per-service Azure SQL + Key Vault + App Insights) deferred throughout.

## Next gate
`COMMIT_AND_PUSH_DEV_MICROSERVICE_SERVICE_SKELETONS_ONLY` — commit exactly this skeleton scope to `dev` and fast-forward push `dev` only (separate, explicitly-authorized gate). Then later: `MICROSERVICE_SERVICE_READ_OWNERSHIP_*`.

## Stop boundaries
Implementation of skeletons only. No commit, no push, no DB, no EF, no migrations, no write endpoints, no DeepSeek/provider calls, no secrets, no Azure, no `main` change, no merge, no force push.
