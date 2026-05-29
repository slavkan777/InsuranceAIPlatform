# infra/ — Azure IaC skeleton (Bicep)

**Status: SKELETON — NOT DEPLOYED.** No `az deployment` has been run; no Azure resources exist for this project. This directory compiles (`az bicep build` → 0 errors / 0 warnings) but is intentionally *not* applied until the explicit `AZURE_MINIMAL_DEPLOY_V0.1` gate.

## Layout
```
infra/
  main.bicep                  # subscription-scope entrypoint: creates RG + wires modules
  parameters/
    demo.bicepparam           # safe demo params (no secrets / no subscription id)
  modules/
    monitoring.bicep          # Log Analytics + App Insights (sampling, 30d retention, daily cap)
    identity.bicep            # user-assigned managed identity (passwordless)
    key-vault.bicep           # Key Vault (RBAC) + Secrets User role for the app identity
    storage.bicep             # Storage + 4 blob containers + lifecycle TTL + Blob role
    container-apps.bicep      # Container Apps env + insurance-api (minReplicas=0 scale-to-zero)
    sql-serverless.bicep      # Azure SQL GP_S serverless, auto-pause 60m, Entra-only auth
    static-web-app.bicep      # Static Web Apps (Free)
    ai-optional.bicep         # Azure OpenAI + Document Intelligence (F0) + AI Search (Free) — only if enableAi=true
    budgets-notes.md          # budget created manually by operator; budget-as-code optional later
  README.md
```

## What this models (cost-shaped)
- **Public = static + free** (Static Web Apps). No backend/DB/AI on anonymous load.
- **insurance-api** Container App `minReplicas=0` → **$0 idle**, cold-start on first authed request.
- **Azure SQL Serverless** `autoPauseDelay=60` → compute **$0 when idle**.
- **Passwordless everywhere**: user-assigned Managed Identity + Key Vault (RBAC) + Storage/SQL via Entra. `allowSharedKeyAccess=false`, SQL `azureADOnlyAuthentication=true` → **no account keys / SQL passwords anywhere**.
- **Blob lifecycle TTL** auto-deletes demo artifacts after `blobTtlDays`.
- **AI is opt-in** (`enableAi=false` default) and uses **F0/Free** tiers; AI Search Basic (the cost trap) is never used.
- **AKS** intentionally absent (deferred — 24/7 node cost).

## Validate (offline — no login, no deploy)
```bash
az bicep build --file infra/main.bicep        # compile to ARM (what we run in this gate)
# az bicep build --file infra/main.bicep --stdout   # inspect emitted template
```

## Deploy — LATER ONLY (AZURE_MINIMAL_DEPLOY_V0.1, after GPT audit + commit)
```bash
# Requires: az login (federated/OIDC in CI), a chosen region, and at deploy time:
#   - insuranceApiImage = ghcr.io/<owner>/insurance-api:<tag>
#   - sqlAdminObjectId  = <Entra group object id>   (supplied at deploy, NEVER committed)
az deployment sub create \
  --location westeurope \
  --template-file infra/main.bicep \
  --parameters infra/parameters/demo.bicepparam
```

## Secret policy
No secrets, API keys, connection strings, subscription ids, or tenant secrets live in this directory. Runtime secrets are added to Key Vault **out of band** and read by the app via Managed Identity. `DEEPSEEK_API_KEY` / Azure AI keys are never committed or logged.
