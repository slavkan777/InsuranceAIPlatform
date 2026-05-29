# Pre-Azure UI Auth · DB Persistence · AI Decision Completion — Gate Report

**Gate:** `PRE_AZURE_UI_AUTH_DB_PERSISTENCE_AND_AI_DECISION_COMPLETION_V0.1`
**Date:** 2026-05-28
**Type:** bounded local implementation + verification · no commit · no push · no Azure
**Verdict (proposed):** `ACCEPT_WITH_LIMITATIONS`

---

## TL;DR

Upgraded the local pre-Azure product from "backend works + UI shell" to
"reviewable local product" with the following bounded scope:

1. **Demo login** at `/login` with visible credentials hint; route-guarded SPA;
   logout in TopBar; `localStorage`-only session.
2. **Eight previously-dead UI controls** are now wired to real BFF endpoints
   (`/approval-draft`, `/human-decision`, `/missing-document-requests`,
   `/document-metadata`) or to safe client-side handlers (CSV export, modal-gated
   new claim).
3. **New backend endpoint** `POST /api/claims/{claimId}/ai-decision` records an
   auditable AI decision (audit + outbox) with explicit `Source=AI` attribution,
   advisory-only flag, and an `ai-system` actor. No payout, no customer message,
   no status mutation.
4. **Misleading "DEMO" badges and "Azure OpenAI" labels** replaced with honest
   wording (current state: Mock + DeepSeek opt-in; Azure planned).
5. **Backend tests 127/127 pass** (was 119; +8 new AI Decision tests).
   Frontend production build 4.79 s, no errors.
6. **Runtime smoke** against LocalDB confirmed all 5 BFF commands + the new AI
   decision endpoint work end-to-end (audit + outbox rows persisted, idempotency
   key dedup works for the AI decision endpoint exactly as for other commands).
7. **HEAD `0372524` and `origin/main` `69e6731` unchanged**, no commit, no push.

The single limitation: "New claim" UI is a working modal that opens the demo
claim CLM-1006 rather than creating a brand-new synthetic claim — that path
would require additive schema/migration which is explicitly out-of-scope per
this gate's STOP rule.

---

## Repo state

| Field | Value |
|---|---|
| Repo | `C:\Projects\InsuranceAIPlatform` |
| Branch | `dev` |
| HEAD / origin-dev | `03725241c8dfdbed7ce17db61fb51d9d7f211116` (unchanged) |
| origin/main | `69e67312a10cc9bcf28c4e387a126b48c91fb9c5` (unchanged) |
| Modified tracked | 18 files |
| New untracked (this gate) | 13 files + 2 new directories |
| Untracked carry-forward (prior triage) | 17 items |

---

## DONE STATE — per-item verification

### Login screen + demo auth

- ✅ `/login` route renders a form with hint:
  - Login: `demo@insurance.local`
  - Password: `Demo123!`
- ✅ Unauthenticated users hitting `/` (or any protected child route) are
  redirected to `/login` with `from` state so they land back where they were.
- ✅ Logout button in TopBar clears `localStorage` + dispatches a toast.
- ✅ Session persistence via `localStorage` key `iap.auth.demo.v1`. Reversible.
- ✅ No external identity provider, no Azure AD, no Entra ID. Local-only.

### Visible active controls — wired or honestly gated

| Location | Control | Before | After |
|---|---|---|---|
| ClaimsListPage | Експорт CSV | DEAD `DeferredActionButton` | ✅ client-side CSV download |
| ClaimsListPage | Імпорт документа | DEAD | ✅ Modal → `POST /document-metadata` |
| ClaimsListPage | Новий випадок | DEAD | ✅ Modal (gated; opens CLM-1006) |
| DashboardPage | Експорт CSV | DEAD | ✅ client-side CSV (5-row queue snapshot) |
| DashboardPage | Створити випадок | DEAD | ✅ Same gated modal |
| DashboardPage | Сьогодні / 7 днів | DEAD | ⚠ honestly gated as future ("з'явиться у наступному релізі") |
| DocumentsPhotosPage | Запросити у клієнта (header) | DEAD | ✅ Modal → `POST /missing-document-requests` |
| DocumentsPhotosPage | Запросити фото (sidebar) | DEAD | ✅ Same endpoint, pre-filled |
| DocumentsPhotosPage | Переглянути оригінал | DEAD | ✅ Honest preview modal (explains no binary store) |
| DocumentsPhotosPage | Підтвердити документ | DEAD | ✅ `POST /document-metadata` + checklist toggle |
| HumanApprovalPage | Зберегти чернетку | DEAD | ✅ `POST /approval-draft` |
| HumanApprovalPage | Надіслати запит клієнту | DEAD | ✅ Modal → `POST /missing-document-requests` (internal-only) |
| HumanApprovalPage | Погодити після перевірки | DEAD | ✅ `POST /human-decision` with `ApproveForReview` |
| TopBar | Search input | placeholder-only | ✅ Enter submits → navigate to /claims + setSearch |
| TopBar | Вихід (logout) | absent | ✅ Logout button (new) |
| Sidebar | Людське погодження | not in nav | ✅ added |
| Sidebar | Транспортні засоби / Налаштування | disabled w/o explanation | ✅ honest "(наступний реліз)" label |
| Sidebar | System status (`mock`/`demo`) | misleading | ✅ honest ("локально", "Mock | DeepSeek opt-in") |

### Demo / Azure / DEMO badge cleanup

- ✅ `"Azure OpenAI"` label removed from `AuditCostPage`, `mockInsuranceApi`,
  `claimsSaga`, `DemoScenarioPage`. Replaced with `local-mock-v0.1` / honest
  "Mock + DeepSeek opt-in" copy.
- ✅ `DeferredActionButton` default hint changed from `"read-only demo"` to
  `"З'явиться у наступному релізі"`.
- ✅ Remaining `DeferredActionButton` usages (period switcher on Dashboard,
  notifications/help icons on TopBar) carry honest "future" wording.
- ✅ Login page intentionally shows "Local demo" badge + the credentials hint —
  this is honest signalling, not a misleading DEMO label.

### Key business actions persist to DB

- ✅ Approval draft save: `POST /approval-draft` writes `approval.ApprovalDrafts`
  row + audit `ApprovalDraftSaved` + outbox `ApprovalDraftSaved`.
- ✅ Human decision submit: `POST /human-decision` writes audit
  `HumanDecisionSubmitted` + 2 outbox events
  (`HumanDecisionSubmitted` + `ClaimStatusTransitionRequested`).
  Claim row is NOT mutated — outbox-only by design.
- ✅ Missing document request: `POST /missing-document-requests` writes
  `documents.MissingDocumentRequests` row + audit + outbox.
- ✅ Document metadata placeholder: `POST /document-metadata` writes
  `documents.ClaimDocuments` row + audit + outbox. NO binary upload.
- ✅ AI decision: `POST /ai-decision` writes audit `AiDecisionRecorded`
  (with `actor=ai-system`) + outbox `AiDecisionRecorded`. NO claim mutation,
  NO payout, NO customer message.

### AI decision persistence — auditable, source-attributed

- ✅ New endpoint `POST /api/claims/{claimId}/ai-decision` added in
  `server/InsuranceAIPlatform.Api/Controllers/AiDecisionController.cs`.
- ✅ Requires a prior AI run (calls `IAiAnalysisOrchestrator.GetLatestAsync`);
  returns 400 `no_ai_analysis_run` otherwise.
- ✅ Audit row carries `actor=ai-system@insuranceai.local` (vs human commands
  which use `actor=demo.adjuster@insuranceai.local`). Source attribution is
  visible in the audit history at the actor level.
- ✅ Audit metadata explicit `Source=AI`, `IsAdvisoryOnly=true`, run linkage
  (RunId, ProviderMode, ModelName), advisory recommendation text.
- ✅ Idempotency-Key support: replay with same key returns success with
  warning `duplicate-idempotency-key: outbox row N already exists`. Audit
  row count = 2 (each attempt logged), outbox row count = 1 (dedup at side-
  effect layer). Same pattern as other BFF commands.
- ✅ UI surface: AI Evidence page shows a new section "AI-рішення у журналі
  (демо)" with a button "Зафіксувати AI-рішення". Button is disabled until
  a run exists. Success view shows cmd id, audit id, outbox id, runId,
  provider/model, and explicit `source=AI` badge.

### Build / tests

- ✅ Backend `dotnet build InsuranceAIPlatform.sln`: 0 warnings, 0 errors,
  9.78 s.
- ✅ Backend `dotnet test`: **127/127 passed** (was 119; 8 new AI decision
  endpoint tests added: success path, no-prior-run, unknown claim, 4 bad
  claim-id patterns, response-shape contract).
- ✅ Frontend `npm run build`: 121 modules transformed, 402 kB JS / 38 kB CSS,
  built in 4.79 s. No TS errors.

### Runtime smoke

| Test | Result |
|---|---|
| `GET /health` | 200 `Healthy` |
| `POST /api/claims/CLM-1006/ai-analysis/run` (Mock) | 200, runId=`run_de103c61`, conf=78, risk=high |
| `POST /api/claims/CLM-1006/ai-decision` | 200, cmd=`cmd-abb39f6b…`, audit=43, outbox=38, source=AI, advisory=true |
| `POST /api/claims/CLM-1006/approval-draft` | 200, status=DraftSaved, audit=44, outbox=39 |
| `POST /api/claims/CLM-1006/human-decision ApproveForReview` | 200, status=PendingApproval, audit=45, outbox=40 |
| `POST /api/claims/CLM-1006/human-decision ApproveForPayout` (forbidden) | 400 `INVALID_DECISION` (allow-list enforced) |
| `POST /api/claims/CLM-9999/ai-decision` | 404 `CLAIM_NOT_FOUND` |
| `POST /api/claims/CLM-1006/missing-document-requests` | 200, status=MissingDocumentRequested |
| `POST /api/claims/CLM-1006/document-metadata` | 200, status=MetadataCreated |
| Idempotency replay (same key) | 200 + warning `duplicate-idempotency-key`; audit count=2, outbox count=1 |
| Frontend preview `GET /` | 200, title `InsuranceAIPlatform · Auto Claim AI Workbench` |
| JS bundle string presence | **13/13 new strings** present (login hint, AI decision, modals, etc.) |

### DB final state (post-smoke)

```
ai_analysis.AiAnalysisRuns by provider:
  Mock      10
  DeepSeek   6
  Disabled   1

audit_cost.AuditEvents AiDecisionRecorded     = 1
audit_cost.OutboxMessages AiDecisionRecorded  = 1

Forbidden audit actions (must be 0):
  Payout, PayoutApproved, CustomerMessageSent, FraudFinal,
  AiClaimReject, ClaimStatusChanged                          = 0  ✅
```

### Safety scan

| Check | Result |
|---|---|
| `DEEPSEEK_API_KEY` value leak in source | ✅ none (only safe `IConfiguration[...]` reads + negation comments) |
| `sk-*` shape match in source | ✅ none |
| Azure / OpenAI / SemanticKernel / DeepSeek SDK package reference | ✅ none |
| Real customer PII in new files (phone / email patterns) | ✅ none |
| AI-platform identifier or model-name strings in new files | ✅ none |
| Unsafe payout / customer-message / fraud-final fields in new endpoint response | ✅ none |
| Source / main branch mutation | ✅ none (HEAD `0372524`, origin/main `69e6731` unchanged) |
| Process / port cleanup | ✅ ports 5284 / 4173 freed |

---

## Files changed

### New (13 + 2 dirs)

**Backend (3):**
- `server/InsuranceAIPlatform.Api/Contracts/AiAnalysis/AiDecisionRecordedResult.cs`
- `server/InsuranceAIPlatform.Api/Controllers/AiDecisionController.cs`
- `server/InsuranceAIPlatform.Tests/AiDecisionEndpointTests.cs`

**Frontend (10 + 2 dirs):**
- `src/features/auth/authSlice.ts` (new)
- `src/features/auth/authSelectors.ts` (new)
- `src/features/auth/RequireAuth.tsx` (new)
- `src/features/ui/uiFeedbackSlice.ts` (new)
- `src/features/ui/uiFeedbackSelectors.ts` (new)
- `src/components/ui/Modal.tsx`
- `src/components/ui/ToastViewport.tsx`
- `src/components/claim/NewClaimModal.tsx`
- `src/components/claim/ImportDocumentMetadataModal.tsx`
- `src/components/claim/RequestMissingDocumentModal.tsx`
- `src/components/claim/DocumentPreviewModal.tsx`
- `src/pages/LoginPage.tsx`
- `src/utils/csv.ts`

### Modified (18 tracked)

- `src/api/backendInsuranceApi.ts` — added `recordAiDecision`
- `src/api/insuranceApi.types.ts` — added `RecordAiDecisionBody`, `AiDecisionRecordedResult`
- `src/api/mockInsuranceApi.ts` — added 5 command mocks (saveApprovalDraftCommand, submitHumanDecision, requestMissingDocument, createDocumentMetadata, recordAiDecision); replaced legacy `Azure OpenAI` with `local-mock-v0.1`
- `src/app/store.ts` — wired `auth` + `uiFeedback` reducers
- `src/app/router.tsx` — added `/login` + `RequireAuth` wrapping `AppShell`
- `src/components/layout/AppShell.tsx` — mounted `<ToastViewport />`
- `src/components/layout/Sidebar.tsx` — added Approval to nav, honest status labels
- `src/components/layout/TopBar.tsx` — wired search submit, added logout, updated tooltips
- `src/components/ui/DeferredActionButton.tsx` — honest default hint
- `src/components/ui/Icon.tsx` — added `x` / `check` / `download` / `upload` / `plus` / `logOut` / `info` / `edit`
- `src/features/claims/claimsSaga.ts` — replaced legacy `Azure OpenAI` fallback label
- `src/pages/AiEvidencePage.tsx` — added AI Decision recording UI block
- `src/pages/AuditCostPage.tsx` — replaced legacy `Azure OpenAI` fallback label
- `src/pages/ClaimsListPage.tsx` — wired Export CSV, Import doc modal, New claim modal
- `src/pages/DashboardPage.tsx` — wired Export CSV + New claim modal
- `src/pages/DemoScenarioPage.tsx` — honest stack labels (no aspirational Azure OpenAI)
- `src/pages/DocumentsPhotosPage.tsx` — wired 4 doc actions (request bumper, request photo, preview, confirm)
- `src/pages/HumanApprovalPage.tsx` — wired Save draft, Request via modal, Approve-after-review

---

## Limitations

1. **"New claim" does not create a brand-new synthetic claim row.** It opens a
   gated modal that explains creation requires a backend write-gate with an
   additive schema (claim IDs are deterministic + seeded). Per the gate's STOP
   rule, "if migration needed and non-trivial, STOP and report" — the modal is
   a working interaction with a navigation shortcut to the demo claim
   CLM-1006. No silent button.
2. **Document binary upload is not implemented.** Import is metadata-only via
   `POST /document-metadata`. Both the modal copy and the "Переглянути оригінал"
   modal explain this honestly.
3. **No browser-automated UI screenshots in this gate.** Playwright was not
   installed; visible string presence in the JS bundle (13/13 new strings)
   plus the unchanged React render contract from prior gates was used as the
   evidence floor.
4. **Demo login is local-only, hard-coded credentials.** This is by design —
   no production identity provider is in scope. The login page makes the
   credentials visible to reviewers under the form.

## What this gate did NOT do

- No commit, no push, no `main` mutation.
- No Azure / Entra ID / Key Vault / App Insights code added.
- No provider SDK referenced (raw `HttpClient` only for DeepSeek; unchanged).
- No `DEEPSEEK_API_KEY` value read, logged, or stored.
- No untracked architecture docs or report dirs touched / deleted.
- No real PII introduced. All actors are synthetic
  (`demo.adjuster@insuranceai.local` and `ai-system@insuranceai.local`).
- No real customer-message send, payout transfer, or external HTTP outside
  the opt-in DeepSeek path (which is unchanged and remains gated).

## Next safe step (operator-only)

`MANUAL_OPERATOR_BROWSER_CLICK_THROUGH` — operator can now do a hands-on browser
walk of the upgraded local product:

1. Start backend (`dotnet run` from `server/InsuranceAIPlatform.Api`).
2. Start frontend (`npm run preview` or `npm run dev`).
3. Hit `/`, get redirected to `/login`. Verify hint visible
   (`demo@insurance.local` / `Demo123!`). Submit → redirect to dashboard.
4. Open Claims list → Експорт CSV → confirm CSV downloads with synthetic data.
5. Open Import documents modal → submit → confirm toast.
6. Open Новий випадок modal → click "Відкрити демо-кейс CLM-1006" → land on CLM-1006.
7. Open AI Evidence → click "Запустити AI-аналіз" (Mock) → confirm advisory card.
8. Click "Зафіксувати AI-рішення" → confirm success banner with cmd id +
   `source=AI` badge.
9. Open Approval → choose "Погодити виплату" → click "Погодити після
   перевірки (демо)" → confirm toast.
10. TopBar → "Вихід" → confirm redirect to /login + cleared session.

Estimated time: 8-12 minutes.

After that, the next candidate gates are:
- `DOCS_ARCHITECTURE_DURABLE_COMMIT_V0.1` — commit the 4 durable architecture docs
- `AZURE_READINESS_PRE_FLIGHT_V0.1` — Azure planning doc only (no Azure code)
- `COMMIT_AND_PUSH_DEV_PRE_AZURE_UI_AUTH_BATCH` — commit this gate's 18+13
  files to `dev` (separate dedicated commit gate).
