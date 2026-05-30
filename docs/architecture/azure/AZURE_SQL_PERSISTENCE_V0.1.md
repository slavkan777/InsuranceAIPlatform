# Azure SQL Persistence — Deployment Record
**Date:** 2026-05-30
**Gate:** AZURE_SQL_FULL_PERSISTENCE_ALL_PAGES_DEPLOY_PUSH_V0.1

---

## Summary

This gate completed the wiring of Azure SQL as the live persistence backend for the
InsuranceAIPlatform API. The prior 500 errors on customer list and create endpoints
(manifesting in the UI as "Failed to fetch") are resolved. List and create endpoints
are now served from Azure SQL.

---

## SQL Resource

| Property | Value |
|---|---|
| Server FQDN | `iap-sql-r2-6c7g465.database.windows.net` |
| Resource group | `rg-iap-demo` |
| Region | `germanywestcentral` |
| Database name | `InsuranceAIPlatform` |
| SKU | `GP_S_Gen5_1` — General Purpose Serverless, 1 vCore max, 0.5 vCore min |
| Auto-pause | 60 minutes idle |
| Max size | 2 GB |
| Backup redundancy | Local (LRS) |
| Auth method | SQL authentication (admin login `iapadmin`) |

### Why germanywestcentral, not westeurope

West Europe and North Europe returned `RegionDoesNotAllowProvisioning` for new SQL
server creation (capacity constraint at time of provisioning). `germanywestcentral`
was the nearest region accepting new SQL servers. Round-trip latency from the
Container App in West Europe is approximately 10 ms — acceptable for a demo workload.
The `AllowAllAzureServices` firewall rule permits the Container App to reach SQL
cross-region.

---

## Authentication and Secret Handling

The SQL admin password was generated securely and is stored **only** as an Azure
Container App secret named `sql-connection` (full connection string value). It is:

- Never committed to the repository.
- Never printed in logs or documentation.
- Not present in any `appsettings.json`, `.env`, or environment variable visible
  outside the Container App runtime.

An Entra-only / Managed Identity Bicep module (`infra/modules/sql-serverless.bicep`)
exists in the repository as an alternative IaC path. It was **not** used at this gate
due to operational and tooling constraints. SQL authentication + Container App secret
is the gate-approved path (§7.1 of this gate).

---

## Firewall

| Rule | Allow range | Status |
|---|---|---|
| `AllowAllAzureServices` | 0.0.0.0 – 0.0.0.0 | Active (permanent) |
| Local migration client-IP rule | *(removed)* | Deleted post-migration |

Only the Azure-services rule remains. The temporary client-IP rule added to run
the DbMigrator locally was removed after successful migration.

---

## Deploy Method (Imperative az Commands)

No secrets are shown below. The connection string was passed inline only at the
Container App secret-set step and is not recorded in this document.

```
# 1. Create SQL server
az sql server create \
  --name iap-sql-r2-6c7g465 \
  --resource-group rg-iap-demo \
  --location germanywestcentral \
  --admin-user iapadmin \
  --admin-password <generated-secret>

# 2. Create database (Serverless, GP_S_Gen5_1)
az sql db create \
  --server iap-sql-r2-6c7g465 \
  --resource-group rg-iap-demo \
  --name InsuranceAIPlatform \
  --edition GeneralPurpose \
  --family Gen5 \
  --capacity 1 \
  --compute-model Serverless \
  --auto-pause-delay 60 \
  --max-size 2GB \
  --backup-storage-redundancy Local

# 3. Firewall — allow Azure services
az sql server firewall-rule create \
  --server iap-sql-r2-6c7g465 \
  --resource-group rg-iap-demo \
  --name AllowAllAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# 4. Wire connection string into Container App (connection string value not shown)
az containerapp secret set \
  --name iap-demo-api \
  --resource-group rg-iap-demo \
  --secrets sql-connection=<connection-string>

az containerapp update \
  --name iap-demo-api \
  --resource-group rg-iap-demo \
  --set-env-vars ConnectionStrings__InsuranceAIPlatform=secretref:sql-connection
```

The Container App update produced revision `iap-demo-api--0000002`.

---

## Schema and Data

Migrations and seed were applied via the `InsuranceAIPlatform.DbMigrator` console app
(run locally, pointed at the Azure SQL endpoint via a temporary client-IP firewall rule
that was subsequently removed). See
[SQL_PERSISTENCE_MODEL_V0.1.md](../backend/SQL_PERSISTENCE_MODEL_V0.1.md) for the
full context/schema/entity/seed inventory.

---

## Live Verification (2026-05-30)

| Check | Result |
|---|---|
| `GET /health` | 200 |
| `GET /api/customers` | 200, `{"total":200,...}` |
| `POST /api/customers` | 200, customer CUST-T0201 created |
| CUST-T0201 persists on page refresh | Confirmed |
| `GET /api/claims` | 200, 15 claims |
| `Access-Control-Allow-Origin` on POST | SWA origin header present |
| UI e2e (headless Chromium, live SWA) | 8/8 checks pass |
| "Failed to fetch" on customer create | Resolved — no longer occurs |

SWA URL: `https://kind-meadow-03cf73103.7.azurestaticapps.net`

---

## Rollback

**Revert API to no-SQL (reverts to prior fallback behavior):**
```
az containerapp update \
  --name iap-demo-api \
  --resource-group rg-iap-demo \
  --remove-env-vars ConnectionStrings__InsuranceAIPlatform
```
Alternatively, roll back to revision `iap-demo-api--0000001`.

**Delete SQL entirely (stops all SQL cost):**
```
az sql server delete \
  --name iap-sql-r2-6c7g465 \
  --resource-group rg-iap-demo \
  --yes
```

Frontend was unchanged at this gate — no frontend rollback step.

---

## Cost Summary

See [AZURE_COST_POST_SQL_CHECK_V0.1.md](AZURE_COST_POST_SQL_CHECK_V0.1.md) for full
cost analysis. The short version: SQL serverless auto-pauses after 60 minutes idle,
leaving only storage billing at that point (~$0.06–0.20/mo). Realistic total
incremental SQL cost under realistic demo usage is estimated at $1–5/mo, well below
the $30/mo stop-line.

---

## Limitations

- Golden-claim sub-resources (documents, AI evidence, risks, approval, audit) and
  dashboard summary metrics remain curated in-memory fixtures by design.
- AI analysis is Mock-only at this gate.
- Demo auth is client-side only.
- All seed data is synthetic — no real PII.
- SQL server is in `germanywestcentral` while the Container App is in West Europe
  (cross-region, ~10 ms latency; acceptable for demo, non-ideal for production).
