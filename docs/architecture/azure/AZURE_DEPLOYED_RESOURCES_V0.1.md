# Azure Deployed Resources — V0.1

Resource group **`rg-iap-demo`** (westeurope) — 8 resources, deployed 2026-05-29.

| Resource | Name | Type | Cost-critical setting |
|---|---|---|---|
| Container App | `iap-demo-api` | `Microsoft.App/containerApps` | **minReplicas=0** (scale-to-zero), maxReplicas=2; image `ghcr.io/slavkan777/insuranceai-api:ce1a1e5`; `/health` probes; UAMI |
| Container Apps env | `iap-demo-cae` | `Microsoft.App/managedEnvironments` | Consumption; logs → Log Analytics |
| Static Web App | `iap-demo-swa` | `Microsoft.Web/staticSites` | **Free** sku |
| Managed Identity | `iap-demo-api-mi` | `Microsoft.ManagedIdentity/userAssignedIdentities` | passwordless app identity |
| Key Vault | `iapdemokv…` | `Microsoft.KeyVault/vaults` | RBAC; no secrets stored yet |
| Storage | `iapdemost…` | `Microsoft.Storage/storageAccounts` | `allowSharedKeyAccess=false`; 4 containers; lifecycle TTL |
| Log Analytics | `iap-demo-law` | `Microsoft.OperationalInsights/workspaces` | 30d retention + 1 GB/day cap |
| App Insights | `iap-demo-appi` | `Microsoft.Insights/components` | workspace-based; 50% sampling |

**Not deployed (by design):** Azure SQL (`enableSql=false`), AI services (`enableAi=false`), AKS, ACR.

**Endpoints**
- API: `https://iap-demo-api.bluehill-ebdd0494.westeurope.azurecontainerapps.io` (`/health` → 200)
- SWA: `https://kind-meadow-03cf73103.7.azurestaticapps.net` (empty until frontend content deploy)

**Identifiers** (subscription/tenant IDs intentionally omitted from this doc).
