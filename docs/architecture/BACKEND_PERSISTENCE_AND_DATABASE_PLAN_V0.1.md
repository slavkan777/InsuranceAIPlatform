---
title: Backend Persistence & Database Plan
type: knowledge
status: active
created: 2026-05-27
tags: [backend, persistence, database, sql-server, ef-core, planning]
---

# Backend Persistence & Database Plan V0.1

> **Scope:** Planning gate only. No DB is created, no code written, no migrations run.
> DB creation is FORBIDDEN until the explicit `BACKEND_SQLSERVER_PERSISTENCE_GATE`.

---

## 1. Persistence Option Comparison

| # | Option | Speed to P0 API | Impl Risk | Demo Reliability | Interview Value | Future Migration | AI-Agent Friendliness | Secret Risk | Env Compatibility |
|---|--------|----------------|-----------|-----------------|----------------|------------------|-----------------------|-------------|-------------------|
| A | **In-memory seed** (Dictionary / List in DI) | Fastest — no config | Lowest | High (no external dep) | Medium (explains layering) | Easy: swap service layer | Highest (no connection string, no port) | None | Any OS, CI, offline |
| B | SQLite (file-based) | Fast | Low | Medium (file path issues in CI) | Medium-low | Straightforward to SQL Server | High (single file) | Low (path only) | Windows/Linux, but watch .litedb vs EF Core provider |
| C | **SQL Server local** (`localhost,19772`) | Medium | Medium | Medium (port must be up) | High — real RDBMS, real SQL | Already on target platform | Medium (needs connection string) | Medium (password in string) | Windows-only; requires running instance |
| D | PostgreSQL container | Slow to setup | Medium-high (Docker required) | Lower (Docker on Windows can be flaky) | High | Requires DB-switch effort | Medium | Medium | Requires Docker Desktop |
| E | EF Core InMemory provider | Fast | Low | High (no external dep) | Low (not production-grade, known query limitations) | Risky (InMemory ≠ relational semantics) | High | None | Any OS |

**Summary:** Option A (in-memory seed) dominates for the P0/P1 read-only API gate. Option C (SQL Server) is the natural next step given the existing `localhost,19772` instance.

---

## 2. Path Comparison: Path 1 vs Path 2

### Path 1 — In-Memory Seed First → SQL Server Gate (RECOMMENDED)

```
Phase 1: In-memory seed → read-only Controllers → UI integration ✅
                              ↓  explicit gate: BACKEND_SQLSERVER_PERSISTENCE_GATE
Phase 2: EF Core + SQL Server → replace in-memory → same contracts
```

**Reasoning for recommendation:**

1. **Implementation-scope safety.** The first backend deliverable is unblocked by any infrastructure dependency. No SQL Server port, no EF migrations, no connection strings — just a seeded in-memory repository behind a clean `IClaimReadRepository` interface. The UI integration is tested against real HTTP responses immediately.
2. **Interface stability.** Controller contracts and DTOs are locked in Phase 1. Phase 2 (SQL Server) swaps the infrastructure layer without touching controllers, DTOs, or frontend.
3. **Demo reliability.** A portfolio demo should never fail because `localhost,19772` is down or a migration didn't run. In-memory seed has zero external dependencies.
4. **Interview value.** Explaining "I designed the persistence layer to be swappable via `IClaimReadRepository` — currently in-memory, SQL Server is gated" demonstrates layered architecture thinking, not just CRUD wiring.
5. **Agent-friendly development.** AI agents writing backend code do not need credentials, running services, or migration tooling at Phase 1.
6. **Explicit gate.** The transition to SQL Server is a deliberate, auditable decision point — not a gradual drift. This matches the governance model (advisory AI, human approval).

### Path 2 — SQL Server Immediately

**Risks:**
- EF Core setup, migrations, DbContext, connection string, provider registration all required before first API response.
- Locks demo reliability to `localhost,19772` availability.
- Significantly higher implementation scope for P0.
- Agent-driven development requires credential injection from the start.

**When Path 2 is acceptable:** Only if the explicit `BACKEND_SQLSERVER_PERSISTENCE_GATE` is reached and SQL Server readiness is confirmed (instance up, `InsuranceAIPlatform` DB created, connection verified, secrets configured in user-secrets).

---

## 3. Mandatory DB Facts

| Fact | Value |
|------|-------|
| SQL Server instance | `localhost,19772` |
| Existing DB (DO NOT TOUCH) | `DevDept` |
| Dedicated DB for this project | `InsuranceAIPlatform` |
| Approved dev alias (if justified) | `InsuranceAIPlatform_Dev` |
| DB creation authorization | **FORBIDDEN** until `BACKEND_SQLSERVER_PERSISTENCE_GATE` |
| Schema | `dbo` |
| Authentication | Windows Auth preferred; SQL Auth via user-secrets only |

---

## 4. Database Reset & Safety Strategy (Phase 18.7)

### 4.1 Allowed Reset Operations

- Truncate tables in `InsuranceAIPlatform` (verified by name assertion before execution).
- Delete all rows from seed tables in `InsuranceAIPlatform`.
- Re-run idempotent seeder (`GoldenClaimSeeder`) against `InsuranceAIPlatform`.
- Drop and recreate `InsuranceAIPlatform` database — only during local dev, only with explicit operator confirmation, only after name assertion.
- Apply EF Core migrations to `InsuranceAIPlatform` — only after `BACKEND_SQLSERVER_PERSISTENCE_GATE` is authorized.

### 4.2 Forbidden Reset Operations

- Any DDL or DML targeting `DevDept` — **absolute prohibition**.
- Any connection string that contains `DevDept` as the database name.
- Any `DROP DATABASE` command without prior name assertion guard.
- Any reset triggered by an AI agent without human confirmation (blast-radius rule).
- Any migration applied to a database not named `InsuranceAIPlatform` or approved alias.
- Any use of `master` or `tempdb` as a target for schema operations.

### 4.3 Guard Checks (Required in all future reset/seed scripts)

Every future reset or seed script — regardless of language (SQL, PowerShell, C#, bash) — MUST:

```
STEP 1: Resolve the target DB name from config (env var / user-secrets).
STEP 2: ASSERT target DB name == "InsuranceAIPlatform" OR approved alias.
         If assertion FAILS → ABORT immediately, print error, exit non-zero.
         Never fall through.
STEP 3: Log "Targeting DB: <name>" before executing any DML/DDL.
STEP 4: Execute reset/seed operations.
STEP 5: Log affected row counts per table.
```

**Example guard (pseudo-SQL):**
```sql
-- Guard: must run before any reset DML
IF DB_NAME() <> 'InsuranceAIPlatform'
BEGIN
    RAISERROR('SAFETY ABORT: Wrong database. Expected InsuranceAIPlatform, got %s', 20, 1, DB_NAME()) WITH LOG;
    RETURN;
END
```

**Example guard (C# seeder):**
```csharp
// GoldenClaimSeeder.cs — guard before any seed operation
var dbName = context.Database.GetDbConnection().Database;
if (dbName != "InsuranceAIPlatform" && dbName != "InsuranceAIPlatform_Dev")
    throw new InvalidOperationException(
        $"SAFETY ABORT: Seeder targeted wrong DB '{dbName}'. Refusing to run.");
```

### 4.4 Idempotent / Deterministic Reset Principles

- All seed data uses **stable, hardcoded IDs** (e.g., claim PK = `"CLM-1006"`, customer PK = `"CUST-001"`).
- Seeder uses `UPSERT` semantics (EF Core: `AddOrUpdate` / `ExecuteSqlRaw MERGE`) — running twice produces identical state.
- Seeder is version-stamped: a `SeedVersion` table records seeder class name + timestamp. Re-run only if version changed.
- Seeder is transaction-wrapped: rollback on any step failure — no partial state.
- No random values, no `DateTime.Now` in seed data — all timestamps are fixed ISO-8601 strings.

### 4.5 No Production Connection Strings

- No production SQL Server connection string may appear in any file in this repository.
- `appsettings.json` committed to repo shows placeholder only: `"ConnectionStrings": { "Default": "SEE_README" }`.
- Actual connection string lives in `dotnet user-secrets` (key: `ConnectionStrings:Default`) or `$env:CONNECTIONSTRINGS__DEFAULT`.

### 4.6 Proving DevDept is Untouched

| Evidence Type | How to Generate |
|---------------|-----------------|
| Script assertion | Every reset/seed script hardcodes `InsuranceAIPlatform`; connection strings never contain `DevDept` — verifiable by `grep -r "DevDept" src/` returning zero matches |
| EF Core DbContext | `InsuranceAiDbContext` targets `InsuranceAIPlatform` — no code path touches any other DB |
| Migration history | `__EFMigrationsHistory` table exists only in `InsuranceAIPlatform` — verifiable via SSMS |
| CI pipeline | Future CI runs `dotnet test` with `USE_INMEMORY=true` — no SQL Server connection at all |
| Manual SSMS check | After any reset: `SELECT name FROM sys.databases` — confirm `DevDept` is present and unmodified (unchanged `create_date`, same table count) |

---

## 5. Implementation Roadmap (Two-Phase)

```
PHASE 1 (current gate — in-memory):
  └── IClaimReadRepository (interface)
  └── InMemoryClaimRepository (implementation)
  └── GoldenClaimSeedData (static factory — CLM-1006 graph)
  └── ClaimsController (read-only)
  └── No DB, no EF, no migrations, no connection strings

PHASE 2 (BACKEND_SQLSERVER_PERSISTENCE_GATE — explicit authorization required):
  └── InsuranceAiDbContext (EF Core)
  └── IEntityTypeConfiguration per entity
  └── GoldenClaimSeeder (runtime idempotent)
  └── EF Core migrations (Infrastructure/Persistence/Migrations/)
  └── DB: InsuranceAIPlatform on localhost,19772
  └── Connection string: user-secrets / env var
```

---

## 6. References

- Schema details: [`BACKEND_SCHEMA_OUTLINE_V0.1.md`](BACKEND_SCHEMA_OUTLINE_V0.1.md)
- EF Core migration strategy: [`BACKEND_EFCORE_MIGRATION_STRATEGY_V0.1.md`](BACKEND_EFCORE_MIGRATION_STRATEGY_V0.1.md)
- Seed data plan: [`BACKEND_SEED_DATA_PLAN_V0.1.md`](BACKEND_SEED_DATA_PLAN_V0.1.md)
- API contracts: [`BACKEND_API_CONTRACTS_V0.1.md`](BACKEND_API_CONTRACTS_V0.1.md)
