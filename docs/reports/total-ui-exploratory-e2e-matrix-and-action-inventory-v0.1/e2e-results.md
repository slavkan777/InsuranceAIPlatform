# Playwright E2E Results — TOTAL_UI_EXPLORATORY_E2E_MATRIX_AND_ACTION_INVENTORY_V0.1

**Run date (UTC):** 2026-05-28T17:44:58.107Z
**Duration:** 233.971 s (3.9 min)
**Browser:** Chromium (Playwright v1223 / Chrome for Testing 148.0.7778.96)
**Workers:** 1 (serial, shared LocalDB)
**Retries:** 0

## Aggregate

| Outcome | Count |
|---|---:|
| Expected (passed) | **87** |
| Unexpected (failed) | 0 |
| Flaky | 0 |
| Skipped | 0 |

## Per-spec breakdown

| Spec | Cases | Coverage |
|---|---:|---|
| `01-auth.spec.ts` | 2 | Login/logout demo flow |
| `02-sidebar.spec.ts` | 8 | One active link per route |
| `03-customers.spec.ts` | 3 | Catalog, search, create |
| `04-claims.spec.ts` | 2 | Default list, create+search |
| `05-claim-1006.spec.ts` | 1 | Golden walk-through |
| `06-claim-actions.spec.ts` | 5 | Upload, missing-doc, AI, payout, audit |
| `07-safety.spec.ts` | 10 | 8 routes + topbar + forbidden hrefs |
| `08-zero-to-end.spec.ts` | 1 | Full reviewer chain |
| `09-dashboard.spec.ts` (new) | 9 | Dashboard render, period chips disabled, segment chips disabled, create-claim, export CSV, topbar logout/help/bell, demo CTA, demo page steps |
| `10-claims-deep.spec.ts` (new) | 5 | CLM-1006 default + status filter, eventType filter, segment chip, empty-state |
| `11-customers-deep.spec.ts` (new) | 4 | Pagination, empty fullName validation, cancel, search T0042 |
| `12-workspace-deep.spec.ts` (new) | 4 | Documents nav, AI evidence nav, approval nav, list-back nav |
| `13-documents-deep.spec.ts` (new) | 4 | Empty content validation, sample template, multiple uploads, preview-modal copy |
| `14-ai-deep.spec.ts` (new) | 5 | Loading state, provider/risk visible, guardrails visible, record twice (different commandId), confidence slider |
| `15-risks.spec.ts` (new) | 5 | Render, ai-evidence nav, documents nav, approval nav, safety wording |
| `16-approval-validation.spec.ts` (new) | 4 | Save draft, approve-after-review enables on tile, amount=0 rejected via HTML5 :invalid, amount=2500 succeeds + SimulationOnly notice |
| `17-audit.spec.ts` (new) | 4 | Header, trail+cost render, governance pills, no forbidden categories |
| `18-persistence.spec.ts` (new) | 2 | Customer survives reload, claim survives reload |
| `19-negative.spec.ts` (new) | 4 | Catch-all redirect, empty vehicle blocked, unknown claim graceful, no "real payment" wording across 8 routes |
| `20-ui-resilience.spec.ts` (new) | 5 | Cancel NewClaim by unique marker, cancel CreateCustomer + empty-state, rapid nav, sidebar disabled-future, topbar Help/Bell disabled |
| **Total** | **87** | |

## Test count delta vs prior gate

| Metric | POST_MANUAL_ZERO_TO_END_UI_FLOW_REWORK_WITH_E2E_TESTS_V0.2 | This gate | Delta |
|---|---:|---:|---:|
| Spec files | 8 | 20 | **+12** |
| Test cases | 32 | 87 | **+55** |
| Duration | 102.3 s | 234.0 s | **+131.7 s** |

## Coverage map vs gate scenarios A-L

All 12 scenarios COVERED (see `scenario-matrix.md`). Reverse index of test files → scenarios:

| Scenarios | Touched by |
|---|---|
| A (first-time reviewer) | `01-auth`, `02-sidebar`, `03-customers`, `04-claims`, `09-dashboard` |
| B (zero-to-end) | `04-claims`, `06-claim-actions`, `08-zero-to-end`, `16-approval-validation` |
| C (existing CLM-1006) | `05-claim-1006`, `06-claim-actions`, `12-workspace-deep`, `15-risks` |
| D (document-heavy) | `06-claim-actions`, `13-documents-deep` |
| E (customer catalog) | `03-customers`, `08-zero-to-end`, `11-customers-deep` |
| F (claims list / filter) | `04-claims`, `10-claims-deep` |
| G (AI flow) | `06-claim-actions`, `14-ai-deep` |
| H (approval / payout sim) | `06-claim-actions`, `16-approval-validation` |
| I (audit / cost) | `06-claim-actions`, `17-audit` |
| J (reload / persistence) | `18-persistence` |
| K (negative / safety) | `07-safety`, `19-negative` |
| L (UI resilience) | `02-sidebar`, `09-dashboard`, `20-ui-resilience` |

## Issues encountered during the run (and fixes applied)

The first full run surfaced **3 test bugs** (not product bugs):

1. **`16-approval-validation:30`** — used regex `/Погодити виплату/` for the approve-tile label. Backend's `getClaimApproval` returns `"Затвердити виплату"`. **Fix:** target tiles by `[data-selected]` attribute on parent button and loop until `approve-after-review` enables. Label-agnostic now.
2. **`16-approval-validation:41`** — asserted `body` contains `/має бути додатня/` after submitting amount=0. The PayoutSimulation amount input is `<input type="number" min="0.01" required>` — HTML5 native validation rejects the submit BEFORE the JS handler can set the React error state. **Fix:** assert the modal stays open AND the input violates the HTML5 constraint (`element.validity.valid === false`), AND no "Симуляція виплати #X створена" toast appears.
3. **`20-ui-resilience:18`** — compared row-count before/after cancelling NewClaimModal. With single-worker serial tests, previous tests in the suite create claims that shift the count. **Fix:** use a UNIQUE never-saved marker string in the form, cancel, then search by that marker and assert the empty-results state. Race-safe.

After the 3 test-bug fixes, full re-run: **87/87 PASS**.

## Screenshots / traces

- HTML report local-only at `playwright-report/`
- JSON results local-only at `playwright-report/results.json`
- No failures → no `screenshot only-on-failure` artifacts archived in `gpt-handoff`
- `trace: 'on-first-retry'` configured; with `retries: 0` locally, no traces emitted
- `test-results/` directory holds the per-test working artifacts (not committed, not shipped)
