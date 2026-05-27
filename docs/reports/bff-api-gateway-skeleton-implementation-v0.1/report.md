# BFF / API Gateway Skeleton Implementation — V0.1 — Report

**Gate:** `BFF_API_GATEWAY_SKELETON_IMPLEMENTATION_V0.1` · **Date:** 2026-05-27
**Type:** implementation (BFF skeleton Stage 1) — no DB/EF/migrations, no write endpoints, no service projects, no AI, no Azure, no source commit/push.

## Current state
- Branch `dev` @ `f8df2b6`; `origin/dev` `f8df2b6`; `origin/main` `69e6731` (unchanged). No commit created this gate.
- Implementation shape: **`current-api-as-bff`** — the existing `server/InsuranceAIPlatform.Api` is marked/extended as the BFF / API Gateway. **0 new projects.**

## Files changed (5, all under `server/**`, uncommitted)
- **M** `server/InsuranceAIPlatform.Api/Program.cs` — Swagger title/description → BFF identity (`:18`); register `CorrelationIdMiddleware` early (`:51`); `using` for middleware (`:1`).
- **+** `server/InsuranceAIPlatform.Api/Middleware/CorrelationIdMiddleware.cs` — accept/validate/generate `X-Correlation-Id`; write `X-Correlation-Id` + `X-Trace-Id` + `X-Bff: api-gateway`; store on `HttpContext.Items` + `Activity`; no secret logging.
- **+** `server/InsuranceAIPlatform.Api/Contracts/BffHealthResponse.cs` — identity record for `/api/bff/health`.
- **+** `server/InsuranceAIPlatform.Api/Controllers/BffController.cs` — route `api/bff`, **2 read-only `[HttpGet]`**: `health` (synthetic identity) + `demo-status` (synthetic passthrough). No write methods, no data ownership.
- **+** `server/InsuranceAIPlatform.Tests/BffSkeletonTests.cs` — 8 test methods (BFF health, correlation header returned + echoed, X-Bff header, 4 preserved routes still 200).

## Routes
- **Preserved (13, unchanged, frontend contract stable):** `/api/claims/summary`, `/api/claims`, `/api/claims/{claimId}` (+documents, ai-evidence, risks, policy, customer-vehicle, approval, audit), `/api/demo/scenario`, `/health`, `/api/system/demo-status`.
- **Added (2, additive read-only):** `GET /api/bff/health`, `GET /api/bff/demo-status`. Total GET endpoints now **15**.
- Frontend needs no route/DTO change; backend mode renders through the same contract (only base URL repoint, when adopted).

## BFF boundary / delegation
BFF (the API project) owns **no business data, no DbContext, no repository**; controllers delegate to `IClaimReadService`. `BffController` injects only `IWebHostEnvironment`. Future internal services extract behind this BFF later.

## Correlation / error
- `X-Correlation-Id` (validated `^[A-Za-z0-9\-_]{8,64}$` or generated `Guid`), `X-Trace-Id`, `X-Bff: api-gateway` on responses; stored on `HttpContext.Items` + `Activity`; no secrets logged.
- Existing `ApiErrorResponse { code, message, traceId }` preserved unchanged; deeper error-envelope/correlation wiring deferred as a low-risk hardening task.

## Verification (independently re-run by coordinator + independent Opus inspector 9/9 CLEARED)
- Backend build: `dotnet build …Api.csproj` → **exit 0**, 0 warnings/errors.
- Backend tests: `dotnet test …Tests.csproj` → **Passed 22, Failed 0, Skipped 0** (19 methods; one pre-existing `[Theory]`×4).
- Frontend build: `npm run build` (`tsc -b && vite build`) → **exit 0**, 107 modules.
- Smoke (API on `:5284`, started then stopped): `/health`, `/api/bff/health`, `/api/claims`, `/api/claims/CLM-1006`, `/api/demo/scenario` → all **200**; `X-Correlation-Id` present on each; incoming `X-Correlation-Id: test-abc-123456` echoed exactly.
- Safety: changeset = `server/**` only (no `src/**`, no `.csproj` package additions); no write verbs in source; no `DbContext`/EF; no AI/DeepSeek call; `DEEPSEEK_API_KEY` not read/logged; no secret value; HEAD unchanged `f8df2b6`; `main` `69e6731` untouched; no commit/push.

## What remains deferred
Internal service extraction; per-service persistence (schema-per-service); write/command endpoints + outbox/events; AI Analysis Service (DeepSeek opt-in/disabled-by-default; mock default); Surface-2 composed views; error/auth hardening; Azure mapping.

## Next safe step
`COMMIT_AND_PUSH_DEV_BFF_API_GATEWAY_SKELETON_ONLY_OR_MICROSERVICE_SERVICE_SKELETONS_PLANNING_V0.1` — either a commit-only gate to land this BFF skeleton on `dev`, or planning the internal service skeletons. Requires a separate explicit gate prompt.
