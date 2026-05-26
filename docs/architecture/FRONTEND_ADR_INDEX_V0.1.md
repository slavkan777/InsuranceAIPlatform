# Frontend ADR Index — V0.1

Concise architecture decision notes.

## ADR-001 — React + TypeScript + Vite
- **Decision:** React 18 + TS (strict) + Vite 5.
- **Context:** Portfolio/interview SPA; fast DX; type safety expected by reviewers.
- **Trade-off:** Vite over Next.js — no SSR/routing-framework, but simpler and sufficient for an internal workbench.
- **Consequence:** Fast builds, strict types, client-rendered SPA.
- **Revisit when:** SEO/SSR or server components become requirements.

## ADR-002 — React Router
- **Decision:** `createBrowserRouter` with nested claim routes.
- **Context:** Product is a route-based workflow over one claim; nested `ClaimShell` shares chrome.
- **Trade-off:** Manual route config vs file-based routing.
- **Consequence:** Explicit 11-route map; deep links work.
- **Revisit when:** Adopting a meta-framework.

## ADR-003 — Redux Toolkit
- **Decision:** RTK `configureStore` + `createSlice`, typed hooks, thunks disabled.
- **Context:** Shared UI/domain-view state across many routes (selection, filters, drafts).
- **Trade-off:** More setup than Context for tiny apps; pays off with cross-route shared state + devtools + selectors.
- **Consequence:** Predictable, typed, inspectable state.
- **Revisit when:** Server cache state dominates → add RTK Query/TanStack Query.

## ADR-004 — Redux-Saga
- **Decision:** Saga for workflow orchestration + side-effect seam.
- **Context:** Multi-step/cancellable flows (AI run, doc request, approval, demo).
- **Trade-off:** Heavier than thunks; chosen for explicit, testable, cancellable workflows and a clean future-API seam.
- **Consequence:** Workflows read as event flows; trivial toggles stay in reducers.
- **Revisit when:** Flows become simple request/response → thunks/RTK Query may suffice.

## ADR-005 — Mock API boundary before backend
- **Decision:** `mockInsuranceApi` typed seam over synthetic data.
- **Context:** Frontend-first; backend is a later gate.
- **Trade-off:** Extra indirection now; enables zero-rewrite backend swap later.
- **Consequence:** UI/sagas depend on async contracts, not data files.
- **Revisit when:** Backend gate opens → implement real client behind same signatures.

## ADR-006 — No real AI / Azure / provider yet
- **Decision:** Everything AI/cloud is mocked and labeled honestly (mock/demo/Local Demo).
- **Context:** Portfolio must be credible and honest; no keys in frontend.
- **Trade-off:** Less "wow" than a live demo; far safer and honest.
- **Consequence:** No secrets, no cost, no PII risk.
- **Revisit when:** Backend holds keys and calls providers server-side.

## ADR-007 — AI advisory, human approval final
- **Decision:** AI never auto-decides; human approval is mandatory; audit/cost always visible.
- **Context:** Insurance governance; interview-critical responsible-AI story.
- **Trade-off:** No full automation; this is the point.
- **Consequence:** `AiRecommendation.advisoryOnly: true`; governance panels everywhere relevant.
- **Revisit when:** Never for final authority; thresholds/automation of low-risk steps can be revisited with guardrails.

## ADR-008 — V3 visual baseline accepted before backend
- **Decision:** Freeze the V3 "Enterprise AI Command Center" visuals; stop redesign; harden architecture.
- **Context:** Visuals accepted; further polish has diminishing returns pre-backend.
- **Trade-off:** Defer visual perfection for architecture credibility.
- **Consequence:** This gate adds docs/types/selectors/mock-API, not pixels.
- **Revisit when:** Post-backend portfolio-polish pass.
