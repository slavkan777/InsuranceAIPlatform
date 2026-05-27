# Microservice Persistence & Seed — Implementation V0.1

**Gate:** `MICROSERVICE_PERSISTENCE_AND_SEED_MEGA_V0.1` · **Branch:** `dev` @ `fed2bc4` (no commit in this gate) · **Date:** 2026-05-27
**Type:** bounded implementation — local EF Core persistence + deterministic synthetic seed behind service-owned boundaries. No writes/commands, no AI provider calls, no Azure, no `src/**` change, no source commit/push.
**Status:** implemented; live DB apply + seed + verification all PASS.

## Status / purpose
Introduce local-first, service-owned persistence behind the existing BFF: one local DB, six service-owned schemas + DbContexts (no shared god context), EF Core migrations, and a deterministic synthetic seed including exactly 200 synthetic test users and demo data (with the `CLM-1006` golden claim preserved). The existing frontend/BFF read contract is preserved unchanged (Option A); persistence is additive and exercised by a dev-only migrator + tests, ready for a later gate to migrate reads onto it.

## Source state before implementation
`dev` @ `fed2bc4` (`feat: add microservice service skeletons`); `origin/main` `69e6731` untouched. 7 class-lib skeletons behind the BFF; reads served by `InMemoryClaimReadService`; build PASS, 35 tests PASS, frontend PASS.

## Persistence strategy
Local-first microservice persistence. Physical: one local SQL Server **LocalDB** instance (`(localdb)\MSSQLLocalDB`), one database `InsuranceAIPlatform`. Logical: six service-owned schemas, each with its own DbContext and its own `__EFMigrationsHistory` table (independent migration lineage per service — verified). No shared all-entity DbContext; no cross-service `DbSet`; cross-service references are id-only strings (e.g. `Claim.CustomerId`), never navigation properties across contexts. The BFF owns no DbContext and performs no direct DB access (its composition root was not modified).

## DB name and safety
Database name: `InsuranceAIPlatform`. Connection uses **Windows integrated auth** (`Trusted_Connection=True`) — there is **no password**, so the connection string is not a secret; it lives only in `appsettings.Development.json` (prod `appsettings.json` has none). An env override is supported: `ConnectionStrings__InsuranceAIPlatform` / `INSURANCEAI_CONNECTION_STRING`. DevDept is never referenced (verified); other LocalDB databases (DiscussHub, EventHubDb) are untouched.

## Service-owned schemas / DbContexts
| Service | Schema | DbContext |
|---|---|---|
| Customers & Policies | `customers_policies` | `CustomersPoliciesDbContext` |
| Claims | `claims` | `ClaimsDbContext` |
| Documents | `documents` | `DocumentsDbContext` |
| Approval | `approval` | `ApprovalDbContext` |
| Audit & Cost | `audit_cost` | `AuditCostDbContext` |
| AI Analysis | `ai_analysis` | `AiAnalysisDbContext` |

Each service ships (under `Persistence/`): its entities, the DbContext (`HasDefaultSchema`), an `IDesignTimeDbContextFactory<T>` (env-or-default connection), a `<Svc>Seeder`, and an `Add<Svc>Persistence(connectionString)` DI extension (the prior `Add<Svc>ServiceSkeleton()` health extension is unchanged). Dependency direction preserved: `Api → Services.* → BuildingBlocks`; services never reference each other.

## Entity map
- **customers_policies:** `SyntheticCustomer`, `Policy`, `Vehicle`.
- **claims:** `Claim`, `ClaimStatusHistory`.
- **documents:** `ClaimDocument`.
- **approval:** `ApprovalDraft`, `ApprovalDecisionOption`.
- **audit_cost:** `AuditEvent`, `CostTrace`, `TokenUsageTrace`.
- **ai_analysis:** `AiAnalysisRun`, `AiFinding`, `AiEvidenceReference`, `AiRiskSignal`.

## Migrations
Six independent initial migrations, one per service project (`<Service>/Migrations/`), via each service's design-time factory: `InitialCustomersPoliciesPersistence`, `InitialClaimsPersistence`, `InitialDocumentsPersistence`, `InitialApprovalPersistence`, `InitialAuditCostPersistence`, `InitialAiAnalysisPersistence`. No shared monolithic migration.

## Seed strategy
Deterministic, synthetic, idempotent. A dev-only console `InsuranceAIPlatform.DbMigrator` (references the six services; not a web host) applies all six migrations then runs the six idempotent seeders. Synthetic markers only: emails `testuserNNN@example.invalid`, names `Synthetic Customer NNN`, synthetic VINs. No production startup seeding; the BFF/test host never seeds or queries the DB.

## 200 synthetic users verification
`SELECT COUNT(*) FROM customers_policies.SyntheticCustomers WHERE Id LIKE 'CUST-T%'` → **200** (exact, IDs `CUST-T0001..CUST-T0200`). The table holds **201 rows total** = the 200 synthetic test users **+ the `CLM-1006` golden-claim customer `CUST-4421`** (Роберт Джонсон), seeded so the golden claim's customer/vehicle context exists. Sample emails confirmed `testuser001@example.invalid` (no real PII).

## Golden claim CLM-1006
Preserved exactly: `claims.Claims` contains `CLM-1006` (Роберт Джонсон / CUST-4421 / Toyota Camry 2021 / POL-2025-AC-4421 / status «В роботі» / RiskScore 82 / recommended payout 1800.00). 15 claims total (CLM-1006..1010 + 10 synthetic) spanning statuses for dashboard variety. Documents (7 for CLM-1006), approval draft, audit timeline (6 events) + cost rows, and AI placeholders (run with `ProviderMode=Disabled`, findings/evidence/risk) all seeded for CLM-1006.

## BFF / read compatibility (Option A)
The existing 13 read routes + 2 additive BFF endpoints + `/api/bff/health` are **unchanged** and still served by `InMemoryClaimReadService` — guaranteeing byte-identical responses (live smoke confirmed `CLM-1006` → status «В роботі», payout 1800.00, correlation + `X-Bff` headers). Reads were intentionally **not** migrated onto the DB this gate (deferred) to eliminate contract-drift risk; the persistence layer is proven by the live migrator/seed + tests and is ready for a later read-ownership gate. The BFF composition root was not modified, so the in-process test host and all read routes remain DB-free.

## Tests added / updated
`PersistenceSeedTests.cs` — 18 new tests (EF-InMemory): CustomersPolicies seeder yields exactly 200 synthetic users with `@example.invalid` emails; Claims seeder yields `CLM-1006` with golden field values + status variety; per-DbContext schema config; entity types not shared across contexts. The existing `ServiceSkeletonTests` boundary/EF-absence guard was narrowed to assert **BuildingBlocks + Api** carry no EF (services now legitimately own EF). The 35 pre-existing tests stay green and DB-free. **Total: 53 passed / 0 failed.**

## Live DB apply / seed result
Applied (LocalDB available). `dotnet run --project server/InsuranceAIPlatform.DbMigrator` created `InsuranceAIPlatform`, applied all six migrations (six `__EFMigrationsHistory` tables, one per schema), and seeded: SyntheticCustomers 201 (200 `CUST-T*` + CUST-4421), Policies 201, Vehicles 201, Claims 15, ClaimDocuments 14, ApprovalDrafts 1 + options 4, AuditEvents 6 + CostTraces 4 + TokenUsageTraces 1, AiAnalysisRuns 1 + Findings 3 + EvidenceReferences 2 + RiskSignals 4. Re-runnable (idempotent).

## Safety scan
No write endpoints (`[HttpPost/Put/Patch/Delete]` = 0); no provider/Azure/AI SDK packages; no DevDept reference; AI runs `ProviderMode=Disabled` only (no provider call); `DEEPSEEK_API_KEY` never read/printed/logged; connection string integrated-auth (no password) and not in prod appsettings; no real PII (synthetic `@example.invalid`); no `src/**` change; no source commit/push; `main` untouched (`69e6731`); HEAD still `fed2bc4`.

## Deferred work (later gates)
Migrate BFF reads onto the service-backed DB (response-identical), then write/command endpoints + transactional outbox/events + audit append, then AI Analysis provider (mock default; DeepSeek opt-in/disabled). Azure mapping (Container Apps + per-service Azure SQL + Key Vault + App Insights) deferred throughout.

## Next gate
`COMMIT_AND_PUSH_DEV_MICROSERVICE_PERSISTENCE_AND_SEED_ONLY` — commit exactly this persistence/seed scope to `dev`, fast-forward push `dev` only; `main` untouched; no force.

## Stop boundaries
Persistence + seed only. No commit/push, no write commands, no AI provider calls, no Azure, no DevDept, no real PII, no secrets, no `main` change, no merge, no force push.
