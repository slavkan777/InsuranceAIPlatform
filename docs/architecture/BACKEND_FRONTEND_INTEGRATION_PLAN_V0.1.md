---
title: Backend-Frontend Integration Plan V0.1
type: knowledge
status: active
created: 2026-05-27
tags: [integration, backend, frontend, api, routing, mock-to-real]
---

# Backend-Frontend Integration Plan V0.1

## 1. Route-to-Backend Endpoint Table

| Frontend Screen | Route | Mock Source Fn | Needed Backend Endpoint | DTO Needed | Priority | Notes |
|---|---|---|---|---|---|---|
| Dashboard | `/` | `getClaimsQueue` (summary slice) | `GET /api/claims?page=1&size=5` | `ClaimRowDto[]` | P0 | Summary stats derived from queue |
| Claims List | `/claims` | `getClaimsQueue` | `GET /api/claims` | `ClaimRowDto[]` | P0 | Pagination, filter by status |
| Workspace | `/claims/CLM-1006` | `getClaimById` | `GET /api/claims/{id}` | `ClaimDetailDto` | P0 | Golden claim CLM-1006 must seed |
| Documents & Photos | `/documents` | `getClaimDocuments`, `getClaimPhotos` | `GET /api/claims/{id}/documents`, `GET /api/claims/{id}/photos` | `DocumentChecklistItemDto[]`, `DamagePhotoDto[]` | P0 | Read-only; upload deferred P2 |
| AI Evidence | `/ai-evidence` | `getAiAnalysis`, `runMockAiAnalysis` | `GET /api/claims/{id}/ai-analysis`, `POST /api/claims/{id}/ai-analysis/run` | `AiAnalysisDto`, `AiRunAckDto` | P0 (GET), P1 (POST) | POST run stays deferred until P1 |
| Risks | `/risks` | `getRiskReview` | `GET /api/claims/{id}/risk-review` | `RiskReviewDto` | P0 | threshold:60 hardcoded; CLM-1006→82 |
| Policy | `/policy` | `getPolicyCoverage` | `GET /api/claims/{id}/policy-coverage` | `PolicyCoverageDto` | P0 | Coverage blocks + validation flags |
| Customer & Vehicle | `/customer-vehicle` | `getCustomerVehicleContext` | `GET /api/claims/{id}/customer-context` | `CustomerVehicleContextDto` | P0 | previousClaims + communicationHistory |
| Human Approval | `/approval` | `saveApprovalDraft` | `POST /api/claims/{id}/approval-draft`, `POST /api/claims/{id}/approval-submit` | `ApprovalDraftCmd`, `ApprovalSubmitCmd`, `AckDto` | P1 | Draft save P1; final submit P1 |
| Audit & Cost | `/audit` | `getAuditTrace` | `GET /api/claims/{id}/audit-trace` | `AuditTraceDto` | P0 | Includes synthetic tokens/cost/latency |
| Demo | `/demo` | `getDemoScenario` | `GET /api/system/demo-status`, `GET /api/demo/scenario` | `DemoStatusDto`, `DemoScenarioDto` | P0 | Auto-advance saga stays deferred P1 |

---

## 2. Local Dev Integration Setup

### Port Assignment

| Service | URL | Justification |
|---|---|---|
| Frontend (Vite dev) | `http://localhost:5173` | Vite default; do not change |
| Frontend (Vite preview) | `http://localhost:4173` | Vite preview default |
| Backend (ASP.NET Core) | `http://localhost:5174` | Adjacent port; avoids OS conflicts with :5000/:5001/:8080; memorable with frontend :5173 |
| SQL Server (future) | `localhost,19772` | Existing instance; not touched until persistence Phase SQL |
| Swagger UI | `http://localhost:5174/swagger` | Standard ASP.NET Core Swashbuckle path |
| Health endpoint | `http://localhost:5174/health` | MapHealthChecks standard |

Port :5174 chosen over :5080 because it is numerically adjacent to :5173, making the dev pair self-documenting.

### CORS Configuration

Backend must allow Vite origins explicitly:

```
AllowedOrigins:
  - http://localhost:5173   # Vite dev server
  - http://localhost:4173   # Vite preview
```

Policy name: `DevCorsPolicyInsurance`. Applied in `Program.cs` via `app.UseCors(...)` before controllers.
Do NOT use `AllowAnyOrigin` in development — be explicit.

### Vite Proxy vs Direct Base URL

**Recommendation: Direct base URL (no Vite proxy).**

Rationale:
- The mock switch is in `src/api/mockInsuranceApi.ts` / `ApiClient`, not in Vite config.
- Adding a Vite proxy couples the switch to build tooling.
- Direct URL with an env var (`VITE_API_BASE_URL`) is portable to preview and CI.
- CORS is already configured on the backend — a proxy layer adds indirection without benefit.

Frontend env var strategy:
```
# .env.development (not committed if it contains secrets — here it does not)
VITE_API_BASE_URL=http://localhost:5174
VITE_USE_MOCK_API=true
```

When `VITE_USE_MOCK_API=true`, the frontend reads from `mockInsuranceApi.ts`. When `false`, it calls `VITE_API_BASE_URL`.

### Offline / Fallback Behavior

If `VITE_USE_MOCK_API=false` and the backend is unreachable, the real `ApiClient` should catch network errors and either:
1. Re-throw with a typed `NetworkError` so the saga can display a banner, OR
2. Fall back to mock (only in dev mode, controlled by a secondary flag `VITE_MOCK_FALLBACK_ON_ERROR=true`).

Default: throw (no silent fallback). Developers toggle fallback locally only.

### Demo Honesty Labels

Any endpoint that returns synthetic AI-generated data must include a field in the response:
```json
{ "demoMode": true, "providerLabel": "Demo/Synthetic (no live AI provider)" }
```
The mock label `"Azure OpenAI"` visible in the current `getAuditTrace` response must be annotated as demo data. The backend must never expose a real provider name it is not actually calling.

---

## 3. Mock-to-Real Replacement Plan

**Core principle:** Never modify `src/api/mockInsuranceApi.ts`. Never change UI routes. Swap at the call-site only.

### Step 1 — Keep Mock File Intact
`src/api/mockInsuranceApi.ts` remains the source of truth for UI development. It is the contract specification. Its 13 function signatures define the interface.

### Step 2 — Add Real API Client with Same Signatures
Create `src/api/realInsuranceApiClient.ts` implementing an identical async interface. Each function calls the corresponding `GET /api/...` endpoint. No saga changes needed.

```typescript
// Same signature as mockInsuranceApi.ts
export async function getClaimById(id: string): Promise<ClaimDetail> {
  const res = await fetch(`${apiBase}/api/claims/${id}`);
  if (!res.ok) throw new ApiError(res.status, await res.json());
  return res.json();
}
```

### Step 3 — Central Config Switch
Create `src/api/insuranceApiIndex.ts` (or `src/api/index.ts`):

```typescript
const useMock = import.meta.env.VITE_USE_MOCK_API === 'true';
export * from useMock ? './mockInsuranceApi' : './realInsuranceApiClient';
```

All sagas import from `src/api/index.ts` — no saga changes required.

### Step 4 — First Integration Pass: P0 Read Endpoints Only
Switch `VITE_USE_MOCK_API=false` only after all P0 GET endpoints return correct data for CLM-1006. Verify each screen individually before committing the switch.

Order of P0 integration:
1. `/health` (smoke)
2. `GET /api/claims/{id}` → Workspace screen
3. `GET /api/claims/{id}/ai-analysis` → AI Evidence screen
4. `GET /api/claims/{id}/risk-review` → Risks screen
5. `GET /api/claims/{id}/audit-trace` → Audit screen
6. Remaining P0 GETs

### Step 5 — Static Buttons Remain Deferred (P1)
POST endpoints (approval, customer-request, ai-run) continue to be handled by saga delays even after the mock switch. The `realInsuranceApiClient.ts` can stub these as promise delays returning mock acks until P1 backend is ready.

---

## 4. Demo Data Ownership Table

| Phase | Source of Truth | Frontend Mock Role | Backend Seed Role |
|---|---|---|---|
| **Pre-backend (current)** | `mockInsuranceApi.ts` hardcoded values | Sole data provider; golden CLM-1006 values live here | Not applicable |
| **P0 backend, mock still on** | `mockInsuranceApi.ts` | Still active; backend being built in parallel | Seed script creates CLM-1006 with matching values for future swap |
| **P0 read swap** | Backend seed data | Fallback only (VITE_MOCK_FALLBACK_ON_ERROR) | Primary source; CLM-1006 values must match mock exactly (82/100, 78%, trc_8f3d2a7e, $0.0187, etc.) |
| **P1+ (write endpoints)** | Backend database (in-memory → SQL Server) | Retired from active path; kept for offline demo | Full CRUD authority; mock values become reference only |
| **Demo mode** | Backend seed (restored on each demo reset) | Available as fallback if backend unreachable | Exposes `GET /api/system/demo-status` returning seed health |

**Divergence prevention:** When backend seed is created, assert exact match against mock constants. Any seed value that differs from the mock must be treated as a bug, not a feature, until the swap is complete.
