# UI Visual Identity Polish & Honest Demo Labeling — Report

**Project:** InsuranceAIPlatform · Auto Insurance Claim AI Workbench
**Gate:** UI_VISUAL_IDENTITY_POLISH_AND_HONEST_DEMO_LABELING
**Style:** Enterprise AI Command Center
**Date:** 2026-05-26
**Scope:** visual identity + honest demo labeling only. No product logic, routes, Redux/Saga, or mock-data semantics changed.

---

## 1. Visual identity summary

Applied a coherent **Enterprise AI Command Center** identity (70% insurance operations · 20% AI evidence/risk intelligence · 10% audit/governance):

- **Azure/electric blue** (`brand`) is the single primary-action / navigation / focus color.
- **Indigo/violet** (`ai`) is a *new, distinct* accent reserved for the AI intelligence layer — AI recommendations, evidence selection, model-confidence bars, the AI confidence figure. It no longer shares a hue with primary actions, so "AI" reads as an overlay, not just another button.
- **Risk semaphore** (`good`/`warn`/`danger`) kept for deterministic risk/checks, visually separate from AI findings.
- **Slate/navy** (`ink`) for surfaces, graphite sidebar, and audit/governance.
- Full 50–900 color scales added for `good/warn/danger/ai`, so colored card accents (tints, borders, left-accents) that were previously no-ops now render correctly.
- Cooler near-white app background; refined card shadow.

## 2. Honest demo labeling changes

| Before | After |
|---|---|
| `API — готовий` | `API — mock` |
| `AI-модуль — готовий` | `AI-модуль — demo` |
| `Пошуковий індекс — готовий` | `Пошуковий індекс — mock` |
| `Сховище документів — готове` | `Сховище документів — demo` |
| `Azure Demo` (env chip) | `Local Demo` |
| (none) | persistent sidebar notice: **"Frontend prototype · synthetic data · mocked AI workflow"** |

- Mock/demo status items now use a distinct indigo dot (not the green "ready" dot); only `Інтерфейс — працює` stays green (the frontend genuinely works).
- `Приклад використання` retained as the primary CTA (no "Запустити демо").
- Real Azure / real AI remain on the demo page as **planned architecture**, never implied as live.
- Not over-warned: the UI still looks finished and portfolio-grade.

## 3. Design tokens / classes changed

- `tailwind.config.js` — new `ai` indigo scale; Azure `brand` scale; full `good/warn/danger` scales; teal 50/100; system-first font stack; refined `shadow-card`/`shadow-soft`/new `shadow-ring`.
- `src/index.css` — cooler background gradient; new `.pill-ai` component class.
- `StatusPill` — added `ai` tone; deepened text contrast on good/warn/danger.
- `ProgressBar` — added `ai` tone.

## 4. Files changed

| File | Change |
|---|---|
| `tailwind.config.js` | token system (ai accent, full scales, azure brand, fonts) |
| `src/index.css` | background + `.pill-ai` |
| `index.html` | removed Google Fonts CDN import; added meta description |
| `src/components/ui/StatusPill.tsx` | `ai` tone |
| `src/components/ui/ProgressBar.tsx` | `ai` tone |
| `src/components/layout/Sidebar.tsx` | honest status labels + indigo dots + honest notice |
| `src/components/layout/TopBar.tsx` | `Azure Demo` → `Local Demo` |
| `src/pages/DashboardPage.tsx` | AI panel confidence + recommended-action → indigo |
| `src/pages/ClaimWorkspacePage.tsx` | AI recommendation label + confidence bar → indigo |
| `src/pages/AiEvidencePage.tsx` | run progress, confidence bars, guardrail, evidence selection, entity confidence → indigo |
| `src/pages/HumanApprovalPage.tsx` | AI recommendation hero, confidence bar, AI-recommended badge → indigo |
| `docs/design/visual-identity-brief.md` | new (brief) |
| `docs/reports/UI_VISUAL_IDENTITY_POLISH_REPORT.md` | new (this report) |

## 5. Routes checked (all 11 rendered via recaptured screenshots)

`/` · `/claims` · `/claims/CLM-1006` · `/claims/CLM-1006/documents` · `/claims/CLM-1006/ai-evidence` · `/claims/CLM-1006/risks` · `/claims/CLM-1006/approval` · `/claims/CLM-1006/audit` · `/claims/CLM-1006/policy` · `/claims/CLM-1006/customer-vehicle` · `/demo`

## 6. Screenshots captured (11)

`docs/screenshots/01-dashboard.png` … `11-demo-scenario.png` — recaptured from the production preview at 1600×1100 (full page), no console/page errors observed during capture.

## 7. Commands run

| Command | Result |
|---|---|
| `git status` | all files untracked, **no HEAD/commits** (init only) — frontend-only confirmed |
| grep `Запустити демо` | 0 hits (already replaced in prior gate) |
| `npm run build` | **exit 0** — 95 modules, CSS 30.44 kB / gzip 5.79 kB, JS 336.53 kB / gzip 103.33 kB, 3.54 s |
| preview + Playwright capture | 11/11 routes captured |

## 8. Build result

PASS — `tsc -b && vite build`, exit 0. CSS grew ~2.4 kB (new color-scale utilities now emitted); `index.html` shrank after removing the web-font CDN import. Bundle JS essentially unchanged (text/markup only).

## 9. Self-review (Phase 11)

1. Serious AI insurance workbench? **Yes** — operational metrics, claim queue, AI intelligence panel, governance.
2. CLM-1006 understandable < 30 s? **Yes** — highlighted in queue + dedicated AI side panel with status/risk/confidence.
3. AI / evidence / risk / approval / audit visually distinct? **Yes** — AI = indigo, risk = red/amber/green, audit = slate/mono.
4. Avoids legacy Dynamics/CRM feel? **Yes** — modern compact dashboard, not gray legacy forms.
5. Avoids toy SaaS feel? **Yes** — restrained palette, thin borders, subtle shadows; no neon/oversized toy cards.
6. Colors consistent? **Yes** — centralized tokens + shared `StatusPill`/`ProgressBar`.
7. Badges consistent? **Yes** — single `StatusPill` source of truth.
8. Tables more professional? **Yes** — compact rows, uppercase headers, readable badges, CLM-1006 highlight.
9. `Приклад використання` present? **Yes** — top bar + demo page.
10. Demo-stage labels honest? **Yes** — mock/demo statuses, Local Demo, persistent prototype notice.
11. All routes still render? **Yes** — 11/11 captured.

## 10. Known limitations

- Inter is used only if installed locally (no web-font import); otherwise the system stack (Segoe UI / system-ui) renders — visually very close.
- Charts remain SVG/Tailwind (risk gauge, cost/confidence bars), not a chart library — intentional for a frontend skeleton.
- Photo tiles remain labelled placeholders (no synthetic raster photos shipped).
- State is in-memory; refresh resets it (expected for a prototype).
- `npm audit` still reports 2 moderate dev-dependency advisories (unchanged; out of scope).

## 11. Forbidden-scope confirmation

No full redesign · no page/route moves · no removed screens · no Redux/Saga behavior change · no mock-data semantics change · no dependency added · no backend · no Azure · no AI provider · no API keys · no real API calls · no real PII · no commit · no push · no source GitHub remote · no other repos touched (except the authorized `gpt-handoff` report upload).

## 12. Next safe step

Await reviewer (GPT) audit + owner approval. Optional, each requiring authorization: reword the `Зупинити демо` stop-state label; commit the skeleton locally; create the source repo + push; begin the planned `.NET` backend phase.
