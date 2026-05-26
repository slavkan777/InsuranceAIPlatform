# InsuranceAIPlatform — Auto Insurance Claim AI Workbench

**Frontend walking skeleton.** Visualises an end-to-end AI-assisted workflow for auto insurance claim adjusters, with mandatory human review and full audit/cost governance.

This repo is a **portfolio mockup**: it is _frontend only_, runs entirely on synthetic data, and makes **no network calls, no AI provider calls, and stores no credentials**. The design follows an accepted Penpot deliverable: [`docs/design/penpot-final-11-screens.pdf`](docs/design/penpot-final-11-screens.pdf).

---

## Frontend architecture (interview)

Not just screens — an intentional, staged architecture:
- **React + TypeScript + Vite**, **React Router** (11 route-based product steps over one golden claim).
- **Redux Toolkit** for shared UI/domain-view state; **typed selectors** (`features/*/*Selectors.ts`) decouple pages from store shape.
- **Redux-Saga** for workflow orchestration + the future side-effect seam (AI run, document request, approval draft/send, guided demo) — never for trivial toggles.
- **Mock API boundary** (`src/api/mockInsuranceApi.ts`) — the single seam a real **.NET backend** replaces later with no UI rewrite.
- **AI is advisory; a human always approves; every AI run is audited and costed** — no autonomous payout/rejection.

Full docs: [`docs/architecture/`](docs/architecture/) — architecture overview, route/state/saga ownership, mock-API boundary, ADRs, backend-contract readiness, interview answer pack, and the ready-for-backend checklist.

---

## Business scenario — the "golden claim"

| Field | Value |
|---|---|
| Claim ID | `CLM-1006` |
| Customer | Роберт Джонсон (synthetic) |
| Vehicle | Toyota Camry 2021 · VIN `****8842` |
| Policy | Auto Comprehensive · `POL-2025-AC-4421` |
| Event | ДТП 18.05.2026, Бориспіль |
| Risk | Високий · 82 / 100 |
| Model confidence | 78 % |
| Documents received | 6 / 7 (missing: rear bumper photo) |
| Repair invoice vs benchmark | $2 720 vs $1 970 (+38 %) |
| Recommended payout | $1 800 |
| Trace / Run ID | `trc_8f3d2a7e` / `run_8f3d2a7e` |
| AI run cost / tokens / time | $0.0187 · 4 261 tokens · 18.9 s |

The skeleton shows the seven-step lifecycle of this claim: registration → documents → AI analysis → risks → human review → audit → completion.

---

## Stack (frontend only)

| Layer | Choice | Why |
|---|---|---|
| Build | **Vite 5** | Fast dev server, modern ESM, low config |
| Language | **TypeScript 5** (strict) | Type-safety on every slice, page, and prop |
| UI | **React 18** | Industry default |
| Routing | **React Router 6** | Nested routes for `/claims/:id/...` workspace |
| Styling | **Tailwind CSS 3** | Utility-first; design system in `tailwind.config.js` |
| State | **Redux Toolkit 2** | Six feature slices, typed hooks, no thunks |
| Async / effects | **Redux-Saga 1** | Long-running flows: mock AI run, document request, approval save/send, demo auto-advance |

### Redux Toolkit + Redux-Saga decision

Modern Redux Toolkit (`createSlice` + `configureStore`) covers all synchronous state mutation: filters, segment chips, tab toggles, checklist items, drafts.

Redux-Saga is **only** used for explicitly multi-step / cancellable flows that simulate backend work:

| Flow | Slice | Saga |
|---|---|---|
| Run mock AI analysis (stepped progress) | `aiReviewSlice` | `aiReviewSaga` |
| Request missing document via SMS+email | `documentsSlice` | `documentsSaga` |
| Save approval draft / send customer request | `approvalSlice` | `approvalSaga` |
| Guided demo scenario auto-advance | `demoSlice` | `demoSaga` |

Simple UI toggles (tabs, filters, opening a modal) live **inside slice reducers**, never as saga effects.

---

## Route map

| Route | Page | PDF reference |
|---|---|---|
| `/` | DashboardPage | Огляд автострахових випадків |
| `/claims` | ClaimsListPage | Автострахові випадки |
| `/claims/CLM-1006` | ClaimWorkspacePage | Робоче місце випадку |
| `/claims/CLM-1006/documents` | DocumentsPhotosPage | Документи та фото |
| `/claims/CLM-1006/ai-evidence` | AiEvidencePage | AI-аналіз та докази |
| `/claims/CLM-1006/risks` | RisksChecksPage | Ризики та перевірки |
| `/claims/CLM-1006/approval` | HumanApprovalPage | Людське погодження |
| `/claims/CLM-1006/audit` | AuditCostPage | Аудит і витрати |
| `/claims/CLM-1006/policy` | PolicyCoveragePage | Поліс і покриття |
| `/claims/CLM-1006/customer-vehicle` | CustomerVehiclePage | Клієнт і транспортний засіб |
| `/demo` | DemoScenarioPage | Demo Flow |

The sidebar and top bar are visible on every route. Claim-scoped routes share a `ClaimShell` outlet with sub-tabs.

---

## Project structure

```
src/
  app/            store, hooks, rootSaga, router
  pages/          11 page components — one per route
  features/       slices + sagas by domain
    claims/       claimsSlice, claimWorkspaceSlice
    documents/    documentsSlice, documentsSaga
    aiReview/     aiReviewSlice, aiReviewSaga
    approval/     approvalSlice, approvalSaga
    demo/         demoSlice, demoSaga
  components/
    layout/       AppShell, Sidebar, TopBar, ClaimShell
    ui/           MetricCard, StatusPill, ProgressBar, SectionHeader
    claim/        ClaimHeader, Timeline
  data/mock/      claims, dashboard, claim-1006 (golden claim data)
  types/          shared TypeScript types
  utils/          tiny helpers (clsx)
docs/
  design/         source PDF + implementation-checklist.md
  screenshots/    one PNG per route (when capture tooling is available)
```

---

## Synthetic data policy

- **No real PII** anywhere. Phone, email, VIN are masked: `+1 (555) ***-2147`, `robert.j****@demo.com`, `VIN ****8842`.
- **No live customer records** — every claim row, document, audit log, and cost line is hard-coded mock data.
- **No external API or AI provider** is ever contacted. The "AI run" is a `delay()` saga that walks through a stepped progress bar.
- **No credentials in source** — there is no `.env`, no API key, no token, no secret of any kind.

---

## How to run locally

Requirements: Node.js **18+** (developed against 21), npm **10+**.

```bash
npm install
npm run dev          # Vite dev server at http://127.0.0.1:5173
npm run build        # tsc -b && vite build → dist/
npm run preview      # serve the production build at http://127.0.0.1:4173
```

There is no test suite or linter wired in this skeleton — keeping scope tight to UI + state + design fidelity.

---

## Current limitations

- **Frontend only.** No backend, no database, no migrations. State is in-memory; refreshing the page resets it.
- **Mocked AI.** All AI work is a saga `delay()`. No tokens are billed; cost numbers are illustrative.
- **Static seed data.** You cannot create a new claim, upload a real photo, or trigger a real notification. Buttons that would do that show a success message based on saga simulation only.
- **Polished but not pixel-perfect.** A few PDF visuals (charts, raster previews, photographic thumbnails) are stylised — see `docs/design/implementation-checklist.md` for the full deviation list.

---

## Future phases (planned, not in this branch)

1. **.NET backend local skeleton** — ASP.NET Core 9 with CQRS / MediatR, Clean Architecture layers, no cloud yet. CRUD over claims + documents.
2. **Mock AI pipeline** — wire the saga calls to a backend service that returns the same shapes, still without a real provider.
3. **Cheap real AI provider** — swap the mock pipeline for a real LLM (Azure OpenAI or a budget-friendly alternative) gated by config.
4. **Evidence / RAG layer** — document embedding + retrieval to support the AI findings panel with real grounding.
5. **Guardrails & evaluators** — content filters, confidence thresholds, automatic regression evals over the golden claim.
6. **Azure cost architecture** — Application Insights, App Service / Container Apps, Blob storage, cost dashboards.
7. **Minimal Azure deployment** — single-region demo deploy behind a free / hobby SKU footprint.

These will land in subsequent branches; this one is intentionally bounded to "ship-ready visual + interactive walking skeleton".
