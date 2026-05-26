---
title: Backend Verification Plan V0.1
type: knowledge
status: active
created: 2026-05-27
tags: [verification, testing, smoke-tests, qa, acceptance, pass-fail]
---

# Backend Verification Plan V0.1

## 1. Verification Philosophy

Every check in this plan has a machine-verifiable PASS criterion. "It looks fine" or "no errors in the console" are not PASS criteria. Each check must produce a specific output, exit code, or observable value. Checks are ordered from fastest/cheapest to slowest/most integrated.

The **"DevDept not touched"** check is mandatory before every push — it is a hard gate, not advisory.

---

## 2. Verification Checks Table

### GROUP A — Build and Static Analysis

| Check | Command | PASS Criteria | FAIL Criteria | When to Run |
|---|---|---|---|---|
| **A1. dotnet build** | `dotnet build src/Api/InsuranceAIPlatform.Api.csproj --no-incremental` | Exit 0; output `Build succeeded. 0 Warning(s) 0 Error(s)` (or only expected warnings) | Exit non-zero; any `Error` in output | Every code change; mandatory before A2–A4 |
| **A2. dotnet test (unit)** | `dotnet test tests/InsuranceAIPlatform.Tests.csproj --no-build` | Exit 0; output `Passed! - Failed: 0` | Any `Failed:` count > 0; exit non-zero | Every code change; mandatory before any commit |
| **A3. No DevDept references in code** | `Select-String -Path "src\**\*.cs" -Pattern "DevDept\|devdept\|localhost,19772.*DevDept"` | Zero matches | Any match in any `.cs` file | Before every `git commit`; absolute gate |
| **A4. Secret scan** | `Select-String -Path "**\*.*" -Pattern "password=\|Server=.*Password\|sk-\|sk-ant-\|Bearer " -Exclude "*.md" -Recurse` in repo root | Zero matches in committed files | Any match | Before every `git push`; absolute gate |

---

### GROUP B — API Smoke Tests

| Check | Command | PASS Criteria | FAIL Criteria | When to Run |
|---|---|---|---|---|
| **B1. Health endpoint** | `Invoke-RestMethod http://localhost:5174/health` | HTTP 200; JSON contains `"status":"Healthy"` | HTTP non-200; missing `status` field; connection refused | First thing after backend starts; after any config change |
| **B2. Swagger reachable** | `(Invoke-WebRequest http://localhost:5174/swagger/index.html).StatusCode` | Returns `200` | Non-200; connection error | After build; confirms Swashbuckle registered correctly |
| **B3. Demo status endpoint** | `Invoke-RestMethod http://localhost:5174/api/system/demo-status` | HTTP 200; JSON contains `"demoReady":true`, `"seedClaimId":"CLM-1006"`, `"demoMode":true` | Missing fields; false values; HTTP non-200 | After B1; confirms seed is active |
| **B4. Claim detail — CLM-1006** | `Invoke-RestMethod http://localhost:5174/api/claims/CLM-1006` | HTTP 200; response contains `"id":"CLM-1006"`, `"policyNumber":"POL-2025-AC-4421"`, claimant name contains "Johnson" | HTTP 404; missing required fields; wrong values | P0 gate; confirms golden claim seed is correct |
| **B5. Claims list** | `(Invoke-RestMethod http://localhost:5174/api/claims).Length` | Returns array with length ≥ 1; CLM-1006 present in result | Empty array; HTTP non-200 | After B4 |
| **B6. Documents endpoint** | `(Invoke-RestMethod http://localhost:5174/api/claims/CLM-1006/documents).Length` | Returns array with length = 7 (6 present + 1 missing); at least one item with `"status":"missing"` | Wrong count; no missing item; HTTP non-200 | P0 gate; 6/7 docs checked in CLM-1006 |
| **B7. AI analysis read** | `$r = Invoke-RestMethod http://localhost:5174/api/claims/CLM-1006/ai-analysis; $r.modelConfidence` | Returns value `0.78` (±0.001); `findings` array not empty; `demoMode: true` present | Wrong confidence value; empty findings; `demoMode` absent | P0 gate; confirms AI placeholder seeded correctly |
| **B8. Risk review** | `$r = Invoke-RestMethod http://localhost:5174/api/claims/CLM-1006/risk-review; $r.score` | Returns `82`; `$r.threshold` = `60`; `$r.pipeline` or `$r.level` contains "Высокий" or "High" | Score ≠ 82; threshold ≠ 60; HTTP non-200 | P0 gate; deterministic score must be stable |
| **B9. Audit trace** | `$r = Invoke-RestMethod http://localhost:5174/api/claims/CLM-1006/audit-trace; $r.traceId` | Returns `"trc_8f3d2a7e"`; `$r.runId` = `"run_8f3d2a7e"`; `$r.tokens` = `4261`; events array contains a `GovernanceBlock` entry | Wrong traceId/runId/tokens; no GovernanceBlock event; HTTP non-200 | P0 gate; audit trace must match frontend mock exactly |
| **B10. Policy coverage** | `(Invoke-RestMethod http://localhost:5174/api/claims/CLM-1006/policy-coverage).blocks.Length` | Returns array length ≥ 1; at least one block present | Empty blocks; HTTP non-200 | P0 |

---

### GROUP C — Contract and Behavioral Tests

| Check | Command | PASS Criteria | FAIL Criteria | When to Run |
|---|---|---|---|---|
| **C1. Deterministic risk score test** | `dotnet test --filter "RiskAssessment_CLM1006_Returns82"` | Test passes; asserts `score == 82`, `threshold == 60`, `level == High` | Test fails; score drifts | Every build; the score must be deterministic for reproducible demos |
| **C2. Deterministic policy check test** | `dotnet test --filter "PolicyCheck_CLM1006_ReturnsBlocks"` | Test passes; asserts at least one coverage block returned for POL-2025-AC-4421 | Test fails | Every build |
| **C3. Audit append/read test** | `dotnet test --filter "AuditTrail_Append_ThenRead_ReturnsEvent"` | Test passes; event appended in-memory, then read back with correct `eventId`, `claimId`, `eventType` | Test fails; event not found on read | Every build; audit must be functional from V0.1 |
| **C4. GovernanceBlock pre-seeded** | `dotnet test --filter "AuditTrail_CLM1006_HasGovernanceBlock"` | Test passes; seed data contains exactly one `GovernanceBlock` event with description matching "Авто-погодження заблоковано" | Test fails; event absent or wrong description | P0 gate |
| **C5. DTO contract — ClaimDetailDto** | `dotnet test --filter "DtoContract_ClaimDetail_RequiredFields"` | All required fields present in serialized JSON: `id`, `policyNumber`, `riskScore`, `confidence`, `status` | Missing fields | Every build; prevents silent DTO regressions |

---

### GROUP D — CORS and Frontend Integration

| Check | Command | PASS Criteria | FAIL Criteria | When to Run |
|---|---|---|---|---|
| **D1. CORS preflight — Vite origin** | `Invoke-WebRequest -Method OPTIONS http://localhost:5174/api/claims -Headers @{"Origin"="http://localhost:5173";"Access-Control-Request-Method"="GET"}` | HTTP 200 or 204; response headers contain `Access-Control-Allow-Origin: http://localhost:5173` | CORS header absent; HTTP 4xx; frontend console shows CORS error | After CORS config; before frontend integration |
| **D2. Frontend read-flow smoke** | With `VITE_USE_MOCK_API=false`, navigate to `/claims/CLM-1006` in Vite dev server at :5173 | Workspace screen renders with claim data; no console errors; no "Network Error" banners; risk score shows 82 | Any console network error; blank screen; mock data leaking when mock is off | P0 integration gate; requires both servers running |

---

### GROUP E — Persistence (SQL Server — deferred until Phase SQL)

| Check | Command | PASS Criteria | FAIL Criteria | When to Run |
|---|---|---|---|---|
| **E1. DB connection test** | `Invoke-RestMethod http://localhost:5174/health/ready` | HTTP 200; `"database":"Healthy"` in response | `"database":"Unhealthy"` or connection refused | Only after SQL Server persistence phase is started |
| **E2. CLM-1006 round-trip** | `dotnet test --filter "Persistence_CLM1006_ReadFromDb"` | Seed applied; `GetClaimById("CLM-1006")` returns correct data from DB | Record not found; wrong values | Only after EF + seed migration applied to InsuranceAIPlatform DB |

---

### GROUP F — Safety Gates (absolute — run before every push)

| Check | Command | PASS Criteria | FAIL Criteria | When to Run |
|---|---|---|---|---|
| **F1. DevDept not touched** | `Select-String -Path "src\**\*.*" -Pattern "DevDept\|Twincore-framework" -Recurse` | Zero matches anywhere in `src/` | Any match | Before every `git commit` and `git push`; non-negotiable |
| **F2. No appsettings.Production.json** | `Test-Path "src\Api\appsettings.Production.json"` | Returns `False` | Returns `True` | Before every `git push` |
| **F3. No .env with secrets staged** | `git diff --cached --name-only \| Select-String "\.env"` | Zero matches | Any `.env*` file in staged set | Before every `git commit` |
| **F4. Secret scan (full)** | (see A4 above, applied to full staged diff) | Zero matches for credential patterns | Any match | Before every `git push` |
| **F5. No appsettings.Development.json committed** | `git ls-files src\Api\appsettings.Development.json` | Empty output (file is gitignored) | File appears in tracked files | Before every `git push` |

---

## 3. Minimum Gate for "P0 Backend Done"

The following checks must ALL pass before the P0 milestone is declared complete:

| Gate | Group | Check(s) |
|---|---|---|
| Build clean | A | A1, A2 |
| No cross-project contamination | A, F | A3, F1 |
| No secrets | A, F | A4, F2–F5 |
| Health + demo ready | B | B1, B2, B3 |
| Golden claim readable | B | B4, B5, B6, B7, B8, B9, B10 |
| Deterministic scoring stable | C | C1, C2, C4 |
| Audit functional | C | C3 |
| CORS configured | D | D1 |
| Frontend reads backend (not mock) | D | D2 |

All 9 gate groups must show PASS with evidence (command + output) before the P0 milestone is reported as done. The implementer may not self-certify the D2 gate — a second pair of eyes (human or reviewer agent) must observe the browser rendering.
