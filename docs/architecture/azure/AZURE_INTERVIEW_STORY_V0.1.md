# Azure Interview Story — InsuranceAIPlatform (v0.1)

## One-liner
"An auto-insurance claim AI workbench — .NET 9 microservice-shaped backend + React SPA — deployed to Azure as **scale-to-zero Container Apps behind Static Web Apps auth**, with **governed (advisory-only) AI**, engineered to look production-shaped while costing a few dollars a month."

## 60-second version
Local-first and verified (137 unit + 89 Playwright E2E), then Azure as IaC (Bicep): a **free static SPA** on Static Web Apps for the public surface; the backend in **one Container App that scales to zero** so it costs nothing idle; **Azure SQL Serverless that auto-pauses**; **passwordless** access via user-assigned Managed Identity + Key Vault (RBAC) — no keys or SQL passwords anywhere; **CI/CD via GitHub Actions with OIDC** (no stored Azure credentials); **observability** in App Insights with sampling + a daily cap; and **AI behind a manual button** through a provider-agnostic interface (Mock default; Azure OpenAI / Document Intelligence F0 / AI Search Free opt-in). Cost is enforced by architecture — scale-to-zero, auto-pause, free tiers, blob TTL — with budget alerts as a backstop.

## "Why Container Apps, not AKS?"
AKS bills a node pool 24/7 (~$70+/mo) plus cluster ops. At 1–2 users/day that's pure waste. **Container Apps gives serverless containers, KEDA scale-to-zero, and Dapr-readiness at $0 idle.** I deploy a **modular monolith in one container** first — the service boundaries already exist in code (service-owned DbContexts, BFF, outbox), so splitting out an `ai-worker` later is a packaging change, not a rewrite. I'd reach for AKS at real scale / multi-team / fine-grained orchestration — and I can explain exactly when that line is crossed.

## "How is the AI governed?"
Advisory-only: the AI never approves payout, rejects a claim, accuses fraud as fact, changes claim status, or messages a customer. Human approval is always final and every AI action writes an audit row (`ActorType=ai-system`). It's provider-agnostic (`IAiProvider`), **Mock by default**, real calls only on an explicit click, token/page-capped, key in Key Vault, never logged. Synthetic data only — no real PII.

## "How do you keep it cheap?"
| Lever | Effect |
|---|---|
| SWA Free + static public | $0 public surface |
| Container Apps `minReplicas=0` | $0 idle compute |
| SQL Serverless auto-pause | $0 idle DB compute |
| Free/F0 AI tiers, AI Search **Free** (not Basic ~$75) | $0 until manual use |
| GHCR instead of ACR Basic | saves ~$5/mo fixed |
| Blob lifecycle TTL + App Insights sampling/cap | bounded storage + logs |
| Budget alerts $5–50 | backstop (notify, not enforce) |
Real target **$5–10/mo**, comfort cap **$30**, explainable ceiling **~$1000** (AKS + always-on + Basic AI Search + heavy AI).

## Deployed vs architecture-ready (honesty table)
| Capability | Status |
|---|---|
| Local product (UI + API + LocalDB + Mock AI), verified, pushed (`2e9443a`) | ✅ built |
| Bicep IaC skeleton (compiles, 0 errors) | ✅ authored, **not applied** |
| Any Azure resource deployed | ❌ none yet |
| Real AI provider active | ❌ Mock default; real = opt-in |

> The honesty table is itself an interview asset: it shows I distinguish "designed and compiled" from "running in production," and I never overclaim.
