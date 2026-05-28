# Untracked Docs / Reports Triage — Pre-Azure Batch V0.1

**Gate:** `PRE_AZURE_LOCAL_COMPLETION_BATCH_UI_AI_RESILIENCE_DOCS_TRIAGE_V0.1`
**Date:** 2026-05-28
**Triage rule:** NO deletion in this gate. Each untracked artifact is classified into one of four bins for a future docs gate.

## Classification key

- **COMMIT_LATER_SOURCE_DOC** — durable project documentation; should be committed to `dev` in a dedicated docs gate.
- **ARCHIVE_LATER** — per-gate evidence/audit artifact; should be moved to a non-tracked archive location or committed as historical evidence in a docs-evidence gate.
- **DEFER** — leave untracked for now; not durable enough to commit and not evidence-y enough to archive; revisit at next architecture cleanup.
- **IGNORE_LOCAL** — local-only artifact; safe to remain untracked indefinitely or be cleaned with explicit operator approval.

## Architecture planning docs (`docs/architecture/*.md`)

| File | Classification | Rationale |
|---|---|---|
| `BFF_API_GATEWAY_ROUTE_CONTRACT_MAP_V0.1.md` | COMMIT_LATER_SOURCE_DOC | Durable BFF route contract — still useful as live documentation. |
| `BFF_API_GATEWAY_SKELETON_PLANNING_V0.1.md` | COMMIT_LATER_SOURCE_DOC | High-level BFF planning; references current architecture. |
| `LOCAL_COMPLETION_ARCHITECTURE_PLAN_BEFORE_DB_WRITE_AI_V0.1.md` | DEFER | Pre-implementation plan; partially superseded by completed gates. |
| `LOCAL_COMPLETION_GATE_SEQUENCE_V0.1.md` | DEFER | Gate roadmap; stale after multiple new gates ran. |
| `MICROSERVICE_ARCHITECTURE_CORRECTION_BEFORE_DB_WRITE_AI_V0.1.md` | DEFER | Intermediate correction note; covered by later durable docs. |
| `MICROSERVICE_LOCAL_GATE_SEQUENCE_V0.1.md` | DEFER | Gate roadmap; stale. |
| `MICROSERVICE_SERVICE_BOUNDARIES_V0.1.md` | COMMIT_LATER_SOURCE_DOC | Durable architecture — service boundary spec. |
| `MICROSERVICE_SERVICE_SKELETONS_CONTRACT_MAP_V0.1.md` | COMMIT_LATER_SOURCE_DOC | Durable cross-service contract map. |
| `MICROSERVICE_SERVICE_SKELETONS_PLANNING_V0.1.md` | DEFER | Planning intent now realised in committed code. |

Tally: 4 COMMIT_LATER, 5 DEFER.

## Per-gate report directories (`docs/reports/<slug>/`)

| Slug | Classification | Rationale |
|---|---|---|
| `bff-api-gateway-skeleton-planning-v0.1/` | ARCHIVE_LATER | Per-gate evidence; keep for audit. |
| `commit-and-push-dev-real-deepseek-opt-in-local-integration-only/` | ARCHIVE_LATER | Commit-gate evidence; matches `1b824b0` push. |
| `local-completion-architecture-plan-before-db-write-ai-v0.1/` | ARCHIVE_LATER | Historical planning evidence. |
| `microservice-architecture-correction-before-db-write-ai-v0.1/` | ARCHIVE_LATER | Historical correction evidence. |
| `microservice-service-skeletons-planning-v0.1/` | ARCHIVE_LATER | Historical skeleton planning evidence. |
| `real-deepseek-opt-in-local-integration-v0.1/` | ARCHIVE_LATER | DeepSeek implementation evidence. |
| `real-deepseek-rotated-key-short-smoke-retry-v0.1/` | ARCHIVE_LATER | Rotated-key smoke success evidence (200 + DB row). |
| `security-rotate-and-fix-deepseek-local-secret-setup-v0.1/` | ARCHIVE_LATER | Hygiene gate evidence. |
| `pre-azure-local-completion-batch-ui-ai-resilience-docs-triage-v0.1/` (this dir) | DEFER | In-flight gate's own report dir; will be classified after gate closes. |

Tally: 8 ARCHIVE_LATER, 1 DEFER (this gate's own dir).

## Recommended follow-up gate(s)

1. **`DOCS_ARCHITECTURE_DURABLE_COMMIT_V0.1`** — commit the 4 `COMMIT_LATER_SOURCE_DOC` items to `dev` in a docs-only gate.
2. **`DOCS_REPORTS_ARCHIVE_V0.1`** — move the 8 `ARCHIVE_LATER` report directories under an `archive/reports/` path that is `.gitignore`'d (or commit them as historical evidence under `docs/history/`), keeping a single index file at `docs/reports/INDEX.md` listing accepted gates with verdict + handoff SHA.
3. The 5 `DEFER` planning docs can be revisited at the next architecture cleanup; no urgency.

## What this gate did NOT do

- No files deleted.
- No files moved.
- No files staged or committed (this gate is no-commit/no-push by scope).
- This triage note is itself untracked under `docs/reports/<slug>/`.
