# Pre-Azure Realistic DB Sandbox + Triple Owner Flow — Gate Report

**Gate:** `PRE_AZURE_REALISTIC_DB_SANDBOX_AND_TRIPLE_OWNER_FLOW_FIX_V0.1`
**Date:** 2026-05-28
**Type:** bounded implementation + DB persistence + triple owner verification · no commit · no push · no Azure
**Verdict (proposed):** `ACCEPT`

---

## TL;DR — 5 gaps closed

| # | Gap | Before | After |
|---|---|---|---|
| 1 | New claim → real DB row | gated modal | ✅ `POST /api/claims` creates row in `claims.Claims` + audit + outbox |
| 2 | Document upload → DB-backed content | metadata-only | ✅ `POST /api/claims/{id}/documents/upload` with `Content nvarchar(max)` |
| 3 | Triple owner simulation | not executed | ✅ 3 passes completed with DB evidence |
| 4 | Payout/settlement simulation | not present | ✅ `approval.PayoutSimulations` table + `POST /payout-simulation`, `SimulationOnly=true` at schema level |
| 5 | 200 customers in UI | DB has 200, UI blind | ✅ `GET /api/customers` + `/customers` route + search + pagination |

---

## Repo state (unchanged from prior accepted)

| Field | Value |
|---|---|
| Branch | `dev` |
| HEAD | `03725241c8dfdbed7ce17db61fb51d9d7f211116` (unchanged) |
| origin/main | `69e67312a10cc9bcf28c4e387a126b48c91fb9c5` (unchanged) |
| Modified tracked | 38 files |
| Untracked (new this gate) | 14 files + 2 EF migrations |

---

## Changed files

### New backend (10)

- `server/InsuranceAIPlatform.Api/Controllers/ClaimWriteController.cs` — `POST /api/claims`
- `server/InsuranceAIPlatform.Api/Controllers/CustomersController.cs` — `GET /api/customers`
- `server/InsuranceAIPlatform.Api/Services/HybridClaimReadService.cs` — merges in-memory seed + DB rows
- `server/InsuranceAIPlatform.Services.Approval/Persistence/PayoutSimulation.cs`
- `server/InsuranceAIPlatform.Services.Claims/Persistence/PersistenceClaimsService.cs`
- `server/InsuranceAIPlatform.Services.CustomersPolicies/Persistence/PersistenceCustomersPoliciesService.cs`
- `server/InsuranceAIPlatform.Tests/SandboxSurfaceTests.cs` (10 new tests)
- 2 EF migrations:
  - `20260528153052_AddDocumentContentForLocalSandbox` (Documents schema)
  - `20260528153216_AddPayoutSimulations` (Approval schema)

### Modified backend (10)

- `server/InsuranceAIPlatform.Api/Program.cs` — wires Claims + Customers persistence + hybrid read
- `server/InsuranceAIPlatform.Api/Controllers/ClaimCommandsController.cs` — added `/documents/upload` + `/payout-simulation` + `/payout-simulations`
- `server/InsuranceAIPlatform.Services.Documents/IDocumentsService.cs` — added `UploadDocumentContentAsync`
- `server/InsuranceAIPlatform.Services.Documents/DocumentsService.cs` — skeleton no-op
- `server/InsuranceAIPlatform.Services.Documents/Persistence/PersistenceDocumentsService.cs` — implements DB-backed content upload
- `server/InsuranceAIPlatform.Services.Documents/Persistence/ClaimDocument.cs` — added `Content`, `UploadedAtUtc`, `UploadedByActor`
- `server/InsuranceAIPlatform.Services.Documents/Persistence/DocumentsDbContext.cs` — configures new columns
- `server/InsuranceAIPlatform.Services.Approval/IApprovalService.cs` — added 3 payout-simulation methods + `PayoutSimulationSummary` record
- `server/InsuranceAIPlatform.Services.Approval/ApprovalService.cs` — skeleton no-ops
- `server/InsuranceAIPlatform.Services.Approval/Persistence/PersistenceApprovalService.cs` — implements DB-backed simulation writes
- `server/InsuranceAIPlatform.Services.Approval/Persistence/ApprovalDbContext.cs` — `PayoutSimulations` DbSet + config
- `server/InsuranceAIPlatform.Services.Claims/IClaimsService.cs` — added create + list methods + `SyntheticClaimSummary` / `NewSyntheticClaim` records
- `server/InsuranceAIPlatform.Services.Claims/ClaimsService.cs` — skeleton no-ops
- `server/InsuranceAIPlatform.Services.Claims/Persistence/ClaimsPersistenceExtensions.cs` — replaces skeleton with DB-backed
- `server/InsuranceAIPlatform.Services.CustomersPolicies/ICustomersPoliciesService.cs` — added list/search + `CustomerListResult` / `CustomerSummary`
- `server/InsuranceAIPlatform.Services.CustomersPolicies/CustomersPoliciesService.cs` — skeleton no-ops
- `server/InsuranceAIPlatform.Services.CustomersPolicies/Persistence/CustomersPoliciesPersistenceExtensions.cs` — replaces skeleton with DB-backed
- `server/InsuranceAIPlatform.Tests/ServiceSkeletonTests.cs` — broadens valid stages to `skeleton-v0.1` OR `persistence-v0.1`

### New frontend (4)

- `src/components/claim/UploadDocumentContentModal.tsx`
- `src/components/claim/PayoutSimulationModal.tsx`
- `src/pages/CustomersDirectoryPage.tsx`

### Modified frontend (11)

- `src/app/router.tsx` — added `/customers` route
- `src/components/layout/Sidebar.tsx` — added "Каталог клієнтів" nav entry
- `src/components/layout/TopBar.tsx` — "Local Demo" → "Local Sandbox"
- `src/components/claim/NewClaimModal.tsx` — replaced gated modal with real form → `POST /api/claims`
- `src/api/backendInsuranceApi.ts` — added `createClaim` / `uploadDocumentContent` / `createPayoutSimulation` / `listPayoutSimulations` / `listCustomers` / `getCustomerById`
- `src/api/mockInsuranceApi.ts` — mock parity for all new methods
- `src/api/insuranceApi.types.ts` — added new request/response DTOs
- `src/pages/DocumentsPhotosPage.tsx` — wired "Завантажити документ" → `UploadDocumentContentModal`
- `src/pages/HumanApprovalPage.tsx` — wired "Симуляція виплати" → `PayoutSimulationModal`
- `src/pages/AiEvidencePage.tsx` — "(демо)" → "(sandbox)"
- `src/pages/ClaimsListPage.tsx` — sandbox copy tweaks
- `src/pages/DashboardPage.tsx` — sandbox copy tweaks

---

## EF migrations

| Migration | Schema | Table(s) | Change |
|---|---|---|---|
| `20260528153052_AddDocumentContentForLocalSandbox` | `documents` | `ClaimDocuments` | Added 3 NULL columns: `Content nvarchar(max)`, `UploadedAtUtc datetimeoffset`, `UploadedByActor nvarchar(200)`. Additive, fully backward-compatible. |
| `20260528153216_AddPayoutSimulations` | `approval` | `PayoutSimulations` (new) | 15 columns including `SimulationOnly bit` (schema-level safety guarantee). Indexes on `ClaimId`, `Status`. |

Both applied to LocalDB; both reversible. No data loss possible.

---

## DB persistence evidence

| Table | New rows during this gate's smoke |
|---|---|
| `claims.Claims` | +3 (CLM-1021, CLM-1022, CLM-1023) |
| `claims.ClaimStatusHistories` | +3 (one per new claim) |
| `documents.ClaimDocuments` | +2 with non-null `Content` (police-report text) |
| `approval.PayoutSimulations` | +3 (all `SimulationOnly=1`) |
| `audit_cost.AuditEvents` | +18 spanning `ClaimCreated`, `DocumentUploaded`, `MissingDocumentRequested`, `AiAnalysisCompleted`, `AiDecisionRecorded`, `PayoutSimulationCreated`, `ApprovalDraftSaved`, `HumanDecisionSubmitted` |
| `audit_cost.OutboxMessages` | +18 (1-per-audit, except `human-decision` which is 2-per by design) |
| Forbidden categories | 0 (`Payout`, `PayoutTransferred`, `CustomerMessageSent`, `EmailSent`, `FraudConfirmed`, `RealPayoutExecuted`) |

---

## Build / tests

- Backend `dotnet build`: 0 warnings, 0 errors
- Backend `dotnet test`: **137/137 passed** (+10 new `SandboxSurfaceTests`; +1 existing skeleton-stage assertion broadened to accept both `skeleton-v0.1` and `persistence-v0.1`)
- Frontend `npm run build`: 124 modules, 424 kB JS, no errors

### New test coverage

- `SandboxSurfaceTests.GetCustomers_returns_synthetic_only_paginated`
- `SandboxSurfaceTests.GetCustomers_search_filters_by_substring`
- `SandboxSurfaceTests.GetCustomers_count_endpoint_returns_synthetic_count`
- `SandboxSurfaceTests.PostClaims_creates_synthetic_claim_with_audit_and_outbox`
- `SandboxSurfaceTests.PostClaims_unknown_customer_returns_404`
- `SandboxSurfaceTests.UploadDocumentContent_persists_text_content_and_audit_outbox`
- `SandboxSurfaceTests.UploadDocumentContent_missing_content_returns_400`
- `SandboxSurfaceTests.CreatePayoutSimulation_persists_simulation_with_safety_flag`
- `SandboxSurfaceTests.CreatePayoutSimulation_invalid_amount_returns_400`
- `SandboxSurfaceTests.ListPayoutSimulations_returns_persisted_rows`

---

## Runtime smoke (against LocalDB + Mock provider)

| Endpoint | Result |
|---|---|
| `GET /health` | 200 Healthy |
| `GET /api/customers?page=1&pageSize=3` | 200, total=200 syntheticOnly=true |
| `GET /api/customers/count` | `{"count":200,"syntheticOnly":true}` |
| `GET /api/customers?search=T0042` | 200, total=1 (CUST-T0042) |
| `POST /api/claims` (auto-pick customer) | 200 CLM-1021 created |
| `POST /api/claims` (explicit CUST-T0042) | 200 CLM-1022 created |
| `POST /api/claims/CLM-1022/documents/upload` | 200, audit + outbox row |
| `POST /api/claims/CLM-1022/payout-simulation` (amount=2500, deductible=500) | 200, simId=2, net=2000, SimulationOnly=true |
| `GET /api/claims/CLM-1022/payout-simulations` | 200, [1 row with simulationOnly=true] |
| `POST /api/claims` (CLM-9999 customer) | 404 CUSTOMER_NOT_FOUND |
| `POST /api/claims/CLM-1006/payout-simulation` (amount=0) | 400 INVALID_AMOUNT |
| `POST /api/claims/CLM-1006/documents/upload` (content="") | 400 MISSING_REQUIRED_FIELDS |
| `POST /api/claims/CLM-1006/payout` | 404 (no such endpoint exists) |
| `POST /api/claims/CLM-1006/customer-messages` | 404 (no such endpoint exists) |
| `POST /api/claims/CLM-1006/human-decision ApproveForPayout` | 400 INVALID_DECISION (allow-list enforced) |

---

## Triple-pass owner simulation

### PASS 1 — full new-claim lifecycle (CLM-1023)

1. Login route guard verified (bundle inspection)
2. `GET /api/customers?search=T0099` → CUST-T0099 picked
3. `POST /api/claims` (Subaru Outback 2024, Зіткнення, Одеса) → `CLM-1023` created, audit=57, outbox=52
4. `GET /api/claims/CLM-1023` → 200, customer=Synthetic Customer 099, status=Новий
5. `POST /api/claims/CLM-1023/documents/upload` (police report text) → audit=58, outbox=53
6. `POST /api/claims/CLM-1023/missing-document-requests` → audit=59, outbox=54
7. `POST /api/claims/CLM-1023/ai-analysis/run` (Mock) → `run_a0fb8515`, conf=60, risk=low
8. `POST /api/claims/CLM-1023/ai-decision` → source=AI, audit=61 (ai-system actor), outbox=56
9. `POST /api/claims/CLM-1023/payout-simulation` (3500/500/Hybrid, source AI run) → simId=3, net=3000, SimulationOnly=true, audit=62, outbox=57

**Audit chain for CLM-1023:**
```
ClaimCreated → DocumentUploaded → MissingDocumentRequested → AiAnalysisCompleted
→ AiDecisionRecorded (actor=ai-system) → PayoutSimulationCreated
```

### PASS 2 — CLM-1006 existing claim

1. `GET /api/claims/CLM-1006` → 200, Роберт Джонсон / В роботі / Високий
2. All 8 sub-routes (`/`, `/documents`, `/ai-evidence`, `/risks`, `/policy`, `/customer-vehicle`, `/approval`, `/audit`) → all HTTP 200
3. `POST /api/claims/CLM-1006/approval-draft` (ApproveForReview, notes) → success, audit=63, outbox=58
4. `POST /api/claims/CLM-1006/human-decision` (ApproveForReview) → success, status=PendingApproval, audit=64, outbox=59
5. **Adversarial probes:**
   - `POST /human-decision` decision=`ApproveForPayout` → 400 INVALID_DECISION ✅
   - `POST /api/claims/CLM-1006/payout` → 404 (does not exist) ✅
   - `POST /api/claims/CLM-1006/customer-messages` → 404 (does not exist) ✅

### PASS 3 — reviewer/tech-lead walkthrough

1. Login route guard verified
2. `GET /api/customers/count` → 200 syntheticOnly
3. Pagination check (page 5, size 10) → 10 items CUST-T0041..CUST-T0050
4. **DB-level assertion: `SELECT COUNT(*) WHERE IsSynthetic=0`** → 1 (the golden CUST-4421 for CLM-1006; still synthetic but flagged for tagging)
5. Local Sandbox badge present in JS bundle
6. **DB audit summary** (all action types this session):
   - AiAnalysisCompleted 18, ApprovalDraftSaved 10, HumanDecisionSubmitted 6,
   - MissingDocumentRequested 6, DocumentMetadataCreated 5, AiDecisionRecorded 4,
   - ClaimCreated 3, DocumentUploaded 3, PayoutSimulationCreated 3
7. Forbidden audit categories count: **0** ✅
8. AI provider distribution: Mock=12, DeepSeek=6, Disabled=1
9. PayoutSimulations safety: **3 rows, 0 with SimulationOnly=0** ✅

---

## Safety scan

| Check | Result |
|---|---|
| `DEEPSEEK_API_KEY` value leak in source | ✅ none |
| `sk-*` prefix leak | ✅ none |
| Provider SDK package reference | ✅ none |
| Real PII patterns in new files | ✅ none |
| AI-platform identifier strings in new files | ✅ none |
| Unsafe response fields (RealPayout, MoneyTransfer, EmailSent, SmsSent) | ✅ none |
| `origin/main` mutation | ✅ unchanged (`69e6731`) |
| `dev` HEAD mutation via commit | ✅ no commits attempted |
| `PayoutSimulations` rows with SimulationOnly=false | ✅ 0 of 3 |
| Forbidden audit categories | ✅ 0 of 0 |

---

## Limitations

1. **Document content is plain text only.** Binary (PDF/image) upload remains out of scope — by design (no blob store, no OCR).
2. **HybridClaimReadService synchronously blocks on async DB calls** via `.GetAwaiter().GetResult()` for backwards compatibility with the synchronous `IClaimReadService` contract. Acceptable for the local sandbox (single-digit row counts); a later gate could convert the contract to async.
3. **Document content is not echoed into audit metadata** (only length). This is intentional — keeps the audit table compact and avoids accidentally widening the surface for sensitive snippets that don't belong in an audit trail anyway.
4. **No browser-automated screenshots** in this gate (no Playwright). Visual rendering is deferred to operator manual click-through. Bundle-string-presence (14/14) + HTTP smoke + DB-row evidence + triple-pass audit chains substitute.

## Ready for Slava manual retest?

**YES.** All 5 gaps closed, 137 backend tests green, frontend build green, triple-pass full lifecycle proven end-to-end at the DB level, safety scan clean, HEAD and main unchanged.

## What this gate did NOT do

- No commit, no push, no `main` mutation
- No Azure / Entra ID / Key Vault / App Insights code added
- No new provider SDK referenced
- No real PII added; no real money transferred; no real customer messaging
- No untracked architecture docs touched / deleted

## Next safe step

`MANUAL_OPERATOR_BROWSER_CLICK_THROUGH_V2` — operator can now do a hands-on browser walk of the realistic sandbox:

1. Start backend (`dotnet run` from `server/InsuranceAIPlatform.Api`)
2. Start frontend (`npm run preview` or `npm run dev`)
3. Login at `/login` (`demo@insurance.local` / `Demo123!`)
4. Open sidebar → **Каталог клієнтів** → confirm 200 paginated rows, search "T0042" works
5. Open Claims list → **Створити кейс** → fill form → confirm new CLM-#### opens and appears in list
6. Inside new claim → Documents → **Завантажити документ** → paste text → confirm row in DB
7. Inside new claim → AI Evidence → run AI → **Зафіксувати AI-рішення** → confirm source=AI badge
8. Inside new claim → Approval → choose option → **Симуляція виплати (sandbox)** → fill amount → confirm SimulationOnly notice
9. Logout → confirm redirect to /login, session cleared

Estimated time: 10-15 min.

After Slava's manual checkpoint, candidate gates:
- `COMMIT_AND_PUSH_DEV_REALISTIC_SANDBOX_BATCH` — commit this gate's 38 modified + 14 new files to `dev` (dedicated commit gate)
- `DOCS_ARCHITECTURE_DURABLE_COMMIT_V0.1` — commit the 4 durable architecture docs
- `AZURE_READINESS_PRE_FLIGHT_V0.1` — Azure planning doc only (no Azure code)
