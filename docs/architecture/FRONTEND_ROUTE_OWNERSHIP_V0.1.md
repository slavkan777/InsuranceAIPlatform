# Frontend Route Ownership — V0.1

Future backend API candidates are **documentation-only**. No `fetch`/`axios` exists in the app.

| Route | Page | Purpose | Primary state source | Selectors used / recommended | Future backend API candidate | AI / governance note |
|---|---|---|---|---|---|---|
| `/` | DashboardPage | Ops overview + queue + AI snapshot | `claims` slice + mock data (dashboard) | `selectSelectedClaimId` (used); `selectClaimsQueue` (rec) | `GET /api/claims/queue` · `GET /api/dashboard/summary` | AI snapshot is advisory; human-review flag shown |
| `/claims` | ClaimsListPage | Claim queue + filters | `claims` slice | `selectClaimsState` (used); `selectClaimsFilters` (rec) | `GET /api/claims/queue?filters` | none (queue view) |
| `/claims/CLM-1006` | ClaimWorkspacePage | Case command center | mock `goldenClaim` + claim-1006 | `selectActiveClaim` (rec) | `GET /api/claims/{id}` | AI recommendation advisory; human next-action |
| `…/documents` | DocumentsPhotosPage | Evidence completeness | `documents` slice + claim-1006 | `selectDocumentChecklist`, `selectMissingEvidenceFlag` (rec) | `GET /api/claims/{id}/documents` · `POST …/documents/request` | missing evidence blocks auto-approval |
| `…/ai-evidence` | AiEvidencePage | AI findings + evidence | `aiReview` slice + claim-1006 | `selectAiRunStatus`, `selectSelectedAiEvidence`, `selectAiConfidenceFilter` (rec) | `GET /api/claims/{id}/ai-analysis` · `POST …/ai-analysis/run` | guardrail: AI does not decide |
| `…/risks` | RisksChecksPage | Risk score + checks | mock claim-1006 (deterministic) | `selectRiskSummary`, `selectRiskFactors` (rec) | `GET /api/claims/{id}/risk-review` | deterministic checks separate from AI; auto-approval blocked |
| `…/approval` | HumanApprovalPage | Human decision draft | `approval` slice | `selectApprovalDecision`, `selectApprovalChecklist`, `selectReviewerNotes` (rec) | `POST /api/claims/{id}/approval-draft` | **human is final**; AI option marked advisory |
| `…/audit` | AuditCostPage | Audit + cost governance | mock claim-1006 (audit) | `selectAuditRun`, `selectCostTrace` (rec) | `GET /api/claims/{id}/audit` | governance panel: auto-approval NOT allowed |
| `…/policy` | PolicyCoveragePage | Policy coverage | mock claim-1006 (policy) | `selectPolicyCoverage` (rec) | `GET /api/claims/{id}/policy` | coverage validation shown |
| `…/customer-vehicle` | CustomerVehiclePage | Customer/vehicle context | mock claim-1006 | `selectCustomerVehicleContext` (rec) | `GET /api/claims/{id}/customer-vehicle` | masked synthetic PII; privacy notice |
| `/demo` | DemoScenarioPage | Guided usage example | `demo` slice + claim-1006 | `selectDemoIsPlaying`, `selectDemoCurrentStep` (rec) | `GET /api/demo/scenario` | demo = usage example, not real execution |

"(used)" = wired today. "(rec)" = selector exists or recommended; page migration is a low-risk backlog item.
