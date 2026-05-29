# Azure Readiness Pre-Flight — InsuranceAIPlatform (v0.1)

**Status:** PLANNING ONLY — no Azure resources exist for this project. Nothing in this document has been deployed.
**Source baseline:** `dev` @ `2e9443a` (local-first product: backend build PASS, xunit 137/137, frontend build PASS, Playwright 89/89).
**Intent:** interview-grade, production-*shaped* Azure deployment at hobby cost (~$5–10/month real, $30 comfort cap, ~$1000 explainable ceiling).

> Companion docs: [`AZURE_SERVICE_MATRIX_V0.1.md`](AZURE_SERVICE_MATRIX_V0.1.md) · [`AZURE_COST_GOVERNANCE_V0.1.md`](AZURE_COST_GOVERNANCE_V0.1.md) · [`AZURE_GATE_PLAN_V0.1.md`](AZURE_GATE_PLAN_V0.1.md)

---

## 1. Guiding principle

> **Public is static and free. Compute and AI sleep until a human logs in and clicks.**

Real traffic is ~1–2 users/day. The architecture must *look* like an adult production system (auth, microservice-shaped API, managed identity, observability, IaC, CI/CD, governed AI) while actually billing near-zero because every paid component is **scale-to-zero, auto-paused, free-tier, or manually triggered**.

## 2. Repo deployment-readiness (as of `2e9443a`)

| Area | Current | Azure mapping | Gap |
|---|---|---|---|
| Frontend | Vite SPA → static `dist/` | Azure Static Web Apps (Free/Standard) | `staticwebapp.config.json` (routes/auth) — future |
| Backend | .NET 9 single Api host referencing 6 service libs in-proc (microservice-shaped **modular monolith**: service-owned DbContexts, BFF, outbox) | **1× Azure Container Apps** (`insurance-api`), scale-to-zero | `Dockerfile` + `.dockerignore` — future |
| Config | `IConfiguration`; connection string via `ConnectionStrings:InsuranceAIPlatform` / `INSURANCEAI_CONNECTION_STRING` env; AI via `AiProvider:*` + `DEEPSEEK_API_KEY` (never logged) | Key Vault references + Managed Identity + ACA env vars | wire Key Vault refs — future |
| DB | SQL Server LocalDB, 6 service schemas, DbMigrator console | Azure SQL Database **Serverless** (auto-pause), same schemas | swap connection string only (no code change) |
| AI | `IAiProvider`: Mock (default) / DeepSeek opt-in (disabled-by-default) | add Azure OpenAI/Foundry provider behind same interface | new provider impl — future, governed |
| CI/CD | none | GitHub Actions → build/test/containerize/deploy via **OIDC federated identity** (no stored secret) | workflow yaml — future |
| IaC | none | Bicep (recommended) | `infra/*.bicep` — future |

**Verdict:** the *application* is deploy-shaped (static frontend + stateless API + connection-string-swappable DB + provider-agnostic AI). The *platform scaffolding* (Docker/IaC/CI/SWA config) is intentionally absent and is the work of the future Azure gates. This is `ACCEPT_AZURE_PREFLIGHT_READY` with documented readiness gaps.

## 3. Target architecture

### 3.1 Anonymous public path (free, always-on)
```
Browser → Azure Static Web Apps (CDN-served SPA)
          • no backend call, no DB, no AI on anonymous load
          • marketing/landing + "Sign in to open the workbench"
```
The public surface is a static bundle on SWA's global CDN. Cost ≈ $0. No compute wakes for an anonymous visitor or a crawler.

### 3.2 Authenticated path (compute wakes on demand)
```
Login (SWA built-in auth / Entra External ID)
  → protected SPA routes
  → first API call wakes insurance-api Container App from 0 replicas (cold start ~1–3 s, acceptable)
  → SQL Serverless resumes from auto-pause on first query
  → Blob/AI touched ONLY by explicit user action
```

### 3.3 Microservice/compute path
| Container App | Role | Scaling |
|---|---|---|
| `insurance-api` | the modular-monolith BFF+services (HTTP) | min 0 / max 1–2, HTTP-scaled (KEDA) |
| `ai-worker` | heavier/async AI jobs (starts folded into `insurance-api`; split later) | min 0, queue/HTTP-triggered |
| `cleanup-job` | scheduled demo-data + blob TTL enforcement | Container Apps **Job** (cron), min 0 |

> Honest framing: we deploy a **modular monolith in one container** first. The service boundaries already exist in code, so splitting `ai-worker` out later is a packaging change, not a rewrite. This is the cost-correct and interview-correct choice at 1–2 users/day.

### 3.4 AI path (governed, manual, mock-first)
- Default provider stays **Mock**. Real Azure OpenAI/Foundry, Document Intelligence (F0 free tier), and AI Search (Free tier) are added behind the existing `IAiProvider` seam and a **manual button** — never on page load, never in a background loop.
- AI remains **advisory only**: no autonomous payout/approval/rejection; human approval + audit trail preserved (unchanged from local product).

### 3.5 Cleanup path
- Blob **lifecycle policy** auto-deletes demo artifacts after N days (e.g. 7–30).
- `cleanup-job` cron prunes synthetic demo rows / stale runs.
- SQL Serverless **auto-pause** (1 h idle) so storage-only cost when nobody is using it.

### 3.6 Observability path
- Application Insights (first 5 GB/mo free) with **sampling** + 30-day retention + daily cap → traces, request/AI-action metrics, errors, correlation-id continuity (the API already emits `X-Correlation-Id`).
- Azure Monitor/Log Analytics workspace shared, capped.

## 4. Low-traffic production-shaped guarantees
1. Anonymous load = static only (no compute/DB/AI).
2. `insurance-api` Container App `minReplicas = 0` → $0 idle.
3. Azure SQL **Serverless auto-pause** → compute $0 when idle.
4. AI runs **only** after login + explicit action; **no background AI loops**.
5. Blob lifecycle TTL + `cleanup-job` keep storage tiny.
6. Logs sampled + retention-capped.

## 5. Truth table — deployed vs architecture-ready (current)
| Capability | Status today |
|---|---|
| Local product (UI + API + LocalDB + Mock AI) | ✅ built, verified, pushed (`2e9443a`) |
| Any Azure resource | ❌ none exist |
| Azure deployment | ❌ not started (this is planning only) |
| Real AI provider active | ❌ Mock default; real = opt-in/disabled |

## 6. Interview story
- **One-liner:** "An auto-insurance claim AI workbench: a .NET 9 microservice-shaped backend + React SPA, deployed to Azure as scale-to-zero Container Apps behind Static Web Apps auth, with governed (advisory-only) AI — engineered to look production-shaped while costing a few dollars a month."
- **60-second:** local-first, verified (137 unit + 89 E2E), then Azure: static public shell on SWA (free), backend in one Container App that scales to zero, Azure SQL Serverless that auto-pauses, secrets in Key Vault via Managed Identity, IaC in Bicep, CI/CD in GitHub Actions via OIDC (no stored creds), observability in App Insights with sampling, and AI behind a manual button using a provider-agnostic interface (Mock default, Azure OpenAI/Document Intelligence/AI Search opt-in). Cost is governed by scale-to-zero + auto-pause + free tiers + budget alerts.
- **AKS vs Container Apps:** AKS means a node pool billing 24/7 (~$70+/mo) and cluster ops; at 1–2 users/day that's pure waste. Container Apps gives serverless containers, KEDA scale-to-zero, and Dapr-readiness with $0 idle. "I'd reach for AKS at real scale / multi-team / fine-grained orchestration — not before."
- **AI governance:** advisory-only, human-approval-final, full audit trail, provider-agnostic, mock-by-default, real calls manual + token-capped, key in Key Vault, never logged.
- **Cost governance:** budget alerts at $5/$10/$20/$30/$50, scale-to-zero everywhere, free tiers (App Insights 5 GB, AI Search Free, Document Intelligence F0), GHCR instead of paid ACR, SQL auto-pause, blob TTL.

## 7. Readiness verdict
`ACCEPT_AZURE_PREFLIGHT_READY` with documented repo-scaffolding gaps (Dockerfile, IaC, CI, SWA config) that are the explicit deliverables of the future Azure gates. No resources created; no `az login`; no secrets.
