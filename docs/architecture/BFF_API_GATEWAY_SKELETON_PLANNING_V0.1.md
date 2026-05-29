# BFF / API Gateway Skeleton — Planning V0.1

**Gate:** `BFF_API_GATEWAY_SKELETON_PLANNING_V0.1` · **Type:** planning-only (no code, no DB, no AI, no commit/push)
**Branch:** `dev` · **Status:** planned

## Purpose

Plan the BFF / API Gateway — the first microservice-aligned backend boundary — so the React frontend keeps **one stable API surface** while the backend evolves from today's single read API into the accepted 7-service map. The BFF hides internal service topology, aggregates reads, routes commands, normalizes errors, and attaches correlation IDs. Azure later maps these boundaries instead of forcing a rewrite.

This gate produces planning artifacts only. No BFF code, no service projects, no DB/EF/migrations, no AI calls, no commit/push.

## Source of truth

- Accepted architecture: `docs/architecture/MICROSERVICE_ARCHITECTURE_CORRECTION_BEFORE_DB_WRITE_AI_V0.1.md`, `MICROSERVICE_SERVICE_BOUNDARIES_V0.1.md`, `MICROSERVICE_LOCAL_GATE_SEQUENCE_V0.1.md`.
- Frontend seam: `docs/architecture/MOCK_API_BOUNDARY_V0.1.md`, `FRONTEND_BACKEND_CONTRACT_READINESS_V0.1.md`, `FRONTEND_ARCHITECTURE_V0.1.md`.
- AIKB decision: `DECISION_2026-05-27_azure_ready_microservice_architecture_local_first.md`.
- Route inventory: `BFF_API_GATEWAY_ROUTE_CONTRACT_MAP_V0.1.md` (companion doc).

## Current state (inspected on `dev` @ `f8df2b6`)

**Frontend** (`src/`): 11 routes/screens (React + TS + Vite + Redux Toolkit + Redux-Saga). API access is a single facade `src/api/insuranceApi.ts` that selects, at module load, between `mockInsuranceApi` (default) and `backendInsuranceApi` based on `VITE_INSURANCE_API_MODE` (`mock` | `backend`). The backend client (`backendInsuranceApi.ts`) is **GET-only**, base URL `VITE_INSURANCE_API_BASE_URL` (default `http://localhost:5284`), and on error throws `BackendApiError` — the saga catches it and sets `apiMode: 'mock-fallback'` in state (NOT silently swapped). 15 api functions; 3 are deferred writes/stubs (`runMockAiAnalysis`, `saveApprovalDraft`, `sendCustomerRequest`).

**Backend** (`server/InsuranceAIPlatform.Api`): a single .NET 9 ASP.NET Core read API, in-memory store, **13 GET endpoints, all read-only, zero write endpoints**:
- `ClaimsController` (`[Route("api/claims")]`): `summary`, `` (list), `{claimId}`, `{claimId}/documents`, `{claimId}/ai-evidence`, `{claimId}/risks`, `{claimId}/policy`, `{claimId}/customer-vehicle`, `{claimId}/approval`, `{claimId}/audit` (10);
- `DemoController` (`[Route("api/demo")]`): `scenario` (1);
- `HealthController`: `/health` (1); `SystemController` (`[Route("api/system")]`): `demo-status` (1).
- Backing service: `IClaimReadService` → `InMemoryClaimReadService` registered `AddSingleton` at `Program.cs:9`; no DB. 13 DTOs under `Contracts/**`; uniform error envelope `ApiErrorResponse { code, message, traceId }`. 13 tests (11 `ClaimsApiTests` + 2 `SystemControllerSmokeTests`); `ClaimId` validated by `^CLM-\d{4}$`; `CLM-1006` is the golden claim (only fully-seeded id).
- CORS allows `http://localhost:5173` (Vite dev). (Earlier docs approximated "11 GET / 14 tests"; the exact current counts are 13 GET / 13 tests.)

This single read API is the **starting point the BFF reshapes** — it is not the accepted end-state shape.

## Why BFF is the next gate

Per the accepted microservice target, the frontend must talk to **one** surface and never to internal services directly. Introducing the BFF first (before any DB / service implementation / persistence) establishes that boundary while the current read API still serves the UI, so every later step (service skeletons → per-service persistence → write/events → AI Analysis) happens *behind* the BFF without breaking the frontend. `SQLSERVER_EFCORE_PERSISTENCE_IMPLEMENTATION_V0.1` and a single `DOTNET_BACKEND_SKELETON_PLANNING` are superseded by this sequence.

## BFF responsibilities

- Expose a stable, frontend-facing API surface (preserve current contracts).
- Aggregate reads across internal services into screen-shaped view models.
- Route commands to the owning service (without making the business decision).
- Hide internal service topology and ports.
- Normalize errors into one envelope; attach correlation/trace IDs.
- Apply partial-failure / fallback policy so a screen degrades gracefully.
- Later: own the edge concerns — auth, CORS, rate-limit, request size.
- Emit audit/observability hooks (cross-service governance trace).

## BFF non-goals (what BFF must NEVER own)

- No business-state ownership; no database; no EF/migrations.
- No direct cross-service DB access.
- No AI provider integration; **never** calls DeepSeek; never reads `DEEPSEEK_API_KEY`.
- No long-running AI workflow logic.
- No claim-lifecycle decision authority; no human-approval authority; no audit-storage authority.
- No payout/finalization decisions.

**Invariant:** Frontend → BFF only. The frontend must not call Claims / Customers & Policies / Documents / AI Analysis / Approval / Audit & Cost services directly.

## Current frontend seam (to preserve)

The frontend's `backendInsuranceApi.ts` calls fixed routes under `/api/claims/...`, `/api/demo/scenario`. **The BFF v1 surface is exactly these routes.** Migration therefore requires only repointing `VITE_INSURANCE_API_BASE_URL` from the read API to the BFF — zero change to frontend code, DTOs, routes, mock fallback, or safe-button honesty.

## Migration options considered

- **Option A — rename current read API into the BFF facade.** Lowest files touched, but conflates "gateway" with "claims read logic" and makes later service extraction messy (the gateway would own read logic it should delegate). Rejected as the end shape.
- **Option B — add a new BFF project that delegates to the current read service/API; keep the current read API logic as the temporary backing for not-yet-extracted services.** Frontend repoints base URL only. Internal services are extracted behind the BFF gradually. **Chosen.**
- **Option C — add BFF as a new project and immediately treat the current read API as an internal "Claims-ish" service over HTTP.** Correct end shape but introduces an extra hop and a second running process before any service split exists — premature for a skeleton. Adopt incrementally *within* Option B (Stage 3).

## Chosen migration path

**Option B, staged.** 
- **Stage 1 (next gate):** new `BFF` project exposes the current `/api/claims/...` + `/api/demo/scenario` routes verbatim and **delegates** to the existing read logic (in-process adapter over `IClaimReadService`, or an internal HTTP call to the current API). Frontend unchanged except base URL. Add health + correlation-ID middleware.
- **Stage 2:** introduce empty service skeleton projects (Claims, Customers & Policies, Documents, AI Analysis, Approval, Audit & Cost) behind the BFF; BFF begins routing per-area calls to the matching skeleton (still returning the same shapes).
- **Stage 3:** per-service persistence (schema-per-service); the read logic in `InMemoryClaimReadService` migrates into the owning services; BFF calls services over HTTP with an anti-corruption layer.
- **Stage 4:** write/commands + transactional outbox/events + audit.
- **Stage 5:** AI Analysis Service (mock default; DeepSeek opt-in/disabled-by-default).

Rule: **preserve current frontend API contracts first; introduce internal services behind the BFF gradually; never rewrite the frontend just to satisfy microservice shape.**

## BFF endpoint groups

Two surfaces:

**Surface 1 — preserved passthrough (v1, contract-stable, frontend unchanged):** the existing routes, served by the BFF, delegating to the current read logic:
`GET /api/claims/summary`, `GET /api/claims`, `GET /api/claims/{claimId}`, `GET /api/claims/{claimId}/documents`, `GET /api/claims/{claimId}/ai-evidence`, `GET /api/claims/{claimId}/risks`, `GET /api/claims/{claimId}/policy`, `GET /api/claims/{claimId}/customer-vehicle`, `GET /api/claims/{claimId}/approval`, `GET /api/claims/{claimId}/audit`, `GET /api/demo/scenario`. Plus gateway infra: `GET /health` (BFF own), and a passthrough/aggregate of `GET /api/system/demo-status`.

**Surface 2 — new aggregation (opt-in, additive, never removes Surface 1):** composed view models the frontend may adopt later, e.g. `GET /api/bff/dashboard` (summary + queue + today's audit counters) and `GET /api/bff/claims/{claimId}/workspace` (claim core + customer/vehicle + policy + ai summary + audit ids in one call). These reduce round-trips once services are split; until adopted, the frontend keeps using Surface 1.

Per-endpoint mapping (screen, current source, future internal service, aggregation type, read/command, DTO, fallback) is in the companion `BFF_API_GATEWAY_ROUTE_CONTRACT_MAP_V0.1.md`.

## Read aggregation model (per screen)

| # | Screen | BFF returns | Internal service(s) later | During transition | Partial-failure behavior |
|---|---|---|---|---|---|
| 1 | Dashboard | counters + queue | Claims (queue/counts), Audit & Cost (processedToday, aiRunning) | current read service | render queue even if counters degrade; `warnings[]` |
| 2 | Claims List | claim rows | Claims | current read service | 200 + empty/cached on partial; error envelope on total failure |
| 3 | Claim Workspace | composed claim detail | Claims (core) + Customers & Policies (customer/vehicle/policy summary) + AI Analysis (ai summary) + Audit (trace ids) | current read service (single DTO) | claim core is critical (404 if missing); ai/audit blocks optional → `warnings[]` |
| 4 | Documents/Photos | checklist + photos | Documents | current read service | render with "evidence unavailable" note on partial |
| 5 | AI Evidence | findings/evidence/confidence/entities | AI Analysis | current read service | advisory → degrade to "AI analysis unavailable", never block claim |
| 6 | Risks | score/threshold/factors/pipeline | AI Analysis (advisory) + Claims (deterministic checks) | current read service | deterministic checks authoritative; advisory optional |
| 7 | Policy/Coverage | coverage blocks + validation | Customers & Policies | current read service | validation is deterministic; show stale-safe on partial |
| 8 | Customer/Vehicle | customer + vehicle + history | Customers & Policies | current read service | history optional → `warnings[]` |
| 9 | Human Approval | draft + options + ai recommendation | Approval (+ AI Analysis for recommendation) | current read service | options/draft critical; ai recommendation optional & labeled advisory |
| 10 | Audit/Cost | run trace + cost distribution | Audit & Cost | current read service | append-only; show partial trace on degrade |
| 11 | Demo Scenario | steps + golden claim id | BFF demo config (synthetic) / Claims for golden id | current read service | static synthetic; safe default |

## Command routing model (planning only — no write endpoints implemented)

Derived from the frontend's deferred / safe-button actions. BFF **routes** commands; it does not decide.

| Frontend action | Future BFF command endpoint (candidate) | Owning service | Validation | Audit event | Idempotency | Forbidden |
|---|---|---|---|---|---|---|
| Save approval draft | `PUT /api/claims/{id}/approval/draft` | Approval | Approval Service | `ApprovalDraftSaved` | claimId+draftHash | AI cannot author final decision |
| Submit human approval | `POST /api/claims/{id}/approval/submit` | Approval | Approval (deterministic transition) | `ApprovalSubmitted` | idempotency-key | human-only; AI never approves/rejects/finalizes |
| Request missing document / photo | `POST /api/claims/{id}/documents/requests` | Documents | Documents | `DocumentRequested` | claimId+docKey | no real SMS/email locally (draft only) |
| Confirm document | `POST /api/claims/{id}/documents/{docId}/confirm` | Documents | Documents | `DocumentConfirmed` | docId+state | — |
| Import/upload metadata | `POST /api/claims/{id}/documents` (metadata) | Documents | Documents | `DocumentAdded` | upload-key | metadata only; no file bytes in skeleton |
| Run AI analysis | `POST /api/claims/{id}/ai-analysis/runs` | AI Analysis | AI Analysis (advisory) | `AiRunStarted/Completed` | run-key | mock default; DeepSeek opt-in/disabled-by-default |
| Generate customer draft | `POST /api/claims/{id}/ai-analysis/customer-draft` | AI Analysis | AI Analysis | `CustomerDraftGenerated` | claimId+draftKey | draft only until human-approved |
| Create case | `POST /api/claims` | Claims | Claims | `ClaimCreated` | client-key | synthetic data only |
| Export / Export CSV | `GET /api/bff/.../export` (read-only generation) | BFF compose (read) | — | `ExportGenerated` | — | no write; no PII |

Rules: BFF routes commands but owns no business decision; AI is advisory-only and never approves/rejects/finalizes/changes status; customer messages are drafts unless human-approved; no real SMS/email in local demo; every command emits an audit event via Audit & Cost.

## DTO / contract boundary

- **Public BFF contracts** = the shapes the frontend already consumes (`ClaimRow`/`ClaimListItemDto`, `ClaimDetail`/`ClaimDetailsDto`, documents, ai-evidence, risks, policy, customer-vehicle, approval, audit, demo). For v1 the BFF public DTOs are **identical** to the current ones to preserve the contract. Composed views (`DashboardView`, `ClaimWorkspaceView`) are additive.
- **Internal service DTOs** are separate and never exposed directly to the frontend.
- **Anti-corruption layer:** BFF holds mappers `currentReadDto → BFF public DTO` (Stage 1, identity/thin) and later `internalServiceDto → BFF public DTO` (Stage 3). The frontend never depends on internal service shapes.
- **Versioning:** keep `/api/claims/...` stable; introduce a version segment only if a breaking change is unavoidable; composed views live under `/api/bff/...`.
- **Backward-compat rule:** a Surface-1 route's response shape must not change without a contract test failing first.

## Error model

- Reuse the existing envelope `ApiErrorResponse { code, message, traceId }` for all BFF errors.
- Retryable (internal service 502/503/timeout) vs non-retryable (400 validation, 404 not found). `ClaimId` keeps `^CLM-\d{4}$` validation → 400 on malformed, 404 on unknown.
- **Partial aggregation:** composed reads return 200 with a `warnings[]` array when a non-critical sub-call fails (e.g., AI summary), and only fail hard when the critical entity (the claim) is missing.

## Correlation / observability

- Inbound: accept/create `X-Correlation-Id`; generate `X-Trace-Id` per request; propagate both to every internal call.
- Response headers (non-secret only): `X-Correlation-Id`, `X-Trace-Id`, `X-Demo-Mode`, and `X-Provider-Mode` carrying only the **mode name** (`mock` | `disabled` | `deepseek`) — never a key or secret.
- Structured logs include service name + correlation/trace id. Audit hooks emit a governance trace to Audit & Cost. Azure later → Application Insights / Log Analytics.

## Auth / security placeholder (planning only)

- No production auth locally; demo reviewer identity is synthetic. No real PII. No secrets in repo; no `.env` with real values.
- Future: BFF owns the edge auth boundary; internal services trust the BFF only; Azure → edge auth + managed identity + Key Vault for secrets; `DEEPSEEK_API_KEY` lives only in env/user-secrets, used solely inside the AI Analysis Service.

## Local development topology (staged; no implementation now)

- **Stage 1:** one solution; `BFF` project + current read logic (delegation); BFF on a local port (e.g. `:5284` taking over the frontend's existing base URL, or `:5180` with the base URL repointed); Swagger on the BFF; `GET /health`.
- **Stage 2:** add service skeleton projects (own ports, own Swagger, own health); BFF routes to them.
- **Stage 3:** per-service persistence (schema-per-service). **Stage 4:** events/outbox.
- Docker Compose is documented as a **later option only**, not part of the skeleton.

## Testing strategy (for the implementation gate)

- BFF route smoke tests: every Surface-1 route returns the same shape as today.
- Contract tests pinning the current frontend DTOs (fail on drift).
- Aggregation tests for composed views; partial-failure / fallback tests; correlation-ID propagation tests.
- Static scan: frontend calls only the BFF base URL (no direct internal-service URLs).
- No DB in BFF tests; no AI provider call; secrets scan in CI.
- Preserve the current backend tests (13) and the frontend build (`tsc -b && vite build`).

## Risks / mitigations

| Risk | Mitigation |
|---|---|
| Overengineering / premature complexity | Stage 1 is a thin facade; services added only when needed |
| BFF becomes a new monolith | BFF owns no data/logic — routing + aggregation + mapping only; enforced by non-goals + tests |
| DTO drift / frontend contract break | Surface-1 routes preserved; contract tests gate changes |
| Service boundary confusion | ownership table in `MICROSERVICE_SERVICE_BOUNDARIES_V0.1.md` + route map |
| Premature DB split | persistence is Stage 3, after skeletons; schema-per-service, not DB-per-service first |
| Distributed-transaction trap | outbox/events for cross-service facts; no 2PC; sync HTTP only for read-aggregation/routing |
| Logging / secrets leakage | `X-Provider-Mode` is a mode name only; never log keys; secrets scan |
| Azure cost | Azure is late; provisioned only after local completion + owner checkpoint |
| DeepSeek call leakage | DeepSeek isolated in AI Analysis Service; BFF never calls it; never reads `DEEPSEEK_API_KEY` |

## Next implementation gate

`BFF_API_GATEWAY_SKELETON_IMPLEMENTATION_V0.1` (Stage 1 only):
- Create/shape the BFF skeleton project; preserve current frontend calls; delegate to current read API/read service/stub as planned.
- Add `GET /health`; add correlation-ID middleware if safe; map the read-only Surface-1 routes.
- **Forbidden:** no DB; no service projects unless explicitly in scope; no write behavior; no AI provider calls; no Azure; no commit/push unless a separate gate authorizes it.
- **DONE:** BFF builds and serves the Surface-1 routes returning the current shapes; frontend works against the BFF base URL with no code change; backend tests + frontend build still pass; route smoke + correlation-ID tests added.
- **Verification:** build output, route smoke output, frontend build pass, changeset confined to the BFF project + tests.
- **Report:** local report + sanitized gpt-handoff. **Next after:** service skeleton planning.

## Stop boundaries

Planning only. No BFF/service code, no controller/frontend changes, no `src/**` or `server/**` implementation changes, no package/lock, no DB/EF/migrations/seed, no write endpoints, no DeepSeek/AI, no Azure, no commit/push, no `main` change, no secrets.
