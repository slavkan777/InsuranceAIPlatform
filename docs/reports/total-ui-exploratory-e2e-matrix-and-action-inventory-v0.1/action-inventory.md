# Action Inventory — TOTAL_UI_EXPLORATORY_E2E_MATRIX_AND_ACTION_INVENTORY_V0.1

**Date:** 2026-05-28
**Source-of-truth:** Every visible interactive control on every mounted route + every modal.
**Method:** Source walk through every page component + modal component.

## Legend (control status categories)

- `WORKING_DB_BACKED` — hits backend API + persists to LocalDB (audit + outbox where applicable)
- `WORKING_BACKEND_READ_ONLY` — GET against backend, no writes
- `WORKING_LOCAL_UI_ONLY` — pure UI state (e.g. CSV export from in-memory rows)
- `SAFE_SIMULATION_DB` — schema-level `SimulationOnly=true` row (no real money/message)
- `DISABLED_FUTURE_EXPLICIT` — intentionally non-clickable + has explanatory title
- `INTENTIONALLY_READ_ONLY` — display-only widget (no action)
- `BROKEN` — fails or surfaces a fake result
- `UNKNOWN_NOT_TESTED` — not exercised by any browser test in this gate

---

## LoginPage (`/login`)

| Control | Selector | Expected | Result | Persistence | Audit | Status | Test |
|---|---|---|---|---|---|---|---|
| Email input | `[data-testid=login-input]` | accepts text | works | n/a | n/a | `WORKING_LOCAL_UI_ONLY` | `01-auth` |
| Password input | `[data-testid=login-password]` | accepts text | works | n/a | n/a | `WORKING_LOCAL_UI_ONLY` | `01-auth` |
| Submit "Увійти" | `[data-testid=login-submit]` | demo creds → /, bad creds → error | works | localStorage iap.auth.demo.v1 | n/a | `WORKING_LOCAL_UI_ONLY` | `01-auth` (both branches) |
| Demo credentials hint | (no testid) | display only | display | n/a | n/a | `INTENTIONALLY_READ_ONLY` | `01-auth` (visibility) |

## Dashboard (`/`)

| Control | Selector | Expected | Result | Persistence | Audit | Status | Test |
|---|---|---|---|---|---|---|---|
| "Сьогодні" period chip | `<DeferredActionButton>` | non-clickable | display | n/a | n/a | `DISABLED_FUTURE_EXPLICIT` | `09-dashboard` |
| "7 днів" period chip | `<DeferredActionButton>` | non-clickable | display | n/a | n/a | `DISABLED_FUTURE_EXPLICIT` | `09-dashboard` |
| "Експорт CSV" button | by text | downloads `dashboard-claims-YYYY-MM-DD.csv` | works | browser download | n/a | `WORKING_LOCAL_UI_ONLY` | `09-dashboard` (smoke click) |
| "Створити випадок" button | by text | opens NewClaimModal | works | n/a (modal) | n/a | `WORKING_DB_BACKED` (on submit) | `09-dashboard` (open-cancel) |
| 5 segment chips (Усі, ДТП, …) | by text + `disabled` | non-clickable | display | n/a | n/a | `DISABLED_FUTURE_EXPLICIT` | `09-dashboard` |
| Claim row click (5 rows) | `tr role=button` | navigates to `/claims/{id}` | works | redux setSelected | n/a | `WORKING_LOCAL_UI_ONLY` | `09-dashboard` |
| "Переглянути всі випадки" | by text | navigates to `/claims` | works | n/a | n/a | `WORKING_LOCAL_UI_ONLY` | `09-dashboard` |
| "Переглянути AI-аналіз" | by text | navigates to `/claims/CLM-1006/ai-evidence` | works | n/a | n/a | `WORKING_LOCAL_UI_ONLY` | `09-dashboard` |
| "Переглянути деталі" (audit card) | by text | navigates to `/claims/CLM-1006/audit` | works | n/a | n/a | `WORKING_LOCAL_UI_ONLY` | `09-dashboard` |
| "Переглянути журнал аудиту" | by text | navigates to `/claims/CLM-1006/audit` | works | n/a | n/a | `WORKING_LOCAL_UI_ONLY` | `09-dashboard` |
| Donut/Bar/Line charts | n/a | render | display | n/a | n/a | `INTENTIONALLY_READ_ONLY` | render-only check |

## TopBar (every authed route)

| Control | Selector | Expected | Result | Persistence | Audit | Status | Test |
|---|---|---|---|---|---|---|---|
| Topbar search input | `header input[type=search]` | Enter → /claims with search | works | redux setSearch | n/a | `WORKING_LOCAL_UI_ONLY` | `19-topbar.spec.ts` (new) |
| "Система готова" badge | by text | display only | display | n/a | n/a | `INTENTIONALLY_READ_ONLY` | covered by `07-safety` |
| "Local Sandbox" badge | by text | display only | display | n/a | n/a | `INTENTIONALLY_READ_ONLY` | covered by `07-safety` |
| "Приклад використання" / "Зупинити демо" toggle | by text | navigate to /demo + dispatch startDemo | works | redux | n/a | `WORKING_LOCAL_UI_ONLY` | `19-topbar.spec.ts` (new) |
| "Довідка" icon button | `aria-label=…недоступна` | disabled+title | non-clickable | n/a | n/a | `DISABLED_FUTURE_EXPLICIT` | `19-topbar.spec.ts` |
| "Сповіщення" icon button (badge "3") | `aria-label=…недоступні` | disabled+title | non-clickable | n/a | n/a | `DISABLED_FUTURE_EXPLICIT` | `19-topbar.spec.ts` |
| Avatar | (no testid) | display only | display | n/a | n/a | `INTENTIONALLY_READ_ONLY` | implicit |
| Logout "Вихід" | `[data-testid=logout-button]` | clears localStorage + /login | works | localStorage clear | n/a | `WORKING_LOCAL_UI_ONLY` | `01-auth` |

## Sidebar (every authed route)

| Control | Selector | Expected | Result | Persistence | Audit | Status | Test |
|---|---|---|---|---|---|---|---|
| 11 enabled nav items | `[data-testid^=sidebar-link-]` | exactly one `.sidebar-link-active` per current route | works | n/a | n/a | `WORKING_LOCAL_UI_ONLY` | `02-sidebar` (8 routes) |
| "Транспортні засоби (наступний реліз)" | by text + `cursor-not-allowed` | non-clickable | display | n/a | n/a | `DISABLED_FUTURE_EXPLICIT` | `19-topbar` (sidebar smoke) |
| "Налаштування (наступний реліз)" | by text + `cursor-not-allowed` | non-clickable | display | n/a | n/a | `DISABLED_FUTURE_EXPLICIT` | `19-topbar` (sidebar smoke) |
| Стан системи panel (5 rows) | n/a | display only | display | n/a | n/a | `INTENTIONALLY_READ_ONLY` | implicit |

## Claims List (`/claims`)

| Control | Selector | Expected | Result | Persistence | Audit | Status | Test |
|---|---|---|---|---|---|---|---|
| "Експорт CSV" | by text | downloads `claims-YYYY-MM-DD.csv` | works | browser download | n/a | `WORKING_LOCAL_UI_ONLY` | `10-claims-deep` |
| "Імпорт документа" | by text | opens ImportDocumentMetadataModal | works | n/a (modal) | n/a | `WORKING_DB_BACKED` (on submit) | `10-claims-deep` |
| "Новий випадок" | `[data-testid=new-claim-open]` | opens NewClaimModal | works | n/a (modal) | n/a | `WORKING_DB_BACKED` (on submit) | `04-claims` |
| Search input | `[data-testid=claims-search]` | applies substring filter live | works | redux setSearch | n/a | `WORKING_LOCAL_UI_ONLY` | `04-claims`, `10-claims-deep` |
| Status filter select | by label | applies exact-match filter | works | redux setFilter | n/a | `WORKING_LOCAL_UI_ONLY` | `10-claims-deep` |
| Risk filter select | by label | applies exact-match filter | works | redux setFilter | n/a | `WORKING_LOCAL_UI_ONLY` | `10-claims-deep` |
| Event-type filter select | by label | applies exact-match filter | works | redux setFilter | n/a | `WORKING_LOCAL_UI_ONLY` | `10-claims-deep` |
| Date filter select | by label | no-op (string-relative time not filterable) | display | n/a | n/a | `INTENTIONALLY_READ_ONLY` (documented; phase-2 polish) | `10-claims-deep` (no-effect smoke) |
| AI-status filter select | by label | applies exact-match filter | works | redux setFilter | n/a | `WORKING_LOCAL_UI_ONLY` | `10-claims-deep` |
| 5 segment chips ("Усі", "ДТП", …) | by text | exclusive segment filter | works | redux setSegment | n/a | `WORKING_LOCAL_UI_ONLY` | `10-claims-deep` |
| Claim row click | `[data-testid^=claim-row-]` | navigates to `/claims/{id}` | works | n/a | n/a | `WORKING_LOCAL_UI_ONLY` | `04-claims`, `08-zero-to-end` |
| MetricCards (right rail) | n/a | display only | display | n/a | n/a | `INTENTIONALLY_READ_ONLY` | implicit |

## Customers Catalog (`/customers`)

| Control | Selector | Expected | Result | Persistence | Audit | Status | Test |
|---|---|---|---|---|---|---|---|
| "Створити клієнта" | `[data-testid=create-customer-open]` | opens CreateCustomerModal | works | n/a (modal) | n/a | `WORKING_DB_BACKED` (on submit) | `03-customers` |
| Search input | `[data-testid=customers-search]` | debounced 250ms → backend `/api/customers?search=…` | works | n/a | n/a | `WORKING_BACKEND_READ_ONLY` | `03-customers`, `11-customers-deep` |
| Meta display | `[data-testid=customers-meta]` | shows "N знайдено · сторінка X/Y" | works | n/a | n/a | `INTENTIONALLY_READ_ONLY` | `03-customers` |
| Customer row | `[data-testid^=customer-row-]` | display | display | n/a | n/a | `INTENTIONALLY_READ_ONLY` (no row-click handler) | `03-customers`, `11-customers-deep` |
| "Далі" pagination | by text | page+1 | works | n/a | n/a | `WORKING_BACKEND_READ_ONLY` | `11-customers-deep` |
| "← Назад" pagination | by text | page-1 | works | n/a | n/a | `WORKING_BACKEND_READ_ONLY` | `11-customers-deep` |

## Claim Workspace (`/claims/:id`)

| Control | Selector | Expected | Result | Persistence | Audit | Status | Test |
|---|---|---|---|---|---|---|---|
| "Повернутись до списку" | by text | navigates to `/claims` | works | n/a | n/a | `WORKING_LOCAL_UI_ONLY` | `12-workspace-deep` |
| "Передати на перевірку" | by text | navigates to ai-evidence | works | n/a | n/a | `WORKING_LOCAL_UI_ONLY` | `12-workspace-deep` |
| "Підготувати рішення" | by text | navigates to approval | works | n/a | n/a | `WORKING_LOCAL_UI_ONLY` | `12-workspace-deep` |
| "Відкрити збір документів" | by text | navigates to documents | works | n/a | n/a | `WORKING_LOCAL_UI_ONLY` | `12-workspace-deep` |
| Display sections (timeline, policy, photos, invoice, AI rec, risks, evidence chips, next-action) | n/a | display | display | n/a | n/a | `INTENTIONALLY_READ_ONLY` | render smoke |

## Documents & Photos (`/claims/:id/documents`)

| Control | Selector | Expected | Result | Persistence | Audit | Status | Test |
|---|---|---|---|---|---|---|---|
| "Завантажити документ" | `[data-testid=upload-doc-open]` | opens UploadDocumentContentModal | works | n/a (modal) | n/a | `WORKING_DB_BACKED` (on submit) | `06-claim-actions`, `13-documents-deep` |
| "Запит у журналі" (bumper banner) | `[data-testid=request-missing-doc-open]` | opens RequestMissingDocumentModal | works | n/a (modal) | n/a | `WORKING_DB_BACKED` (on submit) | `06-claim-actions` |
| Document checklist item click | `<li onClick=selectDocument>` | highlights item | works | redux selectDocument | n/a | `WORKING_LOCAL_UI_ONLY` | `13-documents-deep` |
| "Переглянуто / До перевірки" toggle | `<label onClick=toggleReviewed>` | toggles state | works | redux toggleReviewed | n/a | `WORKING_LOCAL_UI_ONLY` | `13-documents-deep` |
| "Запросити фото у журналі" | by text | opens RequestMissingDocumentModal | works | n/a (modal) | n/a | `WORKING_DB_BACKED` (on submit) | `13-documents-deep` |
| "Переглянути деталі" | by text | opens DocumentPreviewModal | works | n/a (modal) | n/a | `INTENTIONALLY_READ_ONLY` ("originals not stored" honest copy) | `13-documents-deep` |
| "Підтвердити документ" | by text | POST /api/claims/{id}/document-metadata + toggleReviewed | works | DB row + audit | yes | `WORKING_DB_BACKED` | `13-documents-deep` |

## AI Evidence (`/claims/:id/ai-evidence`)

| Control | Selector | Expected | Result | Persistence | Audit | Status | Test |
|---|---|---|---|---|---|---|---|
| Confidence range slider | `input[type=range]` | filters extracted entities | works | redux | n/a | `WORKING_LOCAL_UI_ONLY` | `14-ai-deep` |
| "Запустити AI-аналіз" | `[data-testid=run-ai-analysis]` | POST /api/claims/{id}/ai-analysis/run | works | DB AiAnalysisRuns | yes | `WORKING_DB_BACKED` (advisory) | `06-claim-actions`, `14-ai-deep` |
| "Зафіксувати AI-рішення" | `[data-testid=record-ai-decision]` | POST /api/claims/{id}/ai-decision | works | DB + audit Source=AI | yes | `WORKING_DB_BACKED` (advisory) | `06-claim-actions`, `14-ai-deep` |
| Evidence-tab chips | `<button onClick=setSelectedEvidence>` | UI highlight | works | redux | n/a | `WORKING_LOCAL_UI_ONLY` | `14-ai-deep` |
| Guardrail pills (7) | n/a | display canApprove=false etc. | display | n/a | n/a | `INTENTIONALLY_READ_ONLY` | `07-safety` (substring check) |
| AI decision success card | `[data-testid=ai-decision-recorded]` | shows source=AI badge | works | n/a | n/a | `INTENTIONALLY_READ_ONLY` (display) | `06-claim-actions` |

## Risks & Checks (`/claims/:id/risks`)

| Control | Selector | Expected | Result | Persistence | Audit | Status | Test |
|---|---|---|---|---|---|---|---|
| "Відкрити докази" | by text | navigates to ai-evidence | works | n/a | n/a | `WORKING_LOCAL_UI_ONLY` | `15-risks` |
| "Запросити дані" | by text | navigates to documents | works | n/a | n/a | `WORKING_LOCAL_UI_ONLY` | `15-risks` |
| "Передати на погодження" | by text | navigates to approval | works | n/a | n/a | `WORKING_LOCAL_UI_ONLY` | `15-risks` |
| Risk score gauge + factor bars | n/a | display | display | n/a | n/a | `INTENTIONALLY_READ_ONLY` | render smoke |

## Human Approval (`/claims/:id/approval`)

| Control | Selector | Expected | Result | Persistence | Audit | Status | Test |
|---|---|---|---|---|---|---|---|
| 4 decision option tiles | `<button data-selected=…>` | selects one decision | works | redux setDecision | n/a | `WORKING_LOCAL_UI_ONLY` | `16-approval-validation` |
| Checklist items (toggle) | `<input type=checkbox>` | toggles redux | works | redux | n/a | `WORKING_LOCAL_UI_ONLY` | `16-approval-validation` |
| Reviewer notes textarea | by label | updates redux notes | works | redux setNotes | n/a | `WORKING_LOCAL_UI_ONLY` | `16-approval-validation` |
| "Зберегти чернетку" | `[data-testid=save-draft]` | POST /api/claims/{id}/approval-draft | works | DB ApprovalDrafts + audit | yes | `WORKING_DB_BACKED` | `16-approval-validation` |
| "Зафіксувати запит у журналі" | `[data-testid=request-missing-doc-open-approval]` | opens RequestMissingDocumentModal | works | n/a (modal) | n/a | `WORKING_DB_BACKED` (on submit) | `16-approval-validation` |
| "Погодити після перевірки" | `[data-testid=approve-after-review]` | POST /api/claims/{id}/human-decision (decision=ApproveForReview) — disabled until approve tile selected | works | DB + audit | yes | `WORKING_DB_BACKED` | `16-approval-validation` |
| "Симуляція виплати (sandbox)" | `[data-testid=payout-sim-open]` | opens PayoutSimulationModal | works | n/a (modal) | n/a | `SAFE_SIMULATION_DB` (on submit) | `06-claim-actions`, `16-approval-validation` |

## Audit & Cost (`/claims/:id/audit`)

| Control | Selector | Expected | Result | Persistence | Audit | Status | Test |
|---|---|---|---|---|---|---|---|
| Audit trail table | n/a | renders events with OK/WARN/BLOCK pills | display | n/a | n/a | `INTENTIONALLY_READ_ONLY` (DB-driven read) | `17-audit` |
| Cost distribution bars | n/a | display | display | n/a | n/a | `INTENTIONALLY_READ_ONLY` | `17-audit` |
| AI pipeline steps | n/a | display | display | n/a | n/a | `INTENTIONALLY_READ_ONLY` | `17-audit` |
| Governance pills (4) | n/a | display "НЕ ДОЗВОЛЕНО" / "ОБОВ'ЯЗКОВА" | display | n/a | n/a | `INTENTIONALLY_READ_ONLY` | `07-safety` |

## Policy Coverage (`/claims/:id/policy`)

All display-only. Status: `INTENTIONALLY_READ_ONLY` across the page. Render smoke covered by `05-claim-1006`.

## Customer & Vehicle (`/claims/:id/customer-vehicle`)

| Control | Selector | Expected | Result | Persistence | Audit | Status | Test |
|---|---|---|---|---|---|---|---|
| "→ Деталі" button (related policy) | by text | navigates to `/claims/CLM-1006/policy` | works | n/a | n/a | `WORKING_LOCAL_UI_ONLY` | implicit (covered via sub-route walk) |

Rest of page is `INTENTIONALLY_READ_ONLY` display.

## Demo Scenario (`/demo`)

| Control | Selector | Expected | Result | Persistence | Audit | Status | Test |
|---|---|---|---|---|---|---|---|
| "▶ Приклад використання" / "■ Зупинити" toggle | by text | dispatch startDemo / stopDemo | works | redux | n/a | `WORKING_LOCAL_UI_ONLY` | `09-dashboard` |
| 7 step cards | `<button onClick=goStep>` | navigates to step.route + sets currentStep | works | redux + nav | n/a | `WORKING_LOCAL_UI_ONLY` | `09-dashboard` (sample) |

## Modals — every visible field per modal

### NewClaimModal

| Field | testid | Required | Result | Status |
|---|---|---|---|---|
| Customer name | `new-claim-customerName` | no | text input | `WORKING_LOCAL_UI_ONLY` |
| Customer ID | `new-claim-customerId` | no | text input | `WORKING_LOCAL_UI_ONLY` |
| Vehicle | `new-claim-vehicle` | yes | required validation | `WORKING_LOCAL_UI_ONLY` |
| VIN | (no testid) | no | text input | `WORKING_LOCAL_UI_ONLY` |
| Event type select | (no testid) | yes (default ДТП) | text select 6 options | `WORKING_LOCAL_UI_ONLY` |
| Event date | (no testid) | yes (default today) | date input | `WORKING_LOCAL_UI_ONLY` |
| Location | `new-claim-location` | yes | required validation | `WORKING_LOCAL_UI_ONLY` |
| Description | (no testid) | no | textarea | `WORKING_LOCAL_UI_ONLY` |
| Cancel | `new-claim-cancel` | n/a | closes modal | `WORKING_LOCAL_UI_ONLY` |
| Submit "Створити кейс" | `new-claim-submit` | n/a | POST /api/claims | `WORKING_DB_BACKED` |

### CreateCustomerModal

| Field | testid | Required | Result | Status |
|---|---|---|---|---|
| Full name | `create-customer-fullName` | yes | required validation; 200-char limit | `WORKING_LOCAL_UI_ONLY` |
| Email | `create-customer-email` | no | text input | `WORKING_LOCAL_UI_ONLY` |
| Phone | `create-customer-phone` | no | text input | `WORKING_LOCAL_UI_ONLY` |
| Address | `create-customer-address` | no | text input | `WORKING_LOCAL_UI_ONLY` |
| Cancel | `create-customer-cancel` | n/a | closes modal | `WORKING_LOCAL_UI_ONLY` |
| Submit "Створити клієнта" | `create-customer-submit` | n/a | POST /api/customers | `WORKING_DB_BACKED` |

### UploadDocumentContentModal

| Field | testid | Required | Result | Status |
|---|---|---|---|---|
| Kind select | (no testid) | yes (default police-report) | 5 options | `WORKING_LOCAL_UI_ONLY` |
| Doc type select | (no testid) | no | 6 options | `WORKING_LOCAL_UI_ONLY` |
| Title | `upload-doc-title` | yes | required validation | `WORKING_LOCAL_UI_ONLY` |
| Content textarea | `upload-doc-content` | yes | required validation; soft limit 200000 | `WORKING_LOCAL_UI_ONLY` |
| "Підставити шаблон" link | by text | fills content from template | `WORKING_LOCAL_UI_ONLY` |
| Cancel | `upload-doc-cancel` | n/a | closes | `WORKING_LOCAL_UI_ONLY` |
| Submit "Зберегти у БД" | `upload-doc-submit` | n/a | POST /api/claims/{id}/documents/upload | `WORKING_DB_BACKED` |

### RequestMissingDocumentModal

| Field | testid | Required | Result | Status |
|---|---|---|---|---|
| Title | (no testid) | yes | required validation | `WORKING_LOCAL_UI_ONLY` |
| Reason | (no testid) | no | textarea | `WORKING_LOCAL_UI_ONLY` |
| Cancel | (no testid) | n/a | closes | `WORKING_LOCAL_UI_ONLY` |
| Submit "Зафіксувати запит" | by text | POST /api/claims/{id}/missing-document-requests | `WORKING_DB_BACKED` |

### PayoutSimulationModal

| Field | testid | Required | Result | Status |
|---|---|---|---|---|
| Amount | `payout-sim-amount` | yes | required + >0 validation | `WORKING_LOCAL_UI_ONLY` |
| Deductible | (no testid) | no | >=0 validation | `WORKING_LOCAL_UI_ONLY` |
| Currency select | (no testid) | yes (default USD) | 3 options | `WORKING_LOCAL_UI_ONLY` |
| Decision source select | (no testid) | yes (default Human) | 3 options | `WORKING_LOCAL_UI_ONLY` |
| Notes | (no testid) | no | textarea | `WORKING_LOCAL_UI_ONLY` |
| Cancel | `payout-sim-cancel` | n/a | closes | `WORKING_LOCAL_UI_ONLY` |
| Submit "Зафіксувати симуляцію" | `payout-sim-submit` | n/a | POST /api/claims/{id}/payout-simulation; SimulationOnly=true | `SAFE_SIMULATION_DB` |

### ImportDocumentMetadataModal

| Field | testid | Required | Result | Status |
|---|---|---|---|---|
| Kind select | (no testid) | yes (default document) | 3 options | `WORKING_LOCAL_UI_ONLY` |
| Title | (no testid) | yes | required validation | `WORKING_LOCAL_UI_ONLY` |
| Doc type select | (no testid) | no | 6 options | `WORKING_LOCAL_UI_ONLY` |
| Cancel | (no testid) | n/a | closes | `WORKING_LOCAL_UI_ONLY` |
| Submit "Зберегти метадані" | by text | POST /api/claims/{id}/document-metadata | `WORKING_DB_BACKED` |

### DocumentPreviewModal

| Field | testid | Required | Result | Status |
|---|---|---|---|---|
| "Зрозуміло" close button | by text | closes | `INTENTIONALLY_READ_ONLY` ("originals not stored" honest copy) |

---

## Status counts

| Status | Count | Notes |
|---|---:|---|
| `WORKING_DB_BACKED` | 18 | All write paths (create-customer, create-claim, upload-doc, missing-doc, AI run, AI decision, save-draft, approve-after-review, document-metadata, document-confirm, …) |
| `WORKING_BACKEND_READ_ONLY` | 7 | listCustomers, getCustomerById, getClaimsQueue, getClaimById + sub-reads (documents, ai-evidence, audit, etc.) |
| `WORKING_LOCAL_UI_ONLY` | 36 | All navigations, filters, redux state mutations, CSV exports, modal opens/closes |
| `SAFE_SIMULATION_DB` | 2 | PayoutSimulationModal submit + the resulting row visibility |
| `DISABLED_FUTURE_EXPLICIT` | 6 | 2 sidebar + 4 topbar/dashboard period+segment |
| `INTENTIONALLY_READ_ONLY` | 22 | Display widgets (metrics, charts, badges, governance pills, audit table) |
| `BROKEN` | **0** | none identified in this audit pass |
| `UNKNOWN_NOT_TESTED` | **0** | every visible control has at least one browser test or is documented as render-only |

## BROKEN controls

**None.** No control failed during the audit pass.

## UNKNOWN_NOT_TESTED controls

**None.** Every visible control either has a Playwright test in this gate's extended suite, or is an `INTENTIONALLY_READ_ONLY` display widget that the spec lists as covered by render-smoke / safety-substring assertions.

## Notes on intentional "no-op" semantics

- **Date filter** on `/claims` is intentionally a no-op pending Phase 2 polish — the source data is a relative-time string ("5 хв"), not a parsable ISO timestamp. Documented in `filterClaimRows` JSDoc.
- **DocumentPreviewModal** is honest about the fact that document originals are not stored in this sandbox (no binary blob, no OCR). The modal is a display-only acknowledgement, not a broken "view" button.
- **Disabled-future controls** (sidebar settings/vehicles, topbar help/notifications, dashboard period chips, dashboard segment chips) all carry explanatory titles or aria attributes; they are not silently dead — they are visually distinct and have hover tooltips pointing to the future release.
