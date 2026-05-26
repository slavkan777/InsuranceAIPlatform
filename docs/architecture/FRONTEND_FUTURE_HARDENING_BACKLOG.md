# Frontend Future Hardening Backlog

| Title | Category | Why it matters | When | Risk if ignored | Blocker? |
|---|---|---|---|---|---|
| Migrate remaining pages to selectors | BEFORE BACKEND | Full UI/store decoupling (Workspace, AI Evidence, Risks, Approval, Audit, Policy, Customer, Demo still read mock data / state directly) | before backend wiring | UI tightly coupled to shape; harder swap | non-blocker |
| Route page reads through `mockInsuranceApi` | BEFORE BACKEND | Single seam for all reads, not just approval writes | before backend wiring | inconsistent data access | non-blocker |
| Move store to `src/store/`, split `app/App.tsx`/`providers.tsx`/`router.tsx` | BEFORE BACKEND | Matches target compass; clearer composition | after first commit | cosmetic; import churn | non-blocker |
| Add RTK Query or TanStack Query for server cache | AFTER BACKEND | Proper server-state caching once endpoints exist | backend gate | reinventing caching in slices | non-blocker |
| Real AI provider behind backend | AFTER BACKEND | Replace mocked run with real (cheap) provider | after backend | demo stays mock-only | non-blocker |
| RAG / evidence retrieval | AFTER BACKEND | Ground AI findings in real documents | after AI gate | findings ungrounded | non-blocker |
| Guardrails / evaluators | AFTER BACKEND | Enforce confidence/governance automatically | after AI gate | governance manual-only | non-blocker |
| Azure cost architecture + minimal deploy | AFTER BACKEND | Real telemetry + hosting | deployment gate | no live cost data | non-blocker |
| Unit/integration tests (Vitest + RTL) | TESTING LATER | Regression safety for slices/sagas/selectors | after first commit | refactors risky | non-blocker |
| Persist demo/UI state (optional) | PORTFOLIO POLISH | Survive refresh | optional | resets on refresh (expected) | non-blocker |
| Replace inline SVG icons with a tree-shaken icon set | PORTFOLIO POLISH | Richer icon coverage | optional | simpler icons | non-blocker |
| Reword "Зупинити демо" stop label | PORTFOLIO POLISH | Toggle wording consistency | optional | minor | non-blocker |
| Real auth / roles (adjuster/approver/auditor) | DO NOT DO NOW | Out of current scope | future gate | n/a now | n/a |
| File upload / OCR | DO NOT DO NOW | Out of current scope | future gate | n/a now | n/a |
