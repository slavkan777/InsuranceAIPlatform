# Azure Frontend Deploy — V0.1 (DEPLOYED, mock mode)

First frontend content deploy of InsuranceAIPlatform into the existing Azure Static Web App. **Outcome: FRONTEND_DEPLOYED_PARTIAL** — the SPA is live and fully interactive on **mock data**; live-API wiring is deferred to a CORS-fix gate (see below).

## What was deployed

- **Target:** existing Static Web App `iap-demo-swa` (Free), `rg-iap-demo` / westeurope — **production** environment. No new resource created.
- **Content:** Vite production build `dist/` (React 18 + TS SPA), **mock mode** (`VITE_INSURANCE_API_MODE` unset → app's default self-contained synthetic-data mode).
- **SPA fallback:** `staticwebapp.config.json` (root) copied into `dist/` so the SWA serves `index.html` for client routes (`/claims`, etc.). Confirmed picked up by the deploy ("Found configuration file").
- **Deploy method:** Azure Static Web Apps CLI `2.0.9` via `npx @azure/static-web-apps-cli deploy ./dist --env production`. Deployment token retrieved from `az staticwebapp secrets list` into an env var inline — **never printed, logged, stored, or committed**.

## URLs

- **Frontend (live):** `https://kind-meadow-03cf73103.7.azurestaticapps.net`
- **API:** `https://iap-demo-api.bluehill-ebdd0494.westeurope.azurecontainerapps.io` (`/health` → 200)

## Smoke tests (all PASS)

| Check | Result |
|---|---|
| SWA root | HTTP 200; serves our app (`<title>InsuranceAIPlatform · Auto Claim AI Workbench</title>`, `#root`, hashed assets) — not the Azure placeholder |
| SPA deep link `/claims` | HTTP 200 → `index.html` (navigation fallback works) |
| JS asset `/assets/index-*.js` | HTTP 200, `text/javascript`, ~443 KB |
| CSS asset `/assets/index-*.css` | HTTP 200, `text/css` |
| API `/health` after deploy | HTTP 200 (backend unaffected) |
| Browser ↔ live API | N/A — mock mode makes no cross-origin calls (no CORS error in the running demo) |

## Why mock mode (live-API wiring deferred)

Evidence gathered this gate:
- The API **CORS policy is hardcoded** to `WithOrigins("http://localhost:5173")` (`server/InsuranceAIPlatform.Api/Program.cs`). The SWA origin is **not** allowed, and there is **no env-var override** — so a browser at the SWA origin is CORS-blocked.
- A server-side probe (`GET /api/claims/summary` with the SWA `Origin`) returns **200 + real data** (`{"totalActive":47,…}`) but **no `Access-Control-Allow-Origin` header** → the backend *does* serve data without SQL (in-memory seed), but the browser would block the response.

Therefore a backend-wired build would render a **broken** demo (CORS errors, no data). Mock mode is the app's intended demo mode and produces a fully working portfolio demo immediately, with **zero** API/CORS/SQL dependency.

The build bundle contains the backend client's default base URL `http://localhost:5284` (dead reference — never invoked in mock mode). The live API URL is **not** baked in (verified: `azurecontainerapps` absent from the bundle).

## Cost / resources

- No new resources (RG count unchanged at 9). SWA **Free** (adds $0). Container App still **minReplicas=0**. No SQL/AI/AKS/ACR. Idle ≈ $0.

## Source changes this gate

- **None committed.** New doc files are uncommitted. `dist/` is a build artifact (git-ignored). No source code modified.

## Limitations / next gates

1. **Live-API wiring deferred** → `AZURE_FRONTEND_CORS_FIX_V0.1`: add the SWA origin to `Program.cs` CORS `WithOrigins` (ideally make origins config-driven), rebuild + redeploy the API image, then build the SPA with `VITE_INSURANCE_API_MODE=backend` + `VITE_INSURANCE_API_BASE_URL=<api-url>` and redeploy. Backend already returns data without SQL, so end-to-end should work once CORS allows the origin.
2. **Auth/protected flows** — none configured; deferred.
3. **SQL** — still deferred (later SQL gate).
4. **AI** — Mock until an explicit AI gate.

## Cleanup / rollback

Frontend content lives in the SWA; to clear everything: `az group delete -n rg-iap-demo --yes --no-wait`. (Re-deploying `./dist` replaces content in place.)
