# RAG / LLM Decisions

## Goal

Build a claim-scoped assistant that answers questions about insurance claim evidence with citations, not a generic unrestricted chatbot.

The highest-priority outcome is trustworthy, reviewable decision support for a human adjuster.

## Key product constraint

The assistant is advisory only.

It must not:

- approve payout;
- reject a claim;
- accuse fraud as a final decision;
- send a customer message;
- change claim status;
- hide uncertainty.

It can:

- summarize evidence;
- point to relevant document snippets;
- identify missing information;
- explain policy coverage considerations;
- suggest a next human review step;
- produce auditable traces.

## Claim-scoped retrieval

Retrieval is scoped by `claimId`.

Reasoning:

- Insurance evidence is sensitive.
- A claim assistant should not pull arbitrary evidence from unrelated claims.
- Similar claims can be useful, but should be exposed as claim-level cards, not raw evidence text from other customer files.

## Chunking approach

For the time-boxed assignment, the preferred chunking strategy is simple and explainable:

- chunk by document section when the source has structure;
- otherwise chunk by paragraphs or fixed-size text windows;
- preserve document ID, document kind, claim ID and source label as metadata;
- keep snippets short enough for citations;
- avoid merging evidence from multiple claims into the same chunk.

Production upgrade:

- type-specific chunkers for police reports, policy clauses, invoices, photos/OCR notes and adjuster notes;
- overlap tuned by document type;
- content hashing for idempotent reindexing;
- redaction before embedding where required.

## Embedding model decision

Current local/mock mode uses deterministic behavior to make the demo reviewable without paid model calls.

For production, the preferred choices would be:

1. **Azure OpenAI embeddings** when the app is deployed on Azure and enterprise compliance is required.
2. **OpenAI text-embedding models** for a simple hosted path.
3. **Local sentence-transformer embeddings** for private/offline review environments.

Decision criteria:

- quality on insurance/legal language;
- latency;
- cost;
- deployment constraints;
- privacy/compliance;
- repeatability of evaluation.

## Vector database decision

For assignment-scale work, a simple local/in-memory index is enough to demonstrate the interface and contracts.

Production candidates:

- **Qdrant**: strong local/dev experience and straightforward Docker deployment.
- **pgvector**: good when PostgreSQL is already the system of record.
- **Azure AI Search**: strong Azure-native enterprise option with hybrid search and operational support.

Preferred production path for this project: Azure AI Search or Qdrant depending on the target cloud/runtime constraints.

## Retrieval approach

Baseline:

```text
question + claimId + useCase
  -> retrieve topK claim-scoped chunks
  -> pass compact context to answer prompt
  -> return answer + citations + confidence + trace
```

Expected improvements:

- hybrid keyword + vector retrieval;
- metadata filters by document kind;
- reranking for high-stakes questions;
- minimum evidence threshold;
- explicit `insufficient evidence` answer when retrieval quality is low.

## Prompt and context management

The prompt should enforce:

- advisory-only language;
- citations required for factual claims;
- no final fraud accusation;
- no payout authorization;
- clear uncertainty;
- human adjuster final authority.

Context should include:

- claim ID;
- use case;
- retrieved snippets;
- source metadata;
- safety instructions;
- answer format expectations.

Context should not include:

- unrelated claims' raw evidence;
- hidden chain-of-thought requests;
- secrets;
- raw credentials;
- unnecessary full documents when snippets are enough.

## Guardrails

Implemented/conceptual guardrails:

- advisory-only DTO flags;
- `canApprovePayout=false`;
- `canRejectClaim=false`;
- `canAccuseFraudFinal=false`;
- `canSendCustomerMessage=false`;
- `canChangeClaimStatus=false`;
- human decision endpoints remain separate from AI/RAG endpoints;
- audit/cost trace is returned with AI/RAG operations.

Production guardrails:

- content safety filters;
- prompt injection checks on uploaded document text;
- evidence sufficiency threshold;
- PII redaction;
- per-tenant data isolation;
- approval workflow enforcement server-side;
- model output validation with structured schemas.

## Quality controls

Minimum quality checks:

- answer must cite retrieved chunks;
- answer must remain advisory;
- answer should refuse or mark uncertainty when evidence is insufficient;
- confidence must be displayed;
- retrieved chunk IDs must be auditable;
- trace/correlation ID must be available.

Evaluation set:

- coverage question;
- missing document question;
- risk factor question;
- payout evidence question;
- insufficient evidence question;
- prompt injection / malicious document question.

Production evaluation:

- golden dataset of claims and expected evidence;
- regression tests for retrieval quality;
- hallucination checks;
- citation precision/recall;
- manual adjuster feedback loop.

## Observability

Current contract exposes:

- trace ID;
- correlation ID;
- token counts;
- cost;
- retrieval latency;
- audit entries;
- infrastructure status.

Production telemetry should add:

- OpenTelemetry traces;
- structured logs;
- per-step latency;
- model/provider errors;
- retrieval hit counts;
- empty retrieval rate;
- answer rejection/low-confidence rate;
- cost budgets and alerts.

## Final trade-off

The project prioritizes a stable, explainable, human-reviewed RAG workflow over a complex autonomous agent. For this assignment, a simple working system with clear boundaries, citations, guardrails and documentation is more valuable than a broad but unreliable AI agent.
