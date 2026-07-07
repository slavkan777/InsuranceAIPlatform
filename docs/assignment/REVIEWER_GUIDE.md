# Reviewer Guide

## What this project is

InsuranceAIPlatform is a domain-specific AI/RAG workbench for auto insurance claim review.

It is not a generic chatbot. The product is scoped around one workflow:

```text
claim evidence -> cited answer -> human adjuster review -> audit and cost trace
```

The core idea is that an adjuster can ask questions about a claim and receive an evidence-backed advisory answer with citations, confidence and trace metadata.

## Fast review path

1. Read `README.md` for the product and assignment mapping.
2. Run the frontend in mock mode.
3. Open the golden claim `CLM-1006`.
4. Review the documents, AI evidence, approval and audit pages.
5. Review `src/api/insuranceApi.ts` for mock/backend mode selection.
6. Review `src/api/mockInsuranceApi.ts` for deterministic local RAG behavior.
7. Review `src/api/backendInsuranceApi.ts` for backend contract wiring.
8. Review `src/api/insuranceApi.types.ts` for RAG contracts and guardrails.
9. Read the assignment docs in `docs/assignment/`.

## Recommended commands

```bash
npm install
npm run build
npm run dev
```

Then open:

```text
http://127.0.0.1:5173
```

Mock mode is the default. It does not require external API keys, a vector database or an LLM provider.

## Important files

| File | Why it matters |
|---|---|
| `README.md` | Main assignment explanation and setup |
| `.env.example` | Mock/backend mode configuration |
| `src/api/insuranceApi.ts` | Runtime API facade |
| `src/api/mockInsuranceApi.ts` | Deterministic local demo implementation |
| `src/api/backendInsuranceApi.ts` | Backend HTTP client and DTO mapping |
| `src/api/insuranceApi.types.ts` | AI/RAG DTO contracts |
| `docs/assignment/ARCHITECTURE.md` | System design |
| `docs/assignment/RAG_DECISIONS.md` | RAG decisions and trade-offs |
| `docs/assignment/PRODUCTIONIZATION.md` | Cloud/scaling plan |
| `docs/assignment/AI_ASSISTED_DEVELOPMENT.md` | AI coding workflow notes |

## What to look for

### Product thinking

The assistant is embedded into a realistic insurance claim workflow instead of being a generic document chatbot.

### Engineering boundaries

The frontend uses a stable `insuranceApi` facade. This lets the implementation move from mock to backend without rewriting product pages.

### RAG contract maturity

The RAG response model includes citations, retrieved chunk IDs, confidence, token usage, cost, retrieval latency, trace IDs and an advisory-only flag.

### Guardrails

The system explicitly prevents AI from being the final decision maker. AI can advise; the human adjuster decides.

### Production awareness

The project documents what would be needed for a production deployment: storage, ingestion, vector index, model provider, observability, evals, auth and auditability.

## Known limitations

- Mock mode is deterministic and does not perform real embeddings.
- Backend mode requires an API implementation matching the documented contract.
- Full binary document ingestion and production vector search are documented as productionization work.
- Screenshots/video should be added before a fully polished final delivery if time allows.

## Why this shape was chosen

The assignment asks for engineering judgment, not maximum complexity. This solution keeps the core path simple and reviewable while showing the production seams that would matter in a real insurance AI system.
