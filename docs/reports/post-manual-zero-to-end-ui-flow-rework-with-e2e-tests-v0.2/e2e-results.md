# Playwright E2E Results — POST_MANUAL_ZERO_TO_END_UI_FLOW_REWORK_WITH_E2E_TESTS_V0.2

**Run date (UTC):** 2026-05-28T17:08:41.984Z
**Duration:** 102.263 s
**Browser:** Chromium (Playwright v1223 / Chrome for Testing 148.0.7778.96)
**Workers:** 1 (serial)
**Retries:** 0

## Aggregate

| Outcome | Count |
|---|---:|
| Expected (passed) | **32** |
| Unexpected (failed) | 0 |
| Flaky | 0 |
| Skipped | 0 |

## Per-suite breakdown

```
ok  1 [chromium] › e2e\01-auth.spec.ts:18  › Auth › redirect → login → dashboard → logout → login again
ok  2 [chromium] › e2e\01-auth.spec.ts:50  › Auth › login helper reaches dashboard cleanly
ok  3 [chromium] › e2e\02-sidebar.spec.ts:29 › Sidebar › exactly one active sidebar link for /
ok  4 [chromium] › e2e\02-sidebar.spec.ts:29 › Sidebar › exactly one active sidebar link for /claims
ok  5 [chromium] › e2e\02-sidebar.spec.ts:29 › Sidebar › exactly one active sidebar link for /customers
ok  6 [chromium] › e2e\02-sidebar.spec.ts:29 › Sidebar › exactly one active sidebar link for /claims/CLM-1006
ok  7 [chromium] › e2e\02-sidebar.spec.ts:29 › Sidebar › exactly one active sidebar link for /claims/CLM-1006/documents
ok  8 [chromium] › e2e\02-sidebar.spec.ts:29 › Sidebar › exactly one active sidebar link for /claims/CLM-1006/ai-evidence
ok  9 [chromium] › e2e\02-sidebar.spec.ts:29 › Sidebar › exactly one active sidebar link for /claims/CLM-1006/approval
ok 10 [chromium] › e2e\02-sidebar.spec.ts:29 › Sidebar › exactly one active sidebar link for /claims/CLM-1006/audit
ok 11 [chromium] › e2e\03-customers.spec.ts:18 › Customers catalog › catalog renders backend 200+ synthetic customers, pagination is exposed
ok 12 [chromium] › e2e\03-customers.spec.ts:40 › Customers catalog › search returns the seeded synthetic customer CUST-T0042
ok 13 [chromium] › e2e\03-customers.spec.ts:49 › Customers catalog › create new synthetic customer, then see it in the catalog
ok 14 [chromium] › e2e\04-claims.spec.ts:23 › Claims list / create / search › claims list opens, default filters show all (not just ДТП)
ok 15 [chromium] › e2e\04-claims.spec.ts:32 › Claims list / create / search › create new claim → returned id is NOT CLM-MOCK-*
ok 16 [chromium] › e2e\05-claim-1006.spec.ts:31 › CLM-1006 walkthrough › open detail + every sub-tab reachable
ok 17 [chromium] › e2e\06-claim-actions.spec.ts:22 › Claim workspace actions on CLM-1006 › Document upload — text content saved (success toast)
ok 18 [chromium] › e2e\06-claim-actions.spec.ts:36 › Claim workspace actions on CLM-1006 › Missing document/photo request — visible local-sandbox wording
ok 19 [chromium] › e2e\06-claim-actions.spec.ts:51 › Claim workspace actions on CLM-1006 › AI analysis runs + AI decision recorded with source=AI
ok 20 [chromium] › e2e\06-claim-actions.spec.ts:74 › Claim workspace actions on CLM-1006 › Payout simulation — SimulationOnly notice visible, sim created
ok 21 [chromium] › e2e\06-claim-actions.spec.ts:91 › Claim workspace actions on CLM-1006 › Audit trace page renders with recent actions visible
ok 22 [chromium] › e2e\07-safety.spec.ts:42 › Safety invariants › no real-action / no-secret leak on /
ok 23 [chromium] › e2e\07-safety.spec.ts:42 › Safety invariants › no real-action / no-secret leak on /claims
ok 24 [chromium] › e2e\07-safety.spec.ts:42 › Safety invariants › no real-action / no-secret leak on /customers
ok 25 [chromium] › e2e\07-safety.spec.ts:42 › Safety invariants › no real-action / no-secret leak on /claims/CLM-1006
ok 26 [chromium] › e2e\07-safety.spec.ts:42 › Safety invariants › no real-action / no-secret leak on /claims/CLM-1006/documents
ok 27 [chromium] › e2e\07-safety.spec.ts:42 › Safety invariants › no real-action / no-secret leak on /claims/CLM-1006/ai-evidence
ok 28 [chromium] › e2e\07-safety.spec.ts:42 › Safety invariants › no real-action / no-secret leak on /claims/CLM-1006/approval
ok 29 [chromium] › e2e\07-safety.spec.ts:42 › Safety invariants › no real-action / no-secret leak on /claims/CLM-1006/audit
ok 30 [chromium] › e2e\07-safety.spec.ts:56 › Safety invariants › TopBar advertises local sandbox, not "real demo"
ok 31 [chromium] › e2e\07-safety.spec.ts:61 › Safety invariants › No path to /payout or /customer-messages from the UI
ok 32 [chromium] › e2e\08-zero-to-end.spec.ts:19 › Zero-to-end browser walk › full reviewer chain: login → create customer → create claim → open → logout
```

## Coverage map vs gate spec

| Spec test # | Description | Where covered |
|---:|---|---|
| 1 | Login/logout | `01-auth.spec.ts` (2) |
| 2 | Sidebar active state | `02-sidebar.spec.ts` (8) |
| 3 | Customers catalog 200/search/pagination | `03-customers.spec.ts:18,40` |
| 4 | Create new synthetic customer | `03-customers.spec.ts:49` |
| 5 | Create claim for new customer (real id, list, search, open) | `04-claims.spec.ts:32`, `08-zero-to-end.spec.ts:19` |
| 6 | Existing CLM-1006 | `05-claim-1006.spec.ts:31` |
| 7 | Document upload text | `06-claim-actions.spec.ts:22` |
| 8 | Missing document/photo request | `06-claim-actions.spec.ts:36` |
| 9 | AI analysis + AI decision (source=AI) | `06-claim-actions.spec.ts:51` |
| 10 | Payout simulation (SimulationOnly) | `06-claim-actions.spec.ts:74` |
| 11 | Audit trace | `06-claim-actions.spec.ts:91` |
| 12 | Safety negative checks | `07-safety.spec.ts` (10) |

All 12 specified tests have at least one passing case; several are
covered by multiple tests (e.g. test 2 has 8 cases — one per probed route).

## Screenshots / traces

- HTML report: `playwright-report/` (local-only; `npx playwright show-report`)
- JSON results: `playwright-report/results.json` (local-only)
- No failures → no screenshots/traces archived in `gpt-handoff` (screenshots
  are configured `only-on-failure`, traces `on-first-retry`).
