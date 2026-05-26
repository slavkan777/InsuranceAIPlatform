# InsuranceAIPlatform — Backend (skeleton, v0.1)

Local-first .NET 9 backend skeleton for the Auto Insurance Claim AI Workbench.
This is a **walking skeleton only**: it builds, runs, exposes health/status endpoints,
and serves Swagger UI. There is **no database, no AI provider, and no claims API yet** —
those are planned future gates (see `docs/architecture/`).

> Synthetic data only. AI outputs are advisory; human approval is always final.

## Stack
- .NET 9 · ASP.NET Core Web API (controllers)
- Swagger / OpenAPI via Swashbuckle (Development only)
- xUnit + `WebApplicationFactory` smoke tests
- No EF Core, no SQL Server, no Azure, no AI provider (deferred to later gates)

## Structure
```
server/
  InsuranceAIPlatform.sln
  InsuranceAIPlatform.Api/        # web API
    Program.cs                    # DI, CORS, Swagger, controllers
    Controllers/                  # HealthController, SystemController
    Contracts/                    # response DTOs
    Domain/ Application/ Infrastructure/   # reserved for future gates (hybrid -> modular monolith)
  InsuranceAIPlatform.Tests/      # xUnit smoke tests
```

## Run locally
```bash
dotnet run --project server/InsuranceAIPlatform.Api
```
Server starts on `http://localhost:5284`.

| Endpoint | Purpose |
|---|---|
| `GET /health` | Liveness JSON |
| `GET /api/system/demo-status` | Synthetic demo-stage status |
| `GET /swagger` | Swagger UI (Development) |

CORS is open to the Vite dev server at `http://localhost:5173`.

## Build & test
```bash
dotnet build server/InsuranceAIPlatform.sln
dotnet test  server/InsuranceAIPlatform.sln
```

## Not in this skeleton (planned gates)
Claims read API · SQL Server persistence (dedicated DB `InsuranceAIPlatform`, never `DevDept`) ·
audit/risk placeholders · approval-draft · mock AI pipeline · real AI provider. See
`docs/architecture/BACKEND_IMPLEMENTATION_GATES_V0.1.md`.
