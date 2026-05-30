# README portfolio section — DRAFT V0.1 (copy-paste candidate)

> Draft only — not applied to `README.md` this gate. Review, then a future gate can splice it in.

---

## InsuranceAIPlatform — Auto Insurance Claim AI Workbench

A full-stack demo of an enterprise "auto claim AI workbench": a .NET 9 API + a React/TypeScript SPA, **deployed live to Azure** as a production-*shaped*, hobby-cost stack.

**Live demo**
- Frontend: https://kind-meadow-03cf73103.7.azurestaticapps.net
- API health: https://iap-demo-api.bluehill-ebdd0494.westeurope.azurecontainerapps.io/health
- Demo login: `demo@insurance.local` / `Demo123!` *(public demo creds; client-side auth only; synthetic data)*

**Architecture**
```
React SPA (Azure Static Web Apps, Free)  ──HTTPS/CORS──>  .NET 9 API (Azure Container Apps, scale-to-zero)
                                                          image: GHCR · Bicep IaC · Managed Identity + Key Vault
                                                          App Insights + Log Analytics · Storage (Entra-only)
                                              Azure SQL (serverless, auto-pause) · deferred: real AI provider
```

**Tech stack**
- **Backend:** .NET 9, ASP.NET Core, modular monolith (per-context service libraries), 137 tests, multi-stage Docker (non-root, :8080).
- **Frontend:** React 18, TypeScript, Vite, Redux Toolkit + redux-saga, React Router, Tailwind.
- **Cloud:** Azure Container Apps (scale-to-zero), Azure SQL Database (serverless, auto-pause), Azure Static Web Apps (Free), GHCR, Bicep IaC, Managed Identity, Key Vault (RBAC), App Insights + Log Analytics.
- **AI:** provider-agnostic `IAiProvider` (Mock default; DeepSeek/Azure OpenAI opt-in).

**Highlights**
- Infrastructure as Code (Bicep, subscription-scope + modules) with `enableSql`/`enableAi` cost toggles.
- Scale-to-zero + Free tiers + capped telemetry → **idle ≈ $0**; $30/mo budget with alerts.
- Config-driven, env-overridable CORS (no wildcard, no credentials).
- Persistent demo data in **Azure SQL** (serverless, auto-pause): customers + claims lists and synthetic-customer creation persist; the SPA still degrades gracefully to seeded data if the API is unreachable.
- Bilingual product UI — **English by default** with a one-click **Ukrainian** switch (persisted in `localStorage`); product-positioned copy, no demo/portfolio language in the live UI.

**Deployment status (honest)**
- ✅ Live: .NET API on Container Apps (`/health` 200), React SPA on Static Web Apps, working CORS, **Azure SQL persistence** (customers + claims lists and synthetic-customer creation persist to SQL), live summary + the full golden-claim (CLM-1006) workspace, Bicep IaC, observability, cost governance.
- ⏸ Deferred (by design for cost): a real AI provider (Mock active). No AKS/ACR (Container Apps + GHCR by choice). Demo auth is client-side, not production SSO.

**Cost / teardown**
~$1–5/mo (scale-to-zero API + serverless auto-pause SQL + Free SWA + capped logs); near-$0 when fully idle. Teardown: `az group delete -n rg-iap-demo --yes --no-wait`.
