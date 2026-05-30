# Live Demo Runbook — V0.1

**Project:** InsuranceAIPlatform · **Date:** 2026-05-30
How to run, present, verify, and cost-protect the live Azure demo.

## Links
- **Frontend:** https://kind-meadow-03cf73103.7.azurestaticapps.net
- **API:** https://iap-demo-api.bluehill-ebdd0494.westeurope.azurecontainerapps.io
- **Demo login:** `demo@insurance.local` / `Demo123!` (public, shown on the login page)

## Before a live demo (warm-up — important)
The API scales to zero, so the **first request after idle is a cold start** (a few seconds). 1–2 minutes before presenting, warm it:
```bash
curl -s https://iap-demo-api.bluehill-ebdd0494.westeurope.azurecontainerapps.io/health
```
Then open the frontend so the SPA + SWA edge are warm too.

## The golden path (everything here is LIVE on the API)
1. Open the frontend → you land on **/login**. Enter the demo creds → **Увійти**.
2. **Overview** (`/`) — metric cards (47 / 12 / 6 / 8 / 14.3) are **live** from `/api/claims/summary`. The "AI-рекомендація для CLM-1006" card is live golden-claim data.
3. Click claim **CLM-1006** (or go to `/claims/CLM-1006`) — **Робоче місце** (workspace), live.
4. **Документи та фото** → **AI-докази** (`/ai-evidence`) — the AI workbench: findings, evidence/RAG sources, extracted entities, model-confidence bars, token/cost trace — live golden-claim data.
5. **Ризики** → **Людське погодження** (advisory-only; human decides) → **Audit & Cost** (governance trace).

This sequence matches `GET /api/demo/scenario` (7 steps, `goldenClaimId: CLM-1006`). Screenshots: `screenshots/01-login … 05-audit-cost`.

## Mock vs live — explain it proactively
- **Live from Azure SQL (since 2026-05-30):** the **claims queue list** (`/api/claims`) and **customers directory** (`/api/customers`) load from Azure SQL, and **creating a synthetic customer persists to SQL** (`POST /api/customers`). The earlier `500` / "Failed to fetch" is resolved.
- **Live (real API responses; in-memory golden fixtures by design):** dashboard summary, the entire CLM-1006 workspace + sub-resources, demo scenario, CORS. These curated golden-claim views are served in-memory on purpose (deterministic demo); DB-created claims (CLM-1011+) read from SQL.
- **Still mock (by design):** AI outputs (Mock provider; numbers seeded), demo auth (client-side). Synthetic data only — no real PII.

## Live SQL write demo (persistence proof)
1. Open **Customer directory** (`/customers`) — the list loads from **Azure SQL** (200 synthetic customers).
2. Click **Create customer**, enter a name, submit → success, **no "Failed to fetch"**; a new `CUST-T0xxx` row is written to SQL.
3. **Refresh** the page (or search the name) → the customer is still there → proves **durable SQL persistence** (not localStorage).

## Verify API health / CORS (read-only)
```bash
API=https://iap-demo-api.bluehill-ebdd0494.westeurope.azurecontainerapps.io
curl -s "$API/health"                                   # {"status":"Healthy",...}
curl -s "$API/api/claims/summary"                        # {"totalActive":47,...}
curl -s "$API/api/claims/CLM-1006" -o /dev/null -w "%{http_code}\n"   # 200 (golden claim, live)
curl -s -i -H "Origin: https://kind-meadow-03cf73103.7.azurestaticapps.net" "$API/api/claims/summary" | grep -i access-control-allow-origin
```

## Avoid causing cost
- Read-only `curl` / browsing costs ~nothing (scale-to-zero + Free SWA).
- Don't load-test. Don't enable real AI/AKS/ACR. Don't raise `minReplicas`. (Azure SQL is serverless auto-pause — leave it.)
- Budget alerts at $5/$10/$20/$30/$50 will fire long before anything material.

## Stop / delete (one command)
```bash
az group delete -n rg-iap-demo --yes --no-wait     # removes all resources (Container App, Azure SQL, SWA, Key Vault, Storage, observability)
```
GHCR image + the subscription budget live outside the RG (unaffected). To redeploy later: `az deployment sub create … infra/main.bicep` then `swa deploy ./dist`.

## Troubleshooting
| Symptom | Cause | Fix |
|---|---|---|
| First load slow / 503 then OK | ACA cold start from scale-to-zero | warm with `curl …/health`, retry |
| First data request slow after long idle | Azure SQL serverless auto-paused (60 min) → resume ~30–60s | warm `/api/customers` before presenting; later requests are fast |
| Browser console CORS error | SWA origin not allowed | already fixed (config-driven CORS); if a new SWA hostname appears, add it to `Cors:AllowedOrigins` + redeploy API image |
| Image pull fails on deploy | GHCR package private | ensure `ghcr.io/slavkan777/insuranceai-api` package is **public** |
| SPA deep link 404 | missing SPA fallback | `staticwebapp.config.json` must be in the deployed `dist/` (we copy it in before `swa deploy`) |
| Frontend shows old build | SWA CDN cache | re-deploy `./dist`; hard-refresh |
| Login loops | localStorage blocked | allow storage for the SWA origin; creds `demo@insurance.local` / `Demo123!` |

## Re-deploy cheatsheet (not part of this gate — reference only)
- API: `docker build -f server/Dockerfile server -t ghcr.io/slavkan777/insuranceai-api:<tag>` → push → `az containerapp update -g rg-iap-demo -n iap-demo-api --image …:<tag>`
- Frontend: `npm run build` (set `VITE_INSURANCE_API_MODE=backend` + `VITE_INSURANCE_API_BASE_URL=<api>`), copy `staticwebapp.config.json` into `dist/`, then `swa deploy ./dist --env production` with the SWA token (env var, never printed).
