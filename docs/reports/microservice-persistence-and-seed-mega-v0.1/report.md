# Microservice Persistence & Seed (Mega) — V0.1 — Report

**Gate:** `MICROSERVICE_PERSISTENCE_AND_SEED_MEGA_V0.1` · **Date:** 2026-05-27
**Type:** bounded implementation — local EF Core persistence + deterministic synthetic seed behind service-owned boundaries. No writes/AI/Azure/secrets/commit/push.

## Current state
- Source: branch `dev` @ `fed2bc4`; `origin/main` `69e6731` untouched. **No commit this gate** — HEAD still `fed2bc4`; changes uncommitted in the working tree.
- DB: local SQL Server **LocalDB** `(localdb)\MSSQLLocalDB`, database `InsuranceAIPlatform` created + migrated + seeded.

## What this gate produced
EF Core 9 persistence behind the six service boundaries: one DB, six service-owned schemas + DbContexts (no shared god context), six independent migrations, a dev-only migrator/seeder, deterministic synthetic seed (exactly 200 test users + demo data incl. `CLM-1006`), and 18 new tests. Frontend/BFF read contract preserved unchanged (Option A).

## Persistence shape
| Service | Schema | DbContext | Key entities |
|---|---|---|---|
| Customers & Policies | `customers_policies` | `CustomersPoliciesDbContext` | SyntheticCustomer, Policy, Vehicle |
| Claims | `claims` | `ClaimsDbContext` | Claim, ClaimStatusHistory |
| Documents | `documents` | `DocumentsDbContext` | ClaimDocument |
| Approval | `approval` | `ApprovalDbContext` | ApprovalDraft, ApprovalDecisionOption |
| Audit & Cost | `audit_cost` | `AuditCostDbContext` | AuditEvent, CostTrace, TokenUsageTrace |
| AI Analysis | `ai_analysis` | `AiAnalysisDbContext` | AiAnalysisRun, AiFinding, AiEvidenceReference, AiRiskSignal |

Dependency direction preserved: `Api → Services.* → BuildingBlocks`; services never reference each other; cross-service refs are id-only; BFF owns no DbContext. EF (`Microsoft.EntityFrameworkCore.SqlServer` + `.Design` 9.0.0) added only to the six service projects; `Microsoft.EntityFrameworkCore.InMemory` 9.0.0 added to Tests.

## Migrations
Six independent initial migrations (one per service project): `Initial{CustomersPolicies,Claims,Documents,Approval,AuditCost,AiAnalysis}Persistence`. Each schema has its own `__EFMigrationsHistory` (verified). No monolithic migration.

## Seed + verification (live, LocalDB)
Dev-only `InsuranceAIPlatform.DbMigrator` applies all six migrations + runs six idempotent seeders.
- **Synthetic test users: EXACTLY 200** — `SELECT COUNT(*) FROM customers_policies.SyntheticCustomers WHERE Id LIKE 'CUST-T%'` → **200** (`CUST-T0001..0200`). Table total = 201 (200 + golden customer `CUST-4421`).
- **Golden claim:** `claims.Claims` contains `CLM-1006` (status «В роботі», payout 1800.00); 15 claims total across statuses.
- Documents 14 · ApprovalDrafts 1 + options 4 · AuditEvents 6 + CostTraces 4 + TokenUsage 1 · AiAnalysisRuns 1 (`ProviderMode=Disabled`) + Findings 3 + Evidence 2 + RiskSignals 4.
- No real PII: emails `testuser001@example.invalid`.

## Verification
| Check | Result |
|---|---|
| Backend build (solution) | **0 warnings, 0 errors** |
| Backend tests | **53 passed / 0 failed / 0 skipped** (35 prior + 18 new) |
| Frontend build | **107 modules**, PASS (no `src/**` change) |
| Live DB apply + seed | PASS (DB created, 6 migrations applied, seed run) |
| 200-count | PASS (exact, via sqlcmd) |
| CLM-1006 | present |
| Live smoke (`:5285`) | `/health`, `/api/bff/health`, `/api/bff/demo-status`, `/api/claims`, `/api/claims/CLM-1006`, `/api/claims/summary`, `/api/demo/scenario` → all **200**; CLM-1006 unchanged; correlation + `X-Bff` headers present; API boots without a DB |
| Write endpoints | **0** |
| Provider/Azure/AI SDK packages | none |
| DevDept | not referenced |
| Secrets | integrated-auth connection (no password); not in prod appsettings; `DEEPSEEK_API_KEY` never read |
| Source commit/push | none; `main` untouched (`69e6731`); HEAD `fed2bc4` |

## Files
- **Modified (12):** `appsettings.Development.json` (non-secret connection string), the 6 service `.csproj` (EF packages), `CustomersPoliciesService.cs` + `ICustomersPoliciesService.cs` (new DB-optional `CountSyntheticCustomersAsync`), `Tests.csproj` (EF InMemory), `ServiceSkeletonTests.cs` (EF-absence guard narrowed to BuildingBlocks+Api), `InsuranceAIPlatform.sln` (DbMigrator added).
- **New:** `BuildingBlocks/{IClock,SeedConstants}.cs`; per service a `Persistence/` folder + `Migrations/` folder; `InsuranceAIPlatform.DbMigrator/`; `Tests/PersistenceSeedTests.cs` (18 tests). 18 migration `.cs` files (6 migrations × migration/Designer/snapshot).

## Read compatibility (Option A)
Existing read routes + `/api/bff/health` unchanged (still in-memory) → frontend contract byte-identical (live-smoke confirmed). Read migration onto the DB deferred to a later gate to avoid contract drift; persistence proven via migrator + tests.

## Next safe step
`COMMIT_AND_PUSH_DEV_MICROSERVICE_PERSISTENCE_AND_SEED_ONLY` — commit exactly this persistence/seed scope to `dev`, fast-forward push `dev` only; `main` untouched; no force.
