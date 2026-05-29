# Budgets — notes (not deployed by Bicep)

The cost budget for this project was **created manually by the operator in the Azure Portal** (the recommended first step, before any resource exists):

- Monthly budget: **$30 USD**
- Alerts at: **$5 / $10 / $20 / $30 / $50** (actual + forecast)
- Current spend: **$0**

> ⚠️ Budget alerts **notify only** — they do **not** stop or delete resources. Cost is enforced by the **architecture** (scale-to-zero Container Apps, SQL Serverless auto-pause, free/F0 AI tiers, GHCR over ACR, blob TTL, log sampling), not by alerts.

## Optional future: budget-as-code
A `Microsoft.Consumption/budgets` resource can be added at subscription scope in a later gate if we want the alert thresholds version-controlled. It is intentionally **omitted** from `main.bicep` now because:
1. The operator already created the budget manually.
2. Subscription-scope budget deployment needs the subscription id, which we keep out of the repo.

If added later, it lives in a dedicated subscription-scope module invoked separately — never embedding the subscription id in source.
