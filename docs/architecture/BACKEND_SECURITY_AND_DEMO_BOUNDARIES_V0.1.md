---
title: Backend Security and Demo Boundaries V0.1
type: knowledge
status: active
created: 2026-05-27
tags: [security, pii, secrets, demo, configuration, public-repo]
---

# Backend Security and Demo Boundaries V0.1

## 1. Security, PII, and Public-Repo Safety Plan

### Synthetic Data Only

All claim data in this project is entirely synthetic. The golden claim CLM-1006 (Robert Johnson / Toyota Camry 2021 / VIN ****8842 / policy POL-2025-AC-4421) is fictional and fabricated for portfolio purposes. The following categories of real data are **permanently prohibited** from this repository:

- Real customer names, addresses, phone numbers, email addresses
- Real VIN numbers, plate numbers, or vehicle registration data
- Real policy numbers or insurer identifiers
- Real claim amounts tied to an actual incident or insured party
- Any data that could identify a real individual or entity

Seed data files, test fixtures, and inline mock values must use invented names and synthetic identifiers.

### No API Keys in Repository

No API keys, tokens, or credentials of any kind may appear in:
- `appsettings.json` (committed)
- `appsettings.Development.json` (committed)
- Any `.cs`, `.ts`, `.md`, `.yml`, or `.json` file under version control
- Commit messages, PR descriptions, or issue comments
- README examples (use placeholders only)

If a key is accidentally committed, treat the key as compromised immediately — rotate it before addressing the repository.

### Local DB Connection String Handling

The SQL Server instance at `localhost,19772` is used for future persistence phases only. Connection strings must NEVER appear in committed files. The only permitted forms:

1. `dotnet user-secrets set "ConnectionStrings:InsuranceAIPlatform" "<value>"` — stored in the user profile, outside the repo
2. Environment variable `INSURANCEAIPLATFORM_DB` injected at process start
3. Docker Compose `.env` file that is gitignored

The `InsuranceAIPlatform` database uses schema `dbo`. The DevDept database at `localhost,19772` must NEVER be referenced by this project. These are two separate databases on the same instance; isolation is the developer's responsibility.

### Logging and Sensitive Data

The structured logger (see Observability Plan) must NEVER log:
- Connection strings or any substring that could reveal credentials
- Bearer tokens or session identifiers
- PII fields (name, address, VIN full string)
- Any value read directly from environment variables that could be a secret

Log at structured fields level: `ClaimId`, `RunId`, `TraceId`, `StatusCode`, `DurationMs`, `ErrorCode`. Never log raw request bodies that may contain identifiers.

### Secret Scan Before Any Push

Before every `git push` to the public repo `github.com/slavkan777/InsuranceAIPlatform`, run a secret scan. Minimum check: verify no file in the staged set contains any of:
- Strings matching `sk-`, `sk-ant-`, `Bearer `, `password=`, `pwd=`, `Server=`, `Data Source=`
- Any `.env` file (even empty)
- `appsettings.Production.json`

---

## 2. Configuration and Secrets Strategy Table

| Config Item | Allowed Location | Commit to Repo? | Notes |
|---|---|---|---|
| `appsettings.json` | `src/Api/` | Yes | Non-secret defaults only: logging levels, CORS origins list, health check path, demo mode flag defaults, risk threshold default (60). No connection strings, no keys. |
| `appsettings.Development.json` | `src/Api/` | No — add to `.gitignore` | May contain `DemoMode: true`, local CORS, verbose logging. Must not contain any credentials. Even sanitized, keep gitignored to avoid accidental secret leak. |
| `appsettings.Production.json` | N/A | Never — do not create this file | This is a portfolio project with no production deployment. Creating this file increases the risk of accidentally committing production config. |
| SQL connection string | `dotnet user-secrets` or `INSURANCEAIPLATFORM_DB` env var | Never | Developer runs `dotnet user-secrets set` once locally. CI/CD (if any) injects via environment. |
| Any AI provider API key | `dotnet user-secrets` or env var only | Never | No AI provider is used at V0.1. When integrated in future, key goes only into secrets/env. |
| `DemoMode` flag | `appsettings.json` default = `true` | Yes | Safe to commit; value is `true` (demo) or `false` (real). No credential content. |
| `AllowedOrigins` CORS list | `appsettings.json` | Yes | `["http://localhost:5173","http://localhost:4173"]`. No secrets. |
| Risk score threshold | `appsettings.json` under `RiskAssessment:Threshold` | Yes | Default `60`. Safe config, no secrets. |
| `.env` files (Vite) | `.env.development` in `src/` | `.env.development.local` → No; `.env.development` base → Yes if no secrets | Vite's `.env.development` may contain `VITE_API_BASE_URL=http://localhost:5174` (safe). Never put API keys in Vite env files — they are bundled into JS output. |
| README connection string example | `README.md` | Yes — placeholder only | Use `Server=localhost,19772;Database=InsuranceAIPlatform;Trusted_Connection=True` with a note "replace with your local SQL Server". Never paste a real password. |

---

## 3. Interview-Safe Portfolio Statement

The following statement must be included verbatim (or equivalent) in the project README:

> **Demo & Portfolio Disclaimer:**
> This backend uses entirely synthetic claim data generated for portfolio and demonstration purposes. No real customer, vehicle, or insurance policy data is processed or stored. AI outputs (risk scores, evidence findings, confidence scores) are deterministic placeholders and do not reflect real AI model calls at V0.1. All AI outputs are advisory only — no automated claim approval or rejection occurs. Human adjuster approval is mandatory for any final claim disposition. This project does not connect to any live insurance system, payment processor, or customer database.

This statement protects the portfolio from being misread as a production system and demonstrates responsible AI governance awareness to interviewers.

---

## 4. Public Repo Safety Checklist

Before each push to `github.com/slavkan777/InsuranceAIPlatform`:

- [ ] No `appsettings.Development.json` in staged files
- [ ] No `appsettings.Production.json` anywhere in repo
- [ ] No `.env*` files with credential values in staged files
- [ ] No string `Server=` or `password=` or `Bearer ` in any staged `.cs`/`.json`/`.md` file
- [ ] No `secrets/` directory committed
- [ ] `InsuranceAIPlatform` DB name only — no `DevDept`, no other project DBs referenced
- [ ] `demoMode: true` present in default config
- [ ] Portfolio disclaimer present in README
