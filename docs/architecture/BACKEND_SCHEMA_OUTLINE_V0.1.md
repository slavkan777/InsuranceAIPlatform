---
title: Backend Schema Outline
type: knowledge
status: active
created: 2026-05-27
tags: [backend, schema, sql-server, database, tables, ef-core]
---

# Backend Schema Outline V0.1

> **Scope:** Planning only. No DDL is executed. No DB is created or modified.
> DB creation is FORBIDDEN until the explicit `BACKEND_SQLSERVER_PERSISTENCE_GATE`.
> Target DB: `InsuranceAIPlatform` on `localhost,19772`. Schema: `dbo`.

---

## 1. Why `dbo` Schema

`dbo` is chosen over a custom application schema (e.g., `ins` or `claim`) for the following reasons:

1. **Portfolio clarity.** A single-application DB with one schema is clear and easy to navigate in SSMS. Multi-schema layout adds complexity with no benefit at this scope.
2. **EF Core default.** EF Core scaffolds to `dbo` by default; no `ToTable("X", "schema")` overrides needed, reducing boilerplate.
3. **SQL Server permission simplicity.** `dbo` requires no `GRANT USAGE ON SCHEMA` — reducing setup steps in local dev.
4. **Interview defensibility.** Can explain: "Single-schema for now; migrating to `claim`/`ai`/`audit` schemas is a one-line change per `IEntityTypeConfiguration`."

Custom schema (`ins`, `ai`, `audit`) is deferred to a future refactor gate.

---

## 2. Table Plan

Legend for "Needed in first DB gate?":
- **Yes-P0** = required for P0 read API (claim detail screen)
- **Yes-P1** = required for next gate (list, workflow)
- **Deferred** = not needed until advanced AI/demo features

Legend for "Seeded?":
- **Yes** = included in `GoldenClaimSeeder` (CLM-1006 graph)
- **Partial** = seeded with minimal rows for FK integrity
- **No** = empty at seed time

---

### 2.1 `Claims`

| Attribute | Detail |
|-----------|--------|
| Purpose | Core claim record — root aggregate |
| PK | `ClaimId` NVARCHAR(20) — e.g., `'CLM-1006'` |
| Key Columns | `ClaimId`, `ClaimNumber`, `Status` (NVARCHAR — Ukrainian enum values), `ClaimType`, `IncidentDate`, `IncidentLocation`, `Description`, `RiskScore` INT, `RiskLevel` NVARCHAR(20), `ConfidenceScore` INT, `EstimatedAmount` DECIMAL(12,2), `BenchmarkAmount` DECIMAL(12,2), `DeductibleAmount` DECIMAL(12,2), `RecommendedPayout` DECIMAL(12,2), `CustomerId` NVARCHAR(20), `VehicleId` NVARCHAR(20), `PolicyId` NVARCHAR(30), `CreatedAt` DATETIME2, `UpdatedAt` DATETIME2 |
| FKs | `CustomerId → Customers.CustomerId`, `VehicleId → Vehicles.VehicleId`, `PolicyId → Policies.PolicyId` |
| Indexes | `IX_Claims_Status`, `IX_Claims_CustomerId`, `IX_Claims_PolicyId` |
| Needed in first DB gate? | **Yes-P0** |
| Seeded? | **Yes** — one row: CLM-1006 |
| Sensitive-data note | No real PII; `CustomerId` links to synthetic customer |

---

### 2.2 `ClaimDocuments`

| Attribute | Detail |
|-----------|--------|
| Purpose | Documents submitted with a claim (metadata + status) |
| PK | `DocumentId` NVARCHAR(30) — e.g., `'DOC-1006-001'` |
| Key Columns | `DocumentId`, `ClaimId`, `DocumentType` NVARCHAR(50), `FileName` NVARCHAR(200), `Status` NVARCHAR(20) (`ok`/`warn`/`missing`), `VerificationNote` NVARCHAR(500), `StorageUri` NVARCHAR(500) NULL, `CreatedAt` DATETIME2 |
| FKs | `ClaimId → Claims.ClaimId` |
| Indexes | `IX_ClaimDocuments_ClaimId` |
| Needed in first DB gate? | **Yes-P0** |
| Seeded? | **Yes** — 7 rows (application, police, photo-front, photo-side, invoice, policy-terms, photo-rear MISSING) |
| Sensitive-data note | `StorageUri` is synthetic placeholder; no real blob storage |

---

### 2.3 `ClaimPhotos`

| Attribute | Detail |
|-----------|--------|
| Purpose | Photo-specific metadata (angle, confidence from AI classifier) |
| PK | `PhotoId` NVARCHAR(30) |
| Key Columns | `PhotoId`, `ClaimId`, `DocumentId` NVARCHAR(30) NULL, `Angle` NVARCHAR(30) (`front`/`side`/`rear`), `AiConfidence` INT NULL, `Status` NVARCHAR(20), `StorageUri` NVARCHAR(500) NULL, `CreatedAt` DATETIME2 |
| FKs | `ClaimId → Claims.ClaimId`, `DocumentId → ClaimDocuments.DocumentId` (nullable) |
| Indexes | `IX_ClaimPhotos_ClaimId` |
| Needed in first DB gate? | **Yes-P0** |
| Seeded? | **Yes** — 3 rows: front(92%), side(87%), rear(missing) |
| Sensitive-data note | No PII; confidence scores are synthetic |

---

### 2.4 `DocumentChecklistItems`

| Attribute | Detail |
|-----------|--------|
| Purpose | Checklist of required document types per claim and their status |
| PK | `ChecklistItemId` NVARCHAR(30) |
| Key Columns | `ChecklistItemId`, `ClaimId`, `DocumentType` NVARCHAR(50), `Label` NVARCHAR(200), `Status` NVARCHAR(20) (`ok`/`warn`/`missing`), `Note` NVARCHAR(500) NULL |
| FKs | `ClaimId → Claims.ClaimId` |
| Indexes | `IX_DocumentChecklistItems_ClaimId` |
| Needed in first DB gate? | **Yes-P0** |
| Seeded? | **Yes** — 7 rows matching `ClaimDocuments` |
| Sensitive-data note | None |

---

### 2.5 `Policies`

| Attribute | Detail |
|-----------|--------|
| Purpose | Insurance policy record |
| PK | `PolicyId` NVARCHAR(30) — e.g., `'POL-2025-AC-4421'` |
| Key Columns | `PolicyId`, `PolicyNumber` NVARCHAR(30), `PolicyType` NVARCHAR(100), `CustomerId` NVARCHAR(20), `StartDate` DATE, `EndDate` DATE, `Status` NVARCHAR(20), `CreatedAt` DATETIME2 |
| FKs | `CustomerId → Customers.CustomerId` |
| Indexes | `IX_Policies_CustomerId` |
| Needed in first DB gate? | **Yes-P0** |
| Seeded? | **Yes** — one row: POL-2025-AC-4421, Auto Comprehensive |
| Sensitive-data note | No real PII |

---

### 2.6 `PolicyCoverages`

| Attribute | Detail |
|-----------|--------|
| Purpose | Coverage lines within a policy |
| PK | `CoverageId` NVARCHAR(30) |
| Key Columns | `CoverageId`, `PolicyId`, `CoverageType` NVARCHAR(100), `CoverageAmount` NVARCHAR(50), `Deductible` DECIMAL(12,2), `Notes` NVARCHAR(500) NULL |
| FKs | `PolicyId → Policies.PolicyId` |
| Indexes | `IX_PolicyCoverages_PolicyId` |
| Needed in first DB gate? | **Yes-P0** |
| Seeded? | **Yes** — 5 rows: collision($50k/$500), liability($100k/$0), glass($1.5k/$100), theft(ринкова/$1k), roadside(24/7/$0) |
| Sensitive-data note | None |

---

### 2.7 `PolicyExclusions`

| Attribute | Detail |
|-----------|--------|
| Purpose | Exclusion clauses within a policy |
| PK | `ExclusionId` NVARCHAR(30) |
| Key Columns | `ExclusionId`, `PolicyId`, `ExclusionCode` NVARCHAR(50), `Description` NVARCHAR(500) |
| FKs | `PolicyId → Policies.PolicyId` |
| Indexes | `IX_PolicyExclusions_PolicyId` |
| Needed in first DB gate? | **Yes-P1** |
| Seeded? | **Partial** — 2–3 standard exclusions |
| Sensitive-data note | None |

---

### 2.8 `Customers`

| Attribute | Detail |
|-----------|--------|
| Purpose | Policyholder / claimant synthetic profile |
| PK | `CustomerId` NVARCHAR(20) — e.g., `'CUST-001'` |
| Key Columns | `CustomerId`, `FullName` NVARCHAR(200), `Email` NVARCHAR(200), `Phone` NVARCHAR(30), `Address` NVARCHAR(500), `CreatedAt` DATETIME2 |
| FKs | None (root entity) |
| Indexes | — |
| Needed in first DB gate? | **Yes-P0** |
| Seeded? | **Yes** — one row: "Роберт Джонсон" (synthetic) |
| Sensitive-data note | **Synthetic only** — no real names, emails, or phone numbers. `Email` must use `@example.com` domain. |

---

### 2.9 `Vehicles`

| Attribute | Detail |
|-----------|--------|
| Purpose | Insured vehicle details |
| PK | `VehicleId` NVARCHAR(20) — e.g., `'VEH-001'` |
| Key Columns | `VehicleId`, `CustomerId`, `Make` NVARCHAR(50), `Model` NVARCHAR(50), `Year` INT, `Vin` NVARCHAR(17), `LicensePlate` NVARCHAR(20) NULL, `Color` NVARCHAR(30) NULL |
| FKs | `CustomerId → Customers.CustomerId` |
| Indexes | `IX_Vehicles_CustomerId` |
| Needed in first DB gate? | **Yes-P0** |
| Seeded? | **Yes** — one row: Toyota Camry 2021, VIN `****8842` (masked in seed) |
| Sensitive-data note | VIN stored as masked value (`****8842`) in seed; no real plates |

---

### 2.10 `AiAnalysisRuns`

| Attribute | Detail |
|-----------|--------|
| Purpose | One AI pipeline execution for a claim |
| PK | `RunId` NVARCHAR(30) — e.g., `'run_8f3d2a7e'` |
| Key Columns | `RunId`, `TraceId` NVARCHAR(30), `ClaimId`, `Status` NVARCHAR(20), `ConfidenceScore` INT, `TotalTokens` INT, `TotalCostUsd` DECIMAL(10,6), `DurationSec` DECIMAL(8,2), `ModelVersion` NVARCHAR(50), `CreatedAt` DATETIME2 |
| FKs | `ClaimId → Claims.ClaimId` |
| Indexes | `IX_AiAnalysisRuns_ClaimId` |
| Needed in first DB gate? | **Yes-P0** |
| Seeded? | **Yes** — one row: runId=run_8f3d2a7e, traceId=trc_8f3d2a7e, tokens=4261, cost=$0.0187, duration=18.9s |
| Sensitive-data note | No PII; cost/token metrics are synthetic |

---

### 2.11 `AiFindings`

| Attribute | Detail |
|-----------|--------|
| Purpose | Individual findings/recommendations from an AI run |
| PK | `FindingId` NVARCHAR(30) |
| Key Columns | `FindingId`, `RunId`, `ClaimId`, `Category` NVARCHAR(50), `Severity` NVARCHAR(20), `Message` NVARCHAR(1000), `Recommendation` NVARCHAR(1000) NULL, `Confidence` INT NULL |
| FKs | `RunId → AiAnalysisRuns.RunId`, `ClaimId → Claims.ClaimId` |
| Indexes | `IX_AiFindings_RunId`, `IX_AiFindings_ClaimId` |
| Needed in first DB gate? | **Yes-P0** |
| Seeded? | **Yes** — primary finding: "Запросити фото заднього бампера" |
| Sensitive-data note | None |

---

### 2.12 `EvidenceSources`

| Attribute | Detail |
|-----------|--------|
| Purpose | Source citations used by AI (document segments, external lookups) |
| PK | `SourceId` NVARCHAR(30) |
| Key Columns | `SourceId`, `RunId`, `SourceType` NVARCHAR(50), `SourceLabel` NVARCHAR(200), `Excerpt` NVARCHAR(1000) NULL, `Relevance` DECIMAL(5,4) NULL |
| FKs | `RunId → AiAnalysisRuns.RunId` |
| Indexes | `IX_EvidenceSources_RunId` |
| Needed in first DB gate? | **Yes-P1** |
| Seeded? | **Partial** — 2–3 rows |
| Sensitive-data note | None |

---

### 2.13 `ExtractedEntities`

| Attribute | Detail |
|-----------|--------|
| Purpose | Structured fields extracted by AI from submitted documents |
| PK | `EntityId` NVARCHAR(30) |
| Key Columns | `EntityId`, `RunId`, `ClaimId`, `FieldName` NVARCHAR(100), `FieldValue` NVARCHAR(500), `SourceDocument` NVARCHAR(200), `Confidence` DECIMAL(5,4) |
| FKs | `RunId → AiAnalysisRuns.RunId`, `ClaimId → Claims.ClaimId` |
| Indexes | `IX_ExtractedEntities_RunId`, `IX_ExtractedEntities_ClaimId` |
| Needed in first DB gate? | **Yes-P1** |
| Seeded? | **Yes** — 6 rows (from CLM-1006 extracted entities) |
| Sensitive-data note | Values are synthetic field extractions only |

---

### 2.14 `RiskAssessments`

| Attribute | Detail |
|-----------|--------|
| Purpose | Composite risk assessment result for a claim/run |
| PK | `AssessmentId` NVARCHAR(30) |
| Key Columns | `AssessmentId`, `RunId`, `ClaimId`, `OverallScore` INT, `RiskLevel` NVARCHAR(20), `Summary` NVARCHAR(1000) NULL |
| FKs | `RunId → AiAnalysisRuns.RunId`, `ClaimId → Claims.ClaimId` |
| Indexes | `IX_RiskAssessments_ClaimId` |
| Needed in first DB gate? | **Yes-P0** |
| Seeded? | **Yes** — one row: score=82, level=Високий |
| Sensitive-data note | None |

---

### 2.15 `RiskFactors`

| Attribute | Detail |
|-----------|--------|
| Purpose | Individual contributing risk factors within an assessment |
| PK | `FactorId` NVARCHAR(30) |
| Key Columns | `FactorId`, `AssessmentId`, `FactorCode` NVARCHAR(50), `Label` NVARCHAR(200), `Contribution` INT, `Description` NVARCHAR(500) NULL |
| FKs | `AssessmentId → RiskAssessments.AssessmentId` |
| Indexes | `IX_RiskFactors_AssessmentId` |
| Needed in first DB gate? | **Yes-P0** |
| Seeded? | **Yes** — 5 rows: amount(25), mismatch(18), missing-photo(22), prior(8), confidence(9) → sum=82 |
| Sensitive-data note | None |

---

### 2.16 `PolicyCheckResults`

| Attribute | Detail |
|-----------|--------|
| Purpose | Per-policy-clause check results from AI during a run |
| PK | `CheckId` NVARCHAR(30) |
| Key Columns | `CheckId`, `RunId`, `ClaimId`, `PolicyId`, `ClauseCode` NVARCHAR(50), `Result` NVARCHAR(20), `Rationale` NVARCHAR(500) NULL |
| FKs | `RunId → AiAnalysisRuns.RunId`, `ClaimId → Claims.ClaimId`, `PolicyId → Policies.PolicyId` |
| Indexes | `IX_PolicyCheckResults_ClaimId` |
| Needed in first DB gate? | **Yes-P1** |
| Seeded? | **Partial** — 2 rows |
| Sensitive-data note | None |

---

### 2.17 `ApprovalDrafts`

| Attribute | Detail |
|-----------|--------|
| Purpose | AI-generated approval recommendation draft for human review |
| PK | `DraftId` NVARCHAR(30) |
| Key Columns | `DraftId`, `ClaimId`, `RunId`, `RecommendedAction` NVARCHAR(30) (`request`/`approve`/`reject`/`escalate`), `RecommendedPayout` DECIMAL(12,2) NULL, `Justification` NVARCHAR(2000), `ConfidenceScore` INT, `CreatedAt` DATETIME2 |
| FKs | `ClaimId → Claims.ClaimId`, `RunId → AiAnalysisRuns.RunId` |
| Indexes | `IX_ApprovalDrafts_ClaimId` |
| Needed in first DB gate? | **Yes-P0** |
| Seeded? | **Yes** — one row: action=request, payout=$1800, justification explains missing photo |
| Sensitive-data note | None |

---

### 2.18 `HumanDecisionOptions`

| Attribute | Detail |
|-----------|--------|
| Purpose | Static list of decision actions available to a human adjuster |
| PK | `OptionId` NVARCHAR(30) |
| Key Columns | `OptionId`, `ClaimId` NULL (nullable — can be global), `ActionCode` NVARCHAR(30), `Label` NVARCHAR(100), `Description` NVARCHAR(300) NULL, `IsRecommended` BIT, `DisplayOrder` INT |
| FKs | `ClaimId → Claims.ClaimId` (nullable) |
| Indexes | — |
| Needed in first DB gate? | **Yes-P0** |
| Seeded? | **Yes** — 4 rows: approve, request(IsRecommended=1), reject, escalate |
| Sensitive-data note | None |

---

### 2.19 `AuditTraces`

| Attribute | Detail |
|-----------|--------|
| Purpose | Audit trace header linking a claim to a sequence of audit events |
| PK | `TraceId` NVARCHAR(30) — e.g., `'trc_8f3d2a7e'` |
| Key Columns | `TraceId`, `ClaimId`, `RunId` NULL, `InitiatedAt` DATETIME2, `CompletedAt` DATETIME2 NULL, `OverallResult` NVARCHAR(20) (`OK`/`WARN`/`BLOCK`) |
| FKs | `ClaimId → Claims.ClaimId` |
| Indexes | `IX_AuditTraces_ClaimId` |
| Needed in first DB gate? | **Yes-P0** |
| Seeded? | **Yes** — one row: traceId=trc_8f3d2a7e, result=BLOCK |
| Sensitive-data note | None |

---

### 2.20 `AuditEvents`

| Attribute | Detail |
|-----------|--------|
| Purpose | Individual audit event within an audit trace |
| PK | `EventId` NVARCHAR(30) |
| Key Columns | `EventId`, `TraceId`, `EventTime` DATETIME2, `Actor` NVARCHAR(100), `Action` NVARCHAR(200), `Result` NVARCHAR(20) (`OK`/`WARN`/`BLOCK`), `Details` NVARCHAR(1000) NULL, `SequenceOrder` INT |
| FKs | `TraceId → AuditTraces.TraceId` |
| Indexes | `IX_AuditEvents_TraceId` |
| Needed in first DB gate? | **Yes-P0** |
| Seeded? | **Yes** — 6 rows: AI Pipeline(OK), Doc Classifier(OK), Field Extractor(OK, 47 fields), Risk Engine(WARN, 82/100), Recommender(OK, "запросити фото"), Governance(BLOCK, "Авто-погодження заблоковано") |
| Sensitive-data note | None |

---

### 2.21 `CostTelemetryEvents`

| Attribute | Detail |
|-----------|--------|
| Purpose | Per-stage cost breakdown for an AI run |
| PK | `TelemetryId` NVARCHAR(30) |
| Key Columns | `TelemetryId`, `RunId`, `Stage` NVARCHAR(50), `CostUsd` DECIMAL(10,6), `TokenCount` INT NULL, `SequenceOrder` INT |
| FKs | `RunId → AiAnalysisRuns.RunId` |
| Indexes | `IX_CostTelemetryEvents_RunId` |
| Needed in first DB gate? | **Yes-P0** |
| Seeded? | **Yes** — 4 rows: extract($0.0072), rag($0.0058), risk($0.0029), reco($0.0028) → total≈$0.0187 |
| Sensitive-data note | None |

---

### 2.22 `DemoScenarios`

| Attribute | Detail |
|-----------|--------|
| Purpose | Named demo scenarios for guided walkthrough mode |
| PK | `ScenarioId` NVARCHAR(30) |
| Key Columns | `ScenarioId`, `Name` NVARCHAR(200), `Description` NVARCHAR(500) NULL, `IsDefault` BIT, `CreatedAt` DATETIME2 |
| FKs | None |
| Indexes | — |
| Needed in first DB gate? | **Deferred** |
| Seeded? | **Yes** — one row: "Повний розбір CLM-1006" |
| Sensitive-data note | None |

---

### 2.23 `DemoSteps`

| Attribute | Detail |
|-----------|--------|
| Purpose | Steps within a demo scenario, each mapping to a frontend route |
| PK | `StepId` NVARCHAR(30) |
| Key Columns | `StepId`, `ScenarioId`, `StepOrder` INT, `Route` NVARCHAR(200), `Title` NVARCHAR(200), `Description` NVARCHAR(500) NULL, `HighlightSelector` NVARCHAR(200) NULL |
| FKs | `ScenarioId → DemoScenarios.ScenarioId` |
| Indexes | `IX_DemoSteps_ScenarioId` |
| Needed in first DB gate? | **Deferred** |
| Seeded? | **Yes** — 7 rows mapping to CLM-1006 frontend routes |
| Sensitive-data note | None |

---

## 3. Table Dependency / Creation Order

For future migrations, tables must be created in this order (FK dependency):

```
1. Customers
2. Vehicles          (→ Customers)
3. Policies          (→ Customers)
4. PolicyCoverages   (→ Policies)
5. PolicyExclusions  (→ Policies)
6. Claims            (→ Customers, Vehicles, Policies)
7. ClaimDocuments    (→ Claims)
8. ClaimPhotos       (→ Claims, ClaimDocuments)
9. DocumentChecklistItems (→ Claims)
10. AiAnalysisRuns   (→ Claims)
11. AiFindings       (→ AiAnalysisRuns, Claims)
12. EvidenceSources  (→ AiAnalysisRuns)
13. ExtractedEntities(→ AiAnalysisRuns, Claims)
14. RiskAssessments  (→ AiAnalysisRuns, Claims)
15. RiskFactors      (→ RiskAssessments)
16. PolicyCheckResults (→ AiAnalysisRuns, Claims, Policies)
17. ApprovalDrafts   (→ Claims, AiAnalysisRuns)
18. HumanDecisionOptions (→ Claims nullable)
19. AuditTraces      (→ Claims)
20. AuditEvents      (→ AuditTraces)
21. CostTelemetryEvents (→ AiAnalysisRuns)
22. DemoScenarios
23. DemoSteps        (→ DemoScenarios)
```

---

## 4. Design Principles

1. **String PKs for claim-domain entities** (`CLM-1006`, `POL-2025-AC-4421`) — matches frontend display IDs, stable across seed/reset, interview-friendly.
2. **INT PKs acceptable** for lookup/child tables (`RiskFactors`, `AuditEvents`, `DemoSteps`) — auto-increment simplifies seed scripts.
3. **Not over-normalized.** `Claims` carries denormalized `RiskScore`/`ConfidenceScore` to avoid joins on the hot read path (claim detail screen). Normalized source of truth in `RiskAssessments`.
4. **Seed/reset-friendly.** All string PKs are hardcoded in `GoldenClaimSeeder`. No UUID randomness at seed time.
5. **No real PII.** `Customers.Email` = `robert.johnson@example.com`; VIN = `****8842`.

---

## 5. References

- Persistence plan: [`BACKEND_PERSISTENCE_AND_DATABASE_PLAN_V0.1.md`](BACKEND_PERSISTENCE_AND_DATABASE_PLAN_V0.1.md)
- EF Core migration strategy: [`BACKEND_EFCORE_MIGRATION_STRATEGY_V0.1.md`](BACKEND_EFCORE_MIGRATION_STRATEGY_V0.1.md)
- Seed data plan: [`BACKEND_SEED_DATA_PLAN_V0.1.md`](BACKEND_SEED_DATA_PLAN_V0.1.md)
