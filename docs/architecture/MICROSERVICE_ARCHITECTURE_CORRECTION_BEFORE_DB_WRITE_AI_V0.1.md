# Microservice Architecture Correction (Before DB / Write / AI) — V0.1

## Status / purpose
**Planning / architecture-correction only — no implementation, no commit/push.** Branch `dev` @ `f8df2b6`.
This document supersedes the single-backend assumption of `LOCAL_COMPLETION_ARCHITECTURE_PLAN_BEFORE_DB_WRITE_AI_V0.1.md` and reframes the backend as an **Azure-ready microservice architecture, implemented locally first**. It keeps every still-valid requirement from the prior plan (200 synthetic users, `InsuranceAIPlatform` data boundary, DeepSeek provider abstraction, mock/fallback, AI advisory-only guardrails, audit/cost metadata, gate separation, no-Azure-before-local) and re-homes them into service boundaries. Companion docs: `MICROSERVICE_SERVICE_BOUNDARIES_V0.1.md`, `MICROSERVICE_LOCAL_GATE_SEQUENCE_V0.1.md`.

## Why correction is needed
GPT audit returned **ACCEPT_WITH_ARCHITECTURE_CORRECTION**: the prior plan was sound but pointed at a single .NET backend with one EF-backed repository behind a DI seam, with the next step `SQLSERVER_EFCORE_PERSISTENCE_IMPLEMENTATION_V0.1`. Slava's correction: the backend must be a **microservice architecture from the planning stage**, so reaching Azure is a *mapping* exercise, not a rewrite. This gate makes service boundaries, data ownership, event contracts, AI isolation, and audit governance explicit *now*, while keeping local execution lightweight and cost-safe.

## Superseded assumptions
The following from the prior plan are **superseded** (kept only as local implementation detail / input, never as the target):
- "Single .NET 9 backend is the system of record."
- "Swap `InMemoryClaimReadService` → one EF-backed `IClaimReadRepository` behind one DI seam" — this is now an *internal detail of the Claims read path inside individual services*, **not** a shared cross-service repository.
- "One `InsuranceAiDbContext` owns all 23+ entities" — superseded by **service-owned data** (each service owns its slice of the schema; no shared DbContext).
- Next gate is no longer `SQLSERVER_EFCORE_PERSISTENCE_IMPLEMENTATION_V0.1`; it is `BFF_API_GATEWAY_SKELETON_PLANNING_V0.1` (see the microservice gate sequence).
The prior 23-entity schema, seed values, DeepSeek design, and guardrails remain valid **as inputs**, partitioned across services in §"Service-owned data strategy".

## Accepted target architecture
**Azure-ready microservice architecture, implemented locally first.** Principles:
- Microservice-first **service boundaries** with explicit contracts from day one.
- **Local-first** execution — services run as separate ASP.NET Core projects in one solution, orchestrated locally (e.g. .NET Aspire AppHost or docker-compose, *described in docs only*), no premature Azure.
- **Frontend talks only to the BFF/API Gateway**, never to individual services.
- **Service-owned data** — no shared DbContext, no cross-service DB joins, no shared repository.
- **HTTP for direct queries/commands**, **events + transactional outbox for cross-service facts** — no distributed transactions.
- **Azure later is a mapping** of these services/contracts to Azure resources, not a redesign.
- No uncontrolled distributed complexity: keep it the minimum number of services that tell the product story; some interactions stay synchronous locally with the outbox table ready for async later.

## Current repo / backend state
| Fact | Value |
|---|---|
| Branch / HEAD | `dev` / `f8df2b6`; `origin/dev` `f8df2b6`; `origin/main` `69e6731` (untouched) |
| Frontend | React 18 + TS + Vite + Redux Toolkit + Redux-Saga; 11 routes; facade `src/api/insuranceApi.ts` (mock default / backend GET-only); mock fallback |
| Frontend seam | one API surface (`/api/claims/...`) — see `MOCK_API_BOUNDARY_V0.1` + `FRONTEND_BACKEND_CONTRACT_READINESS_V0.1`; UI/sagas don't change when the implementation behind it changes |
| Backend (current) | single .NET 9 read API, in-memory `IClaimReadService`, 11 GET endpoints, golden `CLM-1006`, 14 tests; **no DB** |
| Done on `dev` | read integration (all 11 screens) + safe-buttons action honesty |
This current single read API is **frozen** and becomes the seed of the **BFF + Claims read path** during migration — it is not thrown away.

## Microservice service map
```
                React app (11 routes, unchanged)
                        │  one API surface  /api/claims/... , /api/demo/...
                        ▼
        ┌───────────────────────────────────────────┐
        │   BFF / API Gateway  (aggregation + routing)│   ← owns NO domain data
        └───────────────────────────────────────────┘
          │ HTTP (read aggregation + command routing)
  ┌───────┼─────────┬───────────────┬──────────────┬───────────────┬───────────────┐
  ▼       ▼         ▼               ▼              ▼               ▼               ▼
Claims  Customers  Documents      AI Analysis    Approval        Audit & Cost
Service & Policies  Service        Service        Service         Service
  │       Service     │              │              │               ▲
  └────────┴──────────┴──────────────┴──────────────┴───────────────┘
            cross-service FACTS via events + transactional outbox (Audit consumes all)
```
Seven services: **BFF/API Gateway · Claims · Customers & Policies · Documents · AI Analysis · Approval · Audit & Cost.** Full per-service contracts in `MICROSERVICE_SERVICE_BOUNDARIES_V0.1.md`; summaries below.

## BFF / API Gateway responsibilities
- The **only** surface the frontend calls; preserves the existing `/api/claims/...` + `/api/demo/...` contracts so the accepted UI/sagas/facade do not change.
- **Aggregates** read models (e.g. Claim Workspace = Claims detail + Documents + AI evidence/risk + Customers&Policies + Approval read + Audit) by fanning out to services over HTTP and composing the DTO shapes the 11 screens already consume.
- **Routes commands** (save draft, request document, approval-submit) to the owning service; performs no business decision itself.
- Owns cross-cutting edges later: CORS, auth, rate-limit, request correlation-id injection.
- Serves the **demo scenario** from static config (read-only; no domain ownership).
- **Must not own:** claim state, customer/policy data, documents, AI logic, approvals, or the audit store. No business rules, no direct DB access to service stores.

## Claims Service
- **Owns:** `Claims` (queue, detail, lifecycle, deterministic **RiskScore/RiskLevel/Status**, monetary fields), the server-authoritative **status state machine**.
- **Reads:** claim queue, claim detail. **Commands:** create/open (synthetic), status transition (human-driven, via Approval). **Publishes:** `ClaimOpened/Created/StatusChanged`. **Consumes:** `DocumentMetadataAdded`, `MissingEvidenceDetected`, `AiAnalysisCompleted`, `RiskReviewGenerated`, `PolicyCoverageValidated`, `HumanDecisionSubmitted`.
- **Must not:** own customer/policy/document/AI/approval data; call DeepSeek; let AI change status.

## Customers & Policies Service
- **Owns:** `TestUsers` (the **200 synthetic test users**), `Customers`, `Vehicles`, `Policies`, `PolicyCoverages`, `PolicyExclusions`, `PolicyCheckResults` (deterministic coverage validation).
- **Reads:** customer/vehicle context, policy coverage. **Commands:** none externally write-facing in local phase beyond seeding. **Publishes:** `PolicyCoverageValidated`. **Consumes:** `ClaimOpened` (to attach context).
- **Seeds the exactly-200** test users (`TUSER-0001..0200`, `tuser####@demo.local`, deterministic, count-asserted). Preserves `CLM-1006`'s customer `CUST-4421`.
- **Must not:** own claims/documents/AI/approval/audit; expose its DB to other services.

## Documents Service
- **Owns:** `ClaimDocuments`, `ClaimPhotos`, `DocumentChecklistItems` (metadata + completeness; blob bytes deferred to Azure Blob).
- **Reads:** documents + photos + checklist for a claim. **Commands:** request missing document (local audited), add document metadata (placeholder/metadata-only). **Publishes:** `DocumentMetadataAdded`, `MissingEvidenceDetected`. **Consumes:** `ClaimOpened`.
- **Must not:** store real files/PII; perform OCR for real now (extraction boundary reserved for AI Analysis / future); own claim state.

## AI Analysis Service
- **Owns:** `AiAnalysisRuns`, `AiFindings`, `EvidenceSources`, `ExtractedEntities`, `RiskAssessments`, `RiskFactors` (AI **risk explanation**; the deterministic risk *score* stays in Claims).
- **Owns the `IAiProvider` abstraction**: `MockAiProvider` (default), `DeepSeekAiProvider` (opt-in, **disabled by default**), `DisabledAiProvider`. Mode `mock|deepseek|disabled` (default `mock`). Models `deepseek-v4-flash` (default) / `deepseek-v4-pro`. `DEEPSEEK_API_KEY` referenced **by name only**, read from local env / user-secrets at runtime, never in repo/logs.
- **Persists & caches** every run (idempotent per claim+input) so repeated page loads **do not** re-call the provider; results are read from the store. **Publishes:** `AiAnalysisRequested/Completed`, `AiFindingGenerated`, `RiskReviewGenerated`, `TokenCostRecorded`. **Consumes:** `DocumentMetadataAdded`, `MissingEvidenceDetected`.
- **Must never:** decide a claim, authorize payout, assert fraud as fact, set status, or be the only copy of an audit/cost record (it emits to Audit&Cost).

## Approval Service
- **Owns:** `ApprovalDrafts`, `HumanDecisionOptions`.
- **Reads:** approval draft/read model + decision options. **Commands:** save approval draft, **human** approval-submit (requests a Claims status transition), draft customer request. **Publishes:** `ApprovalDraftSaved`, `HumanDecisionSubmitted`, `CustomerRequestDrafted`. **Consumes:** `RiskReviewGenerated` (to surface the AI recommendation).
- **Must not:** auto-approve/reject; let AI submit; execute payout or send real messages (local audited records only); own the claim status field (it *requests* the transition; Claims owns it).

## Audit & Cost Service
- **Owns:** `AuditTraces`, `AuditEvents`, `CostTelemetryEvents` — **append-only**.
- **Reads:** audit trail + cost/token trace for a claim/run (for the Audit/Cost screen). **Commands:** none external (ingests events). **Consumes:** every cross-service event (`AuditEventRecorded`, `TokenCostRecorded`, and all domain events for governance evidence). **Publishes:** none (terminal sink).
- **Invariant:** if an operation's audit/cost write fails, the operation is not considered successful (governance INV-3). Correlation/trace ids tie events to runs.
- **Must not:** mutate or delete audit history; make business decisions.

## Service-owned data strategy
Partition the prior 23-entity schema + the new `TestUsers` across services — **each service owns its tables; no shared DbContext; no cross-service DB joins** (cross-service reads go via HTTP/BFF; cross-service facts via events):

| Service | Owned tables |
|---|---|
| Claims | `Claims` |
| Customers & Policies | `TestUsers`, `Customers`, `Vehicles`, `Policies`, `PolicyCoverages`, `PolicyExclusions`, `PolicyCheckResults` |
| Documents | `ClaimDocuments`, `ClaimPhotos`, `DocumentChecklistItems` |
| AI Analysis | `AiAnalysisRuns`, `AiFindings`, `EvidenceSources`, `ExtractedEntities`, `RiskAssessments`, `RiskFactors` |
| Approval | `ApprovalDrafts`, `HumanDecisionOptions` |
| Audit & Cost | `AuditTraces`, `AuditEvents`, `CostTelemetryEvents` |
| BFF | none (static demo config only) |
Each service references others by **id only** (e.g. Documents stores `ClaimId` as a value, not a FK to a Claims table it cannot see). `CLM-1006` golden values are pinned and split across owners. No real PII anywhere. **`DevDept` DB is never touched.**

## Local DB staging strategy
- **Local Phase 1:** one local SQL Server instance, **schema-per-service** inside a single `InsuranceAIPlatform` database boundary (`claims`, `customers`, `documents`, `ai`, `approval`, `audit` schemas). Each service has its own DbContext + migrations scoped to its schema; **no service reads another's schema**; BFF never touches any service DB.
- **Local Phase 2 (optional, portfolio/Azure-mapping):** split to **database-per-service** locally where it strengthens the Azure story, still before Azure.
- **Always:** DB-name/-schema guard asserts the `InsuranceAIPlatform` boundary before any DDL/DML; connection strings via user-secrets/env only (never committed); 200 test users live in Customers & Policies; `CLM-1006` preserved; no real PII.

## Events / outbox strategy
- **Transactional outbox per service:** a domain change + its event row are written in one local DB transaction; a relay publishes from the outbox. Local-first transport can be in-process / a lightweight local bus; the outbox table is the seam that maps to **Azure Service Bus / Event Grid** later with no code rewrite.
- **Sync vs async:** BFF read-aggregation and direct command routing are **synchronous HTTP**; cross-service *facts* (status changed, analysis completed, audit recorded) are **asynchronous events**. No distributed transactions; eventual consistency for projections.
- **Candidate events:** `ClaimOpened`, `ClaimCreated`, `ClaimStatusChanged`, `DocumentMetadataAdded`, `MissingEvidenceDetected`, `AiAnalysisRequested`, `AiAnalysisCompleted`, `AiFindingGenerated`, `PolicyCoverageValidated`, `RiskReviewGenerated`, `ApprovalDraftSaved`, `HumanDecisionSubmitted`, `CustomerRequestDrafted`, `AuditEventRecorded`, `TokenCostRecorded`.
- **Contracts:** versioned event envelopes (`eventType`, `version`, `eventId`, `correlationId`/`traceId`, `occurredAt`, `payload`); **idempotency** by `eventId`; consumers tolerant to new fields. Local-first may keep several interactions synchronous and add async incrementally — the contract shape is fixed now.

## Migration path from current architecture
- **Stage 0 — Current state frozen:** keep accepted frontend + current read API behavior; do not break the UI.
- **Stage 1 — This correction gate** (docs only).
- **Stage 2 — BFF/API Gateway skeleton:** frontend keeps calling one surface; BFF initially delegates to the existing read service / stubs; **no UI rewrite**.
- **Stage 3 — Service project skeletons:** create Claims, CustomersPolicies, Documents, AIAnalysis, Approval, AuditCost projects + contracts; no DB yet.
- **Stage 4 — Persistence per service boundary:** SQL Server behind each service (schema-per-service first); seed 200 users in CustomersPolicies; preserve `CLM-1006`.
- **Stage 5 — Command/write workflows + events/audit;** no autonomous AI decisions.
- **Stage 6 — AI Analysis Service:** provider abstraction, mock default, DeepSeek opt-in, no secrets in repo, no page-load calls.
- **Stage 7 — Full local verification:** per-service build/test/smoke, BFF integration, frontend flow, audit trace, seed count = 200, no Azure.
- **Stage 8 — Owner manual walkthrough** (only after local functional completion).
- **Stage 9 — Azure planning/deploy** (mapping, not rewrite).
Each implementation stage is split into its own implement + commit/push gates (see gate sequence).

## DeepSeek / AI provider position
The DeepSeek provider lives **only inside the AI Analysis Service**, behind `IAiProvider` — never scattered across controllers or other services. Default `mock`; real `deepseek` mode is opt-in and **disabled by default**; `DEEPSEEK_API_KEY` is read from local env/user-secrets at runtime and referenced in docs by name only; results are persisted/cached to prevent repeated page-load calls; mock fallback is explicit (`fallbackReason`, never silent fake-success). Models: `deepseek-v4-flash` (default), `deepseek-v4-pro` (optional).

## AI guardrails (carry forward, enforced in AI Analysis + Approval + Claims)
1. **Advisory only** — AI never auto-approves/rejects or sets status. 2. **No fraud-as-fact** — "elevated risk" yes, "fraud confirmed" no. 3. **Mandatory audit + cost** — every run emits an audit event + cost entry + `runId`/`traceId`; if either fails, no result is returned. 4. **Honest labels** — provider/model label reflects reality (`Demo/Synthetic`/`mock-fallback` unless a real DeepSeek call happened). 5. **Human gate** — only a human `approval-submit` (Approval Service) can request Approved/Rejected; Claims enforces the transition. 6. **Deterministic override** — Claims/Customers&Policies rules outrank AI; AI cannot lift a `BLOCK`.

## Azure readiness mapping
| Local building block | Azure target (later, mapping only) |
|---|---|
| BFF + 6 services (ASP.NET Core) | Azure Container Apps (or App Service) — one per service |
| Local orchestration (Aspire/compose) | Container Apps environment |
| Schema/DB-per-service (SQL Server) | Azure SQL — database or elastic-pool per service per cost plan |
| Transactional outbox + local bus | Azure Service Bus / Event Grid |
| Document metadata (blob deferred) | Azure Blob Storage |
| Secrets (`DEEPSEEK_API_KEY`, conn strings) | Azure Key Vault (referenced, never stored in repo) |
| Audit/cost + logs | Application Insights / Log Analytics |
| RAG evidence (only if needed) | Azure AI Search |
Azure is **not** provisioned until after local completion + the owner manual checkpoint.

## What remains local before manual testing
All seven services run locally; SQL Server local (schema- or DB-per-service); 200 users seeded; `CLM-1006` golden; read aggregation through BFF; safe audited writes; AI in mock (DeepSeek opt-in, disabled by default); full local verification incl. seed-count=200, audit-trace, secret-scan. **No Azure.**

## What remains after manual testing
Owner ACCEPT → fix batch → Azure deployment **planning** → minimal Azure microservice deployment (mapping) → post-Azure verification → `main` promotion (owner-authorized). No `main` change before then.

## Risks and mitigations
| Risk | Mitigation |
|---|---|
| Hidden monolith behind microservice words | service-owned data, no shared DbContext, id-only cross-refs, BFF-only frontend entry — enforced in boundaries doc |
| Over-engineering / distributed complexity locally | minimum 7 services; sync HTTP where fine; outbox optional-async; Aspire/compose local, no Azure early |
| Distributed-transaction temptation | events + outbox + eventual consistency; no cross-service 2PC |
| Frontend breakage during migration | BFF preserves the exact existing seam; Stage 0 freeze; reads unchanged |
| Secret leakage (`DEEPSEEK_API_KEY`, conn strings) | provider isolated in AI service; user-secrets/env only; pre-push secret scan; name-only in docs |
| Cross-service DB coupling | schema/DB-per-service + guard; no cross-schema joins; reviewer checks |
| `DevDept` touched | hard boundary assert on `InsuranceAIPlatform` only |
| Cost blowup (AI/Azure) | mock default; DeepSeek disabled-by-default + cached; Azure deferred + cost-ceilinged in its planning gate |
| Golden CLM-1006 drift across services | values pinned per owner; read contracts asserted unchanged |

## Non-goals
No production insurer integration; no real payout/messaging; no auth/RBAC in local phase (test users are display-only); no Azure/Docker provisioning in planning gates; no real PII; no `main` promotion before the final gate; no big-bang rewrite.

## Stop boundaries
This gate authorizes **no** implementation. Do not, in this gate: implement BFF or services, create projects, create a DB/migrations/EF, implement endpoints, change `src/`/`server/` behavior, call DeepSeek, read/log/print `DEEPSEEK_API_KEY`, start Azure/Docker, commit/push source, or modify `main`. Each capability unlocks only via its named gate.

## Next gates
Immediate next is **`BFF_API_GATEWAY_SKELETON_PLANNING_V0.1`** (not `SQLSERVER_EFCORE_PERSISTENCE_IMPLEMENTATION_V0.1`, which is **superseded**). Full chain: `MICROSERVICE_LOCAL_GATE_SEQUENCE_V0.1.md`.
