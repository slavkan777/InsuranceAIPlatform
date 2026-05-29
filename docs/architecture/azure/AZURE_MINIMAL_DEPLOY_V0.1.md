# Azure Minimal Deploy — V0.1 (DEPLOYED)

First real Azure deployment of InsuranceAIPlatform. Operator-checkpointed (personal account, `LOGIN_APPROVED` + `DEPLOY_APPROVED`). **Outcome: DEPLOYED_MINIMAL.**

## What was deployed

- **Account:** personal Azure subscription (`Azure subscription 1`), **westeurope**. *(Not the corporate tenant — explicitly switched.)*
- **Resource group:** `rg-iap-demo` (subscription-scope `infra/main.bicep`).
- **API:** Azure Container Apps `iap-demo-api`, image `ghcr.io/slavkan777/insuranceai-api:ce1a1e5`, **minReplicas=0** (scale-to-zero), maxReplicas=2, `/health` probes, user-assigned Managed Identity, `AiProvider__Mode=Mock`.
- **Image:** built from `server/Dockerfile`, pushed to **GHCR** (public package), pulled anonymously by ACA.
- **Frontend resource:** Static Web App `iap-demo-swa` (Free) — provisioned; content deploy deferred (see below).
- **Supporting:** Log Analytics + App Insights (capped/sampled), Key Vault (RBAC), Storage (4 containers, shared-key disabled, lifecycle TTL).
- **SQL:** **deferred** (`enableSql=false`) — API runs without a DB.
- **AI:** **off** (`enableAi=false`). **AKS:** none. **ACR:** none.

## Deploy command (no secrets)

```bash
az deployment sub create \
  --name iap-min-<timestamp> \
  --location westeurope \
  --template-file infra/main.bicep \
  --parameters environmentName=demo location=westeurope prefix=iap \
               enableAi=false enableSql=false blobTtlDays=14 \
               insuranceApiImage=ghcr.io/slavkan777/insuranceai-api:ce1a1e5
```
`provisioningState: Succeeded`.

## `enableSql` toggle (this gate, uncommitted)

`main.bicep` gained `param enableSql bool = false`; the `sql` module is now `if (enableSql)` and the `sqlServerFqdn` output is `enableSql ? sql!.outputs.serverFqdn : ''`. Lets the minimal deploy skip SQL cleanly. `az bicep build` → 0 errors / 0 warnings.

## Smoke test

```
GET https://iap-demo-api.bluehill-ebdd0494.westeurope.azurecontainerapps.io/health
→ 200 {"status":"Healthy","service":"InsuranceAIPlatform.Api","environment":"Production",...}
```
Served after a cold start from scale-to-zero — confirms our image (not the placeholder) is live.

## Frontend (SWA) — deferred content deploy

The SWA resource is live (`kind-meadow-03cf73103.7.azurestaticapps.net`) but empty. To publish `dist/`:
- `TOKEN=$(az staticwebapp secrets list -g rg-iap-demo -n iap-demo-swa --query properties.apiKey -o tsv)` then `swa deploy ./dist --deployment-token "$TOKEN" --env production` (needs `@azure/static-web-apps-cli`); **or** wire the GitHub Action with the SWA token as a repo secret.
- Note: `dist/` builds in mock mode; wiring the SPA to the live API needs `VITE_INSURANCE_API_MODE=backend` + the API URL + CORS (SWA **linked backend** recommended, no code change). Deferred to a frontend-deploy gate.

## Cleanup / rollback

```bash
az group delete -n rg-iap-demo --yes --no-wait   # removes everything created here
```

## Next gates
- Commit gate for the uncommitted `enableSql` edit + these docs.
- Frontend-deploy gate (SWA content + API wiring).
- (Later) SQL gate, AI controlled-demo gate.
