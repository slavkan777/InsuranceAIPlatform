# Demo Script

## 90-second walkthrough

### 1. Open the application

Run:

```bash
npm install
npm run dev
```

Open:

```text
http://127.0.0.1:5173
```

Explain:

> This is an insurance claim AI workbench. The selected assignment option is Chat With Your Docs, adapted into a claim-scoped evidence assistant.

### 2. Dashboard and claims queue

Show the dashboard and claims list.

Explain:

> The application is built around the adjuster's workflow, not around a generic chatbot screen. The assistant is contextual to a claim.

### 3. Open the golden claim

Open `CLM-1006`.

Explain:

> This is the golden claim used to demonstrate documents, AI analysis, RAG evidence, risks, human approval and audit/cost tracking.

### 4. Documents and evidence

Open the documents page.

Explain:

> The assistant works over claim-scoped evidence: policy information, damage assessment, police report style evidence, photos and missing document status.

### 5. AI evidence / RAG behavior

Open the AI/evidence page.

Explain:

> RAG answers are expected to return citations, confidence, retrieved chunks, provider mode, token/cost metadata and a correlation ID. Mock mode returns deterministic synthetic responses so the app is reviewable without external services.

### 6. Human approval

Open the approval page.

Explain:

> AI is advisory only. It cannot approve payout, reject the claim, send a customer message or change claim status. The human adjuster remains the final decision-maker.

### 7. Audit and cost

Open the audit page.

Explain:

> The architecture treats traceability as a product requirement: every AI/RAG operation should be auditable with cost, token and evidence metadata.

### 8. Architecture summary

Show `docs/assignment/ARCHITECTURE.md` and `docs/assignment/RAG_DECISIONS.md`.

Explain:

> The key engineering decision is the API facade. The UI runs in mock mode by default, but the same frontend can switch to backend mode through environment configuration.

## Short positioning sentence

> InsuranceAIPlatform is a claim-scoped RAG assistant for insurance evidence review, designed with citations, human-in-the-loop guardrails, auditability, cost awareness and a clean mock/backend API boundary.

## What to say if asked about unfinished parts

> I kept the default reviewer path deterministic and credential-free. The production path is documented separately: persistent ingestion, vector search, model provider routing, auth, observability and RAG evaluation would be the next engineering layer.
