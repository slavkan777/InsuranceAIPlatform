# Assignment Architecture Overview

## Done state

A reviewer should be able to run the application locally in mock mode, inspect the claim workflow, see the RAG/evidence design, and understand how the same UI can connect to a backend .NET API for real ingestion/retrieval.

## Selected assignment option

**Option 1: Chat With Your Docs**

Domain adaptation: claim-scoped insurance evidence assistant.

The assistant answers questions over insurance claim documents and evidence, not over an unrestricted global document collection. This is a deliberate product constraint because claim decisions are sensitive and evidence should not leak across claims.

## High-level system shape

```text
React / TypeScript UI
  |
  v
src/api/insuranceApi.ts
  |-- mock mode ---------------------> src/api/mockInsuranceApi.ts
  |
  |-- backend mode ------------------> src/api/backendInsuranceApi.ts
                                      |
                                      v
                              ASP.NET Core API contract
                                      |
                                      v
                          Claim data + document/evidence store
                                      |
                                      v
                         RAG retrieval + answer generation
```

## Runtime modes

### Mock mode

Default mode. It is deterministic and requires no credentials.

Purpose:

- Let reviewers run the app immediately.
- Avoid secrets and paid model calls.
- Keep the UI and product workflow demonstrable even without local backend services.
- Exercise the same DTO shape that backend mode is expected to satisfy.

### Backend mode

Enabled through:

```env
VITE_INSURANCE_API_MODE=backend
VITE_INSURANCE_API_BASE_URL=http://localhost:5284
```

Purpose:

- Use real HTTP calls through `src/api/backendInsuranceApi.ts`.
- Validate that the frontend is not coupled to mock arrays.
- Demonstrate how the React app can sit in front of a .NET BFF/API.

## Main frontend boundaries

### `src/api/insuranceApi.ts`

A small facade that selects mock or backend implementation once at module load.

Pages and sagas call `insuranceApi`, not `fetch` directly.

### `src/api/mockInsuranceApi.ts`

Deterministic local implementation. It contains synthetic claims, synthetic command results, mock AI analysis, and mock RAG responses.

### `src/api/backendInsuranceApi.ts`

Typed backend client. It maps backend DTOs to frontend view models and throws typed `BackendApiError` for non-2xx responses.

### `src/api/insuranceApi.types.ts`

Shared request/response contracts for command endpoints, AI analysis, RAG answer, citations, evidence search, evaluation questions, audit entries and infrastructure status.

## RAG API contract

The RAG surface is claim-scoped:

```text
POST /api/claims/{claimId}/rag/ask
GET  /api/claims/{claimId}/rag/evidence-search?q={query}&topK={n}
GET  /api/claims/{claimId}/rag/evaluation-questions
GET  /api/claims/{claimId}/rag/audit?limit={n}
GET  /api/claims/{claimId}/rag/similar-claims?topK={n}
GET  /api/claims/{claimId}/rag/infrastructure
POST /api/claims/{claimId}/rag/infrastructure/reindex
```

`rag/ask` returns:

- answer text;
- citations;
- confidence;
- retrieved chunk IDs;
- provider mode;
- prompt/completion token counts;
- cost in micros;
- retrieval latency;
- correlation ID;
- advisory-only flag.

## Conversational assistant behavior

The user asks a claim-scoped question. The backend/mock pipeline retrieves relevant chunks, generates an advisory answer and returns citations. The UI should show the answer, evidence snippets, confidence, and trace/cost data.

No RAG answer can approve a payout, reject a claim, accuse fraud as a final decision, send a customer message, or change claim status. Those actions remain human-controlled.

## Data boundary

The golden claim is `CLM-1006`. Synthetic data is used for claim, customer, vehicle, evidence and audit traces.

PII policy:

- no real customer data;
- masked VIN/phone/email in demo content;
- synthetic-only claim/customer creation;
- no committed credentials.

## Why this shape

The goal is not to maximize framework count. The goal is to show production-oriented thinking under time constraints:

- a stable product shell;
- explicit API contracts;
- deterministic review mode;
- backend-ready seam;
- RAG answers with citations;
- audit and cost governance;
- human-in-the-loop decision control.

## Known architecture limitations

- Mock mode does not perform real embedding or model calls.
- Backend implementation must match the documented contracts.
- Binary document upload/blob storage is not implemented in the frontend contract; text document upload is represented as plain content.
- Authentication, tenant isolation, cloud deployment and observability are documented as productionization work.
