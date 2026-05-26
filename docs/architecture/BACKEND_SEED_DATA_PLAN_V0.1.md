---
title: Backend Seed Data Plan
type: knowledge
status: active
created: 2026-05-27
tags: [backend, seed, clm-1006, golden-claim, mock-data, demo]
---

# Backend Seed Data Plan V0.1

> **Scope:** Planning only. No seeder code is written, no DB is created, no data is inserted.
> Seeder implementation is FORBIDDEN until the explicit `BACKEND_SQLSERVER_PERSISTENCE_GATE`.

---

## 1. Seed Source of Truth — Golden Claim CLM-1006

All seed data is deterministic, synthetic, and derived from the authoritative frontend mock
(`src/api/mockInsuranceApi.ts` / `src/data/mock/`). No real PII. Stable IDs never change.

### 1.1 Claim Summary

| Field | Value |
|-------|-------|
| Claim ID | `CLM-1006` |
| Claim Number | `CLM-1006` (display = PK) |
| Status | `На розгляді` (Ukrainian enum: under review) |
| Claim Type | `ДТП` (traffic accident) |
| Incident Date | `2026-05-18` |
| Incident Location | `Бориспіль, Київська область` |
| Risk Score | `82` |
| Risk Level | `Високий` |
| AI Confidence | `78%` |
| Estimated Amount | `$2720.00` |
| Benchmark Amount | `$1970.00` |
| Mismatch | `+38%` above benchmark |
| Deductible | `$500.00` |
| Recommended Payout | `$1800.00` |
| AI Recommendation | Request additional photo (rear bumper) before any decision |
| Run ID | `run_8f3d2a7e` |
| Trace ID | `trc_8f3d2a7e` |

### 1.2 Customer (Synthetic)

| Field | Value |
|-------|-------|
| Customer ID | `CUST-001` |
| Full Name | `Роберт Джонсон` |
| Email | `robert.johnson@example.com` (**synthetic domain only**) |
| Phone | `+380-XX-XXX-XXXX` (placeholder — not real) |
| Address | `вул. Синтетична 1, Київ, 02000` (synthetic) |

### 1.3 Vehicle (Synthetic)

| Field | Value |
|-------|-------|
| Vehicle ID | `VEH-001` |
| Customer ID | `CUST-001` |
| Make | `Toyota` |
| Model | `Camry` |
| Year | `2021` |
| VIN | `****8842` (masked — last 4 digits only) |
| License Plate | `АА-0000-АА` (synthetic) |

### 1.4 Policy

| Field | Value |
|-------|-------|
| Policy ID | `POL-2025-AC-4421` |
| Policy Number | `POL-2025-AC-4421` |
| Policy Type | `Auto Comprehensive` |
| Customer ID | `CUST-001` |
| Start Date | `2025-01-01` |
| End Date | `2026-12-31` |
| Status | `Active` |

### 1.5 Policy Coverages (5 rows)

| Coverage ID | Type | Coverage Amount | Deductible |
|-------------|------|----------------|------------|
| `COV-001` | Collision | `$50,000` | `$500.00` |
| `COV-002` | Liability | `$100,000` | `$0.00` |
| `COV-003` | Glass | `$1,500` | `$100.00` |
| `COV-004` | Theft | `Ринкова вартість` | `$1,000.00` |
| `COV-005` | Roadside Assistance | `24/7` | `$0.00` |

### 1.6 Documents (7 rows)

| Document ID | Type | File Name | Status | Note |
|-------------|------|-----------|--------|------|
| `DOC-1006-001` | `application` | `claim-application.pdf` | `ok` | Заяву прийнято |
| `DOC-1006-002` | `police` | `police-report-NoБРС-2026-05-441.pdf` | `ok` | No БРС-2026/05/441 |
| `DOC-1006-003` | `photo-front` | `photo-front.jpg` | `ok` | AI confidence: 92% |
| `DOC-1006-004` | `photo-side` | `photo-side.jpg` | `ok` | AI confidence: 87% |
| `DOC-1006-005` | `invoice` | `repair-invoice.pdf` | `warn` | Сума +38% вище бенчмарку |
| `DOC-1006-006` | `policy-terms` | `policy-POL-2025-AC-4421.pdf` | `ok` | Діючий поліс |
| `DOC-1006-007` | `photo-rear` | *(not uploaded)* | `missing` | Фото заднього бампера відсутнє |

**Total:** 5 ok + 1 warn + 1 missing = 6/7 complete.

### 1.7 Photos (3 rows)

| Photo ID | Angle | Document ID | AI Confidence | Status |
|----------|-------|-------------|---------------|--------|
| `PHO-1006-001` | `front` | `DOC-1006-003` | `92` | `ok` |
| `PHO-1006-002` | `side` | `DOC-1006-004` | `87` | `ok` |
| `PHO-1006-003` | `rear` | NULL | NULL | `missing` |

### 1.8 AI Analysis Run

| Field | Value |
|-------|-------|
| Run ID | `run_8f3d2a7e` |
| Trace ID | `trc_8f3d2a7e` |
| Claim ID | `CLM-1006` |
| Status | `Completed` |
| Confidence Score | `78` |
| Total Tokens | `4261` |
| Total Cost USD | `0.018700` |
| Duration Sec | `18.90` |
| Model Version | `gpt-4o-2024-11` (synthetic label — no real AI key) |
| Created At | `2026-05-18T10:42:17Z` |

### 1.9 Risk Assessment & Factors

**Assessment:**

| Field | Value |
|-------|-------|
| Assessment ID | `RISK-1006-001` |
| Run ID | `run_8f3d2a7e` |
| Claim ID | `CLM-1006` |
| Overall Score | `82` |
| Risk Level | `Високий` |

**Risk Factors (5 rows, sum = 82):**

| Factor ID | Code | Label | Contribution |
|-----------|------|-------|-------------|
| `RF-001` | `amount` | Сума заявки | `25` |
| `RF-002` | `mismatch` | Розбіжність із бенчмарком | `18` |
| `RF-003` | `missing-photo` | Відсутнє фото | `22` |
| `RF-004` | `prior` | Попередні звернення | `8` |
| `RF-005` | `confidence` | Низька впевненість AI | `9` |

Verification: 25 + 18 + 22 + 8 + 9 = **82** ✓

### 1.10 Approval Draft

| Field | Value |
|-------|-------|
| Draft ID | `DRAFT-1006-001` |
| Claim ID | `CLM-1006` |
| Run ID | `run_8f3d2a7e` |
| Recommended Action | `request` |
| Recommended Payout | `$1800.00` |
| Confidence Score | `78` |
| Justification | `Відсутнє фото заднього бампера унеможливлює повну верифікацію ушкоджень. Рекомендовано запросити фото перед ухваленням рішення. Сума заявки ($2720) перевищує бенчмарк на 38%. При отриманні фото та підтвердженні ушкоджень — виплата $1800 (після вирахування франшизи $500).` |

### 1.11 Human Decision Options (4 rows)

| Option ID | Action Code | Label | Is Recommended | Display Order |
|-----------|-------------|-------|---------------|---------------|
| `OPT-001` | `approve` | Затвердити виплату | `false` | `2` |
| `OPT-002` | `request` | Запросити додаткові документи | `true` | `1` |
| `OPT-003` | `reject` | Відхилити заявку | `false` | `3` |
| `OPT-004` | `escalate` | Передати до відділу розслідування | `false` | `4` |

### 1.12 Audit Trace & Events

**Audit Trace:**

| Field | Value |
|-------|-------|
| Trace ID | `trc_8f3d2a7e` |
| Claim ID | `CLM-1006` |
| Run ID | `run_8f3d2a7e` |
| Initiated At | `2026-05-18T10:42:17Z` |
| Completed At | `2026-05-18T10:42:35Z` |
| Overall Result | `BLOCK` |

**Audit Events (6 rows):**

| Event ID | Order | Time | Actor | Action | Result | Details |
|----------|-------|------|-------|--------|--------|---------|
| `EVT-001` | `1` | `10:42:17Z` | `AI Pipeline` | `Ініціалізація аналізу` | `OK` | `Pipeline запущено` |
| `EVT-002` | `2` | `10:42:19Z` | `Doc Classifier` | `Класифікація документів` | `OK` | `7 документів прийнято` |
| `EVT-003` | `3` | `10:42:22Z` | `Field Extractor` | `Витяг полів` | `OK` | `47 полів витягнуто` |
| `EVT-004` | `4` | `10:42:27Z` | `Risk Engine` | `Оцінка ризику` | `WARN` | `82/100 — рівень Високий` |
| `EVT-005` | `5` | `10:42:31Z` | `Recommender` | `Формування рекомендації` | `OK` | `Запросити фото заднього бампера` |
| `EVT-006` | `6` | `10:42:35Z` | `Governance` | `Перевірка авто-погодження` | `BLOCK` | `Авто-погодження заблоковано: ризик > 75, потрібен ручний розгляд` |

### 1.13 Cost Telemetry (4 rows, total ≈ $0.0187)

| Telemetry ID | Stage | Cost USD | Token Count | Order |
|--------------|-------|----------|-------------|-------|
| `COST-001` | `extract` | `0.007200` | `1800` | `1` |
| `COST-002` | `rag` | `0.005800` | `1450` | `2` |
| `COST-003` | `risk` | `0.002900` | `580` | `3` |
| `COST-004` | `reco` | `0.002800` | `431` | `4` |

Verification: 0.0072 + 0.0058 + 0.0029 + 0.0028 = **0.0187** ✓ | tokens: 1800+1450+580+431 = **4261** ✓

### 1.14 Extracted Entities (6 rows)

| Entity ID | Field Name | Field Value | Source Document | Confidence |
|-----------|------------|-------------|-----------------|------------|
| `ENT-001` | `incident_date` | `2026-05-18` | `DOC-1006-001` | `0.97` |
| `ENT-002` | `incident_location` | `Бориспіль` | `DOC-1006-002` | `0.94` |
| `ENT-003` | `repair_amount` | `2720.00` | `DOC-1006-005` | `0.99` |
| `ENT-004` | `vehicle_vin` | `****8842` | `DOC-1006-001` | `0.96` |
| `ENT-005` | `police_report_number` | `БРС-2026/05/441` | `DOC-1006-002` | `0.98` |
| `ENT-006` | `deductible` | `500.00` | `DOC-1006-006` | `0.99` |

### 1.15 Previous Claims (for Customer CUST-001)

Seeded as minimal stub rows for the "claim history" UI widget (no full graph):

| Claim ID | Amount | Year | Status |
|----------|--------|------|--------|
| `CLM-1006` | `$2720` | `2026` | Current (full graph) |
| `CLM-0789` | `$340` | `2024` | `Виплачено` (stub row only) |
| `CLM-0512` | `$180` | `2023` | `Виплачено` (stub row only) |

### 1.16 Demo Scenario & Steps (7 rows)

**Scenario:** `DEMO-001` — "Повний розбір CLM-1006"

| Step ID | Order | Route | Title |
|---------|-------|-------|-------|
| `STEP-001` | `1` | `/` | Дашборд — огляд черги |
| `STEP-002` | `2` | `/claims/CLM-1006` | Деталі заявки |
| `STEP-003` | `3` | `/claims/CLM-1006/documents` | Перевірка документів |
| `STEP-004` | `4` | `/claims/CLM-1006/ai-analysis` | AI-аналіз та висновки |
| `STEP-005` | `5` | `/claims/CLM-1006/risk` | Оцінка ризику (82/100) |
| `STEP-006` | `6` | `/claims/CLM-1006/decision` | Рекомендація та рішення |
| `STEP-007` | `7` | `/claims/CLM-1006/audit` | Аудит-слід та телеметрія |

---

## 2. Seed Class / File Location

```
src/InsuranceAIPlatform.Api/
  Infrastructure/
    Persistence/
      Seed/
        GoldenClaimSeeder.cs          ← Main seeder — traverses full CLM-1006 graph
        SeedVersion.cs                ← Version tracking entity (seeder class name + hash)
        SeedData/
          ClaimSeedData.cs            ← Static claim + document records
          CustomerVehicleSeedData.cs  ← Customer + vehicle records
          PolicySeedData.cs           ← Policy + coverages records
          AiRunSeedData.cs            ← AI run + findings + extracted entities
          RiskSeedData.cs             ← Risk assessment + 5 risk factors
          AuditSeedData.cs            ← Audit trace + 6 events
          CostSeedData.cs             ← 4 cost telemetry rows
          DemoSeedData.cs             ← Demo scenario + 7 steps
```

**`GoldenClaimSeeder` responsibilities:**
1. Assert DB name == `InsuranceAIPlatform` (safety guard — see persistence plan).
2. Open transaction.
3. For each `SeedData` sub-class: check if root entity exists by stable ID, insert if missing.
4. Respect FK insertion order (see schema outline § 3).
5. Write/update `SeedVersion` row.
6. Commit transaction.
7. Log summary: N entities upserted across M tables.

**Invocation (development only):**
```csharp
// Program.cs (behind IsDevelopment() guard)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider
        .GetRequiredService<GoldenClaimSeeder>()
        .SeedAsync(CancellationToken.None);
}
```

---

## 3. Reset Strategy

**Full deterministic reset procedure (development only):**

```
Step 1: Assert DB name == "InsuranceAIPlatform" (fail fast if wrong)
Step 2: dotnet ef database update 0      ← drops all tables
Step 3: dotnet ef database update         ← re-applies all migrations (schema only)
Step 4: GoldenClaimSeeder.SeedAsync()     ← inserts CLM-1006 graph
Step 5: Log: "Reset complete. DB: InsuranceAIPlatform. Rows seeded: N"
```

**Idempotency guarantee:**
- Running steps 3+4 on an already-seeded DB produces identical state (UPSERT semantics).
- Timestamps in seed data are fixed ISO-8601 — no `DateTime.Now` drift between resets.
- All PKs are hardcoded strings — no auto-increment gaps to worry about.

---

## 4. Frontend Mock → Backend Seed Mapping Table

This table maps the frontend source of truth to the future backend seed class.
When CLM-1006 story changes, BOTH columns must be updated (see § 5 Demo Data Ownership).

| Domain Concept | Frontend Mock Source | Backend Seed Class | Stable ID |
|----------------|---------------------|-------------------|-----------|
| Claim root | `mockInsuranceApi.ts` → `getClaim('CLM-1006')` | `ClaimSeedData.cs` | `CLM-1006` |
| Customer | `mockInsuranceApi.ts` → `getCustomer` | `CustomerVehicleSeedData.cs` | `CUST-001` |
| Vehicle | inline in claim response | `CustomerVehicleSeedData.cs` | `VEH-001` |
| Policy | `mockInsuranceApi.ts` → `getPolicy` | `PolicySeedData.cs` | `POL-2025-AC-4421` |
| Policy Coverages (5) | inline in policy response | `PolicySeedData.cs` | `COV-001..005` |
| Documents (7) | `getClaimDocuments('CLM-1006')` | `ClaimSeedData.cs` | `DOC-1006-001..007` |
| Photos (3) | inline in documents response | `ClaimSeedData.cs` | `PHO-1006-001..003` |
| Document Checklist (7) | inline in documents response | `ClaimSeedData.cs` | derived from DOC- IDs |
| AI Analysis Run | `getAiAnalysis('CLM-1006')` | `AiRunSeedData.cs` | `run_8f3d2a7e` |
| AI Findings | inline in AI analysis response | `AiRunSeedData.cs` | `FIND-001` |
| Extracted Entities (6) | inline in AI analysis response | `AiRunSeedData.cs` | `ENT-001..006` |
| Evidence Sources | inline in AI analysis response | `AiRunSeedData.cs` | `SRC-001..002` |
| Risk Assessment | `getRiskAssessment('CLM-1006')` | `RiskSeedData.cs` | `RISK-1006-001` |
| Risk Factors (5) | inline in risk response | `RiskSeedData.cs` | `RF-001..005` |
| Policy Check Results | inline in risk response | `RiskSeedData.cs` | `CHK-001..002` |
| Approval Draft | `getApprovalDraft('CLM-1006')` | `AiRunSeedData.cs` | `DRAFT-1006-001` |
| Decision Options (4) | inline in decision response | `ClaimSeedData.cs` | `OPT-001..004` |
| Audit Trace | `getAuditTrace('trc_8f3d2a7e')` | `AuditSeedData.cs` | `trc_8f3d2a7e` |
| Audit Events (6) | inline in audit response | `AuditSeedData.cs` | `EVT-001..006` |
| Cost Telemetry (4) | inline in AI analysis response | `CostSeedData.cs` | `COST-001..004` |
| Demo Scenario | `getDemoScenario` | `DemoSeedData.cs` | `DEMO-001` |
| Demo Steps (7) | inline in demo scenario | `DemoSeedData.cs` | `STEP-001..007` |
| Previous Claims (2 stubs) | inline in customer response | `ClaimSeedData.cs` | `CLM-0789`, `CLM-0512` |

---

## 5. Demo Data Ownership Rule (Phase 18.6)

| Phase | Source of Truth | Frontend Mock Role | Backend Seed Role |
|-------|-----------------|--------------------|-------------------|
| **P0 — In-Memory Seed (current)** | Frontend mock (`src/api/mockInsuranceApi.ts`) | PRIMARY — single source of truth | Not yet implemented |
| **P1 — Backend Read API (Phase 1 gate)** | Backend `InMemoryClaimRepository` (in-memory seed data in C# dictionaries) | Secondary — kept in sync, used as `demo-mode` fallback if API unreachable | PRIMARY — C# seed data classes are source of truth for CLM-1006 story |
| **P2 — SQL Server (BACKEND_SQLSERVER_PERSISTENCE_GATE)** | `GoldenClaimSeeder.cs` + `SeedData/*.cs` | Fallback / demo-mode only — can be removed or frozen | PRIMARY — seeder is authoritative; any CLM-1006 story change edits seeder first |
| **P3 — Production / Real Data** | Real claim records in `InsuranceAIPlatform` DB | Deprecated — CLM-1006 is demo-only, not in production path | Demo fixture only — isolated from real claims |

**Ownership Rules:**

1. **Two-source divergence is forbidden.** At any phase, CLM-1006 data must not silently differ between frontend mock and backend seed. If they diverge, it is a bug, not a design.
2. **Change propagation is deliberate.** Any update to the CLM-1006 story (new document, changed score, new audit event) requires: (a) update the authoritative source for the current phase, (b) update the secondary source to match, (c) note the change in this document.
3. **Frontend mock becomes a fallback, not primary, after backend read API ships.** At P1, the frontend switches from `mockInsuranceApi.ts` to real API calls. Mock remains as `demo-mode` fallback (offline/CI usage) but is no longer edited for story changes.
4. **No speculative data in frontend mock.** Frontend mock must reflect only the canonical CLM-1006 story. Adding "future fields" to the mock that the backend doesn't know about creates silent divergence.
5. **Audit trail for story changes.** Any CLM-1006 story change after P1 is recorded as a decision entry in `docs/architecture/` (e.g., a brief ADR note) to explain why the data changed.

---

## 6. No Real PII Checklist

Before any seeder code is written, verify all seed data against this checklist:

| Check | Value | Status |
|-------|-------|--------|
| Customer name is fictional | "Роберт Джонсон" — generic synthetic name | OK |
| Email uses synthetic domain | `robert.johnson@example.com` | OK |
| Phone is placeholder | `+380-XX-XXX-XXXX` | OK |
| Address is synthetic | `вул. Синтетична 1` | OK |
| VIN is masked | `****8842` | OK |
| License plate is synthetic | `АА-0000-АА` | OK |
| Police report uses real format but fake number | `NoБРС-2026/05/441` — synthetic number | OK |
| No real location identifiable | `Бориспіль` — real city but no real address, GPS, or coordinates | OK |
| AI model version is synthetic label | `gpt-4o-2024-11` — label only, no real API key | OK |
| No real cost data | All costs are synthetic benchmarks | OK |

---

## 7. References

- Persistence plan: [`BACKEND_PERSISTENCE_AND_DATABASE_PLAN_V0.1.md`](BACKEND_PERSISTENCE_AND_DATABASE_PLAN_V0.1.md)
- Schema outline: [`BACKEND_SCHEMA_OUTLINE_V0.1.md`](BACKEND_SCHEMA_OUTLINE_V0.1.md)
- EF Core migration strategy: [`BACKEND_EFCORE_MIGRATION_STRATEGY_V0.1.md`](BACKEND_EFCORE_MIGRATION_STRATEGY_V0.1.md)
- Frontend mock source: `src/api/mockInsuranceApi.ts`
