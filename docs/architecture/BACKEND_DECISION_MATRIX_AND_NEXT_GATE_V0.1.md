---
title: Backend Decision Matrix and Next Gate V0.1 — InsuranceAIPlatform
type: knowledge
status: active
created: 2026-05-27
tags: [architecture, backend, decisions, next-gate, implementation-spec]
---

# Backend Decision Matrix and Next Gate V0.1

**Part of the backend architecture plan.**  
Entry point: [`DOTNET_BACKEND_SKELETON_PLAN_V0.1.md`](./DOTNET_BACKEND_SKELETON_PLAN_V0.1.md)  
Gate decomposition: [`BACKEND_IMPLEMENTATION_GATES_V0.1.md`](./BACKEND_IMPLEMENTATION_GATES_V0.1.md)

---

## Phase 18.1 — Final Decision Matrix

Every row has exactly one **Chosen** value. This plan is authoritative — no open options remain below.

| Decision | Options Compared | **Chosen** | Why | Deferred / Later |
|---|---|---|---|---|
| **.NET version** | .NET 8 LTS (8.0.421) vs .NET 9 Current (9.0.304) | **.NET 9** | Installed; current/interview-strong; no production SLA requirement on LTS | .NET 8 LTS = 1-line fallback (`<TargetFramework>net8.0</TargetFramework>`) if recruiter requires LTS |
| **API style** | Controllers (attribute-routed) vs Minimal APIs | **Controllers** | Scales for 13+ endpoints + future CRUD; tag grouping in Swagger; universal interview familiarity; cleaner action-method separation | Minimal APIs noted as lighter alternative for truly micro projects; not applicable here |
| **Backend structure** | Option A (flat single project) vs Option B (full multi-project modular monolith) vs Option C (hybrid: single Api project + internal folders + separate Tests project) | **Option C Hybrid** | Lowest impl cost for skeleton gate; internal folders enforce logical separation; documented migration path to Option B at persistence and AI gates; 2 projects only (Api + Tests) | Option B multi-project extraction triggered at: `BACKEND_SQLSERVER_PERSISTENCE_GATE` (Infrastructure project), `BACKEND_AUDIT_RISK_PLACEHOLDERS` (Application project), `MOCK_AI_PIPELINE_IMPLEMENTATION` (Domain project) |
| **Persistence path** | In-memory seed from start vs SQL Server from start vs in-memory first then SQL at explicit gate | **In-memory seed first; SQL Server at explicit gate** | Read-only first slice has no DB risk; skeleton gate is completable without SQL dependency; `BACKEND_SQLSERVER_PERSISTENCE_GATE` is explicit trigger for SQL Server | EF Core + SQL Server at `BACKEND_SQLSERVER_PERSISTENCE_GATE`; `localhost,19772` is the target instance |
| **DB name** | `InsuranceAIPlatform` vs reuse `DevDept` vs any other name | **InsuranceAIPlatform** | Dedicated DB per product; `DevDept` is a live portfolio asset — contamination unacceptable; professional boundary awareness | Schema = `dbo`; `DevDept` is NEVER touched |
| **DTO style** | Expose EF entities directly vs screen-friendly read DTOs vs domain DTOs | **Screen-friendly read DTOs for P0** | EF entity shape is a persistence concern; API contract is a product concern; decoupling enables schema evolution without breaking contracts | Domain DTOs and command/result objects at P1/P2 when write endpoints added |
| **Mock-to-real strategy** | Replace mock file entirely vs adapter/config switch with fallback | **Adapter / config switch + fallback** | `mockInsuranceApi.ts` stays intact; real `ApiClient` implements same signatures; config picks mock vs backend; swap P0 reads first, writes later; mock always available as fallback | Full mock removal = post-integration gate decision |
| **Swagger strategy** | No Swagger vs Swagger as portfolio artifact | **Swagger as documented portfolio artifact** | Swagger UI = recruiter/reviewer entry point; tag groups match product flow screens; synthetic + AI-advisory disclaimers on sensitive endpoints; P0 examples inline | Production Swagger security (bearer token, redoc) = deferred to auth gate |
| **First implementation gate scope** | Skeleton only (health + demo-status + Swagger + CORS) vs skeleton + all P0 APIs vs skeleton + DB | **Skeleton only** | Completable in one bounded Claude run; build+test+health verifiable immediately; reviewable in 10 min; discipline signal; all-APIs + DB = separate gates | P0 APIs = Gate 2 (`BACKEND_READ_ONLY_CLAIMS_API`); DB = Gate 4 (`BACKEND_SQLSERVER_PERSISTENCE_GATE`) |

---

## Phase 18.2 — Exact Next Implementation Gate Spec

### Next Implementation Gate: DOTNET_BACKEND_SKELETON_IMPLEMENTATION_V0.1

---

#### GOAL

Create the `server/` folder with a hybrid .NET 9 project structure, a working `/health` endpoint, Swagger UI, CORS configured for the Vite dev server (`:5173`), and a `GET /api/system/demo-status` endpoint returning a synthetic JSON payload. The gate ends when build + test pass and all three endpoints are reachable. No DB, no full API surface, no frontend integration.

---

#### DONE STATE

All of the following must be true simultaneously before this gate is reported as complete:

- [ ] `dotnet build server/InsuranceAIPlatform.sln` exits 0 with 0 errors, 0 warnings (or only expected warnings documented)
- [ ] `dotnet test server/InsuranceAIPlatform.sln` exits 0 (at minimum 1 passing test — smoke test of health endpoint)
- [ ] `GET http://localhost:{port}/health` → HTTP 200
- [ ] `GET http://localhost:{port}/api/system/demo-status` → HTTP 200 + valid JSON (see example below)
- [ ] Swagger UI loads at `http://localhost:{port}/swagger` → HTTP 200, page renders
- [ ] CORS header present: `Access-Control-Allow-Origin: http://localhost:5173` on API responses
- [ ] No secrets in any tracked file (`git grep` for known patterns returns 0 matches)
- [ ] `src/**` unchanged (zero diffs in `src/`)
- [ ] No migrations folder created
- [ ] No Azure / AI provider package referenced in any `.csproj`
- [ ] `DevDept` DB untouched (connection never opened)

---

#### IN SCOPE

- `server/` folder creation
- `InsuranceAIPlatform.sln` solution file
- `InsuranceAIPlatform.Api` project (Option C Hybrid — single project, internal folders)
- `InsuranceAIPlatform.Tests` project (xUnit, minimum 1 smoke test)
- `Controllers/SystemController.cs` (or equivalent minimal file) with `GET /api/system/demo-status`
- `/health` endpoint (built-in ASP.NET Core health checks middleware or minimal inline)
- Swashbuckle.AspNetCore Swagger UI, tag groups, disclaimer XML comment
- CORS policy: allow `http://localhost:5173`
- `appsettings.json` (non-secret only: app name, version, environment placeholder)
- `Properties/launchSettings.json` (local dev port assignment)
- `server/README.md` (optional but recommended: how to run, port, Swagger URL)
- Internal folder structure: `Domain/`, `Application/`, `Infrastructure/`, `Contracts/` (empty stubs with `.gitkeep` or placeholder class)

---

#### OUT OF SCOPE

- Any P0 GET endpoints beyond `/api/system/demo-status` and `/health`
- EF Core, DbContext, any DB connection
- SQL Server or any external DB
- Any AI provider SDK
- Azure SDK
- Auth / JWT / identity
- Frontend changes (`src/**`)
- POST / PUT / DELETE endpoints
- Real data or real PII
- Deployment config or CI pipeline

---

#### FORBIDDEN (absolute stop conditions)

```
src/**                             — never touch UI in this gate
**/Migrations/**                   — no DB migrations ever in skeleton gate
appsettings.Production.json        — no production config
.env (tracked)                     — no tracked env files
Any real API key, token, password  — zero tolerance
Microsoft.EntityFrameworkCore.*    — deferred to persistence gate
Azure.*                            — deferred to Azure gate
OpenAI, Azure.AI, or similar       — deferred to AI gate
DevDept DB                         — forbidden to reference
C:\Projects\Twincore-framework\**  — read-only at most; zero writes
```

---

#### PRESERVE (must remain unchanged)

```
src/**                             — all React/TS/Vite frontend files
src/api/mockInsuranceApi.ts        — mock seam stays intact
package.json, package-lock.json    — no npm changes
vite.config.ts                     — frontend config unchanged
.gitignore (existing entries)      — don't remove existing ignores
Any existing docs/                 — only add, never remove
Accepted commit 69e6731 state in main branch
```

---

#### EXACT FILE TARGETS

```
server/
  InsuranceAIPlatform.sln
  InsuranceAIPlatform.Api/
    InsuranceAIPlatform.Api.csproj          ← net9.0, Nullable=enable, ImplicitUsings=enable
    Program.cs                              ← DI, CORS, Swagger, health, controllers
    appsettings.json                        ← non-secret: AppName, Version, Environment placeholder
    Properties/
      launchSettings.json                   ← local dev port (e.g. 5284 http)
    Controllers/
      SystemController.cs                   ← [Route("api/system")] [Tags("System")] GET demo-status
    Domain/
      .gitkeep                              ← or placeholder class with namespace comment
    Application/
      .gitkeep
    Infrastructure/
      .gitkeep
    Contracts/
      DemoStatusResponse.cs                 ← simple record DTO for demo-status response
  InsuranceAIPlatform.Tests/
    InsuranceAIPlatform.Tests.csproj        ← net9.0, xUnit, project ref to Api
    SystemControllerSmokeTests.cs           ← minimum: WebApplicationFactory GET /health → 200
  README.md                                 ← optional but recommended
```

---

#### STEP-BY-STEP IMPLEMENTATION MAP

1. **Create solution + projects**
   ```powershell
   cd C:\Projects\InsuranceAIPlatform
   mkdir server
   cd server
   dotnet new sln -n InsuranceAIPlatform
   dotnet new webapi -n InsuranceAIPlatform.Api --use-controllers --no-openapi false
   dotnet new xunit -n InsuranceAIPlatform.Tests
   dotnet sln add InsuranceAIPlatform.Api\InsuranceAIPlatform.Api.csproj
   dotnet sln add InsuranceAIPlatform.Tests\InsuranceAIPlatform.Tests.csproj
   dotnet add InsuranceAIPlatform.Tests\InsuranceAIPlatform.Tests.csproj reference InsuranceAIPlatform.Api\InsuranceAIPlatform.Api.csproj
   ```

2. **Configure `.csproj`** — ensure `<TargetFramework>net9.0</TargetFramework>`, `<Nullable>enable</Nullable>`, `<ImplicitUsings>enable</ImplicitUsings>`. Add `Swashbuckle.AspNetCore` to Api project. Add `Microsoft.AspNetCore.Mvc.Testing` to Tests project.

3. **`Program.cs`** — add:
   - `builder.Services.AddControllers()`
   - `builder.Services.AddEndpointsApiExplorer()`
   - `builder.Services.AddSwaggerGen(...)` with title "InsuranceAIPlatform API V0.1", description including synthetic + AI-advisory disclaimer
   - `builder.Services.AddCors(...)` with policy `"ViteDevServer"` allowing `http://localhost:5173`
   - `builder.Services.AddHealthChecks()`
   - `app.UseSwagger()` + `app.UseSwaggerUI()`
   - `app.UseCors("ViteDevServer")`
   - `app.MapControllers()`
   - `app.MapHealthChecks("/health")`

4. **`Contracts/DemoStatusResponse.cs`** — simple record:
   ```csharp
   namespace InsuranceAIPlatform.Api.Contracts;
   public record DemoStatusResponse(
       string Status,
       string Version,
       string Environment,
       string GoldenClaim,
       string Message,
       DateTimeOffset GeneratedAt
   );
   ```

5. **`Controllers/SystemController.cs`** — attribute-routed controller:
   ```csharp
   [ApiController]
   [Route("api/system")]
   [Tags("System")]
   public class SystemController : ControllerBase
   {
       [HttpGet("demo-status")]
       [ProducesResponseType<DemoStatusResponse>(StatusCodes.Status200OK)]
       public IActionResult GetDemoStatus() =>
           Ok(new DemoStatusResponse(
               Status: "operational",
               Version: "0.1.0",
               Environment: "local-dev",
               GoldenClaim: "CLM-1006",
               Message: "Auto Insurance Claim AI Workbench — synthetic data only. AI outputs are advisory and do not constitute final claim decisions.",
               GeneratedAt: DateTimeOffset.UtcNow
           ));
   }
   ```

6. **Create internal folder stubs** — `Domain/`, `Application/`, `Infrastructure/` each with `.gitkeep` or a namespace comment file.

7. **`Properties/launchSettings.json`** — set `applicationUrl` to `http://localhost:5284` (or another free port; document it in server/README.md).

8. **`InsuranceAIPlatform.Tests/SystemControllerSmokeTests.cs`** — WebApplicationFactory-based smoke test:
   - Test 1: `GET /health` → 200
   - Test 2: `GET /api/system/demo-status` → 200 + `Status == "operational"`

9. **`.gitignore` additions** — ensure `bin/`, `obj/`, `*.user`, `appsettings.*.json` (except `appsettings.json` and `appsettings.Development.json` — those are tracked but must contain no secrets).

10. **Safety scan** — before any commit: `git grep -i "password\|secret\|token\|apikey\|connectionstring" server/` must return 0 sensitive values.

---

#### VERIFICATION (run in order, all must pass)

```powershell
# Step 1: Build
cd C:\Projects\InsuranceAIPlatform\server
dotnet build InsuranceAIPlatform.sln
# Expected: Build succeeded. 0 Error(s)

# Step 2: Tests
dotnet test InsuranceAIPlatform.sln
# Expected: Passed: 2, Failed: 0

# Step 3: Run
dotnet run --project InsuranceAIPlatform.Api
# Expected: Server started on http://localhost:5284

# Step 4: Health (new terminal)
curl -s -o /dev/null -w "%{http_code}" http://localhost:5284/health
# Expected: 200

# Step 5: Demo status
curl -s http://localhost:5284/api/system/demo-status
# Expected: JSON matching DemoStatusResponse shape (see example below)

# Step 6: Swagger
curl -s -o /dev/null -w "%{http_code}" http://localhost:5284/swagger/index.html
# Expected: 200

# Step 7: CORS header
curl -s -I -H "Origin: http://localhost:5173" http://localhost:5284/api/system/demo-status | Select-String "Access-Control"
# Expected: Access-Control-Allow-Origin: http://localhost:5173

# Step 8: src/ unchanged
git diff src/
# Expected: (empty — zero diffs)

# Step 9: No secrets in server/
git grep -i "password\|connectionstring\|apikey\|bearer sk-" server/
# Expected: 0 matches
```

**Expected `demo-status` response example:**
```json
{
  "status": "operational",
  "version": "0.1.0",
  "environment": "local-dev",
  "goldenClaim": "CLM-1006",
  "message": "Auto Insurance Claim AI Workbench — synthetic data only. AI outputs are advisory and do not constitute final claim decisions.",
  "generatedAt": "2026-05-27T10:00:00.000+00:00"
}
```

---

#### GITHUB HANDOFF

Report goes to: `slavkan777/gpt-handoff` repo, path `InsuranceAIPlatform/latest-report.md` (overwrite) + `InsuranceAIPlatform/runs/2026-05-27-skeleton-gate.md` (archive copy).

Magic line at end of report: **"GitHub handoff ready. Tell GPT: отчёт."**

---

#### REPORT FORMAT

The implementation report must include ALL of the following (no prose-only claims):

```
GATE: DOTNET_BACKEND_SKELETON_IMPLEMENTATION_V0.1
STATUS: COMPLETE / BLOCKED / PARTIAL

DONE STATE CHECKLIST:
[ ] dotnet build exit 0 — output: [paste last 3 lines of build output]
[ ] dotnet test exit 0 — output: Passed: N, Failed: 0
[ ] GET /health → 200 — curl output: [paste]
[ ] GET /api/system/demo-status → 200 — curl output: [paste full JSON]
[ ] Swagger UI → 200 — curl status: [paste]
[ ] CORS header present — header value: [paste]
[ ] src/ unchanged — git diff src/: [paste "(empty)" or diff]
[ ] No secrets scan — git grep output: [paste "(0 matches)"]
[ ] No Migrations/ created — ls server/**/Migrations: [paste "(not found)"]
[ ] No EF/Azure/AI packages — grep .csproj: [paste]

FILES WRITTEN:
- [list every file path written]

FILES READ (not written):
- [list]

FILES NOT TOUCHED:
- src/** ← confirmed unchanged
- DevDept DB ← confirmed no connection opened

VERIFICATION EVIDENCE:
[all 9 verification commands + outputs]

STOP CONDITIONS HIT: none / [list if any]
```

---

#### STOP LINE

**STOP immediately and report BLOCKED if any of the following occur:**

- Any write to `src/**`
- Any attempt to create `Migrations/` folder
- Any attempt to add EF Core, Azure, or AI provider NuGet packages
- Any secret, token, password, or connection string written to any tracked file
- Any attempt to push to `main`
- Any attempt to open a connection to `DevDept` DB
- Build fails and root cause requires changes outside the allowed WRITE scope
- Test fails and fix requires touching forbidden files
