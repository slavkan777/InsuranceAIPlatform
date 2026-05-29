# Azure Cost Governance & Manual Setup — InsuranceAIPlatform (v0.1)

**Planning only.** No budgets/resources created yet — this defines the model and the manual steps Slava performs in `AZURE_ACCOUNT_AND_BUDGET_SETUP_MANUAL_V0.1`.

## 1. Budget model

| Tier | Amount | Meaning |
|---|---|---|
| **Real target** | **$5–10/month** | Expected steady-state with the design below (often closer to $0–5 if GHCR replaces ACR and SWA Free suffices). |
| **Comfort cap** | **$30/month** | If exceeded, stop and investigate before adding more. |
| **Architecture ceiling** | **~$1000/month** | The explainable upper bound *if* every component were scaled up (AKS, always-on ACA, AI Search Basic, higher SQL tier, heavy AI). Never the operating point — the interview answer for "how would this scale?". |

### Budget alerts (Azure Cost Management — notify only)
Set actual + forecasted alerts at **$5 / $10 / $20 / $30 / $50**.

> ⚠️ **Budget alerts NOTIFY; they do NOT stop or delete resources.** Cost control is enforced by the *architecture* (scale-to-zero, auto-pause, free tiers, manual AI), not by alerts. Alerts are the smoke detector, not the sprinkler.

## 2. Top cost risks → mitigations

| # | Risk | Mitigation |
|---|---|---|
| 1 | Azure SQL not auto-pausing | Serverless tier + auto-pause delay = 1 h; verify "paused" state in portal after idle |
| 2 | Container Apps `minReplicas > 0` | Always set `minReplicas = 0`; confirm scale-to-zero in revision config |
| 3 | AI loops / background AI | AI only on explicit user action; no timers/queues auto-firing AI; Mock default |
| 4 | Large Document Intelligence batches | Stay on **F0** free tier; manual trigger; cap pages per request |
| 5 | Azure AI Search **Basic SKU** (~$75/mo) | **Free tier only**; never provision Basic for a demo |
| 6 | Log/telemetry volume | App Insights **sampling** + 30-day retention + **daily cap** |
| 7 | Blob growth | Lifecycle policy auto-deletes demo blobs after N days |
| 8 | ACR Basic fixed ~$5/mo | Use **GHCR ($0)**; keep ACR as arch-ready option only |
| 9 | Orphaned/forgotten resources | One resource group `rg-iap-prod`; tear down by deleting the RG; monthly cost review |
| 10 | Free-credit expiry surprise | $200 trial credit lasts 30 days; after that, always-free tiers + the design above keep it cheap — re-confirm after day 30 |

## 3. Naming convention (proposed — confirm in gate 1)

```
Resource group:        rg-iap-prod
Static Web App:        iap-prod-swa
Container App env:     iap-prod-cae
Container App (api):   iap-prod-api
Container Apps Job:    iap-prod-cleanup
Azure SQL server/db:   iap-prod-sql / InsuranceAIPlatform
Storage account:       iapprodst<rand>      (lowercase, ≤24 chars, globally unique)
Key Vault:             iap-prod-kv<rand>    (globally unique)
App Insights:          iap-prod-appi
Log Analytics:         iap-prod-law
```
Region: pick **one low-latency, standard-priced** region near the user (e.g. **West Europe** or **North Europe** for a Ukraine-based user; **East US** as a cheap default). Keep ALL resources in one region to avoid egress.

## 4. Slava manual setup checklist (gate `AZURE_ACCOUNT_AND_BUDGET_SETUP_MANUAL_V0.1`)

**DO (manual, in Portal):**
1. Create / sign in to an Azure account (free trial = $200 credit for 30 days + always-free tiers).
2. Confirm the subscription to use (note its ID — do **not** paste it in chat/handoff; reference as "the iap subscription").
3. **Create budget alerts first** ($5/$10/$20/$30/$50, actual + forecast, email to yourself).
4. Choose + record the region.
5. Approve the naming convention above (or amend).
6. Ensure the GitHub repo is reachable for later OIDC federation (no secret yet).

**DO NOT yet:**
- ❌ Create any resource manually (RG, SWA, Container App, SQL, Storage, Key Vault, App Insights).
- ❌ Create any AI deployment (OpenAI/Foundry/Document Intelligence/Search).
- ❌ Paste any secret, key, subscription ID, or connection string anywhere.
- ❌ Enable any paid/always-on/background service.
- ❌ Run `az login` outside an explicitly-opened deploy gate.

Everything except the account, subscription selection, budget alerts, region, and naming is created later **by IaC (Bicep) through CI**, not by hand — so the environment is reproducible and tear-down-able.
