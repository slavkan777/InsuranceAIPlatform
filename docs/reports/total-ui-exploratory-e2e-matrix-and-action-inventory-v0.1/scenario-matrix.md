# Scenario Matrix — TOTAL_UI_EXPLORATORY_E2E_MATRIX_AND_ACTION_INVENTORY_V0.1

**Date:** 2026-05-28
**12 scenarios A-L** as specified in the gate prompt, each mapped to one or more Playwright tests.

| ID | Scenario | Steps | Coverage | Status |
|---|---|---|---|---|
| **A** | First-time reviewer | login → dashboard → customers → claims → search/filter → logout | `e2e/01-auth.spec.ts` (login/logout) + `e2e/02-sidebar.spec.ts` (8 sidebar routes) + `e2e/09-dashboard.spec.ts` (dashboard) + `e2e/03-customers.spec.ts` (catalog read) + `e2e/04-claims.spec.ts` (list + search) | COVERED |
| **B** | Insurance manager zero-to-end | create synth customer → create claim → open claim → upload doc → request missing doc → run AI → record AI decision → approval → payout sim → audit → logout | `e2e/08-zero-to-end.spec.ts` (customer + claim + open + logout) + `e2e/06-claim-actions.spec.ts` (upload, missing doc, AI run + decision, payout sim, audit) + `e2e/16-approval-validation.spec.ts` (approval draft + decision) | COVERED |
| **C** | Existing CLM-1006 | open → visit every tab → safe actions → verify no broken | `e2e/05-claim-1006.spec.ts` (8 sub-routes) + `e2e/12-workspace-deep.spec.ts` (in-page nav buttons) | COVERED |
| **D** | Document-heavy | multiple uploads → empty validation → sample text → list updates | `e2e/13-documents-deep.spec.ts` (multi-upload + empty validation + preview modal smoke + checklist toggle + confirm) | COVERED |
| **E** | Customer catalog | pagination → search T0042 → create customer → search created → use in claim | `e2e/03-customers.spec.ts` (search + create) + `e2e/11-customers-deep.spec.ts` (pagination forward/back + empty-name validation + cancel modal) + `e2e/08-zero-to-end.spec.ts` (use newly-created in claim) | COVERED |
| **F** | Claims list / filter | default → search CLM-1006 → search created → filters by status/type/date/segment → reset | `e2e/04-claims.spec.ts` (search) + `e2e/10-claims-deep.spec.ts` (filter combinations, segment chip, reset, empty-result case) | COVERED |
| **G** | AI flow | run AI → loading/result → confidence/risk/provider → record source=AI → safety wording | `e2e/06-claim-actions.spec.ts` (basic AI run + record source=AI) + `e2e/14-ai-deep.spec.ts` (loading state observation + confidence/risk visible + repeat run idempotency + guardrails visible) | COVERED |
| **H** | Approval / payout sim | draft → decision if present → payout sim → invalid amount → SimulationOnly visible → no real payout | `e2e/06-claim-actions.spec.ts` (basic payout sim) + `e2e/16-approval-validation.spec.ts` (save draft + amount=0 rejection + negative deductible + SimulationOnly wording + approve-after-review enabled only when approve tile selected) | COVERED |
| **I** | Audit / cost | recent categories visible → cost/token area renders → no forbidden real categories | `e2e/06-claim-actions.spec.ts` (categories visible) + `e2e/17-audit.spec.ts` (cost/token panel + forbidden-category absence) | COVERED |
| **J** | Reload / persistence | create records → reload → search/reopen → verify | `e2e/18-persistence.spec.ts` (create customer → reload → search-by-id finds row; create claim → reload → search-by-id finds row + opens detail) | COVERED |
| **K** | Negative / safety | invalid forms → empty required → unknown route → forbidden paths absent → no secret visible → no Azure → no real PII | `e2e/07-safety.spec.ts` (8 routes substring scan + topbar wording + forbidden path absence) + `e2e/19-negative.spec.ts` (unknown route redirect + empty required field validation + no real-payment wording) | COVERED |
| **L** | UI resilience | close/cancel modals → rapid nav → disabled/future explicit | `e2e/20-ui-resilience.spec.ts` (cancel each major modal + rapid nav between routes + disabled-future controls non-clickable) | COVERED |

## Coverage summary

| Total scenarios | 12 |
| Covered | **12** |
| Deferred | **0** |
| Partial | **0** |

## Test-file → scenario reverse index

| Spec file | Scenarios touched |
|---|---|
| `01-auth.spec.ts` | A, K |
| `02-sidebar.spec.ts` | A, L |
| `03-customers.spec.ts` | A, E |
| `04-claims.spec.ts` | A, B, F |
| `05-claim-1006.spec.ts` | A, C |
| `06-claim-actions.spec.ts` | B, C, D, G, H, I |
| `07-safety.spec.ts` | K |
| `08-zero-to-end.spec.ts` | B, E |
| `09-dashboard.spec.ts` (new) | A, L |
| `10-claims-deep.spec.ts` (new) | F |
| `11-customers-deep.spec.ts` (new) | E |
| `12-workspace-deep.spec.ts` (new) | C |
| `13-documents-deep.spec.ts` (new) | D |
| `14-ai-deep.spec.ts` (new) | G |
| `15-risks.spec.ts` (new) | C |
| `16-approval-validation.spec.ts` (new) | B, H |
| `17-audit.spec.ts` (new) | I |
| `18-persistence.spec.ts` (new) | J |
| `19-negative.spec.ts` (new) | K |
| `20-ui-resilience.spec.ts` (new) | L |

## What "covered" means here

- Each scenario has ≥1 Playwright test that exercises the path described.
- Tests assert *visible outcomes* (toast, URL, row appearance, badge text), not just clickability — per gate spec rule "Do not count a click as pass unless expected UI/data result is verified."
- Negative cases (scenario K) explicitly assert forbidden patterns are absent (real-payment wording, secret prefixes, /payout or /customer-messages route).
- Persistence (scenario J) is browser-verified via `page.reload()` between create and search.
