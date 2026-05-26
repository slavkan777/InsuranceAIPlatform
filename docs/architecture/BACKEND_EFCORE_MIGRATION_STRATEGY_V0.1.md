---
title: Backend EF Core Migration Strategy
type: knowledge
status: active
created: 2026-05-27
tags: [backend, ef-core, migrations, dbcontext, seed, sql-server]
---

# Backend EF Core Migration Strategy V0.1

> **CRITICAL GATE NOTICE:**
> EF Core DbContext, migrations, and SQL Server connections are **FORBIDDEN**
> until the explicit `BACKEND_SQLSERVER_PERSISTENCE_GATE` is authorized.
> This document is planning-only. Nothing described here is implemented yet.

---

## 1. DbContext Name

**Recommended:** `InsuranceAiDbContext`

| Option | Recommendation | Reason |
|--------|----------------|--------|
| `InsuranceAiDbContext` | **USE THIS** | Combines domain (`Insurance`) and technology layer (`Ai`) — clear, specific, interview-ready |
| `AppDbContext` | Avoid | Generic — unclear in multi-context scenarios |
| `InsuranceDbContext` | Acceptable alt | Omits AI layer distinction; use if the project scope narrows |
| `ClaimsDbContext` | Avoid for now | Too narrow — this context manages more than just claims |

**Namespace:** `InsuranceAIPlatform.Infrastructure.Persistence`

**Location:** `src/InsuranceAIPlatform.Api/Infrastructure/Persistence/InsuranceAiDbContext.cs`
(or `src/InsuranceAIPlatform.Infrastructure/Persistence/InsuranceAiDbContext.cs` if split to separate project at modular-monolith gate)

---

## 2. Entity Configuration Strategy

**Approach:** One `IEntityTypeConfiguration<TEntity>` class per entity, registered via `ApplyConfigurationsFromAssembly`.

**Rationale:**
- Keeps `OnModelCreating` in `InsuranceAiDbContext` small (one-liner scan).
- Each configuration file is independently reviewable and testable.
- Scales cleanly when modular monolith split adds feature-specific assemblies.
- Interview-friendly: "Each entity has its own configuration class — separation of concerns at the persistence layer."

**Configuration class naming:** `<EntityName>Configuration`
- `ClaimConfiguration`
- `ClaimDocumentConfiguration`
- `AiAnalysisRunConfiguration`
- `AuditEventConfiguration`
- etc.

**Location:** `Infrastructure/Persistence/Configurations/<EntityName>Configuration.cs`

**`OnModelCreating` pattern:**
```csharp
// InsuranceAiDbContext.cs — OnModelCreating
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(InsuranceAiDbContext).Assembly);
    base.OnModelCreating(modelBuilder);
}
```

**Configuration conventions per entity type:**

| Entity Type | Convention |
|-------------|------------|
| String PKs (CLM-*, POL-*, CUST-*) | `.HasMaxLength(30).IsRequired()` on PK; no value generation |
| NVARCHAR columns | Explicit `.HasMaxLength(N)` on all string properties — never leave EF to default `nvarchar(max)` |
| DECIMAL money | `.HasPrecision(12, 2)` on all currency columns |
| DECIMAL cost (USD, small) | `.HasPrecision(10, 6)` for `CostUsd` columns |
| DATETIME2 timestamps | `.HasColumnType("datetime2")` — EF Core default on SQL Server, but declare explicitly |
| Enums stored as string | `.HasConversion<string>()` — readable in SSMS, no `int` mystery values |
| Nullable FKs | `.IsRequired(false)` + `DeleteBehavior.SetNull` |
| Required FKs | `DeleteBehavior.Restrict` (never Cascade on claim-domain FKs — preserves audit integrity) |

---

## 3. Migrations Location

**Path:** `Infrastructure/Persistence/Migrations/`

Within the hybrid single-project layout:
```
src/InsuranceAIPlatform.Api/
  Infrastructure/
    Persistence/
      InsuranceAiDbContext.cs
      Configurations/
        ClaimConfiguration.cs
        ...
      Migrations/
        <timestamp>_InitialCreate.cs
        <timestamp>_InitialCreate.Designer.cs
        InsuranceAiDbContextModelSnapshot.cs
      Seed/
        GoldenClaimSeeder.cs
        SeedVersion.cs
```

If project is later split to `InsuranceAIPlatform.Infrastructure` project, migrations move there with no structural change.

---

## 4. Design-Time Factory

**Verdict: Yes — required.**

Without a design-time factory, `dotnet ef migrations add` requires `Program.cs` to be runnable, which in turn requires a valid connection string in the environment. The factory isolates migration tooling from the runtime startup path.

**Class name:** `InsuranceAiDbContextFactory`
**Implements:** `IDesignTimeDbContextFactory<InsuranceAiDbContext>`
**Location:** `Infrastructure/Persistence/InsuranceAiDbContextFactory.cs`

**Behavior:**
- Reads connection string from `$env:CONNECTIONSTRINGS__DEFAULT` first.
- Falls back to `appsettings.Development.json` (gitignored file — not committed).
- Never reads `appsettings.json` (the committed file shows only `"SEE_README"` placeholder).
- Asserts DB name from resolved connection string matches `InsuranceAIPlatform` or approved alias before returning context — same guard as reset scripts.

**Pattern (planning sketch — not implementation):**
```csharp
// InsuranceAiDbContextFactory.cs
public class InsuranceAiDbContextFactory : IDesignTimeDbContextFactory<InsuranceAiDbContext>
{
    public InsuranceAiDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("CONNECTIONSTRINGS__DEFAULT")
            ?? ReadFromDevAppSettings();  // gitignored file

        // Guard: never target DevDept or unrecognized DB
        AssertSafeDatabase(connectionString);

        var options = new DbContextOptionsBuilder<InsuranceAiDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new InsuranceAiDbContext(options);
    }
}
```

---

## 5. Seed Strategy: `HasData` vs Runtime Idempotent Seeder

**Recommendation: Runtime idempotent seeder (`GoldenClaimSeeder`).**

### Comparison

| Aspect | `HasData` (EF Core built-in) | Runtime Idempotent Seeder |
|--------|------------------------------|---------------------------|
| Location of seed data | Inside `IEntityTypeConfiguration` / `OnModelCreating` | Separate `GoldenClaimSeeder` class |
| Execution trigger | Only during migrations | On app startup or explicit call |
| Seed data in migrations | YES — baked into migration SQL | NO — migration schema only |
| Updating seed data | Requires new migration for every data change | Edit seeder class, re-run |
| Complex graph seeding | Awkward — FK ordering constraints | Natural — seeder controls insertion order |
| Navigation properties | Not supported | Fully supported |
| Idempotency | Via migration history | Via explicit "exists?" check before insert |
| CLM-1006 graph suitability | **Poor** — 23 tables, 6 audit events, 4 cost rows — very unwieldy | **Excellent** — seeder traverses the graph naturally |
| Interview value | Explains EF Core seeding mechanics | Demonstrates domain understanding + infrastructure design |

**Reasoning for runtime seeder:**

The CLM-1006 graph spans 23 tables with FK ordering dependencies, bidirectional relationships (e.g., `AuditTrace.RunId` nullable), and rich sub-collections (6 `AuditEvents`, 5 `RiskFactors`, 5 `PolicyCoverages`). Encoding this into `HasData` produces unwieldy migration SQL that is fragile to update and hard to review. A `GoldenClaimSeeder` class reads naturally, encodes the domain story of CLM-1006, is reviewable by non-DB engineers, and maps 1:1 to the frontend mock data.

**Idempotency guarantee:**
```
Before inserting any entity: check if entity with that stable ID already exists.
If exists → skip (no update, preserves any manual adjuster decisions).
If not exists → insert.
Wrap entire seed operation in a transaction.
Record seeder class name + hash of seed data version in SeedVersion table.
```

**Execution modes:**
1. **App startup** (development only) — `app.Services.GetRequiredService<GoldenClaimSeeder>().SeedAsync()` in `Program.cs` behind `if (app.Environment.IsDevelopment())`.
2. **CLI command** (future) — `dotnet run --seed` or EF Core `dotnet ef database update --seed`.
3. **Never in production** — seeder is wired only in Development environment.

---

## 6. Connection String Source

**Sources (priority order):**

| Priority | Source | Notes |
|----------|--------|-------|
| 1 | `dotnet user-secrets` (key: `ConnectionStrings:Default`) | Recommended for local dev — not tracked by git |
| 2 | Environment variable `CONNECTIONSTRINGS__DEFAULT` | For CI/Docker; never hard-code value |
| 3 | `appsettings.Development.json` (gitignored) | Dev convenience fallback — added to `.gitignore` |
| 4 | `appsettings.json` (committed) | Shows **placeholder only**: `"SEE_README"` — never a real value |

**What is committed to the public repo:**
```json
// appsettings.json (committed — placeholder only)
{
  "ConnectionStrings": {
    "Default": "SEE_README_FOR_LOCAL_SETUP"
  }
}
```

**What is in README (public — no secrets):**
```
## Local Database Setup
1. Run: dotnet user-secrets set "ConnectionStrings:Default" "Server=localhost,19772;Database=InsuranceAIPlatform;Integrated Security=true;"
   (Windows Auth — no password required)
   OR set env var: $env:CONNECTIONSTRINGS__DEFAULT = "..."
2. Run migrations: dotnet ef database update (only after BACKEND_SQLSERVER_PERSISTENCE_GATE)
```

**Never in the repo:**
- Real passwords
- SQL Auth connection strings with `User Id=` / `Password=`
- Any string containing `DevDept`

---

## 7. Migration Naming Convention

Pattern: `<PascalCaseDescription>` (EF Core tooling prepends timestamp automatically)

```
<20260527120000>_InitialCreate                    ← All 23 tables, schema only
<20260527130000>_SeedVersionTable                 ← SeedVersion tracking table
<20260527140000>_AddClaimIndexes                  ← Performance indexes
<YYYYMMDDHHMMSS>_<DescriptiveActionInPascalCase>
```

**Rules:**
- Describe WHAT changes, not HOW (`AddRiskFactorContributionColumn` not `Migration20260527`)
- Never edit a migration after it has been applied to any shared/test environment
- Never delete a migration from the Migrations folder (use `dotnet ef migrations remove` before applying)
- Migration must be reviewed by a human before `dotnet ef database update` is run

---

## 8. Rollback / Reset Strategy

### Schema Rollback

```
Downgrade one migration: dotnet ef database update <previous-migration-name>
Downgrade to empty:      dotnet ef database update 0
```

Both commands require the guard assertions (DB name == `InsuranceAIPlatform`) — enforce in `InsuranceAiDbContextFactory`.

### Data Reset (Dev Only)

```
1. Assert DB name == InsuranceAIPlatform
2. dotnet ef database update 0     ← drops all schema
3. dotnet ef database update        ← re-applies all migrations
4. GoldenClaimSeeder.SeedAsync()    ← re-seeds CLM-1006 graph
```

Or via future PowerShell helper script (`scripts/reset-dev-db.ps1`) which must:
1. Resolve connection string from user-secrets or env var.
2. Assert DB name before any destructive command.
3. Log each step.
4. Report affected DB name + row counts.

### No Reset Without Guard

Any reset path that does NOT include the DB name assertion is considered broken and must not be merged. PR checklist includes: "Does reset script assert DB name?"

---

## 9. Gate Authorization Summary

| Action | Current Status | Authorization Required |
|--------|---------------|----------------------|
| Design this document | Allowed | — |
| Write EF Core entity classes | ALLOWED at Phase 1 (in-memory) | — |
| Write `InsuranceAiDbContext` | **FORBIDDEN** | `BACKEND_SQLSERVER_PERSISTENCE_GATE` |
| Write `IEntityTypeConfiguration` files | **FORBIDDEN** | `BACKEND_SQLSERVER_PERSISTENCE_GATE` |
| `dotnet ef migrations add` | **FORBIDDEN** | `BACKEND_SQLSERVER_PERSISTENCE_GATE` |
| `dotnet ef database update` | **FORBIDDEN** | `BACKEND_SQLSERVER_PERSISTENCE_GATE` |
| Write `GoldenClaimSeeder` | **FORBIDDEN** | `BACKEND_SQLSERVER_PERSISTENCE_GATE` |
| Connect to `localhost,19772` | **FORBIDDEN** | `BACKEND_SQLSERVER_PERSISTENCE_GATE` |
| Create `InsuranceAIPlatform` DB | **FORBIDDEN** | `BACKEND_SQLSERVER_PERSISTENCE_GATE` |

---

## 10. References

- Persistence plan: [`BACKEND_PERSISTENCE_AND_DATABASE_PLAN_V0.1.md`](BACKEND_PERSISTENCE_AND_DATABASE_PLAN_V0.1.md)
- Schema outline: [`BACKEND_SCHEMA_OUTLINE_V0.1.md`](BACKEND_SCHEMA_OUTLINE_V0.1.md)
- Seed data plan: [`BACKEND_SEED_DATA_PLAN_V0.1.md`](BACKEND_SEED_DATA_PLAN_V0.1.md)
