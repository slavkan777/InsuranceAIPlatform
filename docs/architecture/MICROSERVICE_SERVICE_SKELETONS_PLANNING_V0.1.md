# Microservice Service Skeletons — Planning V0.1

**Gate:** `MICROSERVICE_SERVICE_SKELETONS_PLANNING_V0.1` · **Type:** planning-only (no code, no projects, no DB/EF, no AI, no Azure, no source commit/push) · **Branch:** `dev` @ `9f494a1`
**Status:** planned.

## Purpose
Plan the internal service skeletons that will sit behind the already-committed BFF / API Gateway (`9f494a1`). Define the exact, lowest-risk shape the next implementation gate will create so the six accepted services become real boundaries without breaking the frontend, without a database, without web-host sprawl, and without AI/Azure.

## Source of truth
- Accepted architecture: `MICROSERVICE_ARCHITECTURE_CORRECTION_BEFORE_DB_WRITE_AI_V0.1.md`, `MICROSERVICE_SERVICE_BOUNDARIES_V0.1.md`, `MICROSERVICE_LOCAL_GATE_SEQUENCE_V0.1.md`.
- BFF: `BFF_API_GATEWAY_SKELETON_PLANNING_V0.1.md`, `BFF_API_GATEWAY_ROUTE_CONTRACT_MAP_V0.1.md`, `BFF_API_GATEWAY_SKELETON_IMPLEMENTATION_V0.1.md`.
- AIKB decision: `DECISION_2026-05-27_azure_ready_microservice_architecture_local_first.md`.
- Companion: `MICROSERVICE_SERVICE_SKELETONS_CONTRACT_MAP_V0.1.md`.

## Current state after BFF commit
`dev` @ `9f494a1` (`feat: add BFF API gateway skeleton`). The existing `server/InsuranceAIPlatform.Api` is the BFF / API Gateway (`current-api-as-bff`, 0 new projects): 13 preserved read routes + 2 additive read-only BFF endpoints (`/api/bff/health`, `/api/bff/demo-status`), correlation middleware (`X-Correlation-Id`/`X-Trace-Id`/`X-Bff`), delegating to `IClaimReadService` → `InMemoryClaimReadService` (singleton). Solution: `server/InsuranceAIPlatform.Api` + `server/InsuranceAIPlatform.Tests` + `InsuranceAIPlatform.sln`. Build PASS, 22 tests PASS, frontend PASS. `origin/main` `69e6731` untouched.

## Why service-skeleton planning is next
The BFF boundary exists but everything behind it is still one in-memory read service. Before persistence, writes, or AI, the six service boundaries must become explicit so later gates (persistence → write/events → AI) attach to a real owner, not a monolith. Doing this as a skeleton first (contracts + interfaces + DI + health, no data) keeps the boundaries honest and Azure-mappable while avoiding premature complexity.

## Chosen skeleton strategy
**Option C — Hybrid: per-service class-library skeleton projects, in-process, behind the BFF.** Each of the six services becomes its own .NET class-library project exposing a service interface + public contract records + a DI registration extension + health metadata. No separate web hosts, no controllers, no DB in this skeleton. The BFF references the service interfaces and resolves them via DI; each skeleton implementation initially delegates to the current read logic (`InMemoryClaimReadService`) so the preserved routes keep working unchanged. Web-API split (separate hosts) is deferred until persistence/HTTP gates actually need it.

Why this is the right amount: it creates **real compile-time boundaries and dependency direction** (interview-credible, Azure-mappable as one library → one Container App later) while avoiding six running web hosts, six Swaggers, and inter-process HTTP before there is any data or write to justify them.

## Alternative strategies considered
- **Option A — six web-API projects now.** Correct end shape but premature: six hosts/ports/Swaggers and in-process→HTTP wiring before any persistence or write exists. Over-engineering; higher local + token cost; rejected for the skeleton gate (adopted incrementally at the persistence/HTTP gate).
- **Option B — folders/modules inside the current API.** Lowest overhead but weak boundary signal: nothing prevents cross-module coupling or the BFF owning business logic; poor microservice/interview story. Rejected.
- **Option C — hybrid class libraries (chosen).** Real project boundaries + DI seam + clean dependency direction, no host sprawl, no DB. Best fit for "Azure-ready, local-first, no overbuild."

## Service responsibility map
Full per-service detail (owns / does-not-own / artifacts / contracts / future) is in `MICROSERVICE_SERVICE_SKELETONS_CONTRACT_MAP_V0.1.md`. Summary:
1. **Claims** — claim queue, claim detail, claim lifecycle, deterministic status rules.
2. **Customers & Policies** — customers, vehicles, policies, coverage validation; owns the future 200 synthetic test users; does not own claims.
3. **Documents** — document/photo metadata, missing-evidence detection; future upload-metadata + extraction boundary; no blob/Azure now.
4. **AI Analysis** — future DeepSeek adapter behind `IAiProvider` (mock default; no real call in skeleton); AI run/confidence/evidence/cost/token contracts later; advisory-only guardrails.
5. **Approval** — approval drafts, human-controlled decision, deterministic transition requests; no autonomous AI decision; no payout.
6. **Audit & Cost** — append-only audit, token/cost traces, cross-service correlation/governance trace.

## Project / folder structure plan (for the next implementation gate)
```
server/
  InsuranceAIPlatform.Api/                       # BFF / API Gateway (existing; unchanged role)
  InsuranceAIPlatform.BuildingBlocks/            # NEW class lib: primitives ONLY (ServiceNames, correlation accessor,
                                                 #   error envelope, health-contributor abstraction, Result). NO domain.
  InsuranceAIPlatform.Services.Claims/           # NEW class lib: IClaimsService + contracts + DI ext + health
  InsuranceAIPlatform.Services.CustomersPolicies/
  InsuranceAIPlatform.Services.Documents/
  InsuranceAIPlatform.Services.AiAnalysis/
  InsuranceAIPlatform.Services.Approval/
  InsuranceAIPlatform.Services.AuditCost/
  InsuranceAIPlatform.Tests/                     # existing; add skeleton/DI tests
  InsuranceAIPlatform.sln                        # add the 7 new projects
```
Namespaces mirror folders (`InsuranceAIPlatform.Services.Claims`, …, `InsuranceAIPlatform.BuildingBlocks`).

## Dependency direction
`InsuranceAIPlatform.Api` (BFF) → `Services.*` → `BuildingBlocks`. **Services.* never reference each other** (no cross-service compile dependency; cross-service data is id-only via contracts, introduced only when needed). `BuildingBlocks` references nothing internal. The BFF references all six service projects (to register + delegate) but holds no business data itself.

## Shared kernel policy
`BuildingBlocks` is a deliberately thin shared kernel: only cross-cutting primitives (service-name constants, correlation-id accessor, the `{ code, message, traceId }` error envelope, a health-contributor interface, a small `Result`/error type). **No domain entities, no DTOs with business meaning, no DbContext** ever live here — that prevents a shared "god model." If something is business-shaped, it belongs to a service's own contracts, not BuildingBlocks.

## Contracts policy
- Each service owns its **public contract records** inside its own project (e.g. `Services.Claims/Contracts/`). These are the in-process equivalent of its future API contract.
- **BFF-facing DTOs stay the BFF's** (the existing frontend shapes). The BFF maps service contracts → BFF public DTOs (anti-corruption); the frontend never sees internal service shapes.
- During the skeleton phase the service contracts may mirror the current read DTOs (thin) to keep delegation trivial; they diverge later as services gain ownership.
- Versioning: keep BFF public routes/DTOs stable; service contracts are internal and may evolve behind the BFF.

## BFF delegation plan (staged)
- **Stage 1 (current, committed):** BFF delegates to `InMemoryClaimReadService`.
- **Stage 2 (next impl gate):** BFF references `IClaimsService`, `ICustomersPoliciesService`, `IDocumentsService`, `IAiAnalysisService`, `IApprovalService`, `IAuditCostService`, registered via each service's DI extension; the skeleton implementations delegate to the current read logic so the preserved routes return identical responses. No endpoint/DTO change.
- **Stage 3:** read ownership gradually moves into the services (Claims/Customers/Documents own their read slices; the read service is decomposed).
- **Stage 4:** per-service persistence (schema-per-service, per-service DbContext + migrations).
- **Stage 5:** write/command endpoints + transactional outbox/events + audit.
- **Stage 6:** AI Analysis Service provider integration (DeepSeek opt-in/disabled-by-default; mock default).

## Health / DI / observability skeleton plan
- Each service ships a DI registration extension: `services.AddClaimsService()`, etc. The BFF composition root calls all six.
- Each service exposes a health contributor (from `BuildingBlocks`) reporting `{ service, status, stage }`; the BFF's `/api/bff/health` may aggregate the six skeleton statuses (safe, synthetic) — additive, no contract break.
- `ServiceNames` constants live in `BuildingBlocks`; logging category convention `InsuranceAIPlatform.Services.<Name>`; correlation id propagated from the existing middleware via a `BuildingBlocks` accessor. No external telemetry dependency added in this gate (App Insights is an Azure-later concern).

## Testing strategy (for the next implementation gate)
- Solution builds with the 7 new projects.
- DI resolution test: the BFF service provider resolves all six service interfaces.
- Preserved-route tests still pass (the 22 existing tests stay green; delegation is response-identical).
- Optional: BFF health lists the six internal skeleton services.
- Static guards: no `DbContext`/EF, no `[HttpPost/Put/Patch/Delete]`, no new packages beyond what skeletons need (ideally none / framework only), no AI provider call, no `src/**` change, secrets scan. Frontend build unchanged.

## Risks / mitigations
| Risk | Mitigation |
|---|---|
| Too many projects too early | 6 class libs (no hosts); no DB; thin contracts only |
| Fake microservices vs real boundaries | enforced dependency direction (Services.* never reference each other); BFF owns no data |
| Shared model leakage | BuildingBlocks = primitives only; business shapes live per-service |
| BFF becomes a god service | BFF only maps + delegates; service interfaces own behavior; tests assert delegation |
| Premature DB split | persistence is a later gate; skeleton has no DbContext |
| Service-host cost | no web hosts in skeleton; in-process DI |
| Azure cost | Azure deferred; class-lib → Container App is a later mapping |
| Token/context cost | one planning doc + map; implementation can be delegated to a worker with a tight brief |
| Frontend contract break | Stage-2 delegation is response-identical; preserved-route tests gate it |
| Hidden writes / AI leakage | no write endpoints, no provider calls in skeleton; static guards in tests |

## Next implementation gate
`MICROSERVICE_SERVICE_SKELETONS_IMPLEMENTATION_V0.1` — create `BuildingBlocks` + the six `Services.*` class-library projects (interfaces + contracts + DI extensions + health metadata), add them to the `.sln`, register them in the BFF, delegate to the current read logic (response-identical), add DI/health/preserved-route tests. **Forbidden:** no DB/EF/migrations, no write endpoints, no separate web hosts, no AI provider calls, no Azure, no `src/**` change, no source commit/push (separate gate). **DONE:** solution builds; BFF resolves all six service interfaces; preserved routes + 22 tests still pass; no forbidden scope. After it: `COMMIT_AND_PUSH_DEV_MICROSERVICE_SERVICE_SKELETONS_ONLY`.

## Stop boundaries
Planning only. No projects, no `.csproj`/`.sln` edits, no source code, no DB/EF/migrations, no write endpoints, no AI/DeepSeek, no Azure, no source commit/push, no `main` change, no secrets.
