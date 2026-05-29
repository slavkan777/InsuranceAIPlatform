# Azure Minimal Deploy — Prep (v0.1)

Deploy-**preparation** record. Nothing is deployed here; no Azure login, no resources, no image push, no secrets. All files below are left **uncommitted** for the commit gate. Verdict: **ACCEPT_DEPLOY_PREP_READY**.

## What this gate produced

| File | Purpose |
|---|---|
| `server/Dockerfile` | Multi-stage `net9.0` image for the API (BFF/modular monolith). SDK builds + publishes; `aspnet:9.0` runs **non-root** (`USER $APP_UID`) on **:8080**. No secrets baked in. |
| `server/.dockerignore` | Trims the build context (excludes `bin/`, `obj/`, test project, `*.md`). Verified context = **503 KB**. |
| `staticwebapp.config.json` (root) | SWA SPA **navigationFallback** (react-router deep links) + basic security headers; excludes `/api/*` and assets from the fallback. |
| `.github/workflows/azure-deploy-demo.yml` | Now a full **guarded** blueprint: `validate` (bicep + backend + frontend build) always; `build-and-push` (GHCR) + `deploy` (OIDC) jobs are `confirm==DEPLOY`-guarded with steps **commented** until the deploy gate. Adds `packages: write`. Still `workflow_dispatch`-only — never runs on push. |
| `infra/modules/container-apps.bicep` | Added Container App **Liveness/Readiness/Startup probes** → `GET /health` on 8080. |

## Offline validation (all green — Docker WAS available)

| Check | Command | Result |
|---|---|---|
| Docker image build | `docker build -f server/Dockerfile server` | **EXIT 0** — image `insurance-api:preptest` (`sha256:d2dda6f2…`); restored 8 projects, `dotnet publish -c Release`, exported |
| Backend build | (inside docker publish) + `dotnet test` build | **OK** — `InsuranceAIPlatform.Api.dll` + 6 services + BuildingBlocks |
| Backend unit tests | `dotnet test …Tests -c Release` | **137 passed / 0 failed / 0 skipped** (6 s, no DB) |
| Frontend build | `npm run build` (`tsc -b && vite build`) | **EXIT 0** → `dist/` (js 436 KB / gzip 131 KB) |
| Bicep compile | `az bicep build --file infra/main.bicep` | **EXIT 0**, 0/0, ARM 1322 lines (probes included) |
| Secret scan | rg over created/edited files | **clean** (no secrets; no GUIDs added) |

Local toolchain: dotnet 9.0.304, Docker 20.10.23, node v21.3.0.

## Deploy shape (first real deploy)

```
GitHub Actions (manual, confirm=DEPLOY)
  └─ build-and-push → docker build server/ → ghcr.io/slavkan777/insurance-api:<sha>
  └─ deploy → az login (OIDC, no client secret) → az deployment sub create
                 main.bicep (+ insuranceApiImage=<that tag>, sqlAdminObjectId=<entra group oid>)
                   → RG → Container Apps (scale-to-zero) pulls the image on :8080 (probes /health)
                   → Static Web App (Free) serves dist/ ; calls API
                   → SQL Serverless (auto-pause), Storage (TTL), Key Vault + MI, monitoring
```

The API comes up healthy with **Mock AI + in-memory claim reads** even before SQL is wired — `/health` has no DB dependency and there is **no startup migration** (lazy connection), so cold-start is safe.

## Known gaps to resolve at/after deploy (documented, NOT changed here — product code untouched)

1. **CORS.** The API currently allows only `http://localhost:5173`. Two options, no code change preferred:
   - **Recommended:** SWA **linked backend** → SWA proxies `/api/*` to the Container App **same-origin** → no CORS change, no code edit. (`staticwebapp.config.json` already keeps `/api/*` out of the SPA fallback.)
   - Alternative: env-driven allowed-origins — needs a small `Program.cs` change → a separate **code** gate (out of scope here).
2. **SQL wiring.** DB-backed routes need `INSURANCEAI_CONNECTION_STRING` (Entra/MI passwordless: `Authentication=Active Directory Default`) + a one-time `InsuranceAIPlatform.DbMigrator` run against Azure SQL. Deferred to a SQL gate; minimal deploy runs without it.
3. **GHCR image visibility.** Simplest pull path: make the `insurance-api` GHCR package **public** after first push → Container Apps pulls anonymously (no registry secret). Otherwise configure a registry credential.
4. **`sqlAdminObjectId`.** Real Entra group/user objectId supplied at deploy (`--parameters … sqlAdminObjectId=<oid>`), **never committed** (placeholder all-zeros stays in the template).

## Operator checklist — what Slava does before/at the deploy gate

These are **manual** (Azure account + GitHub settings); the executor will not do them:

1. **`az login`** (interactive) — in the deploy gate only.
2. `az account show` → confirm the right **subscription**; `az account set --subscription <id>` if needed.
3. Confirm **region** `westeurope` (or tell me to change it).
4. Confirm the **$30 budget + alerts** are still active (they are).
5. Create/choose an **Entra group** to be SQL admin → capture its **objectId** (passed as `sqlAdminObjectId` at deploy).
6. **OIDC**: create an Entra app registration + **federated credential** trusting `repo:slavkan777/InsuranceAIPlatform:environment:azure-demo`; assign it Contributor on the subscription/RG; add repo **Actions secrets** `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID` (these are identifiers, set via GitHub UI — not pasted in chat). *(Exact `az ad app federated-credential create` shape comes in the deploy gate.)*
7. Approve the first **resource creation** (`az deployment sub create`).
8. After first GHCR push: set the **package visibility** (public) or add a pull credential.

**Do NOT paste anywhere (chat, committed files):** GitHub PAT, any API key, the `DEEPSEEK_API_KEY`, SQL passwords (there are none — Entra-only), or client-secret values. Subscription/tenant/client **IDs** go into GitHub Actions secrets via the UI, not the chat.

## Next gate

**`AZURE_MINIMAL_DEPLOY_V0.1`** — will require `az login` (operator) and **will create Azure resources**:
- activate the commented GHCR build/push steps → publish `insurance-api:<sha>`;
- activate the OIDC login + `az deployment sub create` of `main.bicep` (+ image tag + `sqlAdminObjectId`);
- verify the Container App responds on `/health`; confirm spend stays ~$0 (scale-to-zero idle).
Boundaries then: still no real AI (Mock), no real PII, human-approval preserved, single region, budget-first.
