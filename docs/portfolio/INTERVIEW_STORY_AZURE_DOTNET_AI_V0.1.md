# Interview Story — Azure · .NET · AI — V0.1

**Project:** InsuranceAIPlatform — Auto Insurance Claim AI Workbench · **Date:** 2026-05-30
Truthful talking points for a live, hobby-cost, production-*shaped* Azure deployment. (Live demo + creds in `LIVE_DEMO_RUNBOOK_V0.1.md`.)

## 30-second pitch
"It's an Auto Insurance Claim AI Workbench — a .NET 9 API and a React/TypeScript SPA, both deployed live to Azure. The API runs on Azure Container Apps with scale-to-zero, the SPA on Azure Static Web Apps, everything provisioned with Bicep. It demonstrates a production-shaped cloud setup — Managed Identity, Key Vault, observability, secure CORS — at roughly zero idle cost, with the relational DB and a real AI model deliberately toggle-gated off to keep it a cheap demo."

## 2-minute architecture
"Browser → Azure Static Web Apps serves the React SPA. The SPA calls a .NET 9 modular-monolith API on Azure Container Apps over HTTPS with CORS. The image lives in GitHub Container Registry — I used GHCR instead of ACR to avoid a monthly cost. The whole stack is Bicep: a subscription-scope template plus modules for Container Apps, Static Web Apps, a user-assigned Managed Identity, Key Vault with RBAC, hardened Storage, and observability via App Insights + Log Analytics. Two feature toggles — `enableSql` and `enableAi` — let me deploy a minimal, near-zero-cost footprint and add the paid pieces later. Compute scales to zero, so idle cost is about a dollar or two a month. The golden demo path — open a claim, see documents, AI evidence, risk scoring, human approval, and an audit/cost trace — runs on the live API end-to-end."

## .NET backend
"A modular monolith: one ASP.NET Core host referencing service libraries per bounded context (claims, customers/policies, documents, AI analysis, approval, audit/cost) plus shared building blocks. Read endpoints plus a few human-controlled command endpoints — no automated payouts. AI output is explicitly advisory; a human always decides. 137 unit/integration tests, green. It's containerized with a multi-stage Dockerfile running non-root on port 8080, and there's no EF migration on startup so a missing DB doesn't break a cold start."

## Azure deployment
"Container Apps for the API because I wanted serverless containers with scale-to-zero and no cluster to babysit — cheaper and simpler than AKS at this scale. Static Web Apps Free for the SPA with SPA-fallback routing. I hit a real CORS problem — the API only allowed the localhost dev origin — and fixed it properly: I made the allowed origins config-driven (`Cors:AllowedOrigins`, env-overridable), added the SWA origin, rebuilt and rolled a new revision, and verified the preflight and GET both return the right `Access-Control-Allow-Origin`. No wildcard, no credentials."

## AI engineering (honest)
"The AI is behind a provider-agnostic interface — `IAiProvider` — with a Mock implementation as the default and a DeepSeek opt-in path that's disabled by default. The workbench UI is fully built: advisory findings, evidence/RAG sources, extracted entities with confidence, a model-confidence breakdown, and a token/cost trace. Today those numbers are seeded — I have **not** wired a real model in production; a real Azure OpenAI provider would drop in behind the same interface. I'm deliberate about not overclaiming that."

## Cost control
"Scale-to-zero compute, Free static hosting, GHCR instead of ACR, sampled App Insights and a capped Log Analytics workspace, and a $30/month budget with alert tiers. Idle is essentially zero; the realistic running cost is $5–10/month. Teardown is one `az group delete`."

## Security
"No secrets in the repo — URLs and CORS origins are public; anything secret defers to Managed Identity + Key Vault, which are provisioned. Storage has shared-key access disabled. The API runs non-root. The demo login is intentionally client-side only with public demo credentials — I'm clear that it's a demo, not production SSO, and there's no real PII, only synthetic data."

## What I'd improve next
"Turn on the Azure SQL gate so the list/enumeration endpoints are live instead of falling back to seeded data; wire a real Azure OpenAI provider behind the existing interface; move deploys onto the scaffolded GitHub Actions workflow with OIDC instead of local `az`; and add real identity (Entra) if it needed to be more than a demo."

## Recruiter / HR bullets
- Built and **deployed a full-stack .NET 9 + React app to Azure** (Container Apps + Static Web Apps), live demo available.
- **Infrastructure as Code** with Bicep; **scale-to-zero** architecture at ~$0 idle cost.
- Cloud fundamentals: **Managed Identity, Key Vault, observability (App Insights), secure CORS, containerization**.
- **AI-ready** architecture (provider-agnostic), with honest scoping of what's live vs mocked.

## Senior-engineer bullets
- Subscription-scope **Bicep** with feature toggles (`enableSql`/`enableAi`) for cost-staged deploys; offline-validated.
- **ACA scale-to-zero** + GHCR (no ACR) + capped telemetry → near-zero idle cost; one-command teardown.
- Diagnosed and fixed a real **CORS** failure with **config-driven, env-overridable** origins (no wildcard/credentials); verified preflight + GET.
- **Graceful degradation**: SPA falls back to seeded data when DB-backed endpoints aren't deployed, so the demo never breaks.
- Disciplined delivery: 137 tests green; independent review gate before each commit; secrets never committed.

## Avoid overclaiming (hard rules)
- ❌ "Azure SQL is deployed / data is in a real DB." → It's seeded/in-memory; SQL is toggle-gated off (list endpoints 500 → seeded fallback).
- ❌ "A real LLM scores claims in production." → Mock provider; numbers are seeded.
- ❌ "Runs on AKS." → Container Apps, by choice.
- ❌ "Handles production traffic / enterprise SSO." → It's a hobby-cost demo with client-side demo auth.
- ✅ "Production-shaped architecture, live demo, deliberate cost trade-offs, honest about what's deferred."
