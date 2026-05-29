# Azure Frontend Redeploy — Product i18n (EN default / UA switch) — V0.1

Rebuilt the SPA with product-wide i18n (English default + Ukrainian switch) and the product-copy rewrite, then **redeployed to the existing Azure Static Web App**. No new/changed Azure resources; the live API integration (backend mode + CORS) is preserved.

## Build (backend mode)

```
VITE_INSURANCE_API_MODE=backend
VITE_INSURANCE_API_BASE_URL=https://iap-demo-api.bluehill-ebdd0494.westeurope.azurecontainerapps.io
npm run build
```

- `tsc -b && vite build` → clean. Output: `dist/` — bundle `assets/index-C8i01ucW.js` (the live API URL is baked in; the `localhost:5284` dev fallback is tree-shaken out → confirms backend mode, no mock regression).
- `staticwebapp.config.json` copied into `dist/` before deploy (SPA navigation fallback + security headers — the build does not auto-copy it; there is no `public/` dir).
- Static checks on `dist/`: **no** forbidden product terms, **no** secrets, **both** EN and UA strings present.

## Deploy

- Target: existing SWA **`iap-demo-swa`** (`https://kind-meadow-03cf73103.7.azurestaticapps.net`), **Free** SKU, environment `production`.
- Tool: `@azure/static-web-apps-cli@2.0.9` via `npx`.
- Token: `az staticwebapp secrets list -n iap-demo-swa -g rg-iap-demo --query properties.apiKey -o tsv` → loaded into `SWA_CLI_DEPLOYMENT_TOKEN` (env only; never printed, never on argv, never committed). SWA CLI reads the token from the env var.
- Result: `√ Project deployed to https://kind-meadow-03cf73103.7.azurestaticapps.net`.

## Live smoke — PASS

HTTP:

| Check | Result |
|---|---|
| SWA root `GET /` | `200`, references the new `index-C8i01ucW.js` (deploy live) |
| Deep link `GET /claims` | `200` (SPA fallback) |
| API `GET /health` | `200` Healthy (Production) |
| `GET /api/claims/summary` (Origin = SWA) | `200` + `access-control-allow-origin: <SWA>`; body `{totalActive:47,...}` |

Headless Chromium (1440×900) against the live URL — 12/12 assertions true:

| Assertion | Result |
|---|---|
| First load (empty localStorage) shows **English** hero "AI-Assisted Insurance Claims Workbench" + "Sign in" + value bullets | ✅ |
| No Ukrainian hero leaking on English default | ✅ |
| Click **UA** → "AI-помічник для обробки страхових випадків" + "Увійти"; English hero gone | ✅ |
| Logged-in dashboard + left nav render English | ✅ |
| Walkthrough page shows product copy ("Platform capabilities") | ✅ |
| No forbidden terms on dashboard or walkthrough (walking skeleton / portfolio / interview / Azure-ready / Наступна фаза / Архітектура системи / Три шари) | ✅ |

Screenshots: `docs/portfolio/screenshots/i18n-01-login-en.png`, `i18n-02-login-ua.png`, `i18n-03-dashboard-en.png`, `i18n-04-demo-en.png`.

## Resources / cost (unchanged)

Content-only deploy to the existing SWA. **No** Azure resource created/deleted; **no** SKU/pricing change; **no** backend redeploy, image push, SQL, AI, AKS, ACR, or workflow run. SWA **Free**; ACA **minReplicas=0**; idle ≈ $0. Resource group `rg-iap-demo` unchanged.

## Known limitations (unchanged from prior gate)

- List endpoints `/api/claims` + `/api/customers` still `500` (Azure SQL deferred) → SPA seed fallback; UI stays complete.
- AI is Mock-only; demo auth is client-side.
- Synthetic data-layer strings (golden-claim content, dashboard fallback labels, filter option values) remain in their source language — see `docs/architecture/frontend/I18N_PRODUCT_COPY_V0.1.md` → "Known remaining strings".

## Rollback

```bash
# Revert frontend to the previous bundle: re-deploy the prior dist, or rebuild the prior commit and redeploy ./dist to iap-demo-swa.
# (API unchanged this gate — no API rollback needed.)
```
