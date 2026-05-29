# Route Inventory — TOTAL_UI_EXPLORATORY_E2E_MATRIX_AND_ACTION_INVENTORY_V0.1

**Source:** `src/app/router.tsx` + `src/components/layout/Sidebar.tsx`
**Date:** 2026-05-28

## Mounted routes

| # | Route | Component | Auth required | Render type | Source-of-truth | E2E coverage |
|---|---|---|---|---|---|---|
| 1 | `/login` | `LoginPage` | no | static | localStorage demo flow | `01-auth.spec.ts` |
| 2 | `/` | `DashboardPage` | yes | mixed (metrics + claims queue) | `selectClaimsSummary` ∪ `claimRows`-fallback | `09-dashboard.spec.ts` (new), `02-sidebar.spec.ts` |
| 3 | `/claims` | `ClaimsListPage` | yes | dynamic + filterable | `selectClaimsQueue` via `loadClaimsQueue` saga | `04-claims.spec.ts`, `10-claims-deep.spec.ts` (new) |
| 4 | `/customers` | `CustomersDirectoryPage` | yes | dynamic | `insuranceApi.listCustomers` direct | `03-customers.spec.ts`, `11-customers-deep.spec.ts` (new) |
| 5 | `/claims/:claimId` | `ClaimShell` + `ClaimWorkspacePage` | yes | dynamic detail | `loadClaimDetail` saga | `05-claim-1006.spec.ts`, `12-workspace-deep.spec.ts` (new) |
| 6 | `/claims/:claimId/documents` | `DocumentsPhotosPage` | yes | dynamic + actions | `selectWorkspaceDocuments` | `06-claim-actions.spec.ts`, `13-documents-deep.spec.ts` (new) |
| 7 | `/claims/:claimId/ai-evidence` | `AiEvidencePage` | yes | dynamic + actions | `selectWorkspaceAiEvidence` + `loadLatestAiAnalysis` | `06-claim-actions.spec.ts`, `14-ai-deep.spec.ts` (new) |
| 8 | `/claims/:claimId/risks` | `RisksChecksPage` | yes | dynamic | `selectWorkspaceRisks` | `15-risks.spec.ts` (new) |
| 9 | `/claims/:claimId/approval` | `HumanApprovalPage` | yes | dynamic + actions | `selectWorkspaceApprovalRead` | `06-claim-actions.spec.ts`, `16-approval-validation.spec.ts` (new) |
| 10 | `/claims/:claimId/audit` | `AuditCostPage` | yes | dynamic | `selectWorkspaceAudit` | `06-claim-actions.spec.ts`, `17-audit.spec.ts` (new) |
| 11 | `/claims/:claimId/policy` | `PolicyCoveragePage` | yes | dynamic display | `selectWorkspacePolicy` | `05-claim-1006.spec.ts` |
| 12 | `/claims/:claimId/customer-vehicle` | `CustomerVehiclePage` | yes | dynamic display | `selectWorkspaceCustomerVehicle` | `05-claim-1006.spec.ts` |
| 13 | `/demo` | `DemoScenarioPage` | yes | static | mock data | `09-dashboard.spec.ts` (new) |
| 14 | `*` (catch-all) | `<Navigate to="/" replace />` | yes (via AppShell) | redirect | n/a | `18-negative.spec.ts` (new) |

## Sidebar-pinned routes (re-using same routes)

Same routes 1-12 above. Sidebar additionally exposes two **disabled-future** entries (`/#vehicles`, `/#settings`) which are visually distinct, non-clickable, and intentional placeholders for the next release.

## Disabled-future control inventory

| Sidebar/topbar control | Where | Classification |
|---|---|---|
| "Транспортні засоби (наступний реліз)" | sidebar | `DISABLED_FUTURE_EXPLICIT` |
| "Налаштування (наступний реліз)" | sidebar | `DISABLED_FUTURE_EXPLICIT` |
| "Довідка" icon button | topbar | `DISABLED_FUTURE_EXPLICIT` (title="Довідка з'явиться у наступному релізі") |
| "Сповіщення" icon button (with badge 3) | topbar | `DISABLED_FUTURE_EXPLICIT` (title="Центр сповіщень з'явиться у наступному релізі") |
| "Сьогодні" / "7 днів" period chips on Dashboard | dashboard header | `DISABLED_FUTURE_EXPLICIT` (via `<DeferredActionButton>`) |
| 5 segment chips on Dashboard queue ("Усі","ДТП","Високий ризик","Чекає AI","Чекає рішення") | dashboard queue | `DISABLED_FUTURE_EXPLICIT` (`disabled aria-disabled="true"` with title pointing to /claims) |

## Catch-all behavior

Unknown routes redirect to `/` (Dashboard). Verified by `18-negative.spec.ts` opening `/this-route-does-not-exist` and asserting the URL settles on `/`.

## 404 surface

There is no dedicated 404 page — by design, unknown routes degrade to the Dashboard. This is intentional for a small portfolio app; an explicit 404 page is candidate work for a later gate.

## Counts

- **Mounted routes:** 13 + catch-all = 14
- **Auth-protected:** 13
- **Public:** 1 (`/login`)
- **Click-through reachable from sidebar:** 11 (excluding `/demo` which is topbar-only and the 2 disabled-future entries)
- **Disabled-future entries:** 6 (2 sidebar + 4 topbar/dashboard period+segment)
