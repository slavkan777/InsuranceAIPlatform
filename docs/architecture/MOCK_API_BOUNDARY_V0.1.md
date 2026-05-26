# Mock API Boundary — V0.1

## 1. What is mocked now
The AI workflow and all "server" reads/writes. There is **no backend**: no HTTP, no base URL, no `fetch`/`axios`, no API key, no real provider. The AI "run" is a saga `delay()`; approval/customer-request writes resolve a local acknowledgement.

## 2. What is synthetic data
Everything in `src/data/mock/{claims,dashboard,claim-1006}.ts`: the golden claim CLM-1006, the queue (CLM-1006…1013), documents, photos, AI findings/evidence/confidence, risk factors, policy coverage, customer/vehicle context, audit trail + cost, dashboard aggregates, and the demo scenario. PII is masked (`+1 (555) ***-2147`, `robert.j****@demo.com`, VIN `****8842`).

## 3. Current mock API surface (`src/api/mockInsuranceApi.ts`)
Reads: `getClaimsQueue`, `getClaimById`, `getClaimDocuments`, `getClaimPhotos`, `getAiAnalysis`, `getRiskReview`, `getPolicyCoverage`, `getCustomerVehicleContext`, `getAuditTrace`, `getDemoScenario`.
Run/write: `runMockAiAnalysis`, `saveApprovalDraft`, `sendCustomerRequest`.
All are `async` and return synthetic data via `Promise.resolve`/local `delay`. Result/input contracts live in `src/api/insuranceApi.types.ts`.

## 4. Future .NET endpoint mapping
| Mock function | Future .NET endpoint |
|---|---|
| `getClaimsQueue()` | `GET /api/claims/queue` |
| `getClaimById(id)` | `GET /api/claims/{id}` |
| `getClaimDocuments(id)` | `GET /api/claims/{id}/documents` |
| `getClaimPhotos(id)` | `GET /api/claims/{id}/photos` |
| `getAiAnalysis(id)` | `GET /api/claims/{id}/ai-analysis` |
| `runMockAiAnalysis(id)` | `POST /api/claims/{id}/ai-analysis/run` |
| `getRiskReview(id)` | `GET /api/claims/{id}/risk-review` |
| `getPolicyCoverage(id)` | `GET /api/claims/{id}/policy` |
| `getCustomerVehicleContext(id)` | `GET /api/claims/{id}/customer-vehicle` |
| `getAuditTrace(id)` | `GET /api/claims/{id}/audit` |
| `saveApprovalDraft(id, draft)` | `POST /api/claims/{id}/approval-draft` |
| `sendCustomerRequest(id)` | `POST /api/claims/{id}/customer-request` |
| `getDemoScenario()` | client-only (no endpoint needed) |

Swapping the implementation behind these signatures is the entire integration surface — UI/sagas don't change.

## 5. What must remain frontend-only in this gate
No network, no base URL, no provider keys, no real AI calls, no persistence. The mock API stays local.

## 6. What must NOT be done until the backend gate
Introducing `fetch`/`axios`/clients, a real base URL/env config, RTK Query/TanStack Query wiring, real AI provider calls, auth, or any cloud config. Those belong to the separately-approved backend/integration gates.
