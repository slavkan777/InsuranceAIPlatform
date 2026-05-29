# Azure Frontend CORS Fix + Live-API Wiring — V0.1 (LIVE_API_WIRED)

Converted the live demo from mock-only to **live-API**: config-driven CORS on the API, rebuilt + redeployed the API image, rebuilt the SPA in backend mode, redeployed to the SWA. **Outcome: LIVE_API_WIRED** — browser requests from the SWA origin now reach the live API with correct CORS, end-to-end.

## Root cause (before)

API CORS policy hardcoded `WithOrigins("http://localhost:5173")` (`server/InsuranceAIPlatform.Api/Program.cs`); the SWA origin got no `Access-Control-Allow-Origin`. Preflight from the SWA origin returned `204` with **no** ACAO → browser-blocked. (Backend already served data without SQL.)

## Changes (source — uncommitted, for the next commit gate)

| File | Change |
|---|---|
| `server/InsuranceAIPlatform.Api/Program.cs` | CORS origins now **config-driven**: reads `Cors:AllowedOrigins` (string[]), falls back to `http://localhost:5173` if unset. No `AllowCredentials` → explicit origins, no wildcard. |
| `server/InsuranceAIPlatform.Api/appsettings.json` | Added `"Cors": { "AllowedOrigins": [ "http://localhost:5173", "https://kind-meadow-03cf73103.7.azurestaticapps.net" ] }`. |

**Design note — why base `appsettings.json` (not `appsettings.Production.json`):** the repo's `server/.gitignore` ignores `appsettings.Production.json`, so a prod-only file would not be committable/reproducible. Putting both origins in the tracked base config keeps the fix reproducible from source and copied into the image. Origins remain runtime-overridable via `Cors__AllowedOrigins__N` env vars on the Container App. (A transient `appsettings.Production.json` created during the gate was removed.)

No business logic changed. `dotnet test -c Release` → **137/137 passed**.

## API image + redeploy

- Image: `ghcr.io/slavkan777/insuranceai-api:14d5c81-cors` (digest `sha256:208f9cca…`), built from `server/Dockerfile`, pushed to GHCR (public). Anonymous pull OK.
- Container App `iap-demo-api` updated to the new image → revision **`iap-demo-api--0000001`** (Active, 100% traffic, Running); previous `…--mjdxllx` (`:ce1a1e5`) deprovisioned. **minReplicas=0 / maxReplicas=2 preserved.**

## CORS smoke (after) — PASS

| Request (Origin = SWA) | Result |
|---|---|
| `OPTIONS /api/claims/summary` (preflight) | `204` + `access-control-allow-origin: https://kind-meadow-03cf73103.7.azurestaticapps.net` + `access-control-allow-methods: GET` |
| `GET /api/claims/summary` | `200` JSON + ACAO for the SWA origin |
| `GET` with `Origin: http://localhost:5173` | `200` + ACAO localhost (no dev regression) |
| `GET /health` | `200` Healthy |

## Frontend (backend mode)

- Built with `VITE_INSURANCE_API_MODE=backend` + `VITE_INSURANCE_API_BASE_URL=https://iap-demo-api.bluehill-ebdd0494.westeurope.azurecontainerapps.io`.
- New bundle `assets/index-BJlHvw5H.js` — live API URL baked in; the dead `localhost:5284` fallback was eliminated. Secret scan clean.
- Redeployed `./dist` (with `staticwebapp.config.json`) to `iap-demo-swa` production via SWA CLI 2.0.9 (token via env var, never printed).

## Frontend integration smoke — PASS

- SWA root `200`, references the new `index-BJlHvw5H.js`; deep link `/claims` `200` (SPA fallback); JS/CSS assets `200`.
- Deployed JS contains the live API URL.
- **End-to-end:** browser-equivalent `GET …/api/claims/summary` with `Origin: <SWA>` → `200` JSON + ACAO for the SWA origin (the exact fetch the SPA makes). *(HTTP-level verification; no headless browser run in this environment.)*

## Cost / resources

RG resource count **9** (unchanged — only a Container App revision changed). SWA **Free**. ACA **minReplicas=0**. No SQL/AI/AKS/ACR. Idle ≈ $0.

## Rollback plan

```bash
# Revert API to the previous image (instant revision swap):
az containerapp update -g rg-iap-demo -n iap-demo-api --image ghcr.io/slavkan777/insuranceai-api:ce1a1e5
# Revert frontend to mock mode: rebuild without the VITE_ backend env, then redeploy ./dist to iap-demo-swa.
```

## Limitations

- Live-data path verified at the HTTP/CORS level (no headless-browser console check in this environment).
- Source changes (Program.cs, appsettings.json) are **uncommitted** — next gate commits them.
- SQL still deferred (backend serves seeded in-memory data); AI still Mock.

## Next recommended gate
`AZURE_FRONTEND_CORS_FIX_COMMIT_V0.1` — commit `Program.cs` + `appsettings.json` (+ this doc) to `dev`.
