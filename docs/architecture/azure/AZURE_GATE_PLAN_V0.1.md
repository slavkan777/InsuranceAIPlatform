# Azure Gate Plan — InsuranceAIPlatform (v0.1)

**Planning only.** Each future gate is separately opened by Slava/GPT, with its own DONE STATE, forbidden scope, verification, gpt-handoff report, and stop line. Real cost begins at gate 3.

| # | Gate | Goal | In scope | Out of scope | DONE state | Risk | Effort |
|---|---|---|---|---|---|---|---|
| 1 | **AZURE_ACCOUNT_AND_BUDGET_SETUP_MANUAL_V0.1** | Slava prepares the account safely | account/subscription, **budget alerts** $5–50, region, naming approval, GitHub repo reachable | any resource creation; secrets in chat; `az login` automation | account ready; budget alerts live; region+naming recorded; **0 resources created** | low (manual misclick) | Low (manual) |
| 2 | **AZURE_IAC_SKELETON_V0.1** | Author Bicep skeleton + Dockerfile + CI yaml; **commit** azure docs + IaC to `dev` | `infra/*.bicep`, `Dockerfile`, `.dockerignore`, `staticwebapp.config.json`, `.github/workflows/*.yml` (build/test only, **no deploy step active**), commit these + this gate's `docs/architecture/azure/*` to `dev` | `az deployment` / apply; any resource; secrets | IaC compiles/lints (`bicep build`); Docker image builds locally; CI green on build/test; committed+pushed to `dev`; **nothing applied to Azure** | low–med | Medium |
| 3 | **AZURE_MINIMAL_DEPLOY_V0.1** | First real deploy: static frontend + scale-to-zero API | provision via Bicep: RG, SWA (Free), 1 Container App (`insurance-api`, **minReplicas=0**), Log Analytics + App Insights (sampling), Managed Identity, image via **GHCR**; GitHub Actions deploy via **OIDC**; Mock AI only; in-memory/seed data only (no SQL yet) | SQL, Blob, real AI, ACR | public SPA loads (static, $0); login works; first authed call **cold-starts** API then returns; idle → 0 replicas; budget unmoved beyond ~$0; smoke evidence | **med–high (first real spend)** | High |
| 4 | **AZURE_SQL_SERVERLESS_INTEGRATION_V0.1** | Move persistence to Azure SQL Serverless | Azure SQL Serverless (auto-pause 1 h), DbMigrator run, connection string via **Key Vault + Managed Identity** (no password), swap LocalDB → Azure SQL (config only, **no app-code change**) | real AI; blob; schema redesign | migrations apply to Azure SQL; API reads/writes against it; DB **auto-pauses** when idle (verified); secret only in Key Vault | med–high | Medium-High |
| 5 | **AZURE_BLOB_AND_CLEANUP_V0.1** | Artifacts + self-cleaning demo | Blob Storage (MI access), **lifecycle TTL** policy, `cleanup-job` Container Apps Job (cron) for demo-data + blob pruning | real AI | upload/read a synthetic artifact via MI; lifecycle policy live; cleanup job runs on schedule + scales to 0 | med (storage growth) | Medium |
| 6 | **AZURE_AI_CONTROLLED_DEMO_V0.1** | Governed real AI behind a button | Azure OpenAI/Foundry provider impl behind `IAiProvider`; Document Intelligence **F0**; AI Search **Free**; manual-trigger only; token caps; key in Key Vault; audit every call | autonomous AI; payout/approval by AI; background AI; Basic AI Search SKU | real AI runs **only** on explicit click; Mock still default; advisory-only + human-approval + audit preserved; token/page caps enforced; cost stays in target | **high (token/page spend + safety)** | High |
| 7 | **PORTFOLIO_INTERVIEW_POLISH_V0.1** | Make it interview-ready | README + architecture diagram + cost-dashboard screenshot + "deployed vs arch-ready" truth table + interview Q&A pack; final cost review | new features; new resources | recruiter-readable repo; diagram matches reality; honest deployed-vs-planned table; cost confirmed in target | low | Medium |

## Ordering rationale
- **1 → 2** before any spend: budget guardrails + reproducible IaC first.
- **3** is the smallest thing that *looks deployed* (public static + one scale-to-zero API) at ~$0.
- **4 → 5** add stateful pieces, each independently cost-bounded (auto-pause, TTL).
- **6** is last and most governed — real AI is the only meaningfully variable cost; it stays manual + capped + mock-default.
- **7** converts the working deployment into portfolio/interview value.

## Hard invariants across all Azure gates
- No `main` push without a separate release gate; `dev` is the working branch; no force-push.
- No secret in code/config/repo/handoff; Key Vault + Managed Identity only; `DEEPSEEK_API_KEY` / Azure keys never logged.
- AI advisory-only; human approval final; full audit trail; synthetic data only; no real PII/payout/customer messaging.
- Every paid component defaults to scale-to-zero / auto-pause / free-tier / manual.
- Budget alerts are notifications, not enforcement — the architecture enforces cost.
