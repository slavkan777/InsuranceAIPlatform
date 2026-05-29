# Azure Deployment Runbook — InsuranceAIPlatform (v0.1)

**Current boundary: NO-DEPLOY.** This runbook documents the *future* deploy procedure. Nothing here has been executed. Do not run `az login` / `az deployment` outside the explicit `AZURE_MINIMAL_DEPLOY_V0.1` gate.

## Status checklist
| Step | State |
|---|---|
| Azure account + subscription | ✅ done (operator) |
| Budget $30 + alerts $5–50 | ✅ done (operator), spend $0 |
| Region chosen | ⏳ confirm at deploy (default `westeurope`) |
| Naming approved | ✅ `iap-<env>-*` / `rg-iap-<env>` |
| IaC skeleton (`infra/`) | ✅ compiles, **not applied** |
| Dockerfile for `insurance-api` | ⛔ TODO (deploy gate) |
| GHCR image published | ⛔ TODO (deploy gate) |
| GitHub→Azure OIDC federation | ⛔ TODO (deploy gate) |
| Resources deployed | ⛔ none |

## Prerequisites before first deploy (operator + deploy gate)
1. **Dockerfile** for the .NET API (multi-stage, exposes `:8080`) + `.dockerignore` — authored in the deploy gate.
2. **Publish image to GHCR** (`ghcr.io/<owner>/insurance-api:<sha>`) — free, public-repo.
3. **OIDC federated credential**: an Entra app/UAMI federated to the GitHub repo (no client secret). Configure repo secrets `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID` (ids only — **not** secrets/passwords).
4. **Entra group for SQL admin** → its object id becomes `sqlAdminObjectId` (supplied at deploy, never committed).

## Deploy sequence (future — `AZURE_MINIMAL_DEPLOY_V0.1` onward)
```bash
# 1. login (CI: OIDC; local: interactive) — ONLY inside the deploy gate
az login   # or azure/login@v2 with id-token in CI

# 2. validate (no changes)
az bicep build --file infra/main.bicep
az deployment sub what-if \
  --location westeurope \
  --template-file infra/main.bicep \
  --parameters infra/parameters/demo.bicepparam insuranceApiImage=ghcr.io/<owner>/insurance-api:<sha> sqlAdminObjectId=<entra-group-oid>

# 3. deploy
az deployment sub create \
  --location westeurope \
  --template-file infra/main.bicep \
  --parameters infra/parameters/demo.bicepparam insuranceApiImage=ghcr.io/<owner>/insurance-api:<sha> sqlAdminObjectId=<entra-group-oid>

# 4. DB schema (DbMigrator against Azure SQL via Entra auth)
#    INSURANCEAI_CONNECTION_STRING="Server=tcp:<sqlFqdn>;Database=InsuranceAIPlatform;Authentication=Active Directory Default;Encrypt=True"
dotnet run --project server/InsuranceAIPlatform.DbMigrator

# 5. SWA: connect the GitHub repo build (token-based) to publish the SPA
```

## Post-deploy verification (future)
- Public SPA loads from SWA (static, no compute woken).
- Login → first authed call cold-starts `insurance-api` (1–3 s) → 200.
- Idle 1 h → Container App at 0 replicas, SQL **paused** (verify in portal).
- Budget unmoved beyond ~$0; AI still Mock.

## Rollback / teardown
- Rollback: redeploy previous image tag (Container Apps revision).
- Teardown: `az group delete -n rg-iap-<env>` removes everything (single RG).

## Hard rules (every deploy gate)
- `dev` only for source; `main` only via a separate release gate; no force-push.
- No secret in repo/CI logs; Key Vault + Managed Identity; ids (not secrets) in repo CI vars.
- AI advisory-only, Mock default, real behind a manual button + token caps; synthetic data only.
- Every paid component defaults to scale-to-zero / auto-pause / free-tier / manual.
