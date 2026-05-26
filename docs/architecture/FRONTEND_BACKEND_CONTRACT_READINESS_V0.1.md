# Frontend → Backend Contract Readiness — V0.1

Contract-readiness only. No backend, no network calls implemented.

| Frontend need | Current mock source | Future .NET endpoint | DTO candidate | Notes |
|---|---|---|---|---|
| Claims queue | `mockInsuranceApi.getClaimsQueue` ← `data/mock/claims.claimRows` | `GET /api/claims/queue` | `ClaimSummary[]` (`ClaimRow`) | supports filters/segment later |
| Claim details | `getClaimById` ← `goldenClaim` | `GET /api/claims/{id}` | `Claim` (`ClaimDetail`) | golden claim = CLM-1006 |
| Documents | `getClaimDocuments` ← `documentsChecklist` | `GET /api/claims/{id}/documents` | `ClaimDocument[]` | includes missing-evidence flag |
| Photos | `getClaimPhotos` ← `damagePhotos` | `GET /api/claims/{id}/photos` | `ClaimPhoto[]` | rear-bumper photo missing |
| AI analysis / evidence | `getAiAnalysis` ← findings/evidence/confidence/entities | `GET /api/claims/{id}/ai-analysis` | `AiFinding[]`, `EvidenceSource[]`, `ModelConfidenceBar[]`, `ExtractedEntity[]` | advisory only |
| AI run | `runMockAiAnalysis` | `POST /api/claims/{id}/ai-analysis/run` | `MockAiRunResult` → `AiRunResult` | poll/stream later |
| Risk review | `getRiskReview` ← `riskFactors` | `GET /api/claims/{id}/risk-review` | `RiskAssessment` (`RiskFactor[]`, `DeterministicCheck[]`) | deterministic checks separate |
| Policy coverage | `getPolicyCoverage` ← `policyCoverageBlocks`/`policyValidation` | `GET /api/claims/{id}/policy` | `PolicyCoverage[]` + validation | |
| Customer/vehicle | `getCustomerVehicleContext` ← prev claims/comms | `GET /api/claims/{id}/customer-vehicle` | `Customer`, `Vehicle` | masked PII |
| Human approval draft | `saveApprovalDraft` | `POST /api/claims/{id}/approval-draft` | `ApprovalDraftInput` → `ApprovalDraftResult` | human-controlled draft only |
| Customer request | `sendCustomerRequest` | `POST /api/claims/{id}/customer-request` | `CustomerRequestResult` | SMS+email request |
| Audit / cost trace | `getAuditTrace` ← `auditTrail`/`costDistribution` | `GET /api/claims/{id}/audit` | `AuditRun` (`AuditEvent[]`, `CostTrace`, `ModelTrace`, `TokenUsage`) | governance evidence |
| Demo scenario | `getDemoScenario` ← `demoSteps` | client-only | `DemoStep[]` | no endpoint needed |

Readiness: every UI need maps to (a) a typed mock function, (b) a candidate endpoint, and (c) a DTO candidate already declared in `src/types/*` or `src/api/insuranceApi.types.ts`.
