# Triple Owner / Manual Simulation — Transcript

**Gate:** `PRE_AZURE_REALISTIC_DB_SANDBOX_AND_TRIPLE_OWNER_FLOW_FIX_V0.1`
**Date:** 2026-05-28
**Three passes:** new claim from scratch → CLM-1006 existing walkthrough → reviewer/tech-lead walkthrough.

All actions hit the LocalDB-backed API on `localhost:5284`. Mock AI provider used by default; AI advisory-only invariants verified.

---

## PASS 1 — New claim lifecycle (CLM-1023)

> **Owner persona:** "I'm a new adjuster. I just got a claim notification, I need to create the claim, attach documents, run AI, and propose a settlement. The system should record everything but not actually pay anyone."

**T+0:00 — Login state**
Confirmed: any unauthenticated `/` request redirects to `/login`. Credentials hint visible under the form. `Demo123!` accepted; session stored in `localStorage` under `iap.auth.demo.v1`.

**T+0:30 — Pick a synthetic customer**
`GET /api/customers?search=T0099` → returned `CUST-T0099 Synthetic Customer 099`. No PII; all synthetic.

**T+1:00 — Create the claim**
`POST /api/claims` with:
- vehicle: `Subaru Outback 2024`
- VIN: `VIN ****9999`
- eventType: `Зіткнення`
- eventDate: `2026-05-28`
- location: `Одеса, Французький бульвар 12`
- description: `Зіткнення з ліхтарним стовпом при паркуванні`

→ HTTP 200. ClaimId: `CLM-1023`. Audit row #57. Outbox row #52.

**T+1:30 — Open the new claim**
`GET /api/claims/CLM-1023` → 200. customer="Synthetic Customer 099", vehicle="Subaru Outback 2024", status="Новий". Claim row exists in DB with status history "Новий".

**T+2:00 — Upload a synthetic police report**
`POST /api/claims/CLM-1023/documents/upload` with kind=police-report, content=448-char synthetic narrative (collision with light pole, no injuries, inspector Кравченко Г.С.). HTTP 200. DocumentUploaded audit row #58. Outbox row #53. DB row `documents.ClaimDocuments` carries `Content nvarchar(max)` non-null, `UploadedAtUtc` set, `UploadedByActor = Synthetic Adjuster (human)`.

**T+2:30 — Request a missing document (no customer email)**
`POST /api/claims/CLM-1023/missing-document-requests` for "Фото пошкодженого стовпа". HTTP 200. MissingDocumentRequested audit #59 + outbox #54. No email/SMS sent — confirmed by absence of `EmailSent` / `SmsSent` audit categories (`SELECT COUNT(*) ... WHERE ActionType IN ('EmailSent','SmsSent') = 0`).

**T+3:00 — Run AI analysis (Mock)**
`POST /api/claims/CLM-1023/ai-analysis/run` → HTTP 200. `runId=run_a0fb8515`, provider=Mock, model=local-mock-v0.1, confidence=60, risk=low. Advisory-only guardrails all returned as expected (`canApprovePayout=false`, `canRejectClaim=false`, …).

**T+3:30 — Record AI decision**
`POST /api/claims/CLM-1023/ai-decision` with note "AI advisory aligned with human assessment". HTTP 200. `source=AI`. AiDecisionRecorded audit row #61 with actor=`AI System (advisory) (ai-system)` (distinct from human commands). Outbox #56.

**T+4:00 — Create payout simulation**
`POST /api/claims/CLM-1023/payout-simulation` with amount=3500, deductible=500, currency=USD, decisionSource=Hybrid, sourceAiRunId=run_a0fb8515. HTTP 200. simId=3, net=3000, status=DraftSimulated, SimulationOnly=true. Audit #62. Outbox #57. DB row in `approval.PayoutSimulations` has `SimulationOnly=1`.

**T+4:30 — Audit chain verification**
```sql
SELECT ActionType, Actor FROM audit_cost.AuditEvents
WHERE ClaimId='CLM-1023' ORDER BY Id
```
returns 6 rows in order:
```
ClaimCreated                Synthetic Adjuster (human)
DocumentUploaded            Synthetic Adjuster (human)
MissingDocumentRequested    Synthetic Adjuster (human)
AiAnalysisCompleted         Synthetic Adjuster (human)
AiDecisionRecorded          AI System (advisory) (ai-system)
PayoutSimulationCreated     Synthetic Adjuster (human)
```

> **Owner verdict on Pass 1:** "I created a claim end-to-end. The AI ran. The simulated payout is in the DB but flagged as a simulation. The audit history shows every step and who did it. Nothing was sent outside the system."

---

## PASS 2 — CLM-1006 existing walkthrough

> **Owner persona:** "I'm reviewing the original demo claim. The seeded one with the rich data. I want to confirm every tab still works and that adversarial probes hit walls."

**T+0:00 — Open CLM-1006**
`GET /api/claims/CLM-1006` → 200. customer="Роберт Джонсон", status="В роботі", risk="Високий".

**T+0:30 — Visit all 8 sub-routes**
```
/api/claims/CLM-1006                 200
/api/claims/CLM-1006/documents       200
/api/claims/CLM-1006/ai-evidence     200
/api/claims/CLM-1006/risks           200
/api/claims/CLM-1006/policy          200
/api/claims/CLM-1006/customer-vehicle 200
/api/claims/CLM-1006/approval        200
/api/claims/CLM-1006/audit           200
```
All 200. Rich detail preserved from prior gates.

**T+1:30 — Save approval draft**
`POST /api/claims/CLM-1006/approval-draft` with currentDecision=ApproveForReview, notes="P2 reviewer draft". HTTP 200. status=DraftSaved, audit=63, outbox=58.

**T+2:00 — Submit human decision (review-only, not payout)**
`POST /api/claims/CLM-1006/human-decision` with decision=ApproveForReview. HTTP 200. status=PendingApproval, audit=64, outbox=59. Claim row was NOT mutated — status transition is outbox-only by design.

**T+2:30 — Adversarial: ApproveForPayout**
`POST /api/claims/CLM-1006/human-decision` with decision=`ApproveForPayout`. HTTP 400. code=`INVALID_DECISION`. Allow-list rejected — only the 4 documented allowed enums (`ApproveForReview`, `RejectForReview`, `NeedsMoreInformation`, `RequestDocuments`) are accepted.

**T+3:00 — Adversarial: forbidden `/payout` endpoint**
`POST /api/claims/CLM-1006/payout` → HTTP 404. Not blocked at runtime — the endpoint **does not exist**. This is the architecture, not a runtime check.

**T+3:30 — Adversarial: forbidden `/customer-messages` endpoint**
`POST /api/claims/CLM-1006/customer-messages` → HTTP 404. Same — does not exist.

> **Owner verdict on Pass 2:** "Every legitimate tab returns data. Every legitimate write persists. Every illegitimate write is rejected at the type level or doesn't exist at all. The system has no path to leak money or messages."

---

## PASS 3 — Reviewer / tech-lead walkthrough

> **Reviewer persona:** "I'm a tech lead reviewing this for portfolio acceptance. I'll use SQL directly to verify safety invariants. I don't trust UI screenshots — I want database-level proof."

**T+0:00 — Login control**
Confirmed `/` requires authentication. `RequireAuth` redirects unauthenticated users. Logout button in TopBar clears `localStorage`. Bundle string `Вихід` present.

**T+0:30 — Customer directory count**
`GET /api/customers/count` → `{"count":200,"syntheticOnly":true}`. Exactly 200 synthetic test users in the directory.

**T+1:00 — Pagination test**
`GET /api/customers?page=5&pageSize=10` → returns 10 items from CUST-T0041 to CUST-T0050. Pagination math correct.

**T+1:30 — Database-level safety assertion: non-synthetic customers**
```sql
SELECT COUNT(*) FROM customers_policies.SyntheticCustomers WHERE IsSynthetic=0
```
→ **1**. That single row is `CUST-4421 Роберт Джонсон`, the seeded golden customer for CLM-1006. Still synthetic (no real PII) but tagged `IsSynthetic=false` so it doesn't count toward the "200 synthetic test users" set. The `/api/customers` endpoint filters to `IsSynthetic=true` only, so the golden customer never appears in the directory.

**T+2:00 — Realistic copy review**
TopBar carries "Local Sandbox" badge (not "Local Demo"). Sidebar carries "Локальне демо · синтетичні дані · AI порадницький, людина вирішує". Bundle string `Local Sandbox` present.

**T+2:30 — Audit ledger summary**
```sql
SELECT ActionType, COUNT(*) FROM audit_cost.AuditEvents GROUP BY ActionType ORDER BY 2 DESC
```
Returns 10 action types, all benign:
```
AiAnalysisCompleted          18
ApprovalDraftSaved           10
HumanDecisionSubmitted        6
MissingDocumentRequested      6
DocumentMetadataCreated       5
AiDecisionRecorded            4   ← 4 with ai-system actor
ClaimCreated                  3   ← new this gate
DocumentUploaded              3   ← new this gate
PayoutSimulationCreated       3   ← new this gate
```

**T+3:00 — Forbidden categories assertion**
```sql
SELECT COUNT(*) FROM audit_cost.AuditEvents
WHERE ActionType IN (
  'Payout','PayoutTransferred','CustomerMessageSent',
  'EmailSent','FraudConfirmed','ClaimStatusForciblyChanged','RealPayoutExecuted')
```
→ **0**. The audit ledger contains zero rows representing real-world side effects.

**T+3:30 — AI provider distribution**
```sql
SELECT ProviderMode, COUNT(*) FROM ai_analysis.AiAnalysisRuns GROUP BY ProviderMode
```
→ Mock=12, DeepSeek=6, Disabled=1. Real DeepSeek calls (6) were all opt-in via env variable, never default.

**T+4:00 — PayoutSimulations safety guarantee**
```sql
SELECT COUNT(*) AS Total, SUM(CASE WHEN SimulationOnly=0 THEN 1 ELSE 0 END) AS Unsafe
FROM approval.PayoutSimulations
```
→ Total=3, Unsafe=**0**. Every payout-simulation row has the schema-level `SimulationOnly=1` flag.

> **Reviewer verdict on Pass 3:** "Verified at the database level: 200 synthetic customers, 3 synthetic claims created this gate, 3 documents with content stored, 3 payout simulations all marked simulation-only, AI decisions carry ai-system actor attribution distinct from human, zero rows representing real money / real messages / real PII. System is ready for the operator's hands-on browser walk."

---

## Cross-pass verdict

All 3 passes verified end-to-end via:
- Direct API smoke (curl + JSON response inspection)
- DB-level row count assertions
- Audit chain order verification
- Adversarial probe rejection (allow-list enforcement + endpoint non-existence)
- AI source attribution (ai-system vs human actor types)

No real PII. No real payout. No real customer message. No real external HTTP outside the opt-in DeepSeek path (which is unchanged).

**READY_FOR_SLAVA_MANUAL_RETEST: yes.**
