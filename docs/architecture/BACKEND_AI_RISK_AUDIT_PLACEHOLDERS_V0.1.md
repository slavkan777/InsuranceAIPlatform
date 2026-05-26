---
title: Backend AI, Risk & Audit Placeholders V0.1
type: knowledge
status: active
created: 2026-05-27
tags: [ai, risk, audit, services, interfaces, placeholders, governance]
---

# Backend AI, Risk & Audit Placeholders V0.1

## 1. Service Interfaces Table

### IAiEvidenceService

| Attribute | Detail |
|---|---|
| **Responsibility** | Return structured findings, evidence items, confidence score, and extracted entities for a given claim |
| **V0.1 Behavior** | Returns hardcoded synthetic data matching the frontend mock: findings array, evidence references, `modelConfidence: 0.78`, `extractedEntities` (VIN, date, location, policyNumber). Backed by `SyntheticAiEvidenceProvider` seeded with CLM-1006 values. |
| **Future Real Behavior** | Calls a real LLM provider (e.g. Azure OpenAI via `ILlmProvider` abstraction) to analyze claim documents/photos/context; returns structured JSON parsed from model response |
| **Must Audit** | Every invocation — record model label, token input/output, cost estimate, duration, traceId, runId |
| **Must Never Do** | Approve or reject a claim; assert fraud as a final fact; return a verdict that bypasses human review; expose a real API key or provider name if no real provider is configured |

---

### IRiskAssessmentService

| Attribute | Detail |
|---|---|
| **Responsibility** | Compute a risk score (0–100) from claim factors and compare to governance threshold |
| **V0.1 Behavior** | Deterministic rule-based scoring from 5 factors. CLM-1006 always produces score `82` with threshold `60`, risk level `Высокий` (`High`). Factors: repair estimate discrepancy (+38%), claim velocity, VIN history, document completeness (missing rear bumper photo), coverage gap flag. No ML/AI at V0.1. |
| **Future Real Behavior** | ML model or enhanced rule engine; scores vary dynamically based on real claim data; factor weights configurable |
| **Must Audit** | Score, threshold, level, factor breakdown, version of scoring rules used |
| **Must Never Do** | Automatically deny a claim based on score alone; label a claimant as a fraud perpetrator; suppress the score from the audit trail |

---

### IPolicyCheckService

| Attribute | Detail |
|---|---|
| **Responsibility** | Validate whether claim line items are covered under the associated policy; return coverage blocks and validation flags |
| **V0.1 Behavior** | Deterministic lookup against seeded policy data. CLM-1006 → policy `POL-2025-AC-4421`, type `Auto Comprehensive`; returns coverage blocks with validation flags (e.g. estimate overage, deductible applied). Hardcoded validation logic. |
| **Future Real Behavior** | Integrates with a policy management system or database; real-time coverage validation against policy terms |
| **Must Audit** | Policy ID evaluated, coverage result, validation flags, timestamp |
| **Must Never Do** | Silently override policy terms; hide coverage gaps from the adjuster; assume coverage without explicit policy record |

---

### IAuditTrailService

| Attribute | Detail |
|---|---|
| **Responsibility** | Append audit events for every significant action (AI run, risk score, approval draft, submission, demo step); provide read access to the full event list per claim |
| **V0.1 Behavior** | In-memory append-only event store. Pre-seeded with CLM-1006 audit events matching `getAuditTrace` mock: `traceId: trc_8f3d2a7e`, `runId: run_8f3d2a7e`, events array including Governance BLOCK "Авто-погодження заблоковано". Append is functional for new events written in-session. |
| **Future Real Behavior** | Persisted to SQL Server `dbo.AuditEvents` table; indexed by `ClaimId + Timestamp`; supports pagination and event type filters |
| **Must Audit** | Self-referential: audit service operations (e.g. flush, read) are NOT re-audited to avoid recursion |
| **Must Never Do** | Delete or mutate existing audit events; return a filtered view that omits governance blocks; skip auditing for any AI or approval action |

---

### ICostTelemetryService

| Attribute | Detail |
|---|---|
| **Responsibility** | Track synthetic token usage, cost estimates, and latency for each AI run; expose per-claim and aggregate cost summaries |
| **V0.1 Behavior** | Returns hardcoded CLM-1006 values: `tokens: 4261` (input 2100 + output 2161), `cost: $0.0187`, `durationSec: 18.9`. Distribution breakdown matches mock: Document Extraction 42%, Risk Scoring 28%, Policy Check 18%, Fraud Indicators 12%. No real billing integration. |
| **Future Real Behavior** | Reads token usage from real LLM provider response metadata; applies configured per-token pricing; writes to cost ledger table |
| **Must Audit** | Every cost entry links to `runId` and `traceId`; never orphaned cost records |
| **Must Never Do** | Present synthetic costs as real billing data in a production context; omit cost metadata from audit trace |

---

### IDemoScenarioService

| Attribute | Detail |
|---|---|
| **Responsibility** | Provide an ordered sequence of demo steps for guided walkthrough of the Auto Claim AI Workbench; report demo readiness status |
| **V0.1 Behavior** | Returns deterministic `demoSteps` array matching `getDemoScenario` mock. `GET /api/system/demo-status` returns `{ "demoReady": true, "seedClaimId": "CLM-1006", "demoMode": true }`. Auto-advance saga behavior remains deferred (P1). |
| **Future Real Behavior** | May support multiple scenario scripts; reset-to-seed endpoint for live demo reset |
| **Must Audit** | Demo session start/end events recorded in audit trail |
| **Must Never Do** | Serve demo endpoints in a production deployment without explicit `DemoMode: enabled` configuration flag; auto-advance without user intent in non-demo mode |

---

## 2. Hard Governance Invariants

These invariants apply to ALL services and endpoints. They are non-negotiable and must be enforced at the application layer, not just in documentation.

### INV-1: AI is Advisory Only
AI outputs from `IAiEvidenceService` and `IRiskAssessmentService` are **evidence and recommendations only**. The system MUST NOT:
- Automatically approve or reject a claim based on AI output alone
- Set claim status to `Approved` or `Rejected` without a recorded human decision
- Present an AI recommendation as a final determination to the claimant

The `HumanApprovalController` is the sole authority for final claim disposition. Any attempt to bypass it via an automated rule is a governance violation.

### INV-2: Fraud Assertion Prohibition
The system MUST NOT assert fraud as a final fact. Permitted language:
- "Risk indicators detected" — ALLOWED
- "Elevated risk score: 82/100" — ALLOWED
- "Potential inconsistency in repair estimate" — ALLOWED
- "Fraud confirmed" — FORBIDDEN
- "Fraudulent claim" — FORBIDDEN (in any displayed output or logged verdict)

Risk factors are advisory inputs to human review, not verdicts.

### INV-3: Every AI Run Is Audited and Costed
No invocation of `IAiEvidenceService.RunAnalysis()` may complete (success or failure) without:
1. An audit event appended via `IAuditTrailService`
2. A cost entry recorded via `ICostTelemetryService`
3. `runId` and `traceId` returned to the caller

If audit or cost recording fails, the AI run result MUST NOT be returned to the UI — surface the error instead.

### INV-4: Demo/Mock Provider Honesty
The mock audit trace references `"model": "Azure OpenAI"`. This label exists only in the synthetic demo dataset. The backend MUST:
- Never use the string `"Azure OpenAI"` in real provider calls unless that provider is actually configured and active
- Return `"providerLabel": "Demo/Synthetic"` for V0.1 responses
- Expose a `demoMode` boolean in any response that originates from synthetic data

### INV-5: Human Approval is Final
The only state transitions that move a claim to `Approved` or `Rejected` are those triggered by an authenticated human adjuster via the approval endpoints (`POST /api/claims/{id}/approval-draft`, `POST /api/claims/{id}/approval-submit`). No background job, AI run, or risk score may set these final states.
