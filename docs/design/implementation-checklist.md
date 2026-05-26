# Penpot PDF → React walking skeleton — Implementation Checklist

Source design: [`docs/design/penpot-final-11-screens.pdf`](./penpot-final-11-screens.pdf)
Implementation date: 2026-05-26
Scope: **Frontend walking skeleton only** — no backend, no real AI, no Azure, no API keys.
Stack: React + TypeScript + Vite + Tailwind + React Router + Redux Toolkit + Redux-Saga.

Legend: `✓` implemented to design intent · `~` partial / stylized · `→` deferred to a future phase.

---

## 1. Dashboard / Огляд автострахових випадків — `/`

| Element | Status |
|---|---|
| Overview metrics row (5 KPI: new accidents, waiting docs, AI processed today, high risk, average review time) | ✓ |
| Time-range toggle (Сьогодні / 7 днів) | ✓ |
| Lifecycle phase cards (6 phases with counts; AI-аналіз highlighted) | ✓ |
| Claim queue table (5 claims incl. CLM-1006 highlighted, segment chips, navigation on row click) | ✓ |
| AI analysis side panel for CLM-1006 (confidence, claim meta, findings, evidence chips, recommended action) | ✓ |
| Automation limitation card (auto-approval НІ + human-review ОБОВ'ЯЗКОВА + reasons) | ✓ |
| AI pipeline progress card (7 steps with status pills) | ✓ |
| Audit & cost summary card (model / tokens / cost / time / trace+run IDs) | ✓ |

## 2. Claims List / Автострахові випадки — `/claims`

| Element | Status |
|---|---|
| Header with summary (53 active · 8 high risk · 5 awaiting human) | ✓ |
| Action buttons: Export CSV, Import documents, New claim | ✓ |
| Filters row (search + status + risk + event type + date + AI status) | ✓ |
| Segment chips with counts (Усі / ДТП / Високий ризик / Чекає AI / Чекає рішення) | ✓ |
| Full claim queue table with all rows from PDF (CLM-1006 → CLM-1013) | ✓ |
| Right-side KPI stack (today / overdue SLA / high risk / awaiting human) | ✓ |
| Click row → claim workspace | ✓ |

## 3. Claim Workspace / Робоче місце випадку — `/claims/CLM-1006`

| Element | Status |
|---|---|
| Back link to claims list | ✓ |
| Claim header (CLM-1006, Роберт Джонсон, event meta, status/risk/confidence/SLA pills) | ✓ |
| Event description + location/time/inspector | ✓ |
| Timeline with 7 events from PDF | ✓ |
| Policy check card (Auto Comprehensive, period, deductible, limit) | ✓ |
| Documents completeness card (6/7 progress + missing photo callout) | ✓ |
| Damage photos section (3 cards, rear bumper missing) | ✓ |
| STO invoice card with line items + total $2 720 | ✓ |
| AI recommendation side panel with confidence | ✓ |
| Key risks + evidence chips | ✓ |
| Next action panel + bottom action bar (request data / send to review / prepare decision) | ✓ |

## 4. Documents & Photos / Документи та фото — `/claims/CLM-1006/documents`

| Element | Status |
|---|---|
| Missing document warning banner with request CTA | ✓ |
| Damage photo cards (3 cards) | ✓ |
| Document checklist with status icons (ok / warn / missing) + reviewed toggle | ✓ |
| Document preview panel (police report excerpt + extracted fields + confidence) | ✓ |
| Action buttons: Request photo / View original / Confirm document | ✓ |
| Saga-driven «Request missing photo» flow with success/failure message | ✓ |

## 5. AI Analysis & Evidence / AI-аналіз та докази — `/claims/CLM-1006/ai-evidence`

| Element | Status |
|---|---|
| Trace + run metadata header (trace id, tokens, cost) | ✓ |
| AI findings cards (4 findings color-coded by tone) | ✓ |
| Guardrail panel («AI does not make final decisions») | ✓ |
| Evidence tabs/chips (5 sources) + selected indicator | ✓ |
| Model confidence progress bars (extraction / coverage / damage / recommendation) | ✓ |
| Extracted entities table with confidence filter slider | ✓ |
| Saga-driven «Rerun mock AI» with stepped progress | ✓ |

## 6. Risks & Checks / Ризики та перевірки — `/claims/CLM-1006/risks`

| Element | Status |
|---|---|
| Risk score gauge (82/100 high risk) with auto-approval threshold caption | ✓ |
| Risk factors list with weight bars (+25/+18/+22/+8/+9) | ✓ |
| Policy check summary (coverage / period / deductible / limit / exclusions) | ✓ |
| Cost benchmark card (expected $1 970 vs submitted $2 720 with deviation %) | ✓ |
| Automation limitation panel (Auto НІ / Human ОБОВ'ЯЗК. / Escalation REC) | ✓ |
| Action buttons: Open evidence / Request data / Send to approval | ✓ |

## 7. Human Approval / Людське погодження — `/claims/CLM-1006/approval`

| Element | Status |
|---|---|
| AI recommendation hero with confidence bar | ✓ |
| Decision option cards (Approve / Request / Reject / Escalate) with AI-recommended marker | ✓ |
| Payment draft card (invoice / expected / deviation / deductible / reduction / final) | ✓ |
| Reviewer notes textarea | ✓ |
| Verification checklist (toggleable) | ✓ |
| Responsibility / human-final-decision warning | ✓ |
| Save draft / Send request / Approve actions wired to saga | ✓ |

## 8. Audit & Cost / Аудит і витрати — `/claims/CLM-1006/audit`

| Element | Status |
|---|---|
| Run/trace metadata row (run_id / trace_id / model / tokens / cost / duration) | ✓ |
| «Successful run» badge | ✓ |
| AI run timeline (7 steps with status dots + per-step duration) | ✓ |
| Audit trail table (6 rows: AI Pipeline / Doc Classifier / Field Extractor / Risk / Recommender / Governance) | ✓ |
| Cost distribution with bars (Extract / RAG / Risk / Recommendation) | ✓ |
| Governance box (auto-approval НЕ ДОЗВОЛЕНО / human ОБОВ'ЯЗ. / logs / replay) | ✓ |

## 9. Policy Coverage / Поліс і покриття — `/claims/CLM-1006/policy`

| Element | Status |
|---|---|
| Policy header (Auto Comprehensive + POL-2025-AC-4421 + validity period + active badge) | ✓ |
| Coverage cards (5: collision / liability / glass / theft / roadside) with limits + deductibles | ✓ |
| Limits and deductibles list (overall / per-accident / base / bonus-malus) | ✓ |
| Exclusions list (intoxication / racing / military) | ✓ |
| Policy validation checklist (6 checks) | ✓ |
| Owner card (Роберт Джонсон, customer since 2021) | ✓ |
| Vehicle card (Toyota Camry 2021, VIN, insured value) | ✓ |

## 10. Customer & Vehicle / Клієнт і транспортний засіб — `/claims/CLM-1006/customer-vehicle`

| Element | Status |
|---|---|
| Customer profile with masked phone/email/address | ✓ |
| Vehicle profile (mileage / color / registration / insured value / risk category) | ✓ |
| Previous claims list (CLM-1006 current + CLM-0789 + CLM-0512) | ✓ |
| Communication history (Email / Chat / Phone / Web) | ✓ |
| Related policies card | ✓ |
| Customer documents summary | ✓ |
| Privacy / demo notice banner (PII masked) | ✓ |

## 11. Demo Scenario / Demo Flow — `/demo`

| Element | Status |
|---|---|
| Header with 7 кроків · ~6 хвилин · portfolio caption | ✓ |
| 7-step guided demo cards with route links | ✓ |
| Active step highlight when demo is running | ✓ |
| Start / Stop demo controls (saga-driven auto-advance) | ✓ |
| Architecture diagram: Core .NET / AI Document Intelligence / Azure AI | ✓ |
| Portfolio message panel | ✓ |
| Planned-stack note | ✓ |

---

## Cross-cutting items

| Element | Status |
|---|---|
| Global sidebar visible on every main route, active item highlighted | ✓ |
| Top bar visible on every main route (search, ⌘K hint, system-ready badge, Azure Demo chip, Run demo, alerts, avatar) | ✓ |
| Sidebar «System status» panel (UI / API / AI / Search / Storage — all green) | ✓ |
| Claim shell tab navigation across all claim-scoped routes | ✓ |
| Click claim row on Dashboard or Claims List → opens Claim Workspace | ✓ |
| Workspace links/tabs to every claim section (docs / AI / risks / approval / audit / policy / customer) | ✓ |
| Run-demo control in TopBar starts saga + navigates to `/demo` | ✓ |
| Redux Toolkit slices: claims / claimWorkspace / documents / aiReview / approval / demo | ✓ |
| Redux-Saga used only for: mock AI run, document request, approval save/send, demo auto-advance | ✓ |
| Simple toggles (filters / tabs / checklist) handled inside slices, not sagas (per spec) | ✓ |
| All data synthetic — PII masked (`+1 (555) ***-2147`, `robert.j****@demo.com`, VIN `****8842`) | ✓ |

---

## Deviations from PDF (intentional)

- **Document preview** — rendered as a structured text mock-up, not a rasterised police-report image, because no source raster is shipped with the PDF.
- **Charts** — rendered as Tailwind/SVG progress bars and a gauge ring rather than chart-library output; the design intent is preserved but visual specifics (e.g. exact tick marks) are stylised.
- **Photo thumbnails** — represented as labelled placeholder tiles (no synthetic raster damage photos shipped) — the spec forbids real customer data and we don't ship dummy stock photography for portfolio purposes.
- **Sidebar entries «Транспортні засоби» / «Налаштування»** — present in design but visually disabled (no route mapped per scope: 11 routes only).

## Deferred to future phases

- **Backend integration** — wiring to .NET 9 / ASP.NET Core CQRS API (see README, future phases).
- **Real AI provider** — currently the AI run is saga-mocked with `delay()`; a real Azure OpenAI / cheap third-party provider call comes in a later phase.
- **Charts library** — once we have real telemetry we'll likely add Recharts / Visx for the cost distribution and timeline.
- **WebSockets / SignalR** — for live AI pipeline progress; currently progress is local saga state.
- **Auth + roles** — claims adjuster, senior approver, audit observer.
