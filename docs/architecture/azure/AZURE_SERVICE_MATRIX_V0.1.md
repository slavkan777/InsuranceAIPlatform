# Azure Service Matrix — InsuranceAIPlatform (v0.1)

**Planning only.** Phase = when it enters the build. `now` = first deploy, `later` = subsequent gate, `arch-ready` = designed-for, not built.

| Service | Phase | Purpose | Idle/low-cost behavior | Cost risk | Why selected / why not alternative |
|---|---|---|---|---|---|
| **Azure Static Web Apps** | now | Host the React SPA (public + protected routes) on global CDN | Free tier $0; Standard $9/mo only if SWA-managed auth/custom domains needed | none (static) | Purpose-built for SPA + built-in auth + free TLS/CDN. Not App Service (heavier, no free SPA tier). |
| **SWA Auth / Entra External ID** | now→later | Login gate; compute/AI only after auth | SWA built-in auth $0; Entra External ID free < 50k MAU | none at demo scale | Built-in auth = zero infra. External ID for a "real IdP" interview story. |
| **Azure Container Apps (Consumption)** | now | Run `insurance-api` (modular monolith) | **minReplicas=0 → $0 idle**; consumption per-request | minReplicas>0 burns money | Serverless containers, KEDA scale-to-zero, Dapr-ready. **Not AKS** (24/7 node cost). Not App Service (ACA scale-to-zero + container-native). |
| **Container Registry** | now | Store API image | — | **ACR Basic ≈ $5/mo fixed** (the biggest fixed line) | **Recommend GHCR (GitHub Container Registry) = $0** for portfolio; ACR is arch-ready (private + MI + geo-replication) for the "production" answer. |
| **Azure SQL Database Serverless** | later (SQL gate) | Relational store (6 service schemas) | **Auto-pause after 1 h idle → compute $0**, pay storage only (~$0.10–1/mo tiny) | not pausing; over-provisioned vCores | Textbook low-traffic DB. Not Basic DTU/always-on (more expensive when idle). Cold-start on first query is acceptable for a demo. |
| **Azure Blob Storage** | later | Document/photo artifacts | pennies; Hot/Cool tiers | unbounded growth | Cheap object store; pairs with lifecycle TTL. |
| **Blob Lifecycle Management** | later | Auto-delete demo blobs after N days | $0 | none | Cost hygiene + "self-cleaning demo" story. |
| **Azure Key Vault** | now→later | Secrets (DEEPSEEK/AI keys, conn strings) | per-op pennies | none meaningful | Secrets out of code/config; Key Vault references + MI. |
| **Managed Identity** | now | Passwordless ACA→SQL/Blob/KeyVault/ACR | $0 | none | No connection-string passwords; the modern Azure identity answer. |
| **App Insights / Monitor / Log Analytics** | now | Traces, metrics, errors, AI-action + cost telemetry | **first 5 GB/mo free** | log volume blowout | Sampling + 30-day retention + daily cap keep it free. |
| **Azure Cost Management budgets** | now (Slava manual, gate 1) | Budget alerts | $0 | — | Alerts **notify, do not stop** resources — design must enforce cost, not alerts. |
| **GitHub Actions** | now | CI/CD: build→test→containerize→deploy | free (public repo) | none | **OIDC federated identity to Azure** = no stored Azure secret. |
| **Azure AI Foundry / Azure OpenAI** | later (AI gate) | Governed real LLM behind `IAiProvider` | pay-per-token; $0 if unused | loops/large batches | Provider-agnostic seam already exists; Mock stays default; real behind manual button + token cap. |
| **Azure Document Intelligence** | later (AI gate) | Claim doc/photo field extraction | **F0 free tier ~500 pages/mo** | large batches on paid tier | F0 + manual trigger + small batches. |
| **Azure AI Search** | later (AI gate) | Tiny RAG over policy docs | **Free tier (50 MB, 3 indexes) = $0** | **Basic SKU ≈ $75/mo = trap** | Free tier ONLY. Basic is the single biggest cost trap — explicitly avoided. |
| **AKS** | **deferred** | (would be) orchestration at scale | n/a | 24/7 node pool + ops cost | Explicitly NOT first. ACA covers scale-to-zero containers at $0 idle. AKS = real-scale/multi-team answer only. |

## Cost-shape summary
- **Likely-$0 idle:** SWA (Free), Container Apps (min 0), SQL Serverless (paused), Key Vault, Managed Identity, App Insights (<5 GB), AI Search (Free), Document Intelligence (F0), GitHub Actions, budgets.
- **Small fixed (avoidable):** ACR Basic ~$5/mo → replace with GHCR $0.
- **Pay-per-use (manual only):** Azure OpenAI tokens, Document Intelligence pages beyond F0.
- **Cost traps to never enable by default:** AI Search Basic, ACA minReplicas>0, SQL no-auto-pause, AKS, un-sampled logs.
