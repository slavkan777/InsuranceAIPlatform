# Visual Identity Brief — Enterprise AI Command Center

**Product:** InsuranceAIPlatform · Auto Insurance Claim AI Workbench
**Phase:** frontend walking skeleton (synthetic data, mocked AI workflow)
**Gate:** UI_VISUAL_IDENTITY_POLISH_AND_HONEST_DEMO_LABELING
**Date:** 2026-05-26

## Style name

**Enterprise AI Command Center** — a serious insurance operations console with a visible AI intelligence layer and first-class audit/cost governance.

Design formula: **70%** enterprise insurance operations · **20%** AI evidence / risk intelligence · **10%** technical audit / governance console.

## Design goals

- A reviewer understands within ~10 seconds: insurance claim ops workbench, central case CLM-1006, AI assists with documents/evidence/risk/recommendations, human approval is mandatory, audit/cost/governance are first-class, current phase is a frontend prototype on synthetic data.
- Credible and modern — neither legacy Dynamics/CRM nor toy SaaS/crypto-dashboard.
- AI is *advisory and visually distinct* from primary actions and from deterministic risk checks.

## Where styling lives (audit result)

| Concern | Location |
|---|---|
| Design tokens (colors, fonts, shadows) | `tailwind.config.js` (theme.extend) |
| Global base + shared component classes | `src/index.css` (`@layer base`, `@layer components`) |
| App shell / layout | `src/components/layout/` — `AppShell`, `Sidebar`, `TopBar`, `ClaimShell` |
| Shared UI primitives | `src/components/ui/` — `MetricCard`, `StatusPill`, `ProgressBar`, `SectionHeader` |
| Claim primitives | `src/components/claim/` — `ClaimHeader`, `Timeline` |
| Per-page composition | `src/pages/*` (11 pages) |

Repeated styling (cards, pills, buttons, tables, metric labels, progress bars) is centralized in `index.css` component classes and the shared `ui/` components, so most polish is applied centrally rather than per page.

## Color system

| Token | Role | Hue |
|---|---|---|
| `ink` 50–950 | surfaces, text, borders, **sidebar graphite**, audit/governance | cool slate / navy |
| `brand` 50–900 | **primary** actions, navigation, focus ring | Azure / electric blue |
| `ai` 50–900 | **AI** recommendations, evidence, confidence — the intelligence layer | indigo / violet |
| `good` 50–900 | confirmed / ok | emerald |
| `warn` 50–900 | review / warning | amber |
| `danger` 50–900 | high risk / blocked | controlled red |
| `teal` | secondary accent (AI-doc-intelligence layer chip) | teal |

Key decision: **primary (Azure blue) and AI (indigo/violet) are deliberately different hues** so the AI layer reads as a distinct intelligence overlay, not just another button color. Full 50–900 scales were added for `good/warn/danger/ai` so colored card accents (tints, borders, left-accents) actually render.

Avoided: random colors, neon, heavy gradients, oversized rounded "toy" cards, crypto-dashboard look.

## Typography system

- Stack: `Inter, Segoe UI, system-ui, -apple-system, BlinkMacSystemFont, "Helvetica Neue", Arial, sans-serif` — Inter is used **only if locally installed**; no web-font is imported (the Google Fonts CDN `<link>` was removed for an offline, self-contained prototype).
- Mono: `"JetBrains Mono", ui-monospace, Consolas, Menlo, monospace` for technical IDs (trace/run), tokens, money.
- Compact enterprise scale: confident-but-not-huge headings, small uppercase labels, dense readable tables, visually distinct technical metadata. No marketing hero type.

## Component style rules

- **Cards:** white surface, thin `ink-100` border, subtle `shadow-card`, consistent `rounded-xl`; important AI/risk/audit cards may carry a colored left/top accent.
- **Status pills:** `StatusPill` tones `good | warn | danger | info | ai | muted` — single source of truth for badge colors.
- **Buttons:** primary = Azure `brand-600`; secondary = white/outline; destructive = controlled red only where needed. No playful gradients.
- **Tables:** compact rows, uppercase header row, subtle hover, CLM-1006 highlight, readable badges.
- **Progress/confidence bars:** `ProgressBar` tones `brand | ai | good | warn | danger`; AI confidence uses `ai`.

## AI / evidence / risk / audit visual rules

- **AI recommendation:** indigo `ai` accent, confidence always visible, framed as advisory — never auto-approval.
- **Evidence:** chips with source labels; selected evidence uses `ai`; model-confidence bars use `ai`.
- **Risk:** prominent score; deterministic checks use the red/amber/green semaphore and are visually separate from AI findings (risk is not "AI-colored").
- **Human approval:** serious decision cards; AI-recommended option marked with an `ai` badge; responsibility notice clearly visible.
- **Audit & cost:** slate/navy technical styling; trace/run IDs in mono; tokens/cost/latency read like observability telemetry; governance panel prominent.

## Honest demo labeling rules

- System-status block: `API — mock`, `AI-модуль — demo`, `Пошуковий індекс — mock`, `Сховище документів — demo` (only `Інтерфейс — працює` stays green; mock/demo items use a distinct indigo dot, not the green "ready" dot).
- Environment chip: **`Local Demo`** (was `Azure Demo`).
- Persistent honest notice in the sidebar: **"Frontend prototype · synthetic data · mocked AI workflow"**.
- Primary CTA wording stays **"Приклад використання"** (not "Запустити демо").
- Real Azure / real AI remain **planned architecture** (shown on the demo page's architecture layers), never implied as live.
- Tone: honest and professional, not over-warning; the UI still looks finished and portfolio-grade.

## What will be changed

- Token palette (add `ai`, full `good/warn/danger` scales, Azure `brand`); cooler background.
- Shared classes/components (`StatusPill`, `ProgressBar`, `index.css`).
- Sidebar honest status + notice; topbar `Local Demo`.
- Targeted AI-accent application on AI panels/confidence/evidence across pages.
- Remove web-font CDN import.

## What will NOT be changed

- The 11 routes and their paths.
- The CLM-1006 demo flow and 7-step usage example.
- Redux Toolkit state shape and Redux-Saga workflows.
- The meaning of any synthetic mock data.
- The frontend-only boundary (no backend / Azure / AI provider / API keys / real calls / real PII).
- Component public APIs and business copy meaning.
