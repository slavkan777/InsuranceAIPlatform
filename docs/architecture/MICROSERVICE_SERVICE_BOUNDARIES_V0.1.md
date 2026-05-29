# Microservice Service Boundaries — V0.1

Planning only. Companion to `MICROSERVICE_ARCHITECTURE_CORRECTION_BEFORE_DB_WRITE_AI_V0.1.md`. Per-service contracts for the **Azure-ready microservice architecture, implemented locally first**. Rules that apply to every service: service-owned data (no shared DbContext, no cross-service DB joins, id-only cross-references); cross-service reads via HTTP through the BFF; cross-service facts via events + transactional outbox; correlation/trace id propagated; `InsuranceAIPlatform` data boundary only (never `DevDept`); no real PII; AI advisory-only; no secrets in repo.

Endpoint paths below are the **internal** service contracts; the **frontend only ever calls the BFF** surface (`/api/claims/...`, `/api/demo/...`) which preserves the existing seam.

---

## 1. BFF / API Gateway
- **Purpose:** single frontend entry point; aggregate read models; route commands; hide internal topology; own cross-cutting edges (CORS/auth/rate-limit/correlation-id) later.
- **Owns:** nothing in the domain; only static demo-scenario config + composition logic.
- **Does not own:** claim state, customers/policies, documents, AI logic, approvals, audit store; no business rules; no service DB access.
- **Data boundary:** none (stateless aggregation; static demo config file).
- **Read endpoints (frontend-facing, preserve current seam):** `GET /api/claims/summary`, `GET /api/claims`, `GET /api/claims/{id}`, `/{id}/documents`, `/{id}/ai-evidence`, `/{id}/risks`, `/{id}/policy`, `/{id}/customer-vehicle`, `/{id}/approval`, `/{id}/audit`, `GET /api/demo/scenario`. Workspace/dashboard responses are **composed** from multiple services into the existing DTO shapes.
- **Command endpoints (frontend-facing, routed to owners):** `POST /api/claims/{id}/approval/draft`, `/approval/submit`, `/documents/request` → routed to Approval/Documents.
- **Events published:** none. **Events consumed:** none (may subscribe to read-model-projection cache later; not required locally).
- **Audit responsibilities:** injects/propagates `correlationId`; does not store audit.
- **AI involvement:** none (never calls a provider; only surfaces AI Analysis read models).
- **Tests required:** aggregation contract tests (composed shapes match the 11 screen DTOs); command-routing tests; "BFF holds no DB" check; CORS for `:5173`.
- **Azure mapping later:** Azure Container Apps (public ingress); APIM optional in front; correlation via App Insights.

## 2. Claims Service
- **Purpose:** claim queue, detail, lifecycle, deterministic risk score/level, server-authoritative status state machine.
- **Owns:** `Claims` (incl. `RiskScore`, `RiskLevel`, `Status`, `Estimate`/`Benchmark`/`Deductible`/`RecommendedPayout`).
- **Does not own:** customers/policies/vehicles, documents, AI explanation, approval drafts, audit store.
- **Data boundary:** `claims` schema / Claims DB. References other entities by **id value only** (`CustomerId`, `PolicyId`, `VehicleId`).
- **Read endpoints:** `GET /claims/summary`, `GET /claims`, `GET /claims/{id}`.
- **Command endpoints:** `POST /claims` (synthetic create), `POST /claims/{id}/status` (transition — invoked by Approval's submit, never by AI).
- **Events published:** `ClaimOpened`, `ClaimCreated`, `ClaimStatusChanged`.
- **Events consumed:** `DocumentMetadataAdded`, `MissingEvidenceDetected`, `AiAnalysisCompleted`, `RiskReviewGenerated`, `PolicyCoverageValidated`, `HumanDecisionSubmitted` (to apply the requested, rule-validated transition).
- **Audit responsibilities:** emits an event for each status change → Audit & Cost; never writes audit itself.
- **AI involvement:** consumes AI facts for display only; **AI cannot change status**; deterministic score is authoritative.
- **Tests required:** status state-machine (legal/illegal transitions), risk-threshold block (82 > 60 ⇒ human required), read-shape parity with current API, golden `CLM-1006` unchanged.
- **Azure mapping later:** Container App + Azure SQL (claims); Service Bus topics for its events.

## 3. Customers & Policies Service
- **Purpose:** 200 synthetic test users; customers/vehicles/policies; deterministic policy/coverage validation.
- **Owns:** `TestUsers`, `Customers`, `Vehicles`, `Policies`, `PolicyCoverages`, `PolicyExclusions`, `PolicyCheckResults`.
- **Does not own:** claims, documents, AI, approvals, audit.
- **Data boundary:** `customers` schema / CustomersPolicies DB. Seeds **exactly 200** `TestUsers` (`TUSER-0001..0200`, `tuser####@demo.local`, deterministic; `COUNT(*)=200` asserted). Preserves `CUST-4421` for `CLM-1006`.
- **Read endpoints:** `GET /customers/{id}`, `GET /claims/{id}/customer-vehicle` (context view), `GET /policies/{id}`, `GET /claims/{id}/policy` (coverage + validation).
- **Command endpoints:** none external in local phase beyond seeding/admin.
- **Events published:** `PolicyCoverageValidated`. **Events consumed:** `ClaimOpened` (attach context).
- **Audit responsibilities:** emits `PolicyCoverageValidated` to Audit & Cost; no audit store.
- **AI involvement:** none (deterministic checks); AI may *read* its outputs to explain coverage.
- **Tests required:** **seed-count = 200** assertion; deterministic policy-check outputs; masked-PII assertion; golden customer/vehicle/policy values for `CLM-1006`.
- **Azure mapping later:** Container App + Azure SQL (customers); seed job idempotent in cloud.

## 4. Documents Service
- **Purpose:** document/photo metadata, required-document checklist, missing-evidence detection, upload-metadata placeholder, future extraction boundary.
- **Owns:** `ClaimDocuments`, `ClaimPhotos`, `DocumentChecklistItems`.
- **Does not own:** claim state, AI findings, real file bytes (deferred), customers/policies.
- **Data boundary:** `documents` schema / Documents DB; `StorageUri` nullable (blob bytes deferred to Azure Blob). Metadata-only locally.
- **Read endpoints:** `GET /claims/{id}/documents` (checklist + photos + missing flag).
- **Command endpoints:** `POST /claims/{id}/documents/request` (local audited request — no real send), `POST /claims/{id}/documents/metadata` (metadata-only placeholder).
- **Events published:** `DocumentMetadataAdded`, `MissingEvidenceDetected`. **Events consumed:** `ClaimOpened`.
- **Audit responsibilities:** every request/metadata change emits an audit event → Audit & Cost.
- **AI involvement:** provides document metadata as **input** to AI Analysis; performs no real OCR now (extraction reserved for AI Analysis / future).
- **Tests required:** missing-evidence detection (rear-bumper photo for `CLM-1006`), checklist 6/7, request-command audit emission, no-real-file assertion.
- **Azure mapping later:** Container App + Azure SQL (documents) + Azure Blob (bytes/extraction later).

## 5. AI Analysis Service
- **Purpose:** AI workflow ownership; DeepSeek adapter; mock/fallback; structured outputs (claim summary, document analysis, risk/policy/customer **explanation**); confidence/evidence/cost/token metadata; advisory-only guardrails.
- **Owns:** `AiAnalysisRuns`, `AiFindings`, `EvidenceSources`, `ExtractedEntities`, `RiskAssessments`, `RiskFactors` (AI risk **explanation**; deterministic score stays in Claims). **Owns `IAiProvider`** { `MockAiProvider` (default), `DeepSeekAiProvider` (opt-in, **disabled by default**), `DisabledAiProvider` }.
- **Does not own:** claim status, deterministic risk score, customer/policy data, approval decisions, the audit store.
- **Data boundary:** `ai` schema / AIAnalysis DB. Mode `mock|deepseek|disabled` (default `mock`); models `deepseek-v4-flash`/`deepseek-v4-pro`; `DEEPSEEK_API_KEY` read from local env/user-secrets at runtime, **name-only in docs**. Runs **persisted & cached** (idempotent per claim+input) ⇒ no repeated page-load provider calls.
- **Read endpoints:** `GET /claims/{id}/ai-evidence`, `GET /claims/{id}/risk-explanation`.
- **Command endpoints:** `POST /claims/{id}/ai-analysis/run` (mock by default; real only in `deepseek` mode).
- **Events published:** `AiAnalysisRequested`, `AiAnalysisCompleted`, `AiFindingGenerated`, `RiskReviewGenerated`, `TokenCostRecorded`. **Events consumed:** `DocumentMetadataAdded`, `MissingEvidenceDetected`.
- **Audit responsibilities:** **every run** emits an audit event + cost entry (`runId`/`traceId`) to Audit & Cost; if that fails, the result is not returned (INV-3).
- **AI involvement:** this *is* the AI service — but advisory-only; **must never** decide a claim, authorize payout, assert fraud as fact, set status, or relax a deterministic `BLOCK`.
- **Tests required:** provider unit tests; **DeepSeek-disabled-by-default test** (no HTTP without explicit mode); fallback records `fallbackReason`; guardrail tests (no auto-approve, no fraud-as-fact); cache test (no duplicate calls on reload); audit+cost emitted; golden `CLM-1006` figures (tokens 4261, cost 0.0187, 18.9s) in mock.
- **Azure mapping later:** Container App + Azure SQL (ai); DeepSeek key in Key Vault; Azure AI Search if RAG evidence is added; App Insights for cost/latency.

## 6. Approval Service
- **Purpose:** approval drafts; human approval workflow; decision options; deterministic transition **requests**; customer-draft boundary.
- **Owns:** `ApprovalDrafts`, `HumanDecisionOptions`.
- **Does not own:** the claim status field (Claims owns it), AI logic, audit store.
- **Data boundary:** `approval` schema / Approval DB.
- **Read endpoints:** `GET /claims/{id}/approval` (read model + options + AI recommendation).
- **Command endpoints:** `POST /claims/{id}/approval/draft` (save draft), `POST /claims/{id}/approval/submit` (**human** decision → requests Claims transition), `POST /claims/{id}/customer-request` (local audited draft, no real send).
- **Events published:** `ApprovalDraftSaved`, `HumanDecisionSubmitted`, `CustomerRequestDrafted`. **Events consumed:** `RiskReviewGenerated` (surface AI recommendation).
- **Audit responsibilities:** every draft/submit/request emits an audit event → Audit & Cost.
- **AI involvement:** surfaces the AI *recommendation* (advisory); **AI cannot submit**; only a human actor triggers submit; no autonomous approve/reject.
- **Tests required:** human-only submit guard; no-auto-approval; draft persistence; transition-request validation against Claims' state machine; no real payout/message assertion.
- **Azure mapping later:** Container App + Azure SQL (approval); Service Bus for its events.

## 7. Audit & Cost Service
- **Purpose:** append-only audit; AI run trace; model/provider/tokens/cost/latency; human action trail; cross-service governance evidence.
- **Owns:** `AuditTraces`, `AuditEvents`, `CostTelemetryEvents` (append-only).
- **Does not own:** any business decision or domain mutation.
- **Data boundary:** `audit` schema / AuditCost DB; append-only (no update/delete of history).
- **Read endpoints:** `GET /claims/{id}/audit` (events + cost distribution + model/trace), `GET /runs/{runId}` (trace).
- **Command endpoints:** none external; **ingests events** from all services.
- **Events published:** none (terminal sink). **Events consumed:** all — esp. `AuditEventRecorded`, `TokenCostRecorded`, plus every domain event (`ClaimStatusChanged`, `AiAnalysisCompleted`, `HumanDecisionSubmitted`, …) for governance.
- **Audit responsibilities:** **is** the audit; enforces append-only + correlation/trace linkage; INV-3 (operation fails if its audit/cost write fails).
- **AI involvement:** none (records AI cost/trace, makes no decision).
- **Tests required:** append-only enforcement (no update/delete), event ingestion + idempotency, trace/correlation linkage, cost/token totals match a run, golden `CLM-1006` audit = `BLOCK` with 6 events.
- **Azure mapping later:** Container App + Azure SQL (audit) or append store; App Insights / Log Analytics; retention policy (local demo keeps a bounded window).

---

## Cross-service interaction rules (enforced in review)
- Frontend → **BFF only**. BFF → services via HTTP (reads/commands). Services → services: **events + outbox** for facts; direct synchronous HTTP only for a needed live read (kept minimal).
- **No** shared DbContext, **no** cross-schema/DB joins, **no** distributed transactions. Cross-service references are id values.
- Every state-changing operation emits an audit event; AI runs additionally emit cost. Correlation/trace id flows end-to-end.
- DeepSeek lives only in AI Analysis behind `IAiProvider`; the key is env/user-secrets only and never logged.
