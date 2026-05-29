# Microservice Architecture Correction (Before DB / Write / AI) — V0.1 — Report

**Gate:** `MICROSERVICE_ARCHITECTURE_CORRECTION_BEFORE_DB_WRITE_AI_V0.1` · **Date:** 2026-05-27
**Type:** planning / architecture-correction only — no implementation, no DB, no AI calls, no source commit/push.

## Current state
- Branch `dev` @ `f8df2b6`; `origin/dev` `f8df2b6`; `origin/main` `69e6731` (unchanged). Source tree clean; the 3 prior `LOCAL_COMPLETION_*` planning docs were untracked and are now updated with supersede notices.
- Frontend (11 screens) + single .NET 9 read API (in-memory, 11 GET endpoints, 14 tests). Read integration + safe-buttons honesty already on `dev`.

## Why this gate
GPT returned **ACCEPT_WITH_ARCHITECTURE_CORRECTION**. Slava corrected the target: the backend must be an **Azure-ready microservice architecture from the planning stage**, so Azure becomes a mapping, not a rewrite. This gate reframes the prior single-backend plan into microservice-first, local-first, Azure-ready architecture while keeping all still-valid requirements.

## What this gate produced
3 new architecture docs + supersede notices on the prior 2 docs + this report. No code/DB/migrations/AI calls.

## Files created/updated (local, uncommitted)
- **Created** `docs/architecture/MICROSERVICE_ARCHITECTURE_CORRECTION_BEFORE_DB_WRITE_AI_V0.1.md` (25 sections).
- **Created** `docs/architecture/MICROSERVICE_SERVICE_BOUNDARIES_V0.1.md` (7 services, full per-service contracts).
- **Created** `docs/architecture/MICROSERVICE_LOCAL_GATE_SEQUENCE_V0.1.md` (21 gates).
- **Updated** `docs/architecture/LOCAL_COMPLETION_ARCHITECTURE_PLAN_BEFORE_DB_WRITE_AI_V0.1.md` (ARCHITECTURE CORRECTION NOTICE at top).
- **Updated** `docs/architecture/LOCAL_COMPLETION_GATE_SEQUENCE_V0.1.md` (SUPERSEDED notice at top).
- **Created** `docs/reports/microservice-architecture-correction-before-db-write-ai-v0.1/report.md` (this).

## Accepted architecture
**Azure-ready microservice architecture, implemented locally first** — microservice-first boundaries, local-first execution, frontend → BFF only, service-owned data (no shared DbContext / no cross-service DB joins), HTTP for queries/commands + events/outbox for cross-service facts, Azure later as a mapping.

## Services defined (7)
BFF/API Gateway · Claims · Customers & Policies · Documents · AI Analysis · Approval · Audit & Cost. The 23-entity schema (+ new `TestUsers`) is partitioned by service owner; cross-service references are id-only.

## Superseded assumptions
Single .NET backend as system of record; one shared EF-backed repository behind one DI seam; one `InsuranceAiDbContext` for all entities; `SQLSERVER_EFCORE_PERSISTENCE_IMPLEMENTATION_V0.1` as the next gate. All reframed: service-owned data, BFF-only frontend, per-service persistence after BFF + skeletons.

## Migration path summary
Stage 0 freeze current FE + read API → Stage 1 this correction → Stage 2 BFF skeleton (one surface preserved) → Stage 3 service skeletons → Stage 4 per-service persistence (schema-per-service, 200 users, CLM-1006) → Stage 5 write/events/audit → Stage 6 AI Analysis Service → Stage 7 full local verification → Stage 8 owner manual → Stage 9 Azure mapping.

## Data / DB strategy summary
Local Phase 1: one local SQL Server, **schema-per-service** inside `InsuranceAIPlatform` (claims/customers/documents/ai/approval/audit); per-service DbContext + migrations; no cross-schema access; BFF holds no DB. Local Phase 2 optional DB-per-service. Azure: Azure SQL per service/pool. 200 users in Customers & Policies; CLM-1006 golden; never `DevDept`; no real PII.

## Events / outbox summary
Transactional outbox per service; local bus → maps to Azure Service Bus/Event Grid. Sync HTTP for BFF read-aggregation + command routing; async events for cross-service facts; no distributed transactions. 15 candidate events with versioned envelopes, correlation/trace id, idempotency by eventId.

## AI Analysis Service summary
DeepSeek isolated in this one service behind `IAiProvider` { Mock (default), DeepSeek (opt-in, **disabled by default**), Disabled }; mode `mock|deepseek|disabled` default `mock`; models `deepseek-v4-flash`/`deepseek-v4-pro`; `DEEPSEEK_API_KEY` env/user-secrets only (name-only in docs); runs persisted + **cached (no page-load calls)**; explicit mock fallback; advisory-only guardrails INV-1..6; every run emits audit + cost.

## Azure readiness summary
BFF + 6 services → Azure Container Apps; per-service Azure SQL; outbox → Service Bus/Event Grid; document bytes → Blob; secrets → Key Vault; observability → App Insights/Log Analytics; RAG → Azure AI Search only if needed. Provisioned only after local completion + owner manual checkpoint.

## Verification (this gate)
- Only docs/report/handoff changed; **no `src/**`, no `server/**`, no package/lock** (changeset inspection).
- No DB/migrations/EF; no AI provider call; no Azure.
- `DEEPSEEK_API_KEY` never read/printed/logged; no secret value in any file; no real PII (synthetic seed only).
- No source commit/push; `main` untouched (`69e6731`).
- Docs explicitly name the 7 microservices + boundaries; explicitly mark `SQLSERVER_EFCORE_PERSISTENCE_IMPLEMENTATION_V0.1` as superseded; define the next gate as `BFF_API_GATEWAY_SKELETON_PLANNING_V0.1`.

## Next safe step
`BFF_API_GATEWAY_SKELETON_PLANNING_V0.1` (gate 2 of the microservice sequence) — plan the BFF/API Gateway that preserves the existing frontend seam and routes/aggregates across services. Planning-only; no code.
