# Evaluation Mapping

This file maps the assignment requirements to concrete repository evidence.

## Core functionality

| Requirement | Repository evidence |
|---|---|
| Working solution with simple interface | React/Vite frontend with claim dashboard, claim workspace and product pages |
| Answers based on documents/RAG-style retrieval | RAG DTO contracts and mock RAG answer flow in `src/api/insuranceApi.types.ts` and `src/api/mockInsuranceApi.ts` |
| Upload/provided documents concept | Claim documents and text document upload contract represented in API types |
| Simple interface | Route-based claim workspace and evidence pages |

## RAG / LLM approach

| Topic | Repository evidence |
|---|---|
| Chunking | `docs/assignment/RAG_DECISIONS.md` |
| Embedding model | `docs/assignment/RAG_DECISIONS.md` |
| Vector database | `docs/assignment/RAG_DECISIONS.md` |
| Retrieval approach | `docs/assignment/RAG_DECISIONS.md` |
| Prompt/context management | `docs/assignment/RAG_DECISIONS.md` |
| Guardrails | `docs/assignment/RAG_DECISIONS.md`, `src/api/insuranceApi.types.ts` |
| Quality controls | `docs/assignment/RAG_DECISIONS.md` |
| Observability | `docs/assignment/RAG_DECISIONS.md`, `docs/assignment/PRODUCTIONIZATION.md` |

## Engineering excellence

| Topic | Repository evidence |
|---|---|
| Clean frontend structure | `src/app`, `src/pages`, `src/features`, `src/components`, `src/api` |
| API boundary | `src/api/insuranceApi.ts` |
| Mock/backend separation | `src/api/mockInsuranceApi.ts`, `src/api/backendInsuranceApi.ts` |
| Typed contracts | `src/api/insuranceApi.types.ts` |
| Build scripts | `package.json` |
| Local env example | `.env.example` |
| Architecture docs | `docs/assignment/ARCHITECTURE.md` |

## Productionization

| Requirement | Repository evidence |
|---|---|
| Hyperscaler deployment plan | `docs/assignment/PRODUCTIONIZATION.md` |
| Scalability plan | `docs/assignment/PRODUCTIONIZATION.md` |
| Security/auth plan | `docs/assignment/PRODUCTIONIZATION.md` |
| Observability plan | `docs/assignment/PRODUCTIONIZATION.md` |
| Cost controls | `docs/assignment/PRODUCTIONIZATION.md`, `README.md` |

## AI-assisted development

| Requirement | Repository evidence |
|---|---|
| Explain how AI tools were used | `docs/assignment/AI_ASSISTED_DEVELOPMENT.md` |
| Explain do's and don'ts | `docs/assignment/AI_ASSISTED_DEVELOPMENT.md` |
| Show human judgment | `docs/assignment/AI_ASSISTED_DEVELOPMENT.md`, `README.md` |

## Reviewer convenience

| Need | Repository evidence |
|---|---|
| Quick path | `README.md`, `docs/assignment/REVIEWER_GUIDE.md` |
| Demo script | `docs/assignment/DEMO_SCRIPT.md` |
| Checklist | `docs/assignment/SUBMISSION_CHECKLIST.md` |
| Limitations | `README.md`, `docs/assignment/PRODUCTIONIZATION.md`, `docs/assignment/SUBMISSION_CHECKLIST.md` |

## Honest limitation

The repository is strongest as a productized frontend + API contract + deterministic mock RAG review path. If a reviewer expects a fully deployed backend/vector database/LLM provider in the same repository, that should be called out as future productionization work unless the backend implementation is supplied separately.
