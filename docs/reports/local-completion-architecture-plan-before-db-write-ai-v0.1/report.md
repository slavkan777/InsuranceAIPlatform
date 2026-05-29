# Local Completion Architecture Plan (Before DB / Write / AI) — V0.1 — Report

**Gate:** `LOCAL_COMPLETION_ARCHITECTURE_PLAN_BEFORE_DB_WRITE_AI_V0.1` · **Date:** 2026-05-27
**Type:** planning only — no implementation, no DB, no AI calls, no source commit/push.

## Current state
- Branch `dev` @ `f8df2b6`; `origin/dev` = `f8df2b6`; `origin/main` = `69e6731` (unchanged); working tree clean at start.
- Frontend (React/TS/Vite/Redux/Saga, 11 screens) + .NET 9 backend (in-memory `IClaimReadService`, 11 read endpoints, 14 tests). Read integration + safe-buttons action honesty already on `dev`.

## What this gate produced
Two architecture documents + this report. No code, DB, migrations, or AI calls. The plan **consolidates and extends** the existing `docs/architecture/BACKEND_*` set (23-entity schema, EF strategy, seed plan, AI/risk/audit placeholders, security boundaries, prior implementation gates) and adds the two new requirements from this gate: an **exactly-200 synthetic test-user** seed and a **DeepSeek provider abstraction**.

## Files created (local, uncommitted)
- `docs/architecture/LOCAL_COMPLETION_ARCHITECTURE_PLAN_BEFORE_DB_WRITE_AI_V0.1.md` — 23-section consolidated plan.
- `docs/architecture/LOCAL_COMPLETION_GATE_SEQUENCE_V0.1.md` — 13-gate machine-readable sequence.
- `docs/reports/local-completion-architecture-plan-before-db-write-ai-v0.1/report.md` — this report.

## Key decisions proposed
- Keep the clean DI seam (`IClaimReadService`) — swap `InMemoryClaimReadService` → EF-backed repository without touching controllers/DTOs.
- Adopt the existing 23-entity schema; add one `TestUsers` entity for the 200-user requirement.
- Writes are explicit **command handlers** (not CRUD), each with validation + a mandatory audit event; deterministic server-authoritative status machine; AI advisory-only.
- AI provider abstraction `IAiProvider` { Mock (default), DeepSeek (opt-in, disabled by default), Disabled }; mode `mock|deepseek|disabled` default `mock`; `DEEPSEEK_API_KEY` from local env/user-secrets only; models `deepseek-v4-flash` (default) / `deepseek-v4-pro`.
- Implement → verify → commit-only → push-only separation across the 13-gate sequence; `main` untouched until the final owner-authorized release gate.

## Discrepancies recorded for reconciliation (not fixed in this gate)
1. `CustomerId` `CUST-001` (docs) vs `CUST-4421` (implemented) → **use `CUST-4421`**.
2. Connection-string key/env naming diverges between prior docs → **standardize on `ConnectionStrings:InsuranceAIPlatform` / `INSURANCEAIPLATFORM_DB`**.
3. `AuditTraceDto.Model = "Azure OpenAI (mock)"` → **relabel to `Demo/Synthetic` / `DeepSeek (mock)`** when the AI gate lands (honest-label invariant).

## Verification (this gate)
- Only docs/report/handoff files changed; **no `src/**`, no `server/**`, no package/lock changes** (confirmed by changeset inspection).
- No database, no migrations, no EF packages added; no AI provider call made.
- `DEEPSEEK_API_KEY` never read/printed/logged; no secret value appears in any file.
- No source commit, no source push; `main` untouched.

## Next safe step
`SQLSERVER_EFCORE_PERSISTENCE_IMPLEMENTATION_V0.1` (Gate 1 of the sequence) — add EF Core + SQL Server, create local `InsuranceAIPlatform` DB, migrate the in-memory seed + 200 test users, behind unchanged read interfaces. Followed by its dedicated commit/push gate.
