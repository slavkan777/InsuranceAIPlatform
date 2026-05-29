# Portfolio Azure Proof Pack — V0.1

**Project:** InsuranceAIPlatform — Auto Insurance Claim AI Workbench
**Date:** 2026-05-30 · **Status:** live demo on Azure (hobby-cost, production-*shaped*)

> Honesty contract for this pack: every claim below is marked **LIVE** / **PARTIAL** / **DEFERRED**. Do not present DEFERRED items as built. The architecture is production-shaped; the deployment is a low-cost demo on a personal subscription.

## 1. Executive summary

A full-stack "Auto Claim AI Workbench" deployed to Azure as a **modular-monolith .NET 9 API** (Azure Container Apps, scale-to-zero) + a **React 18 / TypeScript / Vite SPA** (Azure Static Web Apps, Free), provisioned with **Bicep IaC**, image hosted on **GHCR**, with config-driven CORS, Managed Identity + Key Vault architecture, and App Insights / Log Analytics observability. Idle cost ≈ **$0** (scale-to-zero + Free tiers + capped logging). Data layer (Azure SQL) and a real AI provider are **intentionally deferred** behind toggles; the app ships seeded data + a Mock AI provider and **degrades gracefully** so the demo is always presentable.

## 2. Live demo links

| | URL |
|---|---|
| Frontend (SPA) | https://kind-meadow-03cf73103.7.azurestaticapps.net |
| API base | https://iap-demo-api.bluehill-ebdd0494.westeurope.azurecontainerapps.io |
| API health | `GET /health` → `200 {"status":"Healthy","service":"InsuranceAIPlatform.Api","environment":"Production"}` |
| Demo login | `demo@insurance.local` / `Demo123!` (public demo creds, shown on the login page; client-side only) |

Golden demo path (all **LIVE** on the API): Overview → `claims/CLM-1006` → documents → ai-evidence → risks → approval → audit.

## 3. Architecture

```
Browser ──HTTPS──> Azure Static Web Apps (Free)         [React 18 + TS + Vite SPA]
   │                     served static; SPA fallback via staticwebapp.config.json
   └──HTTPS (CORS)──> Azure Container Apps (scale-to-zero, minReplicas=0, max 2)
                          └─ .NET 9 modular-monolith API (Kestrel :8080, non-root)
                          └─ image: ghcr.io/slavkan777/insuranceai-api:14d5c81-cors (GHCR, public)
                          ├─ User-assigned Managed Identity (passwordless)
                          ├─ Key Vault (RBAC; no secrets stored yet)
                          ├─ Storage (shared-key disabled; lifecycle TTL)
                          └─ App Insights (50% sampling) + Log Analytics (30d / 1GB-day cap)
   (deferred behind toggles): Azure SQL Serverless (enableSql=false) · real AI provider (enableAi=false / AiProvider=Mock)
```
Provisioned by **Bicep** (`infra/main.bicep` subscription-scope + `infra/modules/*`). RG `rg-iap-demo` / westeurope, personal subscription. 8 resources (+ a free App Insights smart-detection rule).

## 4. Azure services used (LIVE unless noted)

| Service | Role | Status |
|---|---|---|
| Azure Container Apps + Environment | host the .NET API, scale-to-zero | LIVE |
| Azure Static Web Apps (Free) | host the SPA | LIVE |
| GHCR (GitHub Container Registry) | container image (free, public) — chosen over ACR to avoid $5/mo | LIVE |
| User-assigned Managed Identity | passwordless app identity | DEPLOYED |
| Azure Key Vault (RBAC) | secret store (architecture in place; no secrets yet) | DEPLOYED (unused) |
| Azure Storage | demo blob containers; `allowSharedKeyAccess=false`; TTL | DEPLOYED |
| App Insights + Log Analytics | telemetry; 50% sampling; 30d + 1GB/day cap | DEPLOYED |
| Azure SQL Serverless | relational data | **DEFERRED** (`enableSql=false`) |
| Azure OpenAI / AI services | real AI | **DEFERRED** (`enableAi=false`; Mock active) |
| AKS / ACR | — | **NOT USED** (ACA + GHCR by design) |

## 5. Runtime proof (captured 2026-05-30, read-only)

| Check | Evidence |
|---|---|
| Frontend | `GET /` → 200 (serves our app; `<title>InsuranceAIPlatform · Auto Claim AI Workbench</title>`) |
| API health | `GET /health` → 200 Healthy, env Production |
| Live summary | `GET /api/claims/summary` → 200 `{"totalActive":47,"pendingReview":12,"highRisk":8,"avgSlaRemainingHours":14.3,"processedToday":6,"aiAnalysisRunning":2}` — **and these exact values render in the UI metric cards** (see `screenshots/02-overview.png`) |
| Golden claim (LIVE) | `GET /api/claims/CLM-1006` + `/documents` `/ai-evidence` `/risks` `/policy` `/customer-vehicle` `/audit` `/approval` → **all 200** (see `screenshots/04-ai-evidence.png`) |
| Demo scenario | `GET /api/demo/scenario` → 200 (7-step golden path, `goldenClaimId: CLM-1006`) |
| CORS | preflight + GET with `Origin: <SWA>` → 200 + `access-control-allow-origin: <SWA>`; localhost origin also allowed; no wildcard |
| Cost guardrails | ACA `minReplicas=0`/max 2 · SWA `Free` · no AKS/ACR/SQL · 9 resources |

Screenshots (authenticated as "Demo Adjuster"): `screenshots/01-login.png`, `02-overview.png`, `03-claim-workspace.png`, `04-ai-evidence.png`, `05-audit-cost.png`.

## 6. Cost-control model

- **Compute scale-to-zero** (ACA `minReplicas=0`) → no replica, ~no cost when idle; cold-start on first request.
- **Free static hosting** (SWA Free). **No ACR** (image on free public GHCR). **No always-on App Service. No AKS.**
- **Capped telemetry**: App Insights 50% sampling; Log Analytics 30-day retention + 1 GB/day cap.
- **Budget** $30/mo with alerts ($5/$10/$20/$30/$50). **Idle ≈ $0**; residual LAW/Storage < $1–2/mo. Real target $5–10/mo.
- **One-command teardown**: `az group delete -n rg-iap-demo --yes --no-wait`.

## 7. Security / secrets model

- **No secrets in the repo.** CORS origins + URLs are public, non-secret. Secret-handling defers to Managed Identity + Key Vault (architecture deployed; no secrets stored yet).
- **Storage** `allowSharedKeyAccess=false` (Entra-only data-plane). API runs **non-root** (`USER $APP_UID`) on `:8080`.
- **CORS** is config-driven (`Cors:AllowedOrigins`), explicit origins, **no wildcard, no AllowCredentials**.
- **AI key** (`DEEPSEEK_API_KEY`) is never read/logged/committed; Mock provider is the default.
- **Demo auth** is client-side only (Redux + localStorage), public demo creds — explicitly *not* a production identity provider (no Azure AD). No real PII; synthetic data only.

## 8. CI/CD & IaC model

- **IaC:** Bicep — subscription-scope `infra/main.bicep` + RG-scoped modules; `az bicep build` validates offline (0 warnings); deployed via `az deployment sub create`. Feature toggles `enableSql` / `enableAi` keep the minimal deploy cheap.
- **Container:** multi-stage `server/Dockerfile` (sdk build → aspnet runtime, non-root, no secrets baked).
- **CI workflow:** `.github/workflows/azure-deploy-demo.yml` is a `workflow_dispatch`-guarded blueprint (OIDC placeholders). The current live deploys were **local `az`** operations, not CI runs — state this honestly.
- **Quality gate:** backend `dotnet test` **137/137**; each delivery passed an independent Opus `/qa-inspector` review before commit.

## 9. AI-readiness model (PARTIAL — honest)

- `IAiProvider` abstraction with **Mock** (default) and a **DeepSeek opt-in** path (disabled by default; `RealCallsEnabled=false`). A real Azure OpenAI / Foundry provider drops in behind the same interface.
- The AI workbench UI is **fully built** (advisory-only findings, evidence/RAG sources, extracted entities, model-confidence breakdown, audit/cost trace) and renders **live** golden-claim data — but the underlying AI output is **seeded/Mock**, not a live model call. Do **not** claim a real LLM is wired in production.

## 10. What is real today

- Live .NET 9 API on Azure Container Apps (scale-to-zero) — `/health` 200.
- Live React SPA on Azure Static Web Apps with working SPA routing.
- Live, working CORS between SPA origin and API.
- Live summary metrics + the **entire golden claim CLM-1006 workspace** (8 endpoints) served by the API.
- Bicep IaC, GHCR image, Managed Identity / Key Vault / Storage / App Insights / Log Analytics provisioned.
- Cost governance (scale-to-zero, Free tiers, budget + alerts).

## 11. What is intentionally deferred (and the honest caveat)

- **Azure SQL** not deployed (`enableSql=false`). Consequence: the **list/enumeration endpoints `/api/claims` and `/api/customers` currently return HTTP 500** (no DB to enumerate). The SPA **degrades gracefully** to bundled seeded rows, so the queue/customers pages still render — but that list data is mock, not live. Summary + the golden claim + demo scenario are served live from seeded in-memory data.
- **Real AI provider** not deployed (Mock only).
- **AKS / ACR** not used (ACA + GHCR by design).
- **Production auth** (Azure AD / Entra) not wired — demo login is client-side only.

## 12. Interview-safe claims (say exactly this)

✅ "I deployed a .NET 9 modular-monolith API and a React SPA to Azure (Container Apps + Static Web Apps) with Bicep IaC, scale-to-zero cost control, Managed Identity + Key Vault architecture, and App Insights observability — live, at roughly $0 idle."
✅ "The golden demo path runs on the live API end-to-end; I fixed a real CORS issue with config-driven origins and verified preflight + GET from the SWA origin."
✅ "I deliberately deferred Azure SQL and a real AI provider behind feature toggles to keep cost near zero, and the SPA degrades gracefully to seeded data when the DB-backed endpoints aren't deployed."
🚫 Don't say: "Azure SQL is running", "a real LLM scores claims in production", "it uses AKS", "it serves production traffic", "enterprise SSO".

## 13. Risks / limitations

- List endpoints 500 without SQL (graceful mock fallback masks it in the UI — be ready to explain).
- Cold start on first request after scale-to-zero (seconds).
- Demo auth is not production-grade.
- Live AI is Mock; AI numbers are seeded.

## 14. Cleanup / stop-cost

```bash
az group delete -n rg-iap-demo --yes --no-wait   # removes all 8 resources; GHCR image + budget live outside the RG
```

## Next
`PORTFOLIO_AZURE_PROOF_PACK_COMMIT_V0.1` (commit these docs). Optional: `AZURE_SQL_OPTIONAL_GATE_V0.1` to make list endpoints live (removes the only visible gap).
