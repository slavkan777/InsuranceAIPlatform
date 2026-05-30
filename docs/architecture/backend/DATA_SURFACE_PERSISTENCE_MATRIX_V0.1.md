# Data Surface — Persistence Matrix
**Date:** 2026-05-30
**Gate:** AZURE_SQL_FULL_PERSISTENCE_ALL_PAGES_DEPLOY_PUSH_V0.1

---

## How to Read This Table

- **Source** describes where the data actually originates at runtime:
  - *SQL live* — read from / written to Azure SQL via EF Core.
  - *In-memory golden fixture* — curated static data served from application memory
    (no DB round-trip for these records).
  - *Mock* — stub/placeholder return; no real data source wired yet.
- **Status** reflects state after this gate.

---

## Matrix

| Page / Feature | Data Shown | Source | Read Endpoint | Write Endpoint | Status |
|---|---|---|---|---|---|
| **Customer Directory** | Customer list (name, ID, status) | SQL live | `GET /api/customers` | — | Live — 200 rows returned |
| **Create Customer modal** | New customer form / created record | SQL live | `GET /api/customers?search=...` | `POST /api/customers` | Live — creates + persists; "Failed to fetch" resolved |
| **Claims List** | 15 claims (all) | SQL live | `GET /api/claims` | — | Live — 15 rows |
| **Claim Detail — golden (CLM-1006..1010)** | Full claim workspace | In-memory golden fixture | `GET /api/claims/{id}` | — | In-memory by design (curated rich fixture) |
| **Claim Detail — created (CLM-1011..1020+)** | Claim record fields | SQL live | `GET /api/claims/{id}` | — | Live — served from DB via HybridClaimReadService |
| **Documents tab (golden claims)** | ClaimDocument list | In-memory golden fixture | `GET /api/claims/{id}/documents` | — | In-memory by design |
| **AI Evidence tab (golden claims)** | AiFinding, AiEvidenceReference | In-memory golden fixture | `GET /api/claims/{id}/ai-evidence` | — | In-memory by design |
| **Risk Signals tab (golden claims)** | AiRiskSignal list | In-memory golden fixture | `GET /api/claims/{id}/risks` | — | In-memory by design |
| **Approval tab (golden claims)** | ApprovalDraft, options, payout | In-memory golden fixture | `GET /api/claims/{id}/approval` | — | In-memory by design |
| **Audit / Cost tab (golden claims)** | AuditEvent, CostTrace, TokenUsage | In-memory golden fixture | `GET /api/claims/{id}/audit` | — | In-memory by design |
| **Policy card (golden claims)** | Policy details | In-memory golden fixture | `GET /api/claims/{id}/policy` | — | In-memory by design |
| **Customer & Vehicle card (golden claims)** | SyntheticCustomer + Vehicle | In-memory golden fixture | `GET /api/claims/{id}/customer-vehicle` | — | In-memory by design |
| **Dashboard summary / metrics** | KPI cards, totals, trends | In-memory golden fixture | `GET /api/claims/summary` | — | In-memory by design |
| **Demo scenario** | Scripted walkthrough data | In-memory golden fixture | `GET /api/demo/scenario` | — | In-memory by design |

---

## Notes

**HybridClaimReadService** is the component that merges the two claim sources. For
any claim ID in the golden set (CLM-1006..CLM-1010), it returns the curated in-memory
fixture. For all other IDs (DB-created claims), it queries SQL. This means golden-claim
sub-resources (documents, AI, approval, audit, policy, customer-vehicle) intentionally
never hit SQL — they are display fixtures only.

**In-memory golden fixture** is not a bug or temporary shortcut for the golden claims —
it is the intended mechanism for the polished demo walkthrough. Those fixtures will only
move to SQL if a future gate explicitly decides to migrate them.

**Mock** entries (AI analysis on non-golden paths) return placeholder data. No AI
provider is wired for live inference at this gate.

**Write surface** is currently limited to `POST /api/customers`. Other write paths
(submit claim, upload document, approve, etc.) are either not yet implemented or
return mock responses.
