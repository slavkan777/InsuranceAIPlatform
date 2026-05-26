---
title: "Backend DTO Contracts V0.1 — InsuranceAIPlatform"
type: knowledge
status: active
created: 2026-05-27
tags: [backend, dto, contracts, insurance, dotnet, portfolio]
---

# Backend DTO Contracts V0.1

**Project:** InsuranceAIPlatform — Auto Insurance Claim AI Workbench (portfolio mockup)
**Stack:** .NET 9 · ASP.NET Core · In-memory seed · Screen-friendly read DTOs
**Design rules:**
- Screen-friendly read models for P0 (shape mirrors what the UI components consume).
- No EF entity classes ever exposed from controllers — DTOs only.
- Explicit fields, no dynamic/object/JObject.
- Stable IDs — no random generation in seed data.
- No real PII; synthetic data only.
- No mega-DTO unless justified; prefer focused per-screen DTOs over one universal ClaimDto.
- All `string?` fields are nullable only when genuinely optional in the data model.

---

## 1. Claims — Summary & List

---

### ClaimSummaryDto

**Purpose:** Dashboard aggregate counters. One object returned by GET `/api/claims/summary`.
**Consumer screen:** `/` (Dashboard overview)
**Source mock:** Derived from `getClaimsQueue()` result aggregation
**Priority:** P0
**Backward-compat note:** New counter fields can be added without breaking consumers; never remove existing fields.

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `totalActive` | `int` | No | Total claims in active states |
| `pendingReview` | `int` | No | Claims awaiting human review |
| `highRisk` | `int` | No | Claims with risk level "Високий" |
| `avgSlaRemainingHours` | `double` | No | Average SLA hours remaining across active claims |
| `processedToday` | `int` | No | Claims completed or transitioned today |
| `aiAnalysisRunning` | `int` | No | Claims with AI analysis currently in-progress |

---

### ClaimListItemDto

**Purpose:** One row in the claims queue table. Returned as `ClaimListItemDto[]` by GET `/api/claims`.
**Consumer screen:** `/claims`
**Source mock:** `getClaimsQueue()` → `ClaimRow` (canonical frontend type)
**Priority:** P0
**Backward-compat note:** `documentsCount` is a display string "6/7" matching existing UI; do not change to two ints without updating the UI component.

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `id` | `string` | No | Claim identifier, e.g. "CLM-1006" |
| `customer` | `string` | No | Full name, synthetic only |
| `vehicle` | `string` | No | "Make Model Year", e.g. "Toyota Camry 2021" |
| `eventType` | `string` | No | Event category, e.g. "ДТП", "Крадіжка" |
| `status` | `string` | No | Ukrainian enum value: 'В роботі'\|'Збір документів'\|'AI-обробка'\|'Високий ризик'\|'Готова'\|'Завершено' |
| `documentsCount` | `string` | No | Display string "received/total", e.g. "6/7" |
| `aiStatus` | `string` | No | e.g. "Завершено", "В обробці", "Не запущено" |
| `risk` | `string` | No | Ukrainian: 'Низький'\|'Середній'\|'Високий' |
| `sla` | `string` | No | Remaining SLA display string, e.g. "2год 14хв" |
| `nextAction` | `string` | No | Short action label for the adjuster |
| `updated` | `DateTimeOffset` | No | Last update timestamp (ISO 8601) |

---

### ClaimDetailsDto

**Purpose:** Full claim detail page. All fields needed to render the claim header, financials, and AI summary panel.
**Consumer screen:** `/claims/CLM-1006` and used as base data by all sub-screens
**Source mock:** `getClaimById(id)` → `ClaimDetail` (canonical frontend type)
**Priority:** P0
**Backward-compat note:** Financial fields (`estimate`, `expectedBenchmark`, `deductible`, `recommendedPayout`) are `decimal` in .NET, serialized as JSON number. Do not serialize as string.

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `id` | `string` | No | "CLM-1006" |
| `customer` | `string` | No | Full name |
| `customerId` | `string` | No | "CUST-0081" |
| `vehicle` | `string` | No | "Toyota Camry 2021" |
| `vehicleVin` | `string` | No | Masked VIN, e.g. "****8842" |
| `policy` | `string` | No | Policy product name |
| `policyId` | `string` | No | "POL-2025-AC-4421" |
| `eventType` | `string` | No | "ДТП" |
| `eventDate` | `DateOnly` | No | "2026-05-18" |
| `location` | `string` | No | "Бориспіль" |
| `description` | `string?` | Yes | Free-text event description |
| `status` | `string` | No | Ukrainian ClaimStatus enum value |
| `risk` | `string` | No | Ukrainian RiskLevel enum value |
| `riskScore` | `int` | No | 0–100, e.g. 82 |
| `confidence` | `int` | No | Overall model confidence %, e.g. 78 |
| `slaDeadline` | `DateTimeOffset` | No | SLA expiry |
| `documentsReceived` | `int` | No | 6 |
| `documentsTotal` | `int` | No | 7 |
| `missingDocument` | `string?` | Yes | Label of missing doc, null if complete |
| `estimate` | `decimal` | No | Repair estimate from invoice |
| `expectedBenchmark` | `decimal` | No | Market benchmark amount |
| `deductible` | `decimal` | No | Policy deductible amount |
| `recommendedPayout` | `decimal` | No | AI recommended payout (advisory) |
| `traceId` | `string` | No | "trc_8f3d2a7e" |
| `runId` | `string` | No | "run_8f3d2a7e" |
| `tokens` | `int` | No | Total tokens used in AI run |
| `cost` | `decimal` | No | AI run cost in USD |
| `durationSec` | `double` | No | AI pipeline duration seconds |

---

## 2. Documents & Photos

---

### ClaimDocumentDto

**Purpose:** Single item in the document checklist, covering both uploaded documents and damage photos.
**Consumer screen:** `/documents`
**Source mock:** `getClaimDocuments(id)` + `getClaimPhotos(id)` merged into one list
**Priority:** P0
**Backward-compat note:** `type` field distinguishes "document" from "photo" — the UI uses this to render different icons. Do not collapse into a boolean.

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `id` | `string` | No | Stable item id, e.g. "doc-3" |
| `label` | `string` | No | Display name |
| `detail` | `string?` | Yes | Supplemental detail, e.g. "AI впевненість 92%" |
| `status` | `string` | No | 'ok'\|'warn'\|'missing' — maps to `DocumentChecklistItem.status` |
| `type` | `string` | No | 'document'\|'photo' |
| `confidence` | `int?` | Yes | AI confidence % — only for photos |

---

### ClaimPhotoDto

**Purpose:** Photo item with damage-label context, used for visual review panel.
**Consumer screen:** `/documents` (photo gallery section)
**Source mock:** `getClaimPhotos(id)` → `DamagePhoto`
**Priority:** P0
**Backward-compat note:** `missing` flag must be preserved as a separate boolean; do not infer from `confidence` being null.

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `id` | `string` | No | Stable photo id |
| `label` | `string` | No | "Фото передньої частини" |
| `confidence` | `int?` | Yes | AI damage confidence %, null if photo missing |
| `missing` | `bool` | No | True if photo was not uploaded |

---

### DocumentChecklistItemDto

**Purpose:** Structured checklist entry with status indicator.
**Consumer screen:** `/documents` checklist panel
**Source mock:** `getClaimDocuments(id)` → `DocumentChecklistItem`
**Priority:** P0
**Backward-compat note:** Identical to `ClaimDocumentDto` for read purposes; kept separate to allow checklist-specific fields (e.g. `requiredBy`, `uploadedAt`) in future without polluting the merged list DTO.

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `id` | `string` | No | Item id |
| `label` | `string` | No | Checklist item name |
| `detail` | `string?` | Yes | Additional context |
| `status` | `string` | No | 'ok'\|'warn'\|'missing' |

---

## 3. Policy

---

### PolicyDto

**Purpose:** Full policy coverage detail with all coverage blocks and validation results.
**Consumer screen:** `/policy`
**Source mock:** `getPolicyCoverage(id)` → `{blocks, validation}`
**Priority:** P0

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `policyId` | `string` | No | "POL-2025-AC-4421" |
| `productName` | `string` | No | "Auto Comprehensive" |
| `coverageBlocks` | `PolicyCoverageDto[]` | No | All coverage line items |
| `validation` | `PolicyCheckResultDto` | No | Aggregate validation result for the claim |

---

### PolicyCoverageDto

**Purpose:** One coverage line: limit, deductible, applicability to current event.
**Consumer screen:** `/policy` coverage table
**Source mock:** `getPolicyCoverage(id)` blocks array
**Priority:** P0
**CLM-1006 has 5 blocks:** collision, liability, glass, theft, roadside.

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `id` | `string` | No | "cov-collision" |
| `label` | `string` | No | "Collision" |
| `limit` | `string` | No | Display string, e.g. "$50,000" or "Ринкова" |
| `deductible` | `string` | No | Display string, e.g. "$500" or "$0" |
| `applicable` | `bool` | No | Whether this coverage applies to the current event |
| `note` | `string?` | Yes | Optional adjuster note |

---

### PolicyExclusionDto

**Purpose:** Policy exclusion that may affect claim, if any.
**Consumer screen:** `/policy` exclusions section
**Priority:** P2 (not needed for CLM-1006 demo)

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `id` | `string` | No | Exclusion id |
| `label` | `string` | No | Short name |
| `description` | `string` | No | Full exclusion text |
| `triggered` | `bool` | No | True if this exclusion is triggered by current event |

---

### PolicyCheckResultDto

**Purpose:** Aggregate policy validation result for the event.
**Consumer screen:** `/policy` validation banner
**Priority:** P0

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `covered` | `bool` | No | True if event is within policy scope |
| `coverageType` | `string` | No | Applicable coverage name |
| `validationNotes` | `string[]` | No | List of validation note strings |
| `exclusionTriggered` | `bool` | No | True if any exclusion fired |

---

## 4. Customer & Vehicle

---

### CustomerDto

**Purpose:** Customer identity and history summary.
**Consumer screen:** `/customer-vehicle`
**Source mock:** `getCustomerVehicleContext(id)` customer section
**Priority:** P0
**Backward-compat note:** No real personal data — synthetic names, masked IDs only.

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `customerId` | `string` | No | "CUST-0081" |
| `fullName` | `string` | No | Synthetic full name |
| `previousClaimsCount` | `int` | No | Number of prior claims |
| `customerSince` | `DateOnly` | No | Policy holder since date |
| `communicationHistory` | `CommunicationEntryDto[]` | No | Prior contact entries |

---

### VehicleDto

**Purpose:** Vehicle details for the insured asset.
**Consumer screen:** `/customer-vehicle`
**Source mock:** `getCustomerVehicleContext(id)` vehicle section
**Priority:** P0

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `make` | `string` | No | "Toyota" |
| `model` | `string` | No | "Camry" |
| `year` | `int` | No | 2021 |
| `vin` | `string` | No | Masked, e.g. "****8842" |
| `color` | `string?` | Yes | Optional |
| `mileage` | `int?` | Yes | Optional odometer reading |

---

### CommunicationEntryDto

**Purpose:** One entry in customer communication history.
**Consumer screen:** `/customer-vehicle` history panel
**Priority:** P0

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `date` | `DateOnly` | No | Contact date |
| `channel` | `string` | No | "Email"\|"Phone"\|"Portal" |
| `summary` | `string` | No | Short summary of interaction |

---

## 5. AI Evidence

---

### AiEvidenceDto

**Purpose:** Full AI analysis result for the claim. Advisory only — not a binding decision.
**Consumer screen:** `/ai-evidence`
**Source mock:** `getAiAnalysis(id)` → `{findings, evidence, modelConfidence, extractedEntities}`
**Priority:** P0
**Advisory invariant:** Controller must not label this as a "decision" or "verdict" — it is "AI advisory analysis."

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `runId` | `string` | No | "run_8f3d2a7e" |
| `modelConfidence` | `int` | No | Overall confidence %, 0–100 |
| `findings` | `AiFindingDto[]` | No | Key findings list |
| `evidence` | `EvidenceSourceDto[]` | No | Supporting evidence items |
| `extractedEntities` | `ExtractedEntityDto[]` | No | Structured fields extracted by AI |
| `modelConfidenceBreakdown` | `ConfidenceBreakdownItemDto[]` | No | Per-stage confidence scores |

---

### AiAnalysisRunDto

**Purpose:** Result of triggering a new AI analysis run.
**Consumer screen:** `/ai-evidence` (trigger action)
**Source mock:** `runMockAiAnalysis(id)` → `MockAiRunResult`
**Priority:** P1

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `runId` | `string` | No | New run identifier |
| `status` | `string` | No | 'succeeded'\|'failed'\|'in-progress' |
| `startedAt` | `DateTimeOffset` | No | Run start time |

---

### AiFindingDto

**Purpose:** One AI finding entry — category + text + severity.
**Consumer screen:** `/ai-evidence` findings panel
**Priority:** P0

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `id` | `string` | No | Stable id |
| `category` | `string` | No | e.g. "Документи", "Оцінка збитку", "Покриття" |
| `text` | `string` | No | Finding description |
| `severity` | `string` | No | 'ok'\|'warn'\|'error' |

---

### EvidenceSourceDto

**Purpose:** One supporting evidence source referenced by AI.
**Consumer screen:** `/ai-evidence` evidence panel
**Priority:** P0

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `id` | `string` | No | Stable id |
| `source` | `string` | No | Document name that provided the evidence |
| `text` | `string` | No | Extracted text or summary |
| `confidence` | `int` | No | Confidence % for this evidence item |

---

### ExtractedEntityDto

**Purpose:** One structured field extracted by the AI field extractor.
**Consumer screen:** `/ai-evidence` extracted entities table
**Source mock:** `ExtractedEntity` canonical frontend type
**Priority:** P0

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `field` | `string` | No | Field name, e.g. "VIN", "Дата події" |
| `value` | `string` | No | Extracted value |
| `source` | `string` | No | Source document label |
| `confidence` | `int` | No | 0–100 |

---

### ConfidenceBreakdownItemDto

**Purpose:** Per-stage AI model confidence score.
**Consumer screen:** `/ai-evidence` confidence chart
**Priority:** P0

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `stage` | `string` | No | Pipeline stage name |
| `confidence` | `int` | No | 0–100 |

---

## 6. Risk Assessment

---

### RiskAssessmentDto

**Purpose:** Full risk assessment for the claim including score, factors, and pipeline status.
**Consumer screen:** `/risks`
**Source mock:** `getRiskReview(id)` → `{score, threshold, factors, pipeline}`
**Priority:** P0

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `score` | `int` | No | Risk score 0–100, e.g. 82 |
| `threshold` | `int` | No | Auto-approve threshold, e.g. 60 |
| `level` | `string` | No | 'Низький'\|'Середній'\|'Високий' |
| `factors` | `RiskFactorDto[]` | No | Contributing risk factors |
| `pipeline` | `PipelineStageDto[]` | No | AI pipeline stage statuses |

---

### RiskFactorDto

**Purpose:** One contributing factor to the risk score.
**Consumer screen:** `/risks` factors breakdown
**Source mock:** `RiskFactor` canonical frontend type
**Priority:** P0

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `id` | `string` | No | Stable id |
| `label` | `string` | No | Factor name, e.g. "Сума збитку" |
| `contribution` | `int` | No | Points contributed to total score |

---

### PipelineStageDto

**Purpose:** One stage in the AI processing pipeline with its run status.
**Consumer screen:** `/risks` pipeline panel
**Priority:** P0

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `stage` | `string` | No | Stage name |
| `status` | `string` | No | 'OK'\|'WARN'\|'BLOCK' |

---

## 7. Approval

---

### ApprovalDraftDto

**Purpose:** Current approval state for a claim, plus available decision options.
**Consumer screen:** `/approval`
**Source mock:** `saveApprovalDraft(id, ...)` read side
**Priority:** P0 (read) / P1 (write)

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `claimId` | `string` | No | "CLM-1006" |
| `currentDecision` | `string?` | Yes | Current saved decision, null if not yet drafted |
| `notes` | `string?` | Yes | Current draft notes |
| `savedAt` | `DateTimeOffset?` | Yes | When draft was last saved, null if never saved |
| `submitted` | `bool` | No | True if approval has been submitted (final) |
| `submittedAt` | `DateTimeOffset?` | Yes | Submission timestamp |
| `availableOptions` | `HumanDecisionOptionDto[]` | No | Options shown to adjuster |
| `aiRecommendation` | `string?` | Yes | AI suggested action label (advisory only) |
| `recommendedPayout` | `decimal` | No | AI recommended payout amount (advisory only) |

---

### HumanDecisionOptionDto

**Purpose:** One decision option available to the human adjuster.
**Consumer screen:** `/approval` decision selector
**Priority:** P0
**CLM-1006 has 4 options:** approve, request (recommended), reject, escalate.

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `value` | `string` | No | 'approve'\|'request'\|'reject'\|'escalate' |
| `label` | `string` | No | Display label, e.g. "Запит додатк. матеріалів" |
| `recommended` | `bool` | No | True if AI suggests this option |
| `description` | `string?` | Yes | Optional tooltip description |

---

## 8. Audit & Telemetry

---

### AuditTraceDto

**Purpose:** Complete audit trace for a claim's AI processing run. First-class portfolio artifact.
**Consumer screen:** `/audit`
**Source mock:** `getAuditTrace(id)` → `{runId, traceId, model, tokens, cost, durationSec, events, distribution}`
**Priority:** P0

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `runId` | `string` | No | "run_8f3d2a7e" |
| `traceId` | `string` | No | "trc_8f3d2a7e" |
| `model` | `string` | No | "Azure OpenAI (mock)" in demo |
| `tokens` | `int` | No | Total tokens consumed |
| `cost` | `decimal` | No | Total cost USD |
| `durationSec` | `double` | No | Pipeline duration |
| `events` | `AuditEventDto[]` | No | Ordered audit events |
| `costDistribution` | `CostDistributionItemDto[]` | No | Per-stage cost breakdown |

---

### AuditEventDto

**Purpose:** One event in the audit trail.
**Consumer screen:** `/audit` event log
**Source mock:** `AuditRow` canonical frontend type
**Priority:** P0

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `time` | `string` | No | Display time string, e.g. "10:21:00" |
| `actor` | `string` | No | System component name |
| `action` | `string` | No | Action description |
| `result` | `string` | No | 'OK'\|'WARN'\|'BLOCK' |

---

### CostDistributionItemDto

**Purpose:** Per-stage AI cost allocation.
**Consumer screen:** `/audit` cost chart
**Priority:** P0

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `stage` | `string` | No | Stage name |
| `cost` | `decimal` | No | Cost in USD for this stage |

---

### CostTelemetryDto

**Purpose:** Aggregated cost metrics for a claim or across all claims.
**Consumer screen:** `/audit` cost summary panel
**Source mock:** Partially from `getAuditTrace()` fields
**Priority:** P2

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `claimId` | `string?` | Yes | If filtered by claim; null for aggregate |
| `totalCost` | `decimal` | No | Sum of all AI run costs |
| `totalTokens` | `int` | No | Sum of all tokens |
| `runsCount` | `int` | No | Number of AI runs |
| `avgCostPerRun` | `decimal` | No | Average cost per run |
| `breakdown` | `CostDistributionItemDto[]` | No | Stage breakdown |

---

## 9. Demo & System

---

### DemoScenarioDto

**Purpose:** Structured demo walkthrough steps for the guided tour.
**Consumer screen:** `/demo`
**Source mock:** `getDemoScenario()` → `DemoStep[]`
**Priority:** P0

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `steps` | `DemoStepDto[]` | No | Ordered demo steps |
| `goldenClaimId` | `string` | No | "CLM-1006" — the canonical demo claim |

---

### DemoStepDto

**Purpose:** One demo tour step with navigation target.
**Consumer screen:** `/demo` step cards
**Source mock:** `DemoStep` canonical frontend type
**Priority:** P0

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `step` | `int` | No | Step number, 1-based |
| `title` | `string` | No | Step title |
| `caption` | `string` | No | Explanation text shown in UI |
| `pdfRef` | `string?` | Yes | Reference to portfolio PDF section |
| `route` | `string` | No | Target UI route, e.g. "/claims/CLM-1006" |

---

### SystemDemoStatusDto

**Purpose:** Backend health and demo-seed readiness. Used by frontend at startup and by `/demo` screen.
**Consumer screen:** App init guard, `/demo`
**Source mock:** Replaces the implicit "is backend reachable?" check in `mockInsuranceApi.ts`
**Priority:** P0

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `status` | `string` | No | 'ready'\|'seeding'\|'error' |
| `seedLoaded` | `bool` | No | True if in-memory seed data is loaded |
| `claimsSeeded` | `int` | No | Count of claims in seed |
| `goldenClaimId` | `string` | No | "CLM-1006" |
| `apiVersion` | `string` | No | Semver, e.g. "0.1.0" |
| `environment` | `string` | No | "Development"\|"Production" |
| `timestamp` | `DateTimeOffset` | No | Server UTC time at response |

---

## 10. Write DTOs (Input Models)

These match `src/api/insuranceApi.types.ts` — `ApprovalDraftInput`, `MockApiAck`, `MockAiRunResult`.

---

### ApprovalDraftInput

**Purpose:** Body for POST `/api/claims/{claimId}/approval/draft`.
**Source mock:** `ApprovalDraftInput` from `insuranceApi.types.ts`
**Priority:** P1

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `claimId` | `string` | No | Must match path `{claimId}` |
| `decision` | `string?` | Yes | 'approve'\|'request'\|'reject'\|'escalate'\|null |
| `notes` | `string?` | Yes | Adjuster free-text notes; max 2000 chars |

---

### ApprovalSubmitInput

**Purpose:** Body for POST `/api/claims/{claimId}/approval/submit`. Decision must be non-null.
**Priority:** P1

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `claimId` | `string` | No | Must match path `{claimId}` |
| `decision` | `string` | No | Required: 'approve'\|'request'\|'reject'\|'escalate' |
| `notes` | `string?` | Yes | Final notes |
| `adjusterCode` | `string?` | Yes | Optional adjuster identifier for audit |

---

### CustomerMessageDraftInput

**Purpose:** Body for POST `/api/claims/{claimId}/customer-message/draft`.
**Source mock:** `sendCustomerRequest(id)` — currently no body; this adds structured input.
**Priority:** P1

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `claimId` | `string` | No | Must match path `{claimId}` |
| `channel` | `string` | No | 'email'\|'phone'\|'portal' |
| `subject` | `string?` | Yes | Message subject line |
| `body` | `string` | No | Message body text; max 5000 chars |

---

### MockApiAck

**Purpose:** Standard acknowledgment response for all successful write operations.
**Source mock:** `MockApiAck` from `insuranceApi.types.ts` (`{ok:true, savedAt, note}`)
**Priority:** P1
**Backward-compat note:** `ok` is always `true` on 200 — errors use `ApiErrorDto` with 4xx/5xx status instead.

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `ok` | `bool` | No | Always `true` |
| `savedAt` | `DateTimeOffset` | No | Server timestamp of the save |
| `note` | `string?` | Yes | Optional human-readable note, e.g. "Draft saved." |

---

## 11. Shared / Error

---

### ApiErrorDto

**Purpose:** Uniform error envelope for all 4xx and 5xx responses.
**Consumer:** All screens (error handling in `insuranceApiClient.ts`)
**Priority:** P0 (applies to all endpoints)

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `code` | `string` | No | SCREAMING_SNAKE machine token, e.g. "CLAIM_NOT_FOUND" |
| `message` | `string` | No | Human-readable sentence. No stack trace, no exception type. |
| `traceId` | `string` | No | Propagated from `HttpContext.TraceIdentifier` |

---

## 12. DTO ↔ Mock Getter ↔ Frontend Type Mapping

| DTO | Replaces mock getter | Canonical frontend type | Screen |
|-----|---------------------|------------------------|--------|
| `ClaimSummaryDto` | Derived from `getClaimsQueue()` aggregate | — | `/` |
| `ClaimListItemDto` | `getClaimsQueue()` | `ClaimRow` | `/claims` |
| `ClaimDetailsDto` | `getClaimById(id)` | `ClaimDetail` | `/claims/CLM-1006` |
| `ClaimDocumentDto` | `getClaimDocuments(id)` + `getClaimPhotos(id)` | `DocumentChecklistItem` + `DamagePhoto` | `/documents` |
| `ClaimPhotoDto` | `getClaimPhotos(id)` | `DamagePhoto` | `/documents` |
| `DocumentChecklistItemDto` | `getClaimDocuments(id)` | `DocumentChecklistItem` | `/documents` |
| `PolicyDto` | `getPolicyCoverage(id)` | — | `/policy` |
| `PolicyCoverageDto` | `getPolicyCoverage(id)` blocks | — | `/policy` |
| `PolicyCheckResultDto` | `getPolicyCoverage(id)` validation | — | `/policy` |
| `AiEvidenceDto` | `getAiAnalysis(id)` | — | `/ai-evidence` |
| `AiAnalysisRunDto` | `runMockAiAnalysis(id)` | `MockAiRunResult` | `/ai-evidence` |
| `AiFindingDto` | `getAiAnalysis(id)` findings | — | `/ai-evidence` |
| `EvidenceSourceDto` | `getAiAnalysis(id)` evidence | — | `/ai-evidence` |
| `ExtractedEntityDto` | `getAiAnalysis(id)` extractedEntities | `ExtractedEntity` | `/ai-evidence` |
| `RiskAssessmentDto` | `getRiskReview(id)` | — | `/risks` |
| `RiskFactorDto` | `getRiskReview(id)` factors | `RiskFactor` | `/risks` |
| `CustomerDto` | `getCustomerVehicleContext(id)` | — | `/customer-vehicle` |
| `VehicleDto` | `getCustomerVehicleContext(id)` | — | `/customer-vehicle` |
| `ApprovalDraftDto` | `saveApprovalDraft(id, ...)` read side | — | `/approval` |
| `HumanDecisionOptionDto` | — | — | `/approval` |
| `AuditTraceDto` | `getAuditTrace(id)` | — | `/audit` |
| `AuditEventDto` | `getAuditTrace(id)` events | `AuditRow` | `/audit` |
| `CostTelemetryDto` | `getAuditTrace(id)` cost fields | — | `/audit` |
| `DemoScenarioDto` | `getDemoScenario()` | — | `/demo` |
| `DemoStepDto` | `getDemoScenario()` steps | `DemoStep` | `/demo` |
| `SystemDemoStatusDto` | Implicit backend reachability | — | App init, `/demo` |
| `ApiErrorDto` | — | — | All screens |
| `MockApiAck` | `saveApprovalDraft()` write / `sendCustomerRequest()` | `MockApiAck` | `/approval` |
