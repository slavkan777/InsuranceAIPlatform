# BFF / API Gateway Skeleton — Implementation V0.1

**Gate:** `BFF_API_GATEWAY_SKELETON_IMPLEMENTATION_V0.1` · **Type:** implementation (BFF skeleton Stage 1 only) · **Branch:** `dev` @ `f8df2b6` (no commit in this gate)
**Status:** implemented — build PASS, tests 22/22 PASS, frontend build PASS, smoke PASS.

## Purpose
Implement the Stage-1 BFF / API Gateway skeleton accepted in `BFF_API_GATEWAY_SKELETON_PLANNING_V0.1.md`: a stable frontend-facing surface that preserves the current read contract, delegates to the current read logic, adds correlation/identity behavior, and owns no business data, no DB, and no AI provider. No service extraction, persistence, writes, AI, or Azure in this gate.

## Current state before implementation
Single .NET 9 ASP.NET Core read API (`server/InsuranceAIPlatform.Api`), in-memory `IClaimReadService` → `InMemoryClaimReadService` (singleton), 13 GET endpoints (10 `/api/claims`, 1 `/api/demo/scenario`, `/health`, `/api/system/demo-status`), 13 tests, error envelope `ApiErrorResponse { code, message, traceId }`, `ClaimId` `^CLM-\d{4}$`, golden `CLM-1006`, base `:5284`, CORS `:5173`. The thin-controller → read-service shape was already BFF-like (controllers own no data).

## Implementation shape chosen
**`current-api-as-bff`** (planning Option A/C — lowest risk). The existing `InsuranceAIPlatform.Api` project is marked and extended as the BFF / API Gateway boundary. **No new projects** were created (0 service projects), avoiding a premature split. Future internal services will be extracted behind this BFF in later gates.

Rationale: the current API is the only backend entrypoint and already delegates to a read-service seam; reshaping it in place satisfies Stage-1 with the smallest, reversible change and zero frontend impact.

## Routes preserved (contract stable — frontend unchanged)
All 13 pre-existing routes are untouched and still return 200: `/api/claims/summary`, `/api/claims`, `/api/claims/{claimId}`, `/api/claims/{claimId}/documents`, `/api/claims/{claimId}/ai-evidence`, `/api/claims/{claimId}/risks`, `/api/claims/{claimId}/policy`, `/api/claims/{claimId}/customer-vehicle`, `/api/claims/{claimId}/approval`, `/api/claims/{claimId}/audit`, `/api/demo/scenario`, `/health`, `/api/system/demo-status`. No frontend route or DTO change is required; backend mode continues to render through the same contract.

## BFF boundary implemented
- **BFF identity (Swagger/OpenAPI):** `Program.cs:18` doc title → `"InsuranceAIPlatform BFF / API Gateway"` with a description noting the gateway role + correlation behavior.
- **Additive read-only BFF endpoints** (`Controllers/BffController.cs`, route `api/bff`, `[HttpGet]` only — 2 endpoints):
  - `GET /api/bff/health` → synthetic identity `{ service: "bff-api-gateway", status: "healthy", stage: "skeleton-v0.1", upstream: "in-memory-read-service", environment, correlationId, timestampUtc }` (`Contracts/BffHealthResponse.cs`).
  - `GET /api/bff/demo-status` → passthrough synthetic `DemoStatusResponse` (`Backend: "BffGatewaySkeleton"`).
- These are purely additive; they do not replace or alter Surface-1 routes.

## Delegation model
Controllers (existing + BFF) delegate to `IClaimReadService` / framework services only. The BFF owns **no business data, no DbContext, no repository**. `BffController` injects only `IWebHostEnvironment`. This preserves the planning invariant: BFF aggregates/routes, services (later) own data.

## Correlation / trace behavior
`Middleware/CorrelationIdMiddleware.cs`, registered early in `Program.cs:51` (before CORS):
- reads incoming `X-Correlation-Id` (validated `^[A-Za-z0-9\-_]{8,64}$`); generates a new `Guid` if absent/invalid;
- stores it on `HttpContext.Items["CorrelationId"]` and tags `Activity.Current`;
- writes `X-Correlation-Id`, `X-Trace-Id`, and BFF identity header `X-Bff: api-gateway` on the response via `Response.OnStarting`;
- **no secrets read or logged**; minimal logging.

## Error model
The existing `ApiErrorResponse { code, message, traceId }` envelope is preserved unchanged (successful-response contracts untouched). Wiring the correlation id directly into every error `traceId` and a global exception-to-envelope handler are recorded as low-risk **next hardening tasks** (not done here to avoid changing current response behavior).

## Tests added/updated
`server/InsuranceAIPlatform.Tests/BffSkeletonTests.cs` — **8 new test methods** (`WebApplicationFactory<Program>`): `/api/bff/health` 200 + identity fields; `X-Correlation-Id` returned; incoming `X-Correlation-Id` echoed; `X-Bff` identity header present; preserved routes still 200 (`/health`, `/api/claims`, `/api/claims/CLM-1006`, `/api/demo/scenario`). No existing tests deleted or weakened. Suite total: **19 test methods → 22 executed test cases** (the pre-existing `ClaimsApiTests` has one `[Theory]` with 4 `InlineData` cases; 9 `ClaimsApiTests` + 2 `SystemControllerSmokeTests` methods pre-existing), all passing.

## Verification results
- Backend build: `dotnet build …Api.csproj` → exit 0, 0 warnings/errors.
- Backend tests: `dotnet test …Tests.csproj` → **Passed 22, Failed 0, Skipped 0**.
- Frontend build: `npm run build` (`tsc -b && vite build`) → exit 0, 107 modules.
- Smoke (API started locally on `:5284`, then stopped): `/health`, `/api/bff/health`, `/api/claims`, `/api/claims/CLM-1006`, `/api/demo/scenario` all 200; `X-Correlation-Id` present on each; an incoming `X-Correlation-Id: test-abc-123456` echoed back exactly.
- Independent review: 9/9 CLEARED (changeset scope, no write endpoints, no EF/DB, no AI/secret, no commit, preserved routes).

## What remains deferred (later gates)
- Internal service extraction (Claims, Customers & Policies, Documents, AI Analysis, Approval, Audit & Cost).
- Per-service persistence (schema-per-service), write/command endpoints, transactional outbox/events.
- AI Analysis Service (DeepSeek opt-in/disabled-by-default; mock default).
- Surface-2 composed views (`/api/bff/dashboard`, `/api/bff/claims/{id}/workspace`).
- Error-envelope/correlation hardening; auth at the BFF edge; Azure mapping.

## Next gate
`COMMIT_AND_PUSH_DEV_BFF_API_GATEWAY_SKELETON_ONLY_OR_MICROSERVICE_SERVICE_SKELETONS_PLANNING_V0.1` — either commit/push the accepted BFF skeleton to `dev` (commit-only gate), or proceed to planning the internal service skeletons. Both require a separate explicit gate prompt.

## Stop boundaries
No DB/EF/migrations/seed, no write endpoints, no service projects, no AI/DeepSeek, no Azure, no source commit/push, no `main` change, no secrets — all confirmed in this gate.
