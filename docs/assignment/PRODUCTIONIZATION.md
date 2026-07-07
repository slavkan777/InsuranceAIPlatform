# Productionization Plan

## Goal

Move the current assignment-ready insurance RAG workbench from local/demo mode to a scalable production system on Azure, AWS, GCP or Cloudflare.

The preferred target for this project is Azure because the product shape maps naturally to ASP.NET Core, Azure SQL, Blob Storage, Application Insights and Azure OpenAI/Azure AI Search.

## Production architecture target

```text
Browser
  -> CDN / Static Web App
  -> ASP.NET Core BFF/API
  -> Auth/RBAC
  -> Claims database
  -> Document storage
  -> Background ingestion/indexing worker
  -> Vector search
  -> LLM provider/model gateway
  -> Observability/cost governance
```

## Azure deployment option

Recommended Azure services:

- Azure Static Web Apps or Azure Front Door + Storage/CDN for frontend.
- Azure App Service or Azure Container Apps for ASP.NET Core API.
- Azure SQL or PostgreSQL Flexible Server for relational claim data.
- Azure Blob Storage for uploaded documents and extracted text artifacts.
- Azure AI Search or Qdrant on Container Apps for vector retrieval.
- Azure OpenAI for embeddings and answer generation.
- Azure Key Vault for secrets.
- Application Insights + OpenTelemetry for traces/logs/metrics.
- Azure Service Bus or Storage Queues for ingestion/reindex jobs.

## AWS deployment option

Equivalent AWS services:

- S3 + CloudFront for frontend.
- ECS/Fargate or App Runner for API.
- RDS PostgreSQL for relational data.
- S3 for documents.
- OpenSearch / pgvector / managed Qdrant for retrieval.
- Bedrock or OpenAI-compatible model gateway.
- Secrets Manager.
- CloudWatch + OpenTelemetry.
- SQS for background indexing.

## GCP deployment option

Equivalent GCP services:

- Cloud Storage + Cloud CDN for frontend.
- Cloud Run for API.
- Cloud SQL for relational data.
- Cloud Storage for documents.
- Vertex AI / Matching Engine / pgvector / Qdrant for retrieval.
- Secret Manager.
- Cloud Trace/Logging/Monitoring.
- Pub/Sub for ingestion jobs.

## Data model production needs

Required durable entities:

- Claim
- Customer
- Vehicle
- Policy
- Document metadata
- Document content / extracted text
- Evidence chunk
- Embedding metadata
- RAG trace
- RAG citation
- AI analysis run
- Human decision
- Audit event
- Outbox message
- Cost record

## Document ingestion pipeline

Production ingestion flow:

```text
upload document
  -> virus/content-type validation
  -> store original in blob storage
  -> extract text/OCR if needed
  -> redact sensitive content where needed
  -> chunk document
  -> embed chunks
  -> store chunks + vectors
  -> mark document indexed
```

Operational requirements:

- idempotent indexing;
- retries;
- dead-letter queue;
- per-claim indexing status;
- content hashing;
- reindex endpoint/job;
- audit trail for every ingestion step.

## RAG scalability

Immediate scale strategy:

- metadata filter by `claimId`;
- small topK retrieval;
- cache stable policy clauses;
- keep prompts compact;
- log retrieval latency and token usage.

Larger scale strategy:

- hybrid vector + keyword retrieval;
- reranking for high-stakes queries;
- tenant-aware partitioning;
- async indexing;
- batch embeddings;
- model gateway with provider fallback;
- cost budgets by tenant/project.

## Security and compliance

Required before production:

- authentication;
- role-based access control;
- tenant isolation;
- PII classification and masking;
- secrets only in Key Vault/Secrets Manager;
- HTTPS-only;
- audit logs for all user actions;
- immutable audit trail for AI/human decisions;
- prompt injection defenses for uploaded text;
- safe output validation;
- rate limiting;
- data retention policy.

## AI safety and decision control

Production system must enforce server-side rules:

- AI cannot approve payout.
- AI cannot reject claim.
- AI cannot send customer message.
- AI cannot change claim status.
- AI cannot make final fraud accusation.
- Human decision endpoints are separate from AI/RAG endpoints.
- Every AI/RAG output is stored with trace ID, citations and prompt/model metadata.

## Observability

Production telemetry should include:

- request latency;
- retrieval latency;
- model latency;
- token usage;
- cost per answer;
- empty/low-confidence retrieval rate;
- failed ingestion jobs;
- backend 4xx/5xx;
- vector DB availability;
- LLM provider errors;
- user feedback on answer usefulness.

Recommended stack:

- OpenTelemetry tracing;
- structured logs with correlation IDs;
- Application Insights / CloudWatch / Cloud Monitoring;
- dashboards for cost, latency, quality and errors;
- alerting on provider failure, cost spikes, and indexing failures.

## Testing strategy

Before production:

- unit tests for chunking and DTO mapping;
- integration tests for API endpoints;
- contract tests between frontend and backend;
- Playwright flows for the golden claim;
- RAG evaluation tests over fixed question set;
- hallucination/citation checks;
- prompt injection tests;
- load tests for ingestion and ask endpoints.

## Deployment strategy

Recommended rollout:

1. Local mock mode.
2. Local backend mode with seed data.
3. Containerized dev environment.
4. Single-region cloud demo.
5. Auth-enabled staging.
6. Production pilot with limited tenant/users.
7. Full production with observability, cost budgets and eval regression gates.

## What was intentionally skipped in the assignment

- Full production auth.
- Real PII handling.
- Binary upload/blob storage.
- Full cloud IaC.
- Full observability deployment.
- Real model provider by default.
- Large-scale vector storage.

These were skipped to keep the assignment focused on product boundaries, RAG contracts, guardrails, and a reviewer-friendly local run path.
