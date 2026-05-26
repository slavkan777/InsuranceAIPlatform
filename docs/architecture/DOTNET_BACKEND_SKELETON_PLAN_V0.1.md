---
title: .NET Backend Skeleton Plan V0.1 — InsuranceAIPlatform
type: knowledge
status: active
created: 2026-05-27
tags: [architecture, backend, dotnet, planning, portfolio]
---

# .NET Backend Skeleton Plan V0.1

**Entry point.** This document is the main architecture plan.  
Sibling docs:
- [`BACKEND_IMPLEMENTATION_GATES_V0.1.md`](./BACKEND_IMPLEMENTATION_GATES_V0.1.md) — gate decomposition + Claude file-scope maps
- [`BACKEND_DECISION_MATRIX_AND_NEXT_GATE_V0.1.md`](./BACKEND_DECISION_MATRIX_AND_NEXT_GATE_V0.1.md) — decision matrix + exact next gate spec

---

## Phase 2 — Backend Product Boundary

### V0.1 IN Scope

| Item | Rationale |
|---|---|
| Local .NET 9 API project under `server/` | Portfolio artifact; interview-demonstrable |
| Deterministic system-of-record shell | Clean Arch shell without over-engineering |
| Swagger UI with tag groups + disclaimers | Documented portfolio artifact; recruiter/reviewer-readable |
| Synthetic CLM-1006 seed data (in-memory) | Golden claim matches product flow; no DB dependency in skeleton |
| Read-only P0 endpoints (GET only) | Safe first slice; no mutation risk to any data |
| CORS configured for Vite dev server (:5173) | Frontend integration unblocked without env friction |
| GET /health + GET /api/system/demo-status | Immediate verifiability; proves server runs |
| Ready for DB/AI/audit gates | Folder structure anticipates future gates without implementing them |
| Build + test green locally | CI-readiness signal before any real infrastructure |
| Small, safe, bounded scope | Completable in one Claude run; reviewable in 10 min |

### V0.1 OUT of Scope

| Item | Why Deferred |
|---|---|
| Production platform | Not a portfolio goal; local-first only |
| Real payout / fraud verdicts | Governance: AI advisory only; human approval is final |
| Full CRUD (POST/PUT/DELETE) | P1/P2 gates; skeleton is read-only first |
| Auth / identity / JWT | Deferred to separate auth gate; no real users in mockup |
| Azure / cloud deployment | Cost + complexity deferred; local-first |
| Real AI provider (OpenAI/Azure AI) | Mock pipeline gate first; real AI = separate gate |
| Microservices / separate services | Overkill for portfolio mockup |
| EF Core migrations + SQL Server | Persistence gate (`BACKEND_SQLSERVER_PERSISTENCE_GATE`) |
| Frontend integration (API client swap) | `FRONTEND_BACKEND_READ_FLOW_INTEGRATION` gate |
| Real PII | Synthetic data only throughout |

### WHY — Interview Value

- Demonstrates Clean Architecture discipline without gold-plating
- Shows deliberate incremental delivery (skeleton → read → persist → AI)
- Governance-first design (AI advisory, human approval, audit first-class) = enterprise signal
- Mock seam pattern: swap implementation without touching UI contracts = senior-level thinking
- Dedicated DB (`InsuranceAIPlatform`) with explicit no-touch on `DevDept` = professional boundary awareness

### Portfolio Story

> "I built a .NET 9 backend incrementally behind a mock seam — the React frontend kept working throughout. Each gate was small, verifiable, and independently committable. Governance constraints (AI advisory only, synthetic data, no real PII) were first-class from v0.1."

### Deferred Items (explicit backlog)

- SQL Server persistence (`BACKEND_SQLSERVER_PERSISTENCE_GATE`)
- EF Core + migrations
- POST endpoints (approval draft, customer message, analysis run)
- Auth/identity
- Azure deployment
- Real AI provider integration
- Document upload / blob storage

---

## Phase 3 — .NET Platform Decision

### Version: .NET 9

**Chosen: .NET 9.0.304** (installed, verified on this workstation).

| Criterion | .NET 8 LTS | .NET 9 Current |
|---|---|---|
| Interview signal | Strong (LTS = production discipline) | Stronger (current = forward-thinking) |
| Installed on workstation | ✅ 8.0.421 | ✅ 9.0.304 |
| Support timeline | LTS until Nov 2026 | Current until May 2026 → migrate to .NET 10 |
| New features relevant here | Minimal API improvements | Primary constructors, collection expressions, improved perf |
| Risk | Low | Low for portfolio mockup |
| Migration cost if switch needed | 1-line `<TargetFramework>net8.0</TargetFramework>` | Same |

**Conservative alternative:** Switch `<TargetFramework>` from `net9.0` → `net8.0` in `.csproj`. No other changes required. Use if recruiter/employer requires LTS.

### API Style: Controllers

**Chosen: Attribute-routed Controllers.**

| Criterion | Controllers | Minimal APIs |
|---|---|---|
| Scales for many resource endpoints | ✅ Excellent | ⚠️ Gets verbose past ~20 endpoints |
| Interview familiarity | ✅ Universal | ⚠️ Newer; not universal |
| Tag grouping in Swagger | ✅ `[ApiExplorerSettings]` / `[Tags]` | ✅ Also supported |
| Testability | ✅ Mature patterns | ✅ Good but newer |
| Future CRUD (P1/P2) | ✅ Clean action methods | ⚠️ Route handler proliferation |
| LOC for 13 endpoints | Moderate | Less initially |

Minimal APIs noted as lighter alternative for truly micro projects; not chosen here given 13+ planned endpoints and future CRUD expansion.

### Swagger

**Swashbuckle.AspNetCore** (standard choice, widely understood by reviewers).  
Configuration:
- Tag groups matching product flow screens
- Synthetic data disclaimers on sensitive operations
- AI advisory disclaimer on all AI-analysis endpoints
- P0 response examples inline

### Project-level Settings

```xml
<Nullable>enable</Nullable>
<ImplicitUsings>enable</ImplicitUsings>
```

- Nullable references: **enabled** from day 1 (interview signal: null-safety discipline)
- Implicit usings: **enabled** (reduces boilerplate; standard for .NET 6+)
- Analyzers: deferred to a later gate (don't block skeleton)
- Package strategy: minimal — `Swashbuckle.AspNetCore` only for skeleton gate; add packages at the gate that needs them

---

## Phase 4 — Backend Structure Decision

### Options Compared

#### Option A — Single Project (flat)

```
server/InsuranceAIPlatform.Api/
  Controllers/
  Models/
  Data/
  Program.cs
```

| Attribute | Assessment |
|---|---|
| Impl cost | Lowest |
| AI file-scope friendliness | High (one folder) |
| Interview value | Low — "just a script with controllers" |
| Future migration | Hard — no separation to split later |
| Risk | Grows into a maintenance problem past ~10 files |

**Verdict: Too flat. Not chosen.**

#### Option B — Full Multi-Project Modular Monolith

```
server/
  InsuranceAIPlatform.Api/
  InsuranceAIPlatform.Application/
  InsuranceAIPlatform.Domain/
  InsuranceAIPlatform.Infrastructure/
  InsuranceAIPlatform.Contracts/
  InsuranceAIPlatform.Tests/
```

| Attribute | Assessment |
|---|---|
| Impl cost | High — 5 projects, 5 `.csproj`, project refs, DI wiring from day 1 |
| AI file-scope friendliness | Low — changes spread across 5 projects per feature |
| Interview value | High — shows Clean Arch discipline |
| Future migration | Already there — no migration needed |
| Risk | Over-engineered for skeleton gate; slows first delivery |

**Verdict: Overengineered for first slice. Documented target state for post-persistence gate.**

#### Option C — Hybrid (CHOSEN)

```
server/
  InsuranceAIPlatform.Api/        ← single project, internal folders
    Domain/
    Application/
    Infrastructure/
    Contracts/
    Controllers/
    Program.cs
    InsuranceAIPlatform.Api.csproj
  InsuranceAIPlatform.Tests/      ← separate test project from day 1
  InsuranceAIPlatform.sln
```

| Attribute | Assessment |
|---|---|
| Impl cost | Low — 2 projects, internal folders enforce logical separation |
| AI file-scope friendliness | High — one writable folder for most changes |
| Interview value | High — "I started hybrid, designed for extraction" |
| Future migration | Documented path: extract Application → own project at persistence gate; Domain at AI gate |
| Risk | Low — internal namespaces enforce boundaries even before extraction |

**Migration path to Option B:**
- After `BACKEND_SQLSERVER_PERSISTENCE_GATE`: extract `Infrastructure/` → `InsuranceAIPlatform.Infrastructure.csproj`
- After `BACKEND_AUDIT_RISK_PLACEHOLDERS`: extract `Application/` → `InsuranceAIPlatform.Application.csproj`
- After `MOCK_AI_PIPELINE_IMPLEMENTATION`: extract `Domain/` → `InsuranceAIPlatform.Domain.csproj`
Each extraction is a project-structure refactor, not a logic change.

**Chosen: Option C Hybrid.**

---

## Phase 18.10 — Non-Functional Requirements

| Requirement | Target | Verification |
|---|---|---|
| Startup time | < 30 seconds (cold start local) | `dotnet run` wall-clock from shell |
| No paid services in skeleton | Zero external calls | Code review + Wireshark/Fiddler spot-check |
| No real PII | Synthetic data only (CLM-1006 et al.) | Grep for real name/SSN/email patterns in seed |
| Deterministic seed | Same response on every cold start | Call endpoint twice, compare JSON hashes |
| Build + test local | `dotnet build` and `dotnet test` exit 0 | CI-equivalent shell run |
| Swagger accessible locally | `http://localhost:{port}/swagger` loads | Browser GET, 200 response |
| Fast /health | < 200ms p99 local | `curl` with `-w "%{time_total}"` |
| TraceId on errors | Every 4xx/5xx includes `traceId` field | Trigger 404, inspect response body |
| Public-repo safe | No secrets, tokens, real credentials in any tracked file | `git grep` for known secret patterns |
| No Azure / AI provider dependency | Build + run with no network (airplane mode test) | Disconnect network, `dotnet run`, call /health |
| Reviewer-understandable in 10 min | README + Swagger + this doc sufficient | Ask reviewer to navigate without help |

---

## Phase 17 — Branch / Commit / Push Policy

| Rule | Detail |
|---|---|
| Work branch | Always `dev`; never implement directly on `main` |
| `main` state | Stable, accepted state only (current accepted commit: `69e6731`) |
| No direct `main` push | All backend work lands on `dev` first |
| No force-push | Prohibited without explicit per-session Slava approval |
| Implementation gate ≠ commit gate | Implementation can be verified locally before any commit |
| Commit trigger | After: `dotnet build` exit 0 + `dotnet test` exit 0 + safety scan (no secrets) |
| Push / merge to `main` | Separate approval step; never bundled with implementation |
| Commit message format | Present tense, imperative; no "Co-Authored-By Claude"; no AI tool mentions |
| Safety scan before commit | `git grep` for known secret patterns; confirm no `appsettings.Production.json`, no `.env` tracked |

---

## Phase 18.11 — Interview Narrative

**5–10 senior-level bullets for any technical interview:**

1. **Modular monolith, not microservices** — Portfolio mockup; microservices add distributed systems complexity (network, eventual consistency, service discovery) with no benefit at this scale. Hybrid Option C lets me demonstrate Clean Arch boundaries while staying deliverable.

2. **DTO-first, never expose EF entities** — EF entity shape is a persistence concern; API contract is a product concern. Exposing entities creates coupling that breaks every time the schema evolves. Screen-friendly read DTOs decouple these cleanly.

3. **Mock seam pattern** — `mockInsuranceApi.ts` stays intact; a config switch selects mock vs real `ApiClient`. The UI never changes. This is the strangler fig pattern applied at the API client boundary — safe incremental replacement.

4. **Read-only first** — First slice is GET-only. No mutation risk, no validation complexity, no optimistic concurrency. Proves the architecture before adding write complexity.

5. **Dedicated DB `InsuranceAIPlatform`, never `DevDept`** — Professional boundary: two separate products share a SQL Server instance but never a schema. `DevDept` is a live portfolio asset; contaminating it is unacceptable.

6. **AI advisory only, never final verdict** — Governance constraint from day 0. The system surfaces risk scores and evidence; a licensed human adjuster approves or rejects. This reflects real insurance regulatory constraints (EU GDPR Art. 22, automated decision-making limits).

7. **Audit/cost first-class from v0.1** — Audit trace and cost tracking are not afterthoughts. They're P0 endpoints. This signals enterprise-grade thinking: compliance, billing, and explainability built in, not bolted on.

8. **How it grows to AI/RAG/Azure** — Mock AI pipeline gate → real Azure Document Intelligence or OpenAI embeddings → RAG over policy documents. Each gate is a bounded swap behind the same interface. Azure deployment is an infrastructure gate, not a code rewrite.

9. **Skeleton gate is the hardest discipline** — Stopping at "health + demo-status + Swagger" when you could wire up all 13 endpoints demonstrates delivery discipline. A reviewable, committable skeleton is more valuable than a partially-working everything.

---

## Phase 18.12 — Final Acceptance Checklist for This Plan

Each item references the sibling doc that satisfies it.

- [x] **Clear .NET platform decision** — .NET 9 chosen, .NET 8 LTS as documented fallback. See Phase 3 above.
- [x] **Backend structure decided** — Option C Hybrid chosen with migration path to Option B. See Phase 4 above.
- [x] **Persistence path explicit** — In-memory seed first; SQL Server at `BACKEND_SQLSERVER_PERSISTENCE_GATE`. See Decision Matrix ([`BACKEND_DECISION_MATRIX_AND_NEXT_GATE_V0.1.md`](./BACKEND_DECISION_MATRIX_AND_NEXT_GATE_V0.1.md)).
- [x] **DB name = InsuranceAIPlatform** — Explicit. See NFR table (no DevDept rule), Decision Matrix.
- [x] **No-DevDept rule documented** — IN/OUT table above + NFR + Decision Matrix forbidden column.
- [x] **Route-to-endpoint map** — P0 endpoints listed in Shared Context; full mapping in Implementation Gates doc ([`BACKEND_IMPLEMENTATION_GATES_V0.1.md`](./BACKEND_IMPLEMENTATION_GATES_V0.1.md)).
- [x] **P0 endpoints enumerated** — 13 GET endpoints listed in Shared Context; gate 2 covers them.
- [x] **DTO style decided** — Screen-friendly read DTOs for P0; never EF entities. See Decision Matrix.
- [x] **Swagger strategy** — Portfolio artifact with disclaimers + tag groups. See Phase 3 above + Decision Matrix.
- [x] **NFRs with verification** — Full table in Phase 18.10 above.
- [x] **Small first gate** — Skeleton gate = health + demo-status + Swagger only. See Next Gate Spec in Decision Matrix doc.
- [x] **Future file scopes** — Claude file-scope maps per gate in [`BACKEND_IMPLEMENTATION_GATES_V0.1.md`](./BACKEND_IMPLEMENTATION_GATES_V0.1.md).
- [x] **No code written** — DOCS-ONLY task. No `.cs`, `.sln`, `.csproj`, SQL, migrations.
- [x] **No DB touched** — No DB commands, no migrations, no schema changes.
- [x] **No commit / push** — Not performed. Branch/commit policy documented only.
- [x] **Example JSON** — In Next Gate Spec (`BACKEND_DECISION_MATRIX_AND_NEXT_GATE_V0.1.md`), demo-status response example.
- [x] **Validation / error rules** — Deferred to gate 2 (read-only first); `traceId` on errors is NFR.
- [x] **Schema outline** — Deferred to `BACKEND_SQLSERVER_PERSISTENCE_GATE`; seed strategy in gate 1.
- [x] **EF strategy** — Deferred to persistence gate; no EF in skeleton.
- [x] **Demo data ownership** — Synthetic CLM-1006 seed owned by `server/`; no real PII.
- [x] **DB reset / safety** — Covered by no-DevDept rule + dedicated DB name. Explicit in gates doc.
- [x] **Config / secrets** — `appsettings.json` non-secret only; no `.env` tracked; secrets policy in gates doc.
- [x] **Interview narrative** — Phase 18.11 above (9 bullets).
