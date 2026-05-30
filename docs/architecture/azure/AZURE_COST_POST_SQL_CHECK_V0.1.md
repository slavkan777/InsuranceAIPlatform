# Azure Cost — Post-SQL Deployment Check
**Date:** 2026-05-30
**Gate:** AZURE_SQL_FULL_PERSISTENCE_ALL_PAGES_DEPLOY_PUSH_V0.1

---

## Stop-Line

**$30 USD / month** — total Azure spend must stay below this threshold.
This gate added one new resource (Azure SQL). All other resources are unchanged.

---

## Resource Inventory After This Gate

| Resource | SKU / Tier | Notes |
|---|---|---|
| Container App (`iap-demo-api`) | Consumption plan, minReplicas=0, maxReplicas=2 | Scales to zero when idle |
| Container Apps Managed Environment | Consumption | — |
| Application Insights | — | Linked to the Container App |
| Key Vault | Standard | — |
| User-assigned Managed Identity | — | — |
| Log Analytics Workspace | Pay-as-you-go | — |
| Azure SQL Server | — | `iap-sql-r2-6c7g465`, germanywestcentral |
| Azure SQL Database (`InsuranceAIPlatform`) | GP_S_Gen5_1 Serverless | **New at this gate** |
| Storage Account | Standard LRS | — |
| Static Web App (SWA) | Free tier | Frontend |

---

## SQL Serverless Auto-Pause Economics

The database SKU is **General Purpose Serverless** (`GP_S_Gen5_1`):

- **Auto-pause delay:** 60 minutes of inactivity.
- **While paused:** only storage is billed — approximately $0.115/GB/month. With a
  2 GB max size and minimal actual data, storage cost is roughly **$0.06–0.20/month**.
- **While active:** compute billed per vCore-second (0.5–1 vCore range). A demo session
  of a few hours per week results in negligible active compute cost.

This model means the database is nearly free during periods of no use.

---

## Estimated Monthly Cost

| Component | Estimate |
|---|---|
| SQL Serverless — idle storage | ~$0.06–0.20/mo |
| SQL Serverless — active compute (light demo use) | ~$0.80–4.80/mo |
| **Total incremental SQL cost** | **~$1–5/mo** |
| Container App (scales to zero) | ~$0 idle, minimal active |
| SWA Free tier | $0 |
| Other existing resources | Negligible at demo scale |
| **Realistic total Azure spend** | **Well under $30/mo stop-line** |

These are estimates based on the serverless pricing model and expected low-frequency
demo usage. Costs will be higher if the demo runs continuously or under sustained load.

---

## What Would Push Costs Higher

- Running continuous load tests or scraping the API on a loop (keeps auto-pause from
  triggering, accumulates vCore-second billing).
- Exceeding the 2 GB storage limit (requires a database scale-up or new database).
- Adding paid Azure resources (AKS, ACR, Azure AI services, additional regions,
  premium tiers) — none of these are present at this gate.
- Increasing Container App `maxReplicas` beyond 2 or adding a minimum replica count
  greater than 0.

---

## Stop-Cost Commands

To eliminate all SQL billing immediately:

```
# Deletes the SQL server and the InsuranceAIPlatform database
az sql server delete \
  --name iap-sql-r2-6c7g465 \
  --resource-group rg-iap-demo \
  --yes
```

To also remove the API SQL connection (reverts to prior in-memory-only behavior without
deleting SQL):

```
az containerapp update \
  --name iap-demo-api \
  --resource-group rg-iap-demo \
  --remove-env-vars ConnectionStrings__InsuranceAIPlatform
```

---

## Summary

SQL serverless auto-pause is the key cost-control mechanism for this deployment. As
long as the database is not under sustained active use, total incremental cost remains
in the $1–5/month range — well within the $30/month stop-line. No action is required
to maintain this unless usage patterns change significantly.
