# BFF / API Gateway — Route & Contract Map V0.1

**Gate:** `BFF_API_GATEWAY_SKELETON_PLANNING_V0.1` · companion to `BFF_API_GATEWAY_SKELETON_PLANNING_V0.1.md` · **Branch:** `dev` @ `f8df2b6`
Planning-only. Maps every frontend area to its current source, the BFF surface that preserves it, and the internal service that will own it later.

Conventions: **Surface 1** = preserved passthrough route (frontend unchanged, BFF delegates). **Surface 2** = optional additive composed view under `/api/bff/...`. Backend base today: `http://localhost:5284`; routes under `/api/claims`, `/api/demo`, `/api/system`, `/health`.

## Read map — 11 frontend screens

| UI screen | Current frontend route | Current API/mock function | Current backend endpoint | Future BFF endpoint | Internal service dependencies | DTO / view model | Read/command | Aggregation notes | Failure / fallback behavior | Impl phase |
|---|---|---|---|---|---|---|---|---|---|---|
| **Dashboard** | `/` | `getClaimsSummary()`, `getClaimsQueue()` | `GET /api/claims/summary`, `GET /api/claims` | S1: same two routes · S2: `GET /api/bff/dashboard` | Claims (queue, counts) + Audit & Cost (processedToday, aiAnalysisRunning) | `ClaimSummaryDto` + `ClaimListItemDto[]` → `DashboardView` | Read | counters may come from Audit later; queue from Claims | render queue even if counters degrade; `warnings[]`; mock-fallback labeled | S1 passthrough; S2 after Claims+Audit split |
| **Claims List** | `/claims` | `getClaimsQueue()` | `GET /api/claims` | S1: `GET /api/claims` | Claims | `ClaimListItemDto[]` (→ `ClaimRow[]`) | Read | single-service; no aggregation | 200 + cached/empty on partial; error envelope on total failure | S1 passthrough |
| **Claim Workspace** | `/claims/:claimId` | `getClaimById(claimId)` | `GET /api/claims/{claimId}` | S1: same · S2: `GET /api/bff/claims/{claimId}/workspace` | Claims (core) + Customers & Policies (customer/vehicle/policy summary) + AI Analysis (ai summary) + Audit & Cost (trace/run ids) | `ClaimDetailsDto` (27 fields) → `ClaimWorkspaceView` | Read | **prime aggregation**: ClaimDetails spans 4 services | claim core critical → 404 if missing; ai/audit blocks optional → `warnings[]` | S1 passthrough; S2 is the first real aggregation |
| **Documents / Photos** | `/claims/:claimId/documents` | `getClaimDocuments()`, `getClaimPhotos()` | `GET /api/claims/{claimId}/documents` (both; photos = filtered) | S1: same | Documents | `ClaimDocumentDto[]` → `DocumentChecklistItem[]` + `DamagePhoto[]` | Read | photos derived from same set by `type==='photo'` | render with "evidence unavailable" note on partial | S1 passthrough; Documents owns later |
| **AI Evidence** | `/claims/:claimId/ai-evidence` | `getAiAnalysis(claimId)` (read); `runMockAiAnalysis()` (deferred) | `GET /api/claims/{claimId}/ai-evidence` (read only) | S1: same | AI Analysis | `AiEvidenceDto` (findings/evidence/confidence/entities) | Read (+ future command) | advisory-only data | degrade to "AI analysis unavailable"; never blocks claim; labeled advisory | S1 passthrough; AI Analysis owns later |
| **Risks** | `/claims/:claimId/risks` | `getRiskReview(claimId)` | `GET /api/claims/{claimId}/risks` | S1: same | AI Analysis (advisory score) + Claims (deterministic checks) | `RiskAssessmentDto` (score/threshold/factors/pipeline) | Read | deterministic checks authoritative; AI advisory | deterministic part must render; advisory optional | S1 passthrough; split later |
| **Policy / Coverage** | `/claims/:claimId/policy` | `getPolicyCoverage(claimId)` | `GET /api/claims/{claimId}/policy` | S1: same | Customers & Policies | `PolicyDto` (blocks + validation) | Read | coverage validation deterministic | stale-safe render on partial | S1 passthrough; Customers & Policies owns later |
| **Customer / Vehicle** | `/claims/:claimId/customer-vehicle` | `getCustomerVehicleContext(claimId)` | `GET /api/claims/{claimId}/customer-vehicle` | S1: same | Customers & Policies | `CustomerVehicleContextDto` (customer + vehicle + history) | Read | history may be a later sub-call | history optional → `warnings[]` | S1 passthrough; Customers & Policies owns later |
| **Human Approval** | `/claims/:claimId/approval` | `getClaimApproval(claimId)` (read); `saveApprovalDraft()`, `sendCustomerRequest()` (deferred) | `GET /api/claims/{claimId}/approval` (read only) | S1: same (read) | Approval (+ AI Analysis for aiRecommendation/recommendedPayout) | `ApprovalDraftDto` (draft + options + ai recommendation) | Read (+ future commands) | recommendation is advisory & labeled | options/draft critical; recommendation optional | S1 passthrough; Approval owns later |
| **Audit / Cost** | `/claims/:claimId/audit` | `getAuditTrace(claimId)`, `getRiskReview(claimId)` (pipeline) | `GET /api/claims/{claimId}/audit`, `GET /api/claims/{claimId}/risks` | S1: same | Audit & Cost (+ AI Analysis pipeline) | `AuditTraceDto` (runId/traceId/model/tokens/cost/events/distribution) | Read | append-only trace; pipeline from AI | partial trace render on degrade | S1 passthrough; Audit & Cost owns later |
| **Demo Scenario** | `/demo` | `getDemoScenario()` | `GET /api/demo/scenario` | S1: same | BFF demo config (synthetic) / Claims for golden id | `DemoScenarioDto` (steps + `goldenClaimId: CLM-1006`) | Read | presentation metadata; not a business service | static synthetic safe default | S1 passthrough; stays BFF/demo |

Gateway infra (non-business): `GET /health` → BFF-owned liveness; `GET /api/system/demo-status` → BFF passthrough/aggregate of synthetic demo-stage status. No frontend api-client counterpart today.

## Command map — future write routes (planning only; 0 implemented)

Derived from the deferred / safe-button actions in the frontend. BFF routes; the owning service validates and decides.

| UI screen | Frontend action (deferred today) | Future BFF command endpoint | Owning service | Read/command | Validation | Audit event | Failure / fallback |
|---|---|---|---|---|---|---|---|
| Human Approval | "Зберегти чернетку" (`saveApprovalDraft`) | `PUT /api/claims/{id}/approval/draft` | Approval | Command | Approval Service | `ApprovalDraftSaved` | optimistic UI; envelope on failure |
| Human Approval | "Погодити після перевірки" | `POST /api/claims/{id}/approval/submit` | Approval | Command | deterministic transition; **human-only** | `ApprovalSubmitted` | blocked unless human-signed; AI never finalizes |
| Human Approval / Docs | "Надіслати запит клієнту" / "Запросити у клієнта" / "Запросити фото" (`sendCustomerRequest`) | `POST /api/claims/{id}/documents/requests` | Documents | Command | Documents Service | `DocumentRequested` | **no real SMS/email locally** — draft only |
| Documents / Photos | "Підтвердити документ" | `POST /api/claims/{id}/documents/{docId}/confirm` | Documents | Command | Documents Service | `DocumentConfirmed` | envelope on failure |
| Documents / Claims List | "Імпорт документів" (upload metadata) | `POST /api/claims/{id}/documents` | Documents | Command | Documents Service | `DocumentAdded` | metadata only; no bytes in skeleton |
| AI Evidence | "Run AI analysis" (`runMockAiAnalysis`) | `POST /api/claims/{id}/ai-analysis/runs` | AI Analysis | Command | AI Analysis (advisory) | `AiRunStarted/Completed` | **mock default**; DeepSeek opt-in/disabled-by-default |
| (future) | "Generate customer draft" | `POST /api/claims/{id}/ai-analysis/customer-draft` | AI Analysis | Command | AI Analysis | `CustomerDraftGenerated` | draft only until human-approved |
| Dashboard / Claims List | "+ Створити випадок" / "+ Новий випадок" | `POST /api/claims` | Claims | Command | Claims Service | `ClaimCreated` | synthetic data only |
| Dashboard / Claims List | "Експорт" / "Експорт CSV" | `GET /api/bff/{area}/export` | BFF compose (read-only) | Read | — | `ExportGenerated` | no write; no PII |

Command invariants: AI never approves/rejects/finalizes or changes claim status; customer messages are drafts until human-approved; no real SMS/email in local demo; every command carries an idempotency key and emits an audit event via Audit & Cost; BFF does not make the business decision.

## BFF endpoint → internal service matrix

| Future internal service | BFF read endpoints routed to it | BFF command endpoints routed to it |
|---|---|---|
| **Claims Service** | `/api/claims`, `/api/claims/{id}` (core), `/api/claims/summary` (queue/counts), risks (deterministic checks) | `POST /api/claims` (create case) |
| **Customers & Policies Service** | `/api/claims/{id}/policy`, `/api/claims/{id}/customer-vehicle` | — (synthetic 200 users seeded later) |
| **Documents Service** | `/api/claims/{id}/documents` (docs + photos) | document request / confirm / add (metadata) |
| **AI Analysis Service** | `/api/claims/{id}/ai-evidence`, risks (advisory), approval (recommendation) | run analysis, customer draft (mock default; DeepSeek isolated) |
| **Approval Service** | `/api/claims/{id}/approval` (draft + options) | save draft, submit (human-only) |
| **Audit & Cost Service** | `/api/claims/{id}/audit`, dashboard counters (processedToday, aiRunning) | receives audit events from all commands |
| **BFF / Gateway (no business data)** | `/health`, `/api/system/demo-status`, `/api/demo/scenario`, Surface-2 composed views, exports | routes only — owns no decision |

Notes: `ClaimDetailsDto` (Claim Workspace) is the cross-service join point — its fields are sourced from Claims + Customers & Policies + AI Analysis + Audit & Cost, composed by the BFF (id-only cross-service references; no shared DbContext, no cross-service DB joins). `CLM-1006` remains the golden claim; data stays synthetic; no real PII.
