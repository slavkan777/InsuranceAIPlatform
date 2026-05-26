---
title: Backend Implementation Gates V0.1 — InsuranceAIPlatform
type: knowledge
status: active
created: 2026-05-27
tags: [architecture, backend, gates, implementation, claude-scope]
---

# Backend Implementation Gates V0.1

**Part of the backend architecture plan.**  
Entry point: [`DOTNET_BACKEND_SKELETON_PLAN_V0.1.md`](./DOTNET_BACKEND_SKELETON_PLAN_V0.1.md)  
Decision matrix + next gate spec: [`BACKEND_DECISION_MATRIX_AND_NEXT_GATE_V0.1.md`](./BACKEND_DECISION_MATRIX_AND_NEXT_GATE_V0.1.md)

---

## Phase 18 — Gate Decomposition

Each gate is independently runnable, verifiable, and committable. No gate bundles the work of two gates.

### Gate Table

| Gate | Goal | Done State | Files Likely Touched | Forbidden | Verification | Handoff | Commit / Push Policy |
|---|---|---|---|---|---|---|---|
| **1. DOTNET_BACKEND_SKELETON_IMPLEMENTATION** | Create `server/` folder with hybrid project structure, health endpoint, Swagger, CORS for Vite :5173, GET /api/system/demo-status with synthetic JSON | `dotnet build` exit 0; `dotnet test` exit 0; GET /health → 200; GET /api/system/demo-status → 200 synthetic JSON; Swagger UI loads at `/swagger`; CORS allows `http://localhost:5173` | `server/InsuranceAIPlatform.sln`, `server/InsuranceAIPlatform.Api/**`, `server/InsuranceAIPlatform.Tests/**`, `server/README.md` | `src/**`, `**/Migrations/**`, `appsettings.Production.json`, `.env`, any real secret, any Azure config, any AI provider config, DevDept DB | `dotnet build; dotnet test; curl /health; curl /api/system/demo-status; open Swagger; verify CORS header` | Report: sln path, build output, test output, 3 curl results, Swagger screenshot or status code | Commit after: build exit 0 + test exit 0 + safety scan (git grep for secrets). Push to `dev`. Never to `main`. |
| **2. BACKEND_READ_ONLY_CLAIMS_API** | Implement all P0 GET endpoints with in-memory seed data for CLM-1006; all 13 routes return synthetic JSON matching mock API shape | All P0 endpoints return 200 + synthetic JSON; shapes match `mockInsuranceApi.ts` response types; no EF, no SQL | `server/InsuranceAIPlatform.Api/Controllers/**`, `server/InsuranceAIPlatform.Api/Application/**`, `server/InsuranceAIPlatform.Api/Contracts/**`, `server/InsuranceAIPlatform.Api/Domain/**`, `server/InsuranceAIPlatform.Tests/**` | `src/**`, `**/Migrations/**`, DB connection strings, real AI calls, mutation endpoints, Azure | `curl` all 13 P0 endpoints; compare shape to mock API types; run `dotnet test` | Report: curl results for all 13 endpoints, test counts, shape comparison table | Commit after: all 13 endpoints green + tests pass + safety scan. Push to `dev`. |
| **3. FRONTEND_BACKEND_READ_FLOW_INTEGRATION** | Swap `mockInsuranceApi.ts` reads to real `ApiClient` via config switch; P0 read flows work end-to-end in browser | React app loads dashboard/claims/CLM-1006 workspace using real API; mock switch still works; no UI/route changes | `src/api/ApiClient.ts` (new), `src/api/mockInsuranceApi.ts` (read-only reference), `src/config/apiConfig.ts` (new or modified), possibly `.env.development` | Changing any React route, component props, or UI structure; touching `server/**` in this gate; pushing to `main` | Browser: navigate all 11 routes with real API; verify no console errors; verify mock switch reverts cleanly | Report: screenshot or HAR trace of real API calls, route checklist, mock-revert verification | Commit after: all routes verified in browser + no console errors. Push to `dev` only. |
| **4. BACKEND_SQLSERVER_PERSISTENCE_GATE** | Add EF Core + SQL Server; create `InsuranceAIPlatform` DB (never `DevDept`); migrate in-memory seed to SQL; all P0 endpoints serve from DB | `dotnet ef migrations add InitialCreate` succeeds; `dotnet ef database update` creates `InsuranceAIPlatform` DB; P0 endpoints return same data from SQL; `DevDept` DB untouched | `server/InsuranceAIPlatform.Api/Infrastructure/**`, `server/InsuranceAIPlatform.Api/appsettings.json` (connection string non-secret), `**/Migrations/**`, `server/InsuranceAIPlatform.Tests/**` | `DevDept` DB; `appsettings.Production.json`; any real PII in seed; secrets in tracked files; `src/**` | `dotnet ef database info`; SQL query to confirm `InsuranceAIPlatform` DB exists; `DevDept` DB row count unchanged; P0 endpoints return 200 from SQL | Report: EF migration output, DB info output, DevDept safety confirmation, P0 endpoint spot-check | Commit after: migration applied + tests pass + DevDept safety confirmed + no secrets in tracked files. Push to `dev`. |
| **5. BACKEND_AUDIT_RISK_PLACEHOLDERS** | Add audit-trace and risk-review endpoints backed by SQL; implement placeholder AI-advisory scoring (deterministic rule-based, not real AI) | GET /api/claims/{id}/audit and GET /api/claims/{id}/risks return structured synthetic data; audit entries created on read operations; risk score is deterministic formula | `server/InsuranceAIPlatform.Api/Controllers/AuditController.cs`, `RisksController.cs`, `Application/Audit/**`, `Application/Risks/**`, `**/Migrations/**` | Real AI provider calls; real PII; Azure; approving/rejecting claims (human approval only); `src/**` except config | GET /audit → structured JSON with traceId; GET /risks → risk score + factor list; audit entries visible per claim | Report: curl results, risk score formula documented, audit entry count per claim | Commit after: endpoints return structured data + tests pass. Push to `dev`. |
| **6. BACKEND_APPROVAL_DRAFT_PLACEHOLDER** | Add POST /api/claims/{id}/approval/draft (human approval workflow placeholder); save draft to DB; GET returns draft state | POST approval draft → 201 + draft ID; GET draft → saved state; no auto-approval logic; human-final rule enforced | `server/InsuranceAIPlatform.Api/Controllers/ApprovalController.cs`, `Application/Approval/**`, `**/Migrations/**` | Auto-approval logic; AI-final verdict; `src/**` except config | POST draft → 201; GET draft → same content; attempt double-submit → 409 | Report: POST/GET curl pair, DB row confirmation | Commit after: round-trip verified + no auto-approval path exists. Push to `dev`. |
| **7. MOCK_AI_PIPELINE_PLANNING** | Architecture decision record for AI pipeline: which provider (Azure Document Intelligence / OpenAI / local), interface contract, mock vs real switch design, data flow | ADR written and accepted; interface `IAiAnalysisProvider` defined (code or doc); mock implementation spec complete | `docs/architecture/ADR_AI_PIPELINE_V0.1.md` (new), `server/InsuranceAIPlatform.Api/Application/AI/` (interface doc only) | Any real AI provider API calls; any API keys; committing keys; `src/**` | ADR reviewed by Slava; interface signature agreed; no new code except interface stub | Report: ADR path, interface signature, mock vs real switch design | Commit ADR after Slava approval. No push to `main`. |
| **8. MOCK_AI_PIPELINE_IMPLEMENTATION** | Implement `IAiAnalysisProvider` with rule-based mock; wire to POST /api/claims/{id}/ai-analysis/run; response matches Figma/product AI Evidence screen | POST /ai-analysis/run → 200 + synthetic evidence JSON; response shape matches `getAiAnalysis` mock; no real AI calls | `server/InsuranceAIPlatform.Api/Application/AI/**`, `Controllers/AiAnalysisController.cs`, `**/Migrations/**` (if AI result stored) | Real AI provider calls; API keys in tracked files; Azure; `src/**` except config | POST run → 200; response matches mock shape; `dotnet test` green; no real network calls (offline test) | Report: curl result, shape comparison to mock, offline test confirmation | Commit after: offline test confirms no real AI calls + shape verified. Push to `dev`. |
| **9. REAL_AI_PROVIDER_PLANNING** | Architecture decision for real AI provider integration: Azure Document Intelligence vs OpenAI vs other; cost model; key management; rate limits; fallback | ADR written and accepted; spike cost estimate documented; interface unchanged (only implementation swaps) | `docs/architecture/ADR_REAL_AI_PROVIDER_V0.1.md` (new) | Any real API key in tracked files; any production AI calls without Slava approval; `src/**` | ADR reviewed; cost estimate cited from provider pricing page; interface compatibility confirmed | Report: ADR path, provider chosen, cost model, key management plan | Commit ADR after Slava approval. Real provider integration = separate gate beyond V0.1. |

---

## Phase 18.5 — Future Claude File-Scope Maps

These maps define what a future Claude subagent MAY read, WRITE, and is FORBIDDEN from touching in each gate. They prevent scope creep and protect live assets.

---

### Gate 1: DOTNET_BACKEND_SKELETON_IMPLEMENTATION — File Scope

#### READ allowed
```
server/**                          ← full read for context
docs/architecture/**               ← plan docs
src/api/mockInsuranceApi.ts        ← read only — understand shapes
src/vite.config.ts                 ← read only — confirm dev server port
package.json                       ← read only — confirm Vite port
```

#### WRITE allowed
```
server/InsuranceAIPlatform.sln
server/InsuranceAIPlatform.Api/**
server/InsuranceAIPlatform.Tests/**
server/README.md
docs/reports/**                    ← gate completion report
```

#### FORBIDDEN (absolute — stop if tempted)
```
src/**                             ← no UI changes in skeleton gate
**/Migrations/**                   ← no DB in skeleton gate
appsettings.Production.json        ← no prod config ever
.env                               ← no env files tracked
Any file containing real credentials, tokens, or API keys
Any Azure SDK config
Any AI provider SDK config
DevDept DB connection string
C:\Projects\Twincore-framework\**  ← Igor's repo — read-only, never write
```

#### Verification commands (run in order)
```powershell
cd C:\Projects\InsuranceAIPlatform\server
dotnet build InsuranceAIPlatform.sln
dotnet test InsuranceAIPlatform.sln
# Then in a second terminal:
dotnet run --project InsuranceAIPlatform.Api
# Then:
curl http://localhost:{port}/health
curl http://localhost:{port}/api/system/demo-status
# Open browser: http://localhost:{port}/swagger
```

#### Stop line
STOP if any of the following: write attempt to `src/**`, attempt to add DB connection, attempt to add AI provider package, attempt to write `appsettings.Production.json`, attempt to commit secrets, attempt to push to `main`.

---

### Gate 2: BACKEND_READ_ONLY_CLAIMS_API — File Scope

#### READ allowed
```
server/**
src/api/mockInsuranceApi.ts        ← read — match response shapes
src/types/**                       ← read — match TypeScript types if present
docs/architecture/**
```

#### WRITE allowed
```
server/InsuranceAIPlatform.Api/Controllers/**
server/InsuranceAIPlatform.Api/Application/**
server/InsuranceAIPlatform.Api/Domain/**
server/InsuranceAIPlatform.Api/Contracts/**
server/InsuranceAIPlatform.Tests/**
docs/reports/**
```

#### FORBIDDEN
```
src/**                             ← no frontend changes in this gate
**/Migrations/**                   ← no DB yet
appsettings.Production.json
.env
Real credentials
Azure config
AI provider config
POST/PUT/DELETE endpoints          ← P0 is read-only
DevDept DB
C:\Projects\Twincore-framework\**
```

#### Verification commands
```powershell
dotnet build InsuranceAIPlatform.sln
dotnet test InsuranceAIPlatform.sln
dotnet run --project InsuranceAIPlatform.Api
# Then curl all 13 P0 GET endpoints:
curl http://localhost:{port}/api/claims/summary
curl http://localhost:{port}/api/claims
curl http://localhost:{port}/api/claims/CLM-1006
curl http://localhost:{port}/api/claims/CLM-1006/documents
curl http://localhost:{port}/api/claims/CLM-1006/photos
curl http://localhost:{port}/api/claims/CLM-1006/ai-evidence
curl http://localhost:{port}/api/claims/CLM-1006/risks
curl http://localhost:{port}/api/claims/CLM-1006/policy
curl http://localhost:{port}/api/claims/CLM-1006/customer-vehicle
curl http://localhost:{port}/api/claims/CLM-1006/approval
curl http://localhost:{port}/api/claims/CLM-1006/audit
curl http://localhost:{port}/api/demo/scenario
curl http://localhost:{port}/api/system/demo-status
```

#### Stop line
STOP if: any `src/**` write, any DB connection attempt, any P1/P2 endpoint added beyond explicit approval, any real data introduced, any mutation (POST/PUT/DELETE) added.

---

### Gate 4: BACKEND_SQLSERVER_PERSISTENCE_GATE — File Scope

#### READ allowed
```
server/**
docs/architecture/**
C:\Projects\Twincore-framework\Src\DevDept\**  ← READ ONLY for DB context
```

#### WRITE allowed
```
server/InsuranceAIPlatform.Api/Infrastructure/**
server/InsuranceAIPlatform.Api/appsettings.json   ← connection string placeholder only
server/InsuranceAIPlatform.Api/appsettings.Development.json  ← dev connection string (localhost,19772 / InsuranceAIPlatform)
server/InsuranceAIPlatform.Tests/**
**/Migrations/**                   ← ONLY within server/ scope
docs/reports/**
```

#### FORBIDDEN
```
appsettings.Production.json
.env (tracked)
DevDept DB — zero writes, zero schema changes, zero migrations
Any connection string pointing to DevDept
Real credentials / passwords in tracked files
src/**
C:\Projects\Twincore-framework\**  ← zero writes
Azure / cloud DB config
```

#### Verification commands
```powershell
dotnet ef migrations add InitialCreate --project server\InsuranceAIPlatform.Api
dotnet ef database update --project server\InsuranceAIPlatform.Api
# Confirm InsuranceAIPlatform DB created:
sqlcmd -S "localhost,19772" -Q "SELECT name FROM sys.databases WHERE name IN ('InsuranceAIPlatform','DevDept')"
# Expected: InsuranceAIPlatform present, DevDept unmodified (same row count as before gate)
dotnet test InsuranceAIPlatform.sln
```

#### Stop line
STOP if: any migration targets `DevDept`, any connection string contains `DevDept`, any credential in tracked file, any `appsettings.Production.json` touched.

---

### Gate 3: FRONTEND_BACKEND_READ_FLOW_INTEGRATION — File Scope

#### READ allowed
```
src/**                             ← full read — understand current mock usage
server/**                          ← full read — understand API endpoints
docs/architecture/**
```

#### WRITE allowed
```
src/api/ApiClient.ts               ← new real API client
src/config/apiConfig.ts            ← config switch (mock vs real)
src/api/mockInsuranceApi.ts        ← add export for switch compatibility ONLY; no logic changes
.env.development                   ← VITE_API_BASE_URL only; no real secrets
docs/reports/**
```

#### FORBIDDEN
```
Any React component files (*.tsx, *.jsx)  ← no UI changes
Any route definitions                      ← routes unchanged
server/**                                  ← server fully frozen in this gate
**/Migrations/**
Real credentials
appsettings.Production.json
C:\Projects\Twincore-framework\**
```

#### Verification commands
```powershell
# Start backend in one terminal:
cd server; dotnet run --project InsuranceAIPlatform.Api
# Start frontend in another:
cd C:\Projects\InsuranceAIPlatform; npm run dev
# Open browser: http://localhost:5173
# Navigate: / → /claims → /claims/CLM-1006 → /documents → /ai-evidence → /risks → /policy → /customer-vehicle → /approval → /audit → /demo
# Check Network tab: requests go to localhost:{backendPort}/api/...
# Toggle mock switch back: verify mock data returns, no console errors
```

#### Stop line
STOP if: any `.tsx`/`.jsx` component edited, any route changed, any server file modified, any real credentials introduced.
