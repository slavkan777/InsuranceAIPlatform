# Frontend Architecture — V0.1

## 1. Purpose
A **frontend-only walking skeleton** for an AI-assisted **auto insurance claim workbench**. It demonstrates an intentional architecture — route-based product workflow, typed domain model, Redux Toolkit for shared UI/domain state, Redux-Saga for mock workflow orchestration, selectors to decouple UI from store shape, and a mock API boundary that a real .NET backend can later replace without rewriting the UI.

## 2. Current scope
Frontend-only · synthetic data · mocked AI workflow. **No** backend, **no** Azure, **no** real AI provider, **no** real customer data, **no** auth, **no** network calls.

## 3. Product / demo flow ("Приклад використання")
Dashboard → Claims List → CLM-1006 Workspace → Documents & Photos → AI Evidence → Risks → Policy → Customer & Vehicle → Human Approval → Audit & Cost → Demo.

## 4. Route map
| # | Route | Page | Product purpose | Answers |
|---|---|---|---|---|
| 1 | `/` | DashboardPage | Operations overview | "What needs attention now?" |
| 2 | `/claims` | ClaimsListPage | Claim queue | "Which claim do I open?" |
| 3 | `/claims/CLM-1006` | ClaimWorkspacePage | Case command center | "What is this claim about?" |
| 4 | `…/documents` | DocumentsPhotosPage | Evidence completeness | "What's missing?" |
| 5 | `…/ai-evidence` | AiEvidencePage | AI findings + evidence | "What did the AI find, and how sure?" |
| 6 | `…/risks` | RisksChecksPage | Risk score + checks | "Why is this risky / blocked?" |
| 7 | `…/approval` | HumanApprovalPage | Human decision | "What should the human decide?" |
| 8 | `…/audit` | AuditCostPage | Audit + cost governance | "What did the AI run cost & log?" |
| 9 | `…/policy` | PolicyCoveragePage | Policy coverage | "Is it covered?" |
| 10 | `…/customer-vehicle` | CustomerVehiclePage | Customer/vehicle context | "Who/what is involved?" |
| 11 | `/demo` | DemoScenarioPage | Guided usage example | "Show me the whole flow." |

## 5. Module map
| Layer | Location |
|---|---|
| App shell / providers | `src/main.tsx` (Provider + RouterProvider), `src/app/router.tsx`, `src/components/layout/{AppShell,Sidebar,TopBar,ClaimShell}.tsx` |
| Pages (1 per route) | `src/pages/*` |
| Shared UI | `src/components/ui/*` (MetricCard, StatusPill, ProgressBar, SectionHeader, Icon), `src/components/charts/*` (DonutChart, BarList, LineChart), `src/components/claim/*` |
| Store | `src/app/store.ts` (configureStore + saga middleware), `src/app/hooks.ts` (typed hooks), `src/app/rootSaga.ts` |
| Feature slices | `src/features/{claims,documents,aiReview,approval,demo}/*Slice.ts` (+ `claims/claimWorkspaceSlice.ts`) |
| Sagas | `src/features/{aiReview,approval,demo,documents}/*Saga.ts` |
| Selectors | `src/features/*/*Selectors.ts` |
| Mock API | `src/api/mockInsuranceApi.ts`, `src/api/insuranceApi.types.ts` |
| Mock data | `src/data/mock/{claims,dashboard,claim-1006}.ts` |
| Domain types | `src/types/{index,insurance,ai,audit,demo}.ts` |
| Docs / reports | `docs/architecture/*`, `docs/reports/*` |

> Note: store/hooks/rootSaga currently live under `src/app/`. The "compass" target of a dedicated `src/store/` is documented in the backlog; it was **not** moved because every slice imports `@/app/store` and the move's risk outweighs its benefit before the first commit.

## 6. State architecture summary
Redux Toolkit (`createSlice` + `configureStore`, thunks disabled, saga middleware concatenated). Six slices, feature-oriented, fully typed. Ownership: see `FRONTEND_STATE_MODEL_V0.1.md`. Redux holds **shared UI + domain-view state** only — not a database, not styling, not final business authority.

## 7. Saga architecture summary
Redux-Saga owns multi-step / cancellable **workflow orchestration** and the future side-effect/API seam: mock AI analysis run, document request, approval draft/send, guided demo auto-advance. Trivial toggles live in slice reducers. See `FRONTEND_SAGA_WORKFLOWS_V0.1.md`.

## 8. Selector boundary
Feature `*Selectors.ts` files expose typed read functions (`selectClaimsQueue`, `selectActiveClaim`, `selectAiRunStatus`, …) so pages depend on **selector contracts**, not raw `state.x.y` shape. Critical pages (Dashboard, Claims List) consume selectors today; remaining page migrations are a documented backlog item (low risk, additive).

## 9. Mock API boundary
`src/api/mockInsuranceApi.ts` is the **single seam** between UI/workflows and synthetic data — typed async functions (`getClaimsQueue`, `getClaimById`, `runMockAiAnalysis`, `saveApprovalDraft`, …) that resolve local data with no network. The approval saga already routes its write path through this seam. See `MOCK_API_BOUNDARY_V0.1.md`.

## 10. Domain type contracts
Canonical primitives in `src/types/index.ts`; grouped/extended DTO-like contracts in `insurance.ts` (Claim, ClaimId, PolicyCoverage, Customer, Vehicle, RiskAssessment, HumanReviewRequirement…), `ai.ts` (AiFinding, AiRecommendation, ConfidenceScore, AiRunStatus, AiProviderMode…), `audit.ts` (AuditEvent, CostTrace, ModelTrace, AuditRun…), `demo.ts` (DemoStep, DemoScenarioState).

## 11. AI governance boundary
- AI is **advisory only** (`AiRecommendation.advisoryOnly: true`).
- **Human approval is mandatory** above risk/confidence thresholds; auto-approval is explicitly NO.
- Audit + cost trace (run id, trace id, tokens, cost, latency, governance) are first-class, visible product features.
- No autonomous payout or rejection exists anywhere in the app.

## 12. Future integration path
.NET backend skeleton → mock AI pipeline behind backend → low-cost real AI provider → RAG/evidence layer → guardrails/evaluators → Azure cost architecture → minimal deployment. Each is a separate, separately-approved gate.

## 13. Interview explanation
> "This isn't just screens. It's a controlled AI-workbench frontend: 11 route-based product steps over one golden claim, a typed domain model, Redux Toolkit for shared UI/domain state, Redux-Saga for mock workflow orchestration and the future side-effect seam, selectors that decouple the UI from store shape, and a mock API boundary I can swap for a real .NET client without touching the UI. AI is advisory; a human always approves; every AI run is audited and costed. It's intentionally staged — frontend first, backend next."

## 14. Known limitations
In-memory state (resets on refresh); no backend/persistence; mocked AI (no real OCR/RAG/provider); no production auth; charts are hand-built SVG; icons are inline SVG primitives.
