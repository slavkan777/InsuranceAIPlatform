# Technical Proof Matrix — V0.1

**Project:** InsuranceAIPlatform · **Date:** 2026-05-30
Capability → evidence (file/source) → runtime check → status → how to explain honestly. Status legend: **LIVE** (deployed + verified at runtime), **PARTIAL** (built/architected, but not fully live), **DEFERRED** (intentionally not deployed).

| # | Capability / claim | Evidence (file/source) | Runtime proof (read-only) | Status | How to explain |
|---|---|---|---|---|---|
| 1 | .NET 9 API on Azure Container Apps | `server/InsuranceAIPlatform.Api/`, `infra/modules/container-apps.bicep`, `server/Dockerfile` | `GET /health` → 200; ACA revision `iap-demo-api--0000001` Active 100% | **LIVE** | "Modular-monolith .NET 9 API containerized and run on ACA, scale-to-zero." |
| 2 | React 18 + TS + Vite SPA on Static Web Apps | `package.json`, `vite.config.ts`, `staticwebapp.config.json` | `GET /` → 200; deep link `/claims` → 200 (SPA fallback) | **LIVE** | "SPA on SWA Free with navigation fallback for client routing." |
| 3 | Container image in GHCR (free registry) | `server/Dockerfile`; image `ghcr.io/slavkan777/insuranceai-api:14d5c81-cors` | `docker manifest inspect …` OK; ACA pulls anonymously (public) | **LIVE** | "Image on GHCR (public) — skipped ACR to avoid $5/mo." |
| 4 | Infrastructure as Code (Bicep) | `infra/main.bicep` (sub-scope) + `infra/modules/*.bicep` | `az bicep build` → 0 warnings; deployed via `az deployment sub create` | **LIVE** | "Whole stack is Bicep; `enableSql`/`enableAi` toggles keep deploys minimal." |
| 5 | Scale-to-zero cost control | `infra/modules/container-apps.bicep` (`minReplicas=0`) | `az containerapp show … scale.minReplicas` → 0, max 2 | **LIVE** | "Compute scales to zero — ~$0 when idle, cold-start on first hit." |
| 6 | Managed Identity (passwordless) | `infra/modules/*` (`iap-demo-api-mi`, `AZURE_CLIENT_ID` env) | resource `iap-demo-api-mi` present; UAMI assigned to ACA | **DEPLOYED** | "User-assigned MI is wired for passwordless access (Key Vault/Storage)." |
| 7 | Key Vault (RBAC) | infra KV module (`iapdemokv…`, RBAC) | resource present; **no secrets stored yet** | **DEPLOYED (unused)** | "Key Vault + RBAC is in place for when secrets are needed; none stored yet." |
| 8 | Storage hardening | infra storage module | `allowSharedKeyAccess=false`; lifecycle TTL | **DEPLOYED** | "Storage is Entra-only (shared-key disabled) with TTL on demo blobs." |
| 9 | Observability | infra App Insights + Log Analytics modules | `iap-demo-appi` (50% sampling) + `iap-demo-law` (30d/1GB-day) | **DEPLOYED** | "App Insights + Log Analytics, sampled and capped for cost." |
| 10 | Secure CORS (config-driven) | `server/…/Program.cs` (`Cors:AllowedOrigins`), `appsettings.json` | preflight + GET from SWA origin → 200 + ACAO; no wildcard | **LIVE** | "Config-driven CORS, explicit origins, no wildcard, no credentials; env-overridable." |
| 11 | Live summary metrics | `/api/claims/summary`; `src/pages/DashboardPage.tsx` | 200 `{totalActive:47,…}` — same values render in UI cards | **LIVE** | "Dashboard metric cards are fed by the live API." |
| 12 | Golden-claim workspace (CLM-1006) | `src/pages/Claim*`, `server` claims service | 8 endpoints (`/CLM-1006`, `/documents`, `/ai-evidence`, `/risks`, `/policy`, `/customer-vehicle`, `/audit`, `/approval`) → all 200 | **LIVE** | "The full claim workspace for the golden claim is served live." |
| 13 | AI-ready architecture | `IAiProvider` (Mock + DeepSeek opt-in), `appsettings` `AiProvider:Mode=Mock` | AI workbench UI renders live golden-claim data; output is Mock/seeded | **PARTIAL** | "Provider-agnostic AI behind an interface; Mock active; a real model drops in behind the same contract." |
| 14 | Graceful degradation | `src/pages/DashboardPage.tsx` (`queue.length>0 ? live : mock`) | `/api/claims` 500 → UI still renders seeded queue rows | **LIVE** | "When DB-backed list endpoints aren't deployed, the SPA falls back to seeded data so the UI never breaks." |
| 15 | Backend test suite | `server/InsuranceAIPlatform.Tests` | `dotnet test -c Release` → 137/137 passed | **LIVE** | "137 backend tests green, run each delivery." |
| 16 | CI/CD blueprint | `.github/workflows/azure-deploy-demo.yml` | `workflow_dispatch`-guarded; OIDC placeholders; **not run** | **PARTIAL** | "CI workflow is scaffolded (OIDC); current deploys were local `az` — honest." |
| 17 | Azure SQL (relational data) | `infra/main.bicep` (`enableSql=false`), `infra/modules/sql-serverless.bicep` | not deployed; `/api/claims` & `/api/customers` → 500 | **DEFERRED** | "SQL is toggle-gated off for cost; list endpoints await it — graceful fallback covers the demo." |
| 18 | Real AI provider | `enableAi=false` | not deployed | **DEFERRED** | "No paid AI deployed; Mock only." |
| 19 | AKS / ACR | — | none in subscription | **NOT USED** | "Chose ACA + GHCR over AKS + ACR for cost/simplicity at this scale." |
| 20 | Budget / cost governance | operator budget on subscription | $30/mo + alerts $5/$10/$20/$30/$50; idle ≈ $0 | **LIVE** | "Hard budget with alert tiers; designed for $5–10/mo real." |

## Quick re-verify commands (read-only, safe)

```bash
API=https://iap-demo-api.bluehill-ebdd0494.westeurope.azurecontainerapps.io
curl -s -o /dev/null -w "%{http_code}\n" https://kind-meadow-03cf73103.7.azurestaticapps.net/        # 200
curl -s "$API/health"                                                                                 # Healthy
curl -s "$API/api/claims/summary"                                                                     # {"totalActive":47,...}
curl -s -i -H "Origin: https://kind-meadow-03cf73103.7.azurestaticapps.net" "$API/api/claims/summary" | grep -i access-control-allow-origin
az resource list -g rg-iap-demo --query "length(@)" -o tsv                                            # 9
az containerapp show -g rg-iap-demo -n iap-demo-api --query properties.template.scale.minReplicas -o tsv  # 0
```

## Deferred / risk register
- `/api/claims`, `/api/customers` → 500 until Azure SQL gate (UI masks via seeded fallback).
- Live AI is Mock (seeded numbers).
- Demo auth is client-side; not production identity.
- CI deploy path scaffolded but not exercised (local `az` used).
