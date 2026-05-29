# Local Completion Architecture Plan — Before DB / Write / AI — V0.1

> ## ⚠️ ARCHITECTURE CORRECTION NOTICE (2026-05-27)
> The **single-backend / clean-DI-seam / one-EF-repository** target in this document is **SUPERSEDED** by the microservice correction. The accepted target backend is now **"Azure-ready microservice architecture, implemented locally first."**
> - **Authoritative docs:** [`MICROSERVICE_ARCHITECTURE_CORRECTION_BEFORE_DB_WRITE_AI_V0.1.md`](MICROSERVICE_ARCHITECTURE_CORRECTION_BEFORE_DB_WRITE_AI_V0.1.md), [`MICROSERVICE_SERVICE_BOUNDARIES_V0.1.md`](MICROSERVICE_SERVICE_BOUNDARIES_V0.1.md), [`MICROSERVICE_LOCAL_GATE_SEQUENCE_V0.1.md`](MICROSERVICE_LOCAL_GATE_SEQUENCE_V0.1.md).
> - The "single .NET backend is the system of record", the "swap `InMemoryClaimReadService` → one shared EF-backed `IClaimReadRepository`", and "one `InsuranceAiDbContext` owns all entities" assumptions are now **temporary local implementation detail only** — the target is **service-owned data** across 7 services (BFF + Claims + Customers&Policies + Documents + AI Analysis + Approval + Audit&Cost), no shared DbContext.
> - The next gate is **NOT** `SQLSERVER_EFCORE_PERSISTENCE_IMPLEMENTATION_V0.1`; it is `BFF_API_GATEWAY_SKELETON_PLANNING_V0.1`.
> - **Still valid (kept as input, re-homed into services):** 200 synthetic test users, `InsuranceAIPlatform` data boundary (never `DevDept`), DeepSeek provider abstraction (mock default, opt-in, disabled-by-default), mock/fallback, AI advisory-only guardrails (INV-1..6), audit/cost/token/confidence metadata, gate separation, no-Azure-before-local, no secrets in repo, golden `CLM-1006`, exact seed values, the 23-entity schema (now partitioned by service owner).
> Read this document only for those still-valid details; treat its topology as reframed by the microservice docs.

**Status:** planning only · no implementation in this gate.
**Branch:** `dev` · **HEAD at planning:** `f8df2b6`.
**Supersedes nothing; consolidates and extends** the prior backend planning set in `docs/architecture/` (notably `BACKEND_SCHEMA_OUTLINE_V0.1`, `BACKEND_PERSISTENCE_AND_DATABASE_PLAN_V0.1`, `BACKEND_EFCORE_MIGRATION_STRATEGY_V0.1`, `BACKEND_SEED_DATA_PLAN_V0.1`, `BACKEND_AI_RISK_AUDIT_PLACEHOLDERS_V0.1`, `BACKEND_IMPLEMENTATION_GATES_V0.1`, `BACKEND_SECURITY_AND_DEMO_BOUNDARIES_V0.1`).

This document defines the exact architecture and sequencing for finishing the **local** application — SQL Server persistence, write workflows, and a DeepSeek-backed AI layer — *before* the owner's manual acceptance walkthrough and before any Azure work. It changes no code; every implementation step is deferred to a named future gate (see `LOCAL_COMPLETION_GATE_SEQUENCE_V0.1.md`).

---

## 1. Current repo state confirmed

| Fact | Value |
|---|---|
| Repo | `slavkan777/InsuranceAIPlatform` (public) |
| Branch / HEAD | `dev` / `f8df2b6` (`feat: make read-only UI actions honest`) |
| `origin/dev` | `f8df2b6` |
| `origin/main` | `69e6731` (stable portfolio branch, not promoted) |
| Working tree | clean at planning time |
| Frontend | React 18 + TypeScript 5 + Vite 5 + Redux Toolkit + Redux-Saga + Tailwind; 11 routes/screens |
| Frontend API seam | `src/api/insuranceApi.ts` facade → `mockInsuranceApi.ts` (default) or `backendInsuranceApi.ts` (GET-only fetch client) via `VITE_INSURANCE_API_MODE` + `VITE_INSURANCE_API_BASE_URL` |
| Backend | .NET 9 ASP.NET Core Web API; attribute-routed controllers; Swashbuckle; xUnit + `WebApplicationFactory<Program>` (14 tests) |
| Backend data | `IClaimReadService` → `InMemoryClaimReadService` (singleton, deterministic, **no DB**) |
| Read endpoints | 11 GET endpoints; golden claim `CLM-1006` full graph + 4 list-only stub claims (CLM-1007..1010) |
| Read integration | All 11 screens render backend data in backend mode with non-lossy mock fallback (committed on `dev`) |
| Action honesty | Write-looking / dead actions deferred via `DeferredActionButton`; AI run is explicit local mock; no write paths reach backend |

**The single clean seam for everything that follows:** `Program.cs:9` registers `AddSingleton<IClaimReadService, InMemoryClaimReadService>()`. Persistence and AI work swap implementations behind interfaces without touching controllers or DTOs.

---

## 2. Product target before owner manual walkthrough

A fully local, deterministic Auto-Insurance Claim AI Workbench where:

1. A local **SQL Server** database `InsuranceAIPlatform` is the system of record (read + write), replacing the in-memory service behind the existing interfaces.
2. The DB is seeded with deterministic synthetic data, **including exactly 200 synthetic test users**, while the golden `CLM-1006` demo path is preserved byte-for-byte.
3. A small set of **safe, audited write commands** exist (save approval draft, request missing document, append audit event, human approval-submit) — none of which authorize payout or send real messages.
4. A **DeepSeek-backed AI provider layer** behind an abstraction produces advisory, structured, audited analysis — **disabled by default**, mock by default, real only when explicitly configured locally.
5. Every screen/process shows an **AI agent presence** that is advisory-only and fully auditable.
6. The app passes a full local-machine verification (build, tests, EF migrate, seed-count = 200, read/write smoke, audit assertions, AI mock + DeepSeek-disabled-by-default, secret-scan) before the owner manually walks all 11 screens.

Everything remains **local**; Azure and `main` promotion are explicitly out of scope until later gates.

---

## 3. Non-goals / forbidden scope (this plan and the gates it defines)

- No production insurer integration, no real payout, no real customer SMS/email.
- AI never makes a final claim decision, never authorizes payout, never asserts fraud as fact.
- No real PII — synthetic data only, permanently.
- No secrets in the repo. `DEEPSEEK_API_KEY` and DB connection strings live only in local user-secrets / environment, never committed, never logged, never printed.
- No Azure, Docker, or cloud resources in the local-completion phase.
- `main` stays the stable portfolio branch; no promotion until the final release gate.
- This planning gate changes **no** `src/**` / `server/**` code, creates **no** DB/migrations, makes **no** AI calls.

---

## 4. Target local architecture

```
React (Vite) ── insuranceApi facade ──┬─ mockInsuranceApi  (default; offline)
  Redux Toolkit + Saga                └─ backendInsuranceApi (GET reads + NEW guarded writes)
                                              │  http://localhost:5284
                                              ▼
        ASP.NET Core (.NET 9)  Controllers (read GET + NEW write POST)
                  │
        ┌─────────┴───────────┐
   IClaimReadService     IClaimWriteService / command handlers  (NEW)
        │                      │
   IClaimReadRepository ◄──────┘      IAiProvider (NEW)
        │                                  ├─ MockAiProvider (default)
   EF Core  InsuranceAiDbContext           ├─ DeepSeekAiProvider (opt-in, disabled by default)
        │                                  └─ DisabledAiProvider
   SQL Server  DB = InsuranceAIPlatform    (deterministic .NET rules ALWAYS override AI)
```

Layering decisions:
- **Storage behind interfaces.** `InMemoryClaimReadService` → an EF-backed `IClaimReadRepository` implementation (per `BACKEND_PERSISTENCE_AND_DATABASE_PLAN_V0.1`). Controllers/DTOs unchanged.
- **Writes are commands, not CRUD.** Each write is an explicit command handler with validation + a mandatory audit event. No generic entity mutation from the UI.
- **AI is a side advisory service.** Deterministic .NET rules (risk threshold, document completeness, benchmark deviation) are the source of truth; AI augments explanations and extraction only.

### What changes vs. stays
| Area | Before manual walkthrough |
|---|---|
| Becomes DB-backed | All 11 read models; approval drafts; audit events; AI run records |
| Becomes write-enabled (guarded) | Save approval draft · request missing document · append audit event · human approval-submit |
| Becomes AI-backed (advisory, opt-in) | AI Evidence findings/extraction; risk *explanation* (not the score); approval *recommendation* |
| Stays mock/deterministic | Default app mode (no DB/AI needed to demo); risk *score* + governance = deterministic .NET rules |
| Stays frontend-only | Demo scenario player; local UI toggles/filters/tabs |
| Waits for Azure | Hosting, managed SQL, blob storage, real provider keys in cloud, CI/CD |

---

## 5. Data model / DB schema proposal

Adopt the **23-entity schema already defined in `BACKEND_SCHEMA_OUTLINE_V0.1`** unchanged in shape (DB `InsuranceAIPlatform`, `dbo`, string PKs, NVARCHAR), with **one addition** for the new requirement:

- **`TestUsers`** (NEW) — `TestUserId` NVARCHAR(20) PK; `FullName`, `Email`, `Role` (adjuster/reviewer/admin — demo only, no auth), `CustomerId` NULL FK→`Customers` (links a subset of test users to synthetic customers), `IsSeed` BIT, `CreatedAt`. This carries the **exactly-200** synthetic-user requirement without overloading `Customers`.

Existing 23 entities (reference only — defined in the schema-outline doc): `Customers`, `Vehicles`, `Policies`, `PolicyCoverages`, `PolicyExclusions`, `Claims`, `ClaimDocuments`, `ClaimPhotos`, `DocumentChecklistItems`, `AiAnalysisRuns`, `AiFindings`, `EvidenceSources`, `ExtractedEntities`, `RiskAssessments`, `RiskFactors`, `PolicyCheckResults`, `ApprovalDrafts`, `HumanDecisionOptions`, `AuditTraces`, `AuditEvents`, `CostTelemetryEvents`, `DemoScenarios`, `DemoSteps`.

**Discrepancies to reconcile during the persistence gate (recorded now, not fixed here):**
1. `CustomerId` is `CUST-001` in the planning docs but `CUST-4421` in `InMemoryClaimReadService.cs`. **Decision: the implemented value `CUST-4421` wins** (it is what the read API + tests already return); align the seed doc to it.
2. Connection-string naming diverges between docs: `ConnectionStrings:Default` / `CONNECTIONSTRINGS__DEFAULT` (EF doc) vs `ConnectionStrings:InsuranceAIPlatform` / `INSURANCEAIPLATFORM_DB` (security doc). **Decision: standardize on user-secrets key `ConnectionStrings:InsuranceAIPlatform` and env var `INSURANCEAIPLATFORM_DB`** in the persistence gate; update the EF doc.
3. `AuditTraceDto.Model` is currently the string `"Azure OpenAI (mock)"`. Governance requires a non-misleading label. **Decision: change to `"Demo/Synthetic"` (or `"DeepSeek (mock)"`) when the AI gate lands.**

---

## 6. EF Core boundary and migration strategy

Adopt `BACKEND_EFCORE_MIGRATION_STRATEGY_V0.1` as-is, with the reconciliations above:
- **DbContext:** `InsuranceAiDbContext` in `InsuranceAIPlatform.Infrastructure.Persistence` (new `Infrastructure/Persistence/` folder inside the Api project, or a new Infrastructure project — decided in the persistence gate).
- **Provider:** EF Core SQL Server (`UseSqlServer`), pinned to the .NET 9 EF Core line; packages added **only** in the persistence gate.
- **Migrations:** `Infrastructure/Persistence/Migrations/`; naming `<timestamp>_<PascalCaseDescription>` (`InitialCreate`, `AddTestUsers`, `AddClaimIndexes`).
- **One `IEntityTypeConfiguration<T>` per entity**, registered via `ApplyConfigurationsFromAssembly`.
- **Design-time factory** `InsuranceAiDbContextFactory` reads the connection string from env / gitignored dev settings and **asserts the DB name == `InsuranceAIPlatform`** before returning a context (hard guard against touching `DevDept`).
- **Connection-string strategy (no secrets committed):** priority user-secrets `ConnectionStrings:InsuranceAIPlatform` → env `INSURANCEAIPLATFORM_DB` → `appsettings.Development.json` (gitignored). Committed `appsettings.json` carries only a placeholder + README pointer.
- **Swap path:** keep `IClaimReadService`/`IClaimReadRepository`; register the EF implementation in `Program.cs` instead of `InMemoryClaimReadService`. Controllers and DTOs do not change.

---

## 7. Seed data strategy

Per `BACKEND_SEED_DATA_PLAN_V0.1`: a **runtime idempotent seeder** (`GoldenClaimSeeder` + new `TestUserSeeder`), invoked only in Development, **not** `HasData`-baked into migrations; deterministic hardcoded string PKs, fixed ISO timestamps, UPSERT-by-stable-id semantics, transaction-wrapped, tracked by a `SeedVersion` table. Every seed/reset script asserts DB name == `InsuranceAIPlatform` first.

Two seed tiers:
1. **Golden tier (preserve exactly):** `CLM-1006` full graph + the 4 list-only stub claims (CLM-1007..1010) + the dashboard `ClaimSummaryDto` values, all matching the current in-memory values (see §9).
2. **Bulk tier (new):** the 200 synthetic test users + a modest spread of synthetic claims/policies/vehicles so the list/dashboard look populated — **without** disturbing the golden demo path.

---

## 8. 200 synthetic test users requirement

- Seed **exactly 200** rows in `TestUsers`, deterministic IDs `TUSER-0001` … `TUSER-0200` (zero-padded, stable).
- Deterministic synthetic identities (faker-style but seeded, no randomness, no real PII): names from a fixed synthetic pool, emails `tuser0001@demo.local` … `tuser0200@demo.local`, rotating demo roles.
- A defined subset (e.g. first 20) links to synthetic `Customers` to make claim ownership realistic; the rest are standalone demo accounts.
- **Verification (acceptance-blocking):** `SELECT COUNT(*) FROM TestUsers WHERE IsSeed = 1` must equal **200** exactly — not 199, not 201. The full-local-verification gate asserts this count.
- No authentication/authorization is implemented — these are demo records only; "role" is a display attribute.

---

## 9. Claims / policies / vehicles / documents / audit seed model (golden CLM-1006)

Preserve the implemented golden values exactly (source: `InMemoryClaimReadService.cs`):
- **Claim:** `CLM-1006` · customer `Роберт Джонсон` (`CUST-4421`) · `Toyota Camry 2021` (`VIN ****8842`) · policy `Auto Comprehensive` (`POL-2025-AC-4421`) · ДТП · 2026-05-18 · Бориспіль · status `В роботі` · risk `Високий`/`82` · confidence `78`.
- **Money:** estimate `2720.00` · benchmark `1970.00` (+38%) · deductible `500.00` · recommended payout `1800.00`.
- **AI run:** `run_8f3d2a7e` / `trc_8f3d2a7e` · tokens `4261` · cost `0.0187` · duration `18.9s`.
- **Documents:** 6/7 received; missing = rear-bumper damage photo. **Photos:** front 92 / side 87 / rear missing.
- **Risk factors:** 5 rows summing to 82 (amount 25 / mismatch 18 / missing-photo 22 / prior 8 / confidence 9), threshold 60.
- **Audit:** 6 events, overall `BLOCK` ("auto-approval blocked: risk > 75"). **Cost distribution:** extract/rag/risk/reco.
- **Approval draft:** action `request`, recommended payout `1800`, not submitted.
- **Dashboard summary:** TotalActive 47 / PendingReview 12 / HighRisk 8 / AvgSlaRemainingHours 14.3 / ProcessedToday 6 / AiAnalysisRunning 2.

These become DB rows during the persistence gate; the read endpoints' output must remain identical so the frontend and the 14 existing tests stay green.

---

## 10. Read endpoint plan

No new read endpoints are required for local completion — the 11 existing GETs already cover all 11 screens (`/api/claims/summary`, `/api/claims`, `/api/claims/{id}`, `/{id}/documents`, `/{id}/ai-evidence`, `/{id}/risks`, `/{id}/policy`, `/{id}/customer-vehicle`, `/{id}/approval`, `/{id}/audit`, `/api/demo/scenario`). The persistence gate keeps these contracts byte-identical, only changing the data source from in-memory to EF. Error contract unchanged: 400 `INVALID_CLAIM_ID`, 404 `CLAIM_NOT_FOUND`, `TraceId` echoed.

---

## 11. Write endpoint / command workflow plan

Four safe, audited write commands. Each = endpoint + command DTO + validation + mandatory `AuditEvent` append + forbidden behavior + tests. **All are reversible/local and none authorize payout or send real messages.**

| Command | Endpoint (candidate) | DTO | Validation | Audit event | Forbidden |
|---|---|---|---|---|---|
| Save approval draft | `POST /api/claims/{id}/approval/draft` | `{ decision?, notes? }` | claim exists; decision ∈ options; notes ≤ N chars | `ApprovalDraftSaved` | setting final status |
| Request missing document | `POST /api/claims/{id}/documents/request` | `{ documentType }` | claim exists; type is a known missing item | `DocumentRequested` (local only) | sending real SMS/email |
| Append audit note | `POST /api/claims/{id}/audit/event` | `{ action, details }` | claim exists; action whitelisted | the event itself | arbitrary actor spoofing |
| Human approval-submit | `POST /api/claims/{id}/approval/submit` | `{ decision, notes }` | claim exists; **human actor**; decision ∈ {approve, reject, request, escalate}; risk-gate acknowledged | `ApprovalSubmitted` + status transition | AI invoking this; payout execution |

Rules (enforced server-side):
- AI cannot call approval-submit; only a human-initiated request can set Approved/Rejected (governance INV-5).
- No real payout, no real customer messaging — "request document" / "send to customer" are local audited records with demo labels.
- Status transitions are deterministic (see §12); writes that violate the state machine are rejected with a typed error.

---

## 12. Human approval / status transition rules

Deterministic state machine (server-authoritative; AI advisory only):

```
В роботі / Збір документів / AI-обробка
        │ (human approval-submit)
        ├─ approve  → Готова → (later) Завершено      [blocked if risk>threshold & not acknowledged]
        ├─ reject   → Відхилено
        ├─ request  → Збір документів (audited document request)
        └─ escalate → На ескалації
```
- Auto-approval is **never** allowed when `RiskScore > threshold` (CLM-1006: 82 > 60 → human required); the audit overall result stays `BLOCK` until a human acts.
- Every transition writes an `AuditEvent` with actor = the human, and is rejected if the source state doesn't permit the target.
- AI may *recommend* a transition (advisory `ApprovalDraft.RecommendedAction`) but cannot execute it.

---

## 13. Audit trail and event model

Reuse `AuditTraces` + `AuditEvents` (+ `CostTelemetryEvents`) from the schema. Every write command and every AI run appends an append-only `AuditEvent` (`time, actor, action, result ∈ {OK,WARN,BLOCK}, details, sequenceOrder`) under a claim's `AuditTrace`. Invariant: **if the audit event (or the AI cost entry) fails to persist, the operation is not considered successful and its result is not returned** (governance INV-3). The Audit/Cost screen reads this trail unchanged.

---

## 14. DeepSeek provider abstraction

Introduce an AI provider abstraction (aligns with the `ILlmProvider`/`IAiAnalysisProvider` hint in `BACKEND_AI_RISK_AUDIT_PLACEHOLDERS_V0.1`):

```
IAiProvider
  ├─ MockAiProvider       (default; deterministic synthetic output; offline)
  ├─ DeepSeekAiProvider   (opt-in; calls DeepSeek; DISABLED by default)
  └─ DisabledAiProvider   (explicit no-op; returns "AI disabled" advisory)
```

- **Mode selection** via config `Ai:ProviderMode ∈ { mock, deepseek, disabled }`, default **`mock`**. Real calls happen **only** when mode is explicitly `deepseek` AND a key is present.
- **Key handling:** `DEEPSEEK_API_KEY` read **only** from local environment / `dotnet user-secrets` at runtime. Never committed, never logged, never printed, never in reports. The repo references only the *name* of the variable.
- **Models:** default `deepseek-v4-flash`; optional `deepseek-v4-pro` via `Ai:Model`.
- **Output:** structured JSON mapped to existing DTO shapes (findings, extracted entities, evidence refs). Every result carries `confidence`, `evidenceRefs`, `tokenUsage`, `costEstimate`, `provider`, `model`, `runId`, `auditStatus`, and `fallbackReason` when mock/fallback was used.
- **Adapter is isolated** (`Infrastructure/Ai/DeepSeekAiProvider.cs`), behind `IAiProvider`, with an HTTP timeout, cancellation, and structured-output parsing; failures fall back to `MockAiProvider` with a recorded `fallbackReason` (never a silent fake-success).

---

## 15. Mock / fallback provider strategy

- **Default everywhere is mock** — the app demos fully with no key and no network. This preserves the current honest "Local Demo / mock" labels and the deterministic CLM-1006 numbers.
- **Fallback is explicit:** if `deepseek` mode is selected but the call fails (no key, timeout, parse error), the system returns the mock result **with** `provider = "mock-fallback"` and a `fallbackReason`, and records an audit event — it never presents a fabricated success as a real provider result (mirrors the frontend's existing `apiMode:'mock-fallback'` pattern and the project's stub-fallback-detection rule).
- **Disabled-by-default test (acceptance-blocking):** a test asserts that with no `Ai:ProviderMode` override, no DeepSeek HTTP call is made.

---

## 16. AI-agent presence per screen / process

Advisory-only on every screen; deterministic rules and human decisions are always authoritative.

| # | Screen / process | AI agent presence | Reads | May write | Must never decide | Audit | FE / data dependency |
|---|---|---|---|---|---|---|---|
| 1 | Dashboard / summary | Portfolio-level "AI workload" + recommendation widget (advisory copy) | summary, AI run stats | — | queue priority as fact | run refs | summary endpoint |
| 2 | Claims List | Per-row AI status chip (advisory) | claims list, aiStatus | — | claim outcome | — | claims endpoint |
| 3 | Claim Workspace | AI recommendation card (advisory) + next-step hint | claim detail, draft | — | final disposition | run/trace ref | claim detail endpoint |
| 4 | Documents/Photos | Doc/photo completeness + extraction confidence | documents, photos | request missing doc (audited, local) | mark verified as fact | DocumentRequested | documents endpoint + write |
| 5 | AI Evidence | The core AI analysis: findings, extracted entities, confidence | ai-evidence | — (re-run is mock/opt-in) | fraud as fact | AiRun + cost | ai-evidence endpoint + provider |
| 6 | Risks | AI **explanation** of factors; score stays deterministic | risks | — | the score itself | run ref | risks endpoint |
| 7 | Policy/Coverage | AI policy-clause check assist (advisory) | policy | — | coverage as legal fact | check result | policy endpoint |
| 8 | Customer/Vehicle | Context summarization (advisory) | customer-vehicle | — | risk profile as fact | — | customer-vehicle endpoint |
| 9 | Human Approval | AI recommendation + draft; human decides | approval read | save draft / submit (human) | approve/reject/payout | ApprovalDraftSaved / Submitted | approval endpoints + writes |
| 10 | Audit/Cost | Surfaces AI run cost/tokens/trace (read of governance) | audit | — | hide/alter audit | n/a (is the audit) | audit endpoint |
| 11 | Demo Scenario | Scripted guided-tour narration only | demo scenario | — | anything | — | demo endpoint (read) |

---

## 17. AI guardrails (governance invariants — carry forward INV-1..5)

1. **Advisory only** — AI never auto-approves, auto-rejects, or sets final claim status.
2. **No fraud-as-fact** — "elevated/high risk score" is allowed; "fraud confirmed" is forbidden.
3. **Mandatory audit + cost** — every AI run emits an audit event and a cost entry and returns `runId`/`traceId`; if either fails, the result is not returned.
4. **Honest labels** — provider/model label reflects reality (`Demo/Synthetic` or `mock-fallback` unless a real DeepSeek call actually happened).
5. **Human gate** — only a human-initiated `approval/submit` can set Approved/Rejected.
6. **Deterministic override** — .NET rules (risk threshold, completeness, benchmark deviation) outrank AI; AI cannot relax a `BLOCK`.

---

## 18. Cost / token / audit metadata

Every AI invocation records (persisted in `AiAnalysisRuns` + `CostTelemetryEvents`, surfaced on Audit/Cost): `model`, `tokenInput`, `tokenOutput`, `totalTokens`, `costEstimateUsd`, `durationSec`, `provider`, `runId`, `traceId`, `auditStatus`, and `fallbackReason` if applicable. Cost distribution by stage (extract/rag/risk/reco) is retained for the cost chart. Golden CLM-1006 keeps its exact synthetic figures (tokens 4261, cost 0.0187, 18.9s) for the mock path.

---

## 19. Frontend integration impact

- **Reads:** zero shape changes — DTOs stay identical, so the facade/mappers and all 11 screens are unaffected by the persistence gate.
- **Writes:** the existing `DeferredActionButton`-deferred actions (save draft, request document, approval submit) get **enabled** behind the facade once their write endpoints exist — re-classified from `DEFERRED_DISABLED` to a real (local, audited) action, still default-mock. New `POST` methods are added to `backendInsuranceApi.ts` only in the write gate; mock client keeps stubs.
- **AI:** the "Перезапустити mock-аналіз" action remains mock by default; a real run is only possible in `deepseek` mode. The advisory/"human decides" copy and honest labels are preserved.
- **No visual redesign**; the accepted V3 UI baseline and mock-default behavior are preserved.

---

## 20. Testing and verification strategy

Per `BACKEND_VERIFICATION_PLAN_V0.1`, the later gates must each prove:
- `dotnet build` 0/0; `dotnet test` green (≥ 14, growing as gates add tests).
- EF migration applies cleanly to a local `InsuranceAIPlatform` DB; DB-name guard proven (DevDept untouched).
- **Seed count = exactly 200 test users** (`COUNT(*)` assertion).
- Read-endpoint smoke (all 11) returns the golden CLM-1006 shapes unchanged.
- Write-endpoint smoke: each command persists + appends the expected audit event; state-machine rejections work.
- AI provider: mock tests deterministic; **DeepSeek-disabled-by-default test** (no HTTP call without explicit mode); fallback records `fallbackReason`.
- **Secret-scan**: no `sk-`/`Bearer `/`password=`/`Server=`/`Data Source=`/`DEEPSEEK_API_KEY` value / `.env` in the diff before any push.
- Frontend `npm run build` PASS; backend-mode + mock-mode smoke.
- No Azure verification (explicitly out of scope until the Azure gates).

---

## 21. Gate sequence after this plan

See `LOCAL_COMPLETION_GATE_SEQUENCE_V0.1.md` for the machine-readable, per-gate contract. Summary order:
1. `SQLSERVER_EFCORE_PERSISTENCE_IMPLEMENTATION_V0.1`
2. `COMMIT_AND_PUSH_DEV_SQLSERVER_EFCORE_PERSISTENCE_ONLY`
3. `WRITE_ACTIONS_BACKEND_IMPLEMENTATION_V0.1`
4. `COMMIT_AND_PUSH_DEV_WRITE_ACTIONS_ONLY`
5. `AI_PROVIDER_LOCAL_INTEGRATION_AND_GUARDRAILS_V0.1`
6. `COMMIT_AND_PUSH_DEV_AI_PROVIDER_GUARDRAILS_ONLY`
7. `FULL_LOCAL_MACHINE_VERIFICATION_BEFORE_SLAVA_MANUAL_V0.1`
8. `PRE_AZURE_FULL_LOCAL_MANUAL_ACCEPTANCE_CHECKPOINT`
9. `POST_MANUAL_ACCEPTANCE_FIX_BATCH_V0.1`
10. `AZURE_DEPLOYMENT_PLANNING_V0.1`
11. `AZURE_MINIMAL_DEPLOYMENT_IMPLEMENTATION_V0.1`
12. `POST_AZURE_FINAL_VERIFICATION_V0.1`
13. `MAIN_PROMOTION_PORTFOLIO_RELEASE_V0.1`

Each implementation gate is followed by its own commit/push gate, keeping the implement → verify → commit → push separation (project rule G7).

---

## 22. Risks and mitigations

| Risk | Mitigation |
|---|---|
| Accidentally touching `DevDept` DB | Hard DB-name assert in design-time factory + every seed/reset script; connection string never points at DevDept |
| Secret leakage (`DEEPSEEK_API_KEY`, conn string) | user-secrets/env only; pre-push secret scan; placeholder-only committed config; this plan never reads the key |
| Real AI cost / accidental live calls | mode default `mock`; `deepseek` requires explicit config + key; disabled-by-default test |
| Golden CLM-1006 drift after DB migration | seed values pinned to current in-memory values; read tests assert unchanged shapes |
| Seed count off-by-one | exact `COUNT(*) = 200` acceptance gate |
| AI overreach (deciding/accusing) | governance invariants INV-1..6 enforced server-side; deterministic override |
| Doc/code discrepancies (CustomerId, conn-string key, model label) | reconciled in §5; fixed during persistence/AI gates, not now |
| Scope creep into Azure/main early | gate sequence forbids it until gates 10–13 |

---

## 23. STOP boundaries

This plan authorizes **no** implementation. Do not, in this gate: create a DB, add EF Core, create migrations, run seed, implement endpoints, change frontend behavior, call DeepSeek, read/log/print `DEEPSEEK_API_KEY`, touch Azure, commit/push source, or modify `main`. Each capability is unlocked **only** by its named gate, each of which carries its own STOP line.
