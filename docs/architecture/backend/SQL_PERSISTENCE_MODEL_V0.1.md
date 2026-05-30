# SQL Persistence Model
**Date:** 2026-05-30
**Gate:** AZURE_SQL_FULL_PERSISTENCE_ALL_PAGES_DEPLOY_PUSH_V0.1

---

## Overview

The InsuranceAIPlatform API uses **6 service-owned EF Core DbContexts**, each mapped
to its own SQL schema with an independent `__EFMigrationsHistory` table. There is no
single all-entity DbContext. 11 EF Core migrations are checked in to the source tree
and were applied at this gate.

No code changes were required at this gate. The persistence layer pre-existed in the
source tree; this gate provisioned the Azure SQL resource and wired the connection.

---

## DbContext / Schema / Entity Map

| DbContext | SQL Schema | Entities |
|---|---|---|
| `ClaimsDbContext` | `claims` | `Claim`, `ClaimStatusHistory` |
| `CustomersPoliciesDbContext` | `customers_policies` | `SyntheticCustomer`, `Policy`, `Vehicle` |
| `DocumentsDbContext` | `documents` | `ClaimDocument`, `MissingDocumentRequest` |
| `ApprovalDbContext` | `approval` | `ApprovalDraft`, `ApprovalDecisionOption`, `PayoutSimulation` |
| `AuditCostDbContext` | `audit_cost` | `AuditEvent`, `CostTrace`, `TokenUsageTrace`, `OutboxMessage` |
| `AiAnalysisDbContext` | `ai_analysis` | `AiAnalysisRun`, `AiFinding`, `AiEvidenceReference`, `AiRiskSignal` |

Each schema has its own `__EFMigrationsHistory` — migrations for one context do not
appear in another schema's history table.

---

## Migrations

11 EF Core migrations are present in the committed source tree, distributed across the
6 contexts. All were applied to the Azure SQL `InsuranceAIPlatform` database at this
gate via the `InsuranceAIPlatform.DbMigrator` console app.

---

## DbMigrator — Migrate + Seed Flow

The `InsuranceAIPlatform.DbMigrator` console app is the single entrypoint for
database setup. One run migrates and idempotently seeds all 6 contexts in sequence:

1. Apply any pending EF Core migrations for each DbContext.
2. Run idempotent seed logic for each context (checks for existing records by stable
   ID before inserting; safe to run multiple times).

The DbMigrator was run locally, pointed at the Azure SQL endpoint via a temporary
client-IP firewall rule that was removed after successful execution.

---

## Connection String Resolution Order

The API resolves its SQL connection string in this priority order:

1. `ConnectionStrings:InsuranceAIPlatform` (config key; in Azure, injected as
   env var `ConnectionStrings__InsuranceAIPlatform` from the Container App secret
   `sql-connection`).
2. `INSURANCEAI_CONNECTION_STRING` environment variable (alternate env-var name).
3. LocalDB fallback — used automatically in local development when neither of the
   above is set.

In the current Azure deployment the Container App secret `sql-connection` supplies
the full connection string via the `ConnectionStrings__InsuranceAIPlatform` env var.

---

## Seed Strategy

All seed data is **synthetic only** — no real PII. Seeds use stable, predictable IDs
so the idempotency check (insert-if-not-exists by ID) is deterministic across re-runs.

### Seed Counts by Context

| Context / Schema | Entity | Rows |
|---|---|---|
| `customers_policies` | `SyntheticCustomer` | 201 (golden CUST-4421 + 200 synthetic CUST-T####) |
| `customers_policies` | `Policy` | 201 |
| `customers_policies` | `Vehicle` | 201 |
| `claims` | `Claim` | 15 (golden CLM-1006..CLM-1010 + synthetic CLM-1011..CLM-1020) |
| `documents` | `ClaimDocument` | 14 |
| `approval` | `ApprovalDraft` | 1 |
| `approval` | `ApprovalDecisionOption` | 4 |
| `audit_cost` | `AuditEvent` | 6 |
| `audit_cost` | `CostTrace` | 4 |
| `audit_cost` | `TokenUsageTrace` | 1 |
| `ai_analysis` | `AiAnalysisRun` | 1 |
| `ai_analysis` | `AiFinding` | 3 |
| `ai_analysis` | `AiEvidenceReference` | 2 |
| `ai_analysis` | `AiRiskSignal` | 4 |

### Golden vs Synthetic Claims

The 5 golden claims (CLM-1006..CLM-1010) have rich curated fixture data used by the
in-memory path for sub-resources (documents, AI evidence, risks, approval, audit).
The 10 synthetic claims (CLM-1011..CLM-1020) are DB-only rows with minimal fields.

`HybridClaimReadService` serves golden claims from curated in-memory fixtures and
DB-created claims from SQL, combining both sources in list and detail reads.

---

## No Code Change at This Gate

The persistence layer (DbContexts, entities, migrations, DbMigrator, seed logic,
connection-string resolution, HybridClaimReadService) was already present in the
committed source tree before this gate. This gate's only changes were infrastructure
provisioning and Container App configuration.
