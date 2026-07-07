# InsuranceAIPlatform — Claim-Scoped RAG Assistant for Auto Insurance Claims

InsuranceAIPlatform is a domain-specific AI workbench for auto insurance claim review. It started as a polished React walking skeleton and now documents the assignment-ready direction: a claim-scoped conversational RAG assistant that answers questions about insurance claim evidence, policy clauses, damage assessments, audit records, and related claim context.

The project is intentionally framed as an **AI-assisted decision support system**, not an autonomous claim decision engine:

- AI output is advisory only.
- A human adjuster remains the final decision-maker.
- RAG answers must include citations/evidence.
- AI runs, retrieval, confidence, cost and audit traces are first-class product concepts.
- Mock mode must run locally without credentials or paid providers.

---

## Reviewer quick path

```bash
npm install
npm run build
npm run dev
```

Open:

```text
http://127.0.0.1:5173
```

Recommended review order:

1. Open the dashboard.
2. Open the claims queue.
3. Open claim `CLM-1006`.
4. Review documents, AI evidence, approval and audit/cost pages.
5. Read `docs/assignment/REVIEWER_GUIDE.md` and `docs/assignment/EVALUATION_MAPPING.md`.

Mock mode is the default. It is deterministic and does not need any LLM key, vector database or backend process.

---

## Assignment mapping

**Selected assignment option:** Option 1 — Chat With Your Docs.

**Domain adaptation:** instead of a generic PDF chatbot, this project implements an insurance claim evidence assistant. The assistant is scoped to a claim and answers questions over claim documents/evidence with cited snippets, confidence, retrieved chunk IDs, cost trace, and audit history.

Example questions:

- Is the submitted damage covered under the policy?
- What evidence supports the recommended payout?
- Which documents are missing?
- What are the main risk factors in this claim?
- Are there similar historical claims?

---

## Current implementation state

This repository currently contains:

- React + TypeScript + Vite frontend.
- Route-based insurance claim workspace.
- Redux Toolkit state slices and Redux-Saga workflow orchestration.
- Mock API implementation for local deterministic demo mode.
- Backend API facade switchable by environment variable.
- Backend client that targets a local .NET API contract at `http://localhost:5284`.
- Claim-scoped RAG DTO contracts and frontend API calls:
  - `POST /api/claims/{claimId}/rag/ask`
  - `GET /api/claims/{claimId}/rag/evidence-search`
  - `GET /api/claims/{claimId}/rag/evaluation-questions`
  - `GET /api/claims/{claimId}/rag/audit`
  - `GET /api/claims/{claimId}/rag/similar-claims`
  - `GET /api/claims/{claimId}/rag/infrastructure`
  - `POST /api/claims/{claimId}/rag/infrastructure/reindex`
- Deterministic mock RAG responses with answer, citations, confidence, retrieved chunks, tokens, cost, retrieval latency, advisory mode and correlation ID.
- Advisory-only AI analysis and human-in-the-loop decision contracts.

This submission should be reviewed primarily through the frontend product flow and the documented architecture/RAG decisions. Backend mode is designed around the `.NET API` contract exposed by `src/api/backendInsuranceApi.ts`; mock mode remains the default so the reviewer can run the app without local secrets or external services.

---

## Quick setup

### Requirements

- Node.js 18+; developed against modern Node/npm.
- npm 10+ recommended.

### Mock mode — default, no credentials

```bash
npm install
npm run dev
```

Open:

```text
http://127.0.0.1:5173
```

Mock mode uses deterministic synthetic data. It performs no external API calls and does not require LLM credentials.

### Backend mode — optional local API contract

Create `.env.local` from `.env.example`:

```bash
cp .env.example .env.local
```

Set:

```env
VITE_INSURANCE_API_MODE=backend
VITE_INSURANCE_API_BASE_URL=http://localhost:5284
```

Then run:

```bash
npm run dev
```

Backend mode expects a local API matching the contracts in `src/api/backendInsuranceApi.ts` and `src/api/insuranceApi.types.ts`.

### Build / checks

```bash
npm run build
npm run lint
npm run test:e2e
```

`npm run build` is the minimum required verification before submission.

---

## Frontend architecture

The UI is not just static screens. It is structured as an interview-ready product shell:

- **React + TypeScript + Vite** for the application shell.
- **React Router** for route-based claim workspace navigation.
- **Redux Toolkit** for typed product state.
- **Redux-Saga** for multi-step workflows and future side-effect seams.
- **API facade** in `src/api/insuranceApi.ts`, selecting mock or backend mode through `VITE_INSURANCE_API_MODE`.
- **Mock API boundary** in `src/api/mockInsuranceApi.ts`, keeping local demo deterministic and credential-free.
- **Backend API client** in `src/api/backendInsuranceApi.ts`, mapping .NET API DTOs into frontend view models.

The important architectural decision is that pages and sagas call `insuranceApi`, not raw mock data or raw `fetch`. This keeps the UI stable while the implementation behind the API boundary moves from mock to backend.

---

## RAG flow

Conceptual flow:

```text
Insurance claim workspace
  -> user asks claim-scoped question
  -> insuranceApi.ragAsk(claimId, question/useCase)
  -> mock or backend implementation
  -> retrieve relevant evidence chunks
  -> generate advisory answer
  -> return citations, confidence, cost, retrieval timing and trace ID
  -> render answer with evidence and audit context
```

The RAG contract returns:

- `traceId`
- `claimId`
- `useCase`
- `question`
- `answer`
- `confidence`
- `citations[]`
- `retrievedChunkIds[]`
- `providerMode`
- `promptTokens`
- `completionTokens`
- `costMicros`
- `retrievalMs`
- `advisoryOnly`
- `correlationId`
- `createdAtUtc`

RAG is intentionally **claim-scoped**. The assistant should not leak evidence text across unrelated claims. Similar-claim search returns claim-level cards only, not full evidence from other claims.

---

## Business scenario — golden claim

| Field | Value |
|---|---|
| Claim ID | `CLM-1006` |
| Customer | Роберт Джонсон (synthetic) |
| Vehicle | Toyota Camry 2021 · VIN `****8842` |
| Policy | Auto Comprehensive · `POL-2025-AC-4421` |
| Event | ДТП 18.05.2026, Бориспіль |
| Risk | High · 82 / 100 |
| Model confidence | 78% |
| Documents received | 6 / 7; missing rear bumper photo |
| Repair invoice vs benchmark | $2,720 vs $1,970 (+38%) |
| Recommended payout | $1,800 |
| Trace / Run ID | `trc_8f3d2a7e` / `run_8f3d2a7e` |

The skeleton shows the lifecycle of this claim: registration -> documents -> AI analysis -> RAG evidence -> risks -> human review -> audit -> completion.

---

## Route map

| Route | Purpose |
|---|---|
| `/` | Dashboard |
| `/claims` | Claims queue |
| `/claims/CLM-1006` | Claim workspace |
| `/claims/CLM-1006/documents` | Documents and photos |
| `/claims/CLM-1006/ai-evidence` | AI analysis and evidence/RAG-related panels |
| `/claims/CLM-1006/risks` | Risk checks |
| `/claims/CLM-1006/approval` | Human approval |
| `/claims/CLM-1006/audit` | Audit and cost governance |
| `/claims/CLM-1006/policy` | Policy and coverage |
| `/claims/CLM-1006/customer-vehicle` | Customer and vehicle context |
| `/demo` | Guided demo flow |

---

## Project structure

```text
src/
  app/            store, hooks, rootSaga, router
  api/            insuranceApi facade, mock API, backend API client, DTO contracts
  pages/          route-level product pages
  features/       Redux slices and sagas by domain
  components/     layout, UI and claim-specific components
  data/mock/      synthetic claim/evidence data
  types/          shared TypeScript types
  utils/          formatting and helper utilities

docs/
  architecture/   existing frontend/backend contract notes
  assignment/     assignment-specific docs and decision records
  design/         source design artifacts
```

---

## Engineering standards followed

- TypeScript strict typing for API contracts and frontend state.
- API boundary instead of direct mock imports inside pages.
- Deterministic local mode without paid AI calls.
- Synthetic data only; no real PII.
- Human-in-the-loop guardrail model.
- Advisory-only AI/RAG answers.
- Explicit DTOs for command results, AI analysis, RAG answer, citations, audit and infrastructure status.
- Cost/audit concepts present in UI and contracts.
- Local-first reviewer experience.

Standards intentionally limited or deferred:

- Production authentication/RBAC.
- Real binary document storage.
- Full backend deployment instructions.
- Full RAG evaluation suite.
- Cloud IaC.
- Production observability stack.

These are documented in `docs/assignment/PRODUCTIONIZATION.md`.

---

## Documentation for reviewers

- `docs/assignment/REVIEWER_GUIDE.md` — fast review path and key files.
- `docs/assignment/EVALUATION_MAPPING.md` — assignment requirements mapped to repository evidence.
- `docs/assignment/DEMO_SCRIPT.md` — concise demo walkthrough script.
- `docs/assignment/ARCHITECTURE.md` — system shape, mock/backend mode, boundaries.
- `docs/assignment/RAG_DECISIONS.md` — chunking, embeddings, retrieval, prompt/context, guardrails, quality.
- `docs/assignment/PRODUCTIONIZATION.md` — what is required to scale/deploy on Azure/AWS/GCP.
- `docs/assignment/AI_ASSISTED_DEVELOPMENT.md` — how AI coding tools were used and controlled.
- `docs/assignment/SUBMISSION_CHECKLIST.md` — reviewer checklist.

---

## What I would do with more time

1. Add a local .NET API implementation into the same repo if not already provided in the review bundle.
2. Add real document ingestion with PDF/text parsing and persistent document storage.
3. Add a real vector backend: Qdrant, pgvector or Azure AI Search.
4. Add production LLM provider routing: Azure OpenAI / OpenAI / local fallback.
5. Add RAG evaluation tests over golden questions.
6. Add OpenTelemetry traces across frontend, API, retrieval and model call.
7. Add auth, tenant isolation and PII redaction.
8. Add Docker Compose for frontend + API + DB + vector store.
9. Add short demo video and screenshots for the main flow.

---

## Synthetic data and safety policy

- No real PII.
- No real customer records.
- No autonomous payout.
- No autonomous rejection.
- No final fraud accusation by AI.
- No real money movement.
- AI/RAG output is advisory only.
- Human adjuster makes the final decision.
