---
title: "Backend API Contracts V0.1 — InsuranceAIPlatform"
type: knowledge
status: active
created: 2026-05-27
tags: [backend, api, contracts, insurance, dotnet, portfolio]
---

# Backend API Contracts V0.1

**Project:** InsuranceAIPlatform — Auto Insurance Claim AI Workbench (portfolio mockup)
**Stack:** .NET 9 · ASP.NET Core Controllers · In-memory seed (Path 1) · Swagger/OpenAPI
**Branch target:** `dev` (main = accepted frontend commit 69e6731)
**Seam replaced:** `src/api/mockInsuranceApi.ts` — 13 mock functions → real HTTP endpoints
**Invariants:** AI advisory only (never final approver); human approval is final; synthetic data only; no real PII; no Azure/AI-provider wired now; no stack traces in responses.

---

## 1. Endpoint Plan

> `/api` prefix on all endpoints. Versioning (`/api/v1/`) deferred — add when breaking changes arise.

| # | Method | Path | Purpose | Request | Response DTO | Consumer Screen | Priority | Error Cases | Audit Needed | Notes |
|---|--------|------|---------|---------|--------------|-----------------|----------|-------------|--------------|-------|
| 1 | GET | `/api/claims/summary` | Dashboard counters | — | `ClaimSummaryDto` | `/` (Dashboard) | **P0** | — | No | Single aggregate object; computed from in-mem seed |
| 2 | GET | `/api/claims` | Claims queue list | `?status=&risk=&page=&pageSize=` | `ClaimListItemDto[]` | `/claims` | **P0** | 400 invalid filter | No | Replaces `getClaimsQueue()` |
| 3 | GET | `/api/claims/{claimId}` | Full claim detail | path: `claimId` | `ClaimDetailsDto` | `/claims/CLM-1006` | **P0** | 400 bad format; 404 not found | No | Replaces `getClaimById()` |
| 4 | GET | `/api/claims/{claimId}/documents` | Document checklist | path: `claimId` | `ClaimDocumentDto[]` | `/documents` | **P0** | 404 not found | No | Replaces `getClaimDocuments()` + `getClaimPhotos()` merged view |
| 5 | GET | `/api/claims/{claimId}/ai-evidence` | AI analysis result | path: `claimId` | `AiEvidenceDto` | `/ai-evidence` | **P0** | 404 not found | No | Replaces `getAiAnalysis()` |
| 6 | GET | `/api/claims/{claimId}/risks` | Risk assessment | path: `claimId` | `RiskAssessmentDto` | `/risks` | **P0** | 404 not found | No | Replaces `getRiskReview()` |
| 7 | GET | `/api/claims/{claimId}/policy` | Policy coverage blocks | path: `claimId` | `PolicyDto` | `/policy` | **P0** | 404 not found | No | Replaces `getPolicyCoverage()` |
| 8 | GET | `/api/claims/{claimId}/customer-vehicle` | Customer + vehicle context | path: `claimId` | `CustomerVehicleContextDto` | `/customer-vehicle` | **P0** | 404 not found | No | Replaces `getCustomerVehicleContext()` |
| 9 | GET | `/api/claims/{claimId}/approval` | Approval draft state + options | path: `claimId` | `ApprovalDraftDto` | `/approval` | **P0** | 404 not found | No | Replaces `saveApprovalDraft()` read side |
| 10 | GET | `/api/claims/{claimId}/audit` | Audit trace for claim | path: `claimId` | `AuditTraceDto` | `/audit` | **P0** | 404 not found | No | Replaces `getAuditTrace()` |
| 11 | GET | `/api/demo/scenario` | Demo walkthrough steps | — | `DemoScenarioDto` | `/demo` | **P0** | — | No | Replaces `getDemoScenario()` |
| 12 | GET | `/api/system/demo-status` | Health + seed status | — | `SystemDemoStatusDto` | App init, `/demo` | **P0** | — | No | Used at startup to confirm backend reachable |
| 13 | GET | `/health` | ASP.NET health check | — | `{ status: "Healthy" }` | Infra / CI | **P0** | — | No | Standard `Microsoft.Extensions.Diagnostics.HealthChecks` |
| 14 | POST | `/api/claims` | Create new claim | `CreateClaimInput` | `ClaimDetailsDto` | — | **P2** | 400 validation | Yes | Not needed for CLM-1006 demo flow |
| 15 | POST | `/api/claims/{claimId}/documents/upload` | Upload document | multipart/form-data | `ClaimDocumentDto` | `/documents` | **P2** | 400; 413 too large | Yes | File size limit 20 MB |
| 16 | POST | `/api/claims/{claimId}/approval/draft` | Save approval draft | `ApprovalDraftInput` | `MockApiAck` | `/approval` | **P1** | 400; 404; 409 already submitted | Yes | Replaces `saveApprovalDraft()` write side |
| 17 | POST | `/api/claims/{claimId}/approval/submit` | Submit final decision | `ApprovalSubmitInput` | `MockApiAck` | `/approval` | **P1** | 400; 404; 409 | Yes | Triggers governance audit event |
| 18 | POST | `/api/claims/{claimId}/customer-message/draft` | Save/send customer message draft | `CustomerMessageDraftInput` | `MockApiAck` | `/approval` | **P1** | 400; 404 | Yes | Replaces `sendCustomerRequest()` |
| 19 | POST | `/api/claims/{claimId}/ai-analysis/run` | Trigger AI analysis run | — | `AiAnalysisRunDto` | `/ai-evidence` | **P1** | 404; 409 already running | Yes | Replaces `runMockAiAnalysis()`; mock returns immediate result |
| 20 | GET | `/api/audit/runs/{runId}` | Audit detail for a specific run | path: `runId` | `AuditTraceDto` | `/audit` | **P2** | 404 | No | Cross-claim run lookup |
| 21 | GET | `/api/costs/summary` | Aggregated AI cost telemetry | `?claimId=` optional | `CostTelemetryDto` | `/audit` sidebar | **P2** | — | No | Portfolio metric display |

---

## 2. Standard Error Shape

All 4xx and 5xx responses return a consistent `ApiErrorDto`:

```json
{
  "code": "CLAIM_NOT_FOUND",
  "message": "Claim with id 'CLM-9999' was not found.",
  "traceId": "trc_8f3d2a7e"
}
```

**Rules:**
- `code` — SCREAMING_SNAKE machine-readable token; never include raw exception type.
- `message` — human-readable English sentence; safe to surface in dev UIs.
- `traceId` — propagated from `HttpContext.TraceIdentifier` (override with `W3C` Activity ID when tracing is enabled).
- No `stackTrace`, no inner exception, no domain entity fields in error responses.
- Every error is logged server-side tagged with `traceId`.

---

## 3. Validation & Error Rules

| Scenario | HTTP Status | Error Code | Message Style | Notes |
|----------|-------------|------------|---------------|-------|
| `claimId` format invalid (not CLM-####) | 400 | `INVALID_CLAIM_ID_FORMAT` | "ClaimId must match pattern CLM-####." | Regex `^CLM-\d{4}$` |
| `claimId` valid format but not found | 404 | `CLAIM_NOT_FOUND` | "Claim 'CLM-XXXX' was not found." | Check seed/DB |
| `runId` not found | 404 | `RUN_NOT_FOUND` | "Run 'run_XXXX' was not found." | |
| Query param `status` value not in enum | 400 | `INVALID_FILTER_VALUE` | "Status 'X' is not a valid ClaimStatus." | List valid values in message |
| Approval already submitted (idempotency) | 409 | `APPROVAL_ALREADY_SUBMITTED` | "Approval for claim 'CLM-XXXX' has already been submitted." | |
| AI analysis already in progress | 409 | `AI_ANALYSIS_IN_PROGRESS` | "An AI analysis run is already in progress for claim 'CLM-XXXX'." | |
| Upload file too large | 413 | `FILE_TOO_LARGE` | "Uploaded file exceeds 20 MB limit." | Configure via `RequestSizeLimitAttribute` |
| Required field missing in POST body | 400 | `VALIDATION_ERROR` | "Field 'decision' is required." | Use DataAnnotations or FluentValidation |
| Unhandled server error | 500 | `INTERNAL_ERROR` | "An unexpected error occurred." | Never leak exception details |

---

## 4. API Response Examples (Concrete — CLM-1006 Synthetic Data)

### 4.1 GET /api/claims/summary

```json
{
  "totalActive": 47,
  "pendingReview": 12,
  "highRisk": 8,
  "avgSlaRemainingHours": 14.3,
  "processedToday": 6,
  "aiAnalysisRunning": 2
}
```

---

### 4.2 GET /api/claims

```json
[
  {
    "id": "CLM-1006",
    "customer": "Роберт Джонсон",
    "vehicle": "Toyota Camry 2021",
    "eventType": "ДТП",
    "status": "Високий ризик",
    "documentsCount": "6/7",
    "aiStatus": "Завершено",
    "risk": "Високий",
    "sla": "2год 14хв",
    "nextAction": "Розглянути ризик",
    "updated": "2026-05-18T10:22:00Z"
  },
  {
    "id": "CLM-1007",
    "customer": "Марія Петренко",
    "vehicle": "Honda Civic 2020",
    "eventType": "Крадіжка",
    "status": "В роботі",
    "documentsCount": "4/5",
    "aiStatus": "В обробці",
    "risk": "Середній",
    "sla": "8год 00хв",
    "nextAction": "Очікування документів",
    "updated": "2026-05-26T08:15:00Z"
  }
]
```

---

### 4.3 GET /api/claims/CLM-1006

```json
{
  "id": "CLM-1006",
  "customer": "Роберт Джонсон",
  "customerId": "CUST-0081",
  "vehicle": "Toyota Camry 2021",
  "vehicleVin": "****8842",
  "policy": "Auto Comprehensive",
  "policyId": "POL-2025-AC-4421",
  "eventType": "ДТП",
  "eventDate": "2026-05-18",
  "location": "Бориспіль",
  "description": "Зіткнення на перехресті, пошкоджений задній бампер та лівий борт.",
  "status": "Високий ризик",
  "risk": "Високий",
  "riskScore": 82,
  "confidence": 78,
  "slaDeadline": "2026-05-27T12:00:00Z",
  "documentsReceived": 6,
  "documentsTotal": 7,
  "missingDocument": "Фото заднього бампера",
  "estimate": 2720.00,
  "expectedBenchmark": 1970.00,
  "deductible": 500.00,
  "recommendedPayout": 1800.00,
  "traceId": "trc_8f3d2a7e",
  "runId": "run_8f3d2a7e",
  "tokens": 4261,
  "cost": 0.0187,
  "durationSec": 18.9
}
```

---

### 4.4 GET /api/claims/CLM-1006/documents

```json
[
  {
    "id": "doc-1",
    "label": "Заява про страховий випадок",
    "detail": null,
    "status": "ok",
    "type": "document"
  },
  {
    "id": "doc-2",
    "label": "Протокол поліції",
    "detail": "№ БРС-2026/05/441",
    "status": "ok",
    "type": "document"
  },
  {
    "id": "doc-3",
    "label": "Фото передньої частини",
    "detail": "AI впевненість 92%",
    "status": "ok",
    "type": "photo",
    "confidence": 92
  },
  {
    "id": "doc-4",
    "label": "Фото бокової частини",
    "detail": "AI впевненість 87%",
    "status": "ok",
    "type": "photo",
    "confidence": 87
  },
  {
    "id": "doc-5",
    "label": "Рахунок на ремонт",
    "detail": "+38% від бенчмарку",
    "status": "warn",
    "type": "document"
  },
  {
    "id": "doc-6",
    "label": "Умови полісу",
    "detail": null,
    "status": "ok",
    "type": "document"
  },
  {
    "id": "doc-7",
    "label": "Фото заднього бампера",
    "detail": "ВІДСУТНЄ",
    "status": "missing",
    "type": "photo",
    "confidence": null
  }
]
```

---

### 4.5 GET /api/claims/CLM-1006/ai-evidence

```json
{
  "runId": "run_8f3d2a7e",
  "modelConfidence": 78,
  "findings": [
    {
      "id": "f1",
      "category": "Документи",
      "text": "Відсутнє фото заднього бампера. 6 з 7 документів надано.",
      "severity": "warn"
    },
    {
      "id": "f2",
      "category": "Оцінка збитку",
      "text": "Оцінка $2720 перевищує бенчмарк $1970 на 38%.",
      "severity": "warn"
    },
    {
      "id": "f3",
      "category": "Покриття",
      "text": "Подія ДТП підпадає під Auto Comprehensive. Франшиза $500 застосовна.",
      "severity": "ok"
    }
  ],
  "evidence": [
    {
      "id": "e1",
      "source": "Протокол поліції",
      "text": "Підтверджено факт ДТП 18.05.2026, Бориспіль.",
      "confidence": 95
    },
    {
      "id": "e2",
      "source": "Рахунок на ремонт",
      "text": "Загальна сума $2720. Деталізація: бампер $980, лак $740, кузов $1000.",
      "confidence": 87
    }
  ],
  "extractedEntities": [
    { "field": "Власник ТЗ", "value": "Роберт Джонсон", "source": "Заява", "confidence": 99 },
    { "field": "VIN", "value": "****8842", "source": "Поліс", "confidence": 97 },
    { "field": "Дата події", "value": "18.05.2026", "source": "Протокол", "confidence": 98 },
    { "field": "Місце події", "value": "Бориспіль", "source": "Протокол", "confidence": 96 },
    { "field": "Сума збитку", "value": "$2720", "source": "Рахунок", "confidence": 91 },
    { "field": "№ справи поліції", "value": "БРС-2026/05/441", "source": "Протокол", "confidence": 99 }
  ],
  "modelConfidenceBreakdown": [
    { "stage": "Вилучення даних", "confidence": 95 },
    { "stage": "Перевірка покриття", "confidence": 92 },
    { "stage": "Оцінка пошкоджень", "confidence": 71 },
    { "stage": "Рекомендація", "confidence": 78 }
  ]
}
```

---

### 4.6 GET /api/claims/CLM-1006/risks

```json
{
  "score": 82,
  "threshold": 60,
  "level": "Високий",
  "factors": [
    { "id": "r1", "label": "Сума збитку", "contribution": 25 },
    { "id": "r2", "label": "Розбіжність з бенчмарком", "contribution": 18 },
    { "id": "r3", "label": "Відсутність фото", "contribution": 22 },
    { "id": "r4", "label": "Попередні звернення", "contribution": 8 },
    { "id": "r5", "label": "Довіра до моделі", "contribution": 9 }
  ],
  "pipeline": [
    { "stage": "Класифікатор документів", "status": "OK" },
    { "stage": "Вилучення полів", "status": "OK" },
    { "stage": "Рушій ризиків", "status": "WARN" },
    { "stage": "Рекомендатор", "status": "OK" },
    { "stage": "Управління", "status": "BLOCK" }
  ]
}
```

---

### 4.7 GET /api/claims/CLM-1006/audit

```json
{
  "runId": "run_8f3d2a7e",
  "traceId": "trc_8f3d2a7e",
  "model": "Azure OpenAI (mock)",
  "tokens": 4261,
  "cost": 0.0187,
  "durationSec": 18.9,
  "events": [
    { "time": "10:21:00", "actor": "AI Pipeline", "action": "Запуск аналізу", "result": "OK" },
    { "time": "10:21:03", "actor": "Doc Classifier", "action": "Класифікація документів", "result": "OK" },
    { "time": "10:21:07", "actor": "Field Extractor", "action": "Вилучено 47 полів", "result": "OK" },
    { "time": "10:21:12", "actor": "Risk Engine", "action": "Оцінка ризику 82/100", "result": "WARN" },
    { "time": "10:21:16", "actor": "Recommender", "action": "Рекомендована виплата $1800", "result": "OK" },
    { "time": "10:21:19", "actor": "Governance", "action": "Авто-погодження заблоковано", "result": "BLOCK" }
  ],
  "costDistribution": [
    { "stage": "Вилучення", "cost": 0.0072 },
    { "stage": "RAG", "cost": 0.0058 },
    { "stage": "Ризик", "cost": 0.0029 },
    { "stage": "Рекомендація", "cost": 0.0028 }
  ]
}
```

---

### 4.8 GET /api/system/demo-status

```json
{
  "status": "ready",
  "seedLoaded": true,
  "claimsSeeded": 5,
  "goldenClaimId": "CLM-1006",
  "apiVersion": "0.1.0",
  "environment": "Development",
  "timestamp": "2026-05-27T09:00:00Z"
}
```

---

### 4.9 Standard ApiErrorDto Example

```json
{
  "code": "CLAIM_NOT_FOUND",
  "message": "Claim 'CLM-9999' was not found.",
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01"
}
```

---

## 5. Swagger / OpenAPI Portfolio Configuration

**Required `AddSwaggerGen` settings:**

```
Title:       InsuranceAIPlatform API
Version:     v0.1
Description: Synthetic demo data only. All AI outputs are advisory — no automated approvals,
             no fraud conclusions, no real PII. Human approval is always final.
             Portfolio project — not a production insurance system.
```

**Tag groups** (map to controller `[ApiExplorerSettings(GroupName=...)]` or tag attribute):

| Tag | Controllers / Routes covered |
|-----|------------------------------|
| `Claims` | `/api/claims`, `/api/claims/{claimId}` |
| `Documents` | `/api/claims/{claimId}/documents` |
| `AI Evidence` | `/api/claims/{claimId}/ai-evidence`, `/api/claims/{claimId}/ai-analysis/run` |
| `Risks` | `/api/claims/{claimId}/risks` |
| `Policy` | `/api/claims/{claimId}/policy` |
| `Customer & Vehicle` | `/api/claims/{claimId}/customer-vehicle` |
| `Approval` | `/api/claims/{claimId}/approval/draft`, `/approval/submit` |
| `Audit` | `/api/claims/{claimId}/audit`, `/api/audit/runs/{runId}`, `/api/costs/summary` |
| `Demo & System` | `/api/demo/scenario`, `/api/system/demo-status`, `/health` |

**P0 example responses:** All P0 GET endpoints must declare `[ProducesResponseType(typeof(T), 200)]` and include an `[SwaggerResponseExample]` or XML doc `<example>` block referencing CLM-1006 synthetic data.

**Must NOT state or imply** that real claims are processed, real insurers or customers are represented, or that the AI model autonomously approves or denies claims.

---

## 6. Mock-to-Real Adapter Strategy

The frontend currently calls `mockInsuranceApi.ts` directly. The replacement path:

1. **Introduce `src/api/insuranceApiClient.ts`** — wraps `fetch`/`axios` calls to `/api/*` with the same TypeScript signatures as the mock functions.
2. **Feature-flag switch** — environment variable `VITE_USE_MOCK_API=true|false` (or `VITE_API_BASE_URL`). When `false`, `insuranceApiClient.ts` is imported instead of `mockInsuranceApi.ts`.
3. **Fallback** — if backend returns 5xx or is unreachable during dev, optionally fall back to mock data with a console warning.
4. **No UI/route changes** — all 11 routes (`/`, `/claims`, `/claims/CLM-1006`, `/documents`, `/ai-evidence`, `/risks`, `/policy`, `/customer-vehicle`, `/approval`, `/audit`, `/demo`) remain unchanged. The switch is entirely in the data layer.
5. **Migration order** — replace P0 GETs first (endpoints 1–13 above), then P1 POSTs, then P2.

---

## 7. Cross-Cutting Notes

- **Claim ID stability:** All seeded claim IDs (CLM-1006, etc.) are stable across restarts in in-memory mode. Never generate random IDs for seed data.
- **DB scope:** When SQL Server is introduced, database name is `InsuranceAIPlatform`, schema `dbo`. Never mix with `DevDept` or any other project DB.
- **No secrets in source:** No connection strings, API keys, or provider credentials in `appsettings.json` committed to repo. Use `appsettings.Development.json` (gitignored) or `dotnet user-secrets`.
- **Audit logging (P1+):** POST endpoints that mutate state must emit a structured audit log entry. Use `ILogger<T>` with structured properties `{ClaimId, Action, Actor, Result}` minimum.
- **AI advisory invariant:** No controller action may return a response where an AI result is presented as a final binding decision. Approval and rejection are always initiated by a human via the `/approval/submit` endpoint.
