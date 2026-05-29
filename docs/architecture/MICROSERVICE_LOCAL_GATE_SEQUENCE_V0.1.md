# Microservice Local Gate Sequence — V0.1

Ordered, machine-readable gate chain for the **Azure-ready microservice architecture, implemented locally first**. Supersedes `LOCAL_COMPLETION_GATE_SEQUENCE_V0.1.md` (single-backend). Companion to `MICROSERVICE_ARCHITECTURE_CORRECTION_BEFORE_DB_WRITE_AI_V0.1.md` + `MICROSERVICE_SERVICE_BOUNDARIES_V0.1.md`.

**Global rules (every gate):** synthetic data only · no real PII · no secrets in repo/logs/reports · `DEEPSEEK_API_KEY` never read/printed/logged (name-only) · service-owned data (no shared DbContext, no cross-service DB joins) · frontend talks to BFF only · DeepSeek isolated in AI Analysis · `InsuranceAIPlatform` boundary only, never `DevDept` · `main` untouched until gate 21 · no force-push · implement → verify → commit-only → push-only separation · independent Opus inspector before any commit/push or "done" · no Azure before local completion + owner manual checkpoint.

`dev` = integration branch; `main` = stable portfolio branch.

---

### Gate 1 — `MICROSERVICE_ARCHITECTURE_CORRECTION_BEFORE_DB_WRITE_AI_V0.1`  *(this gate — done)*
- **Purpose:** correct the architecture to microservice-first, Azure-ready, local-first; define services, data ownership, events, migration, Azure mapping.
- **Scope:** docs only (`docs/architecture/`, `docs/reports/...`, gpt-handoff).
- **Forbidden:** code/DB/EF/AI/Azure/commit-source/push-source/main.
- **DONE:** 3 correction docs + supersede notices on the prior 2 docs + local report + handoff; services & boundaries explicit; SQL-EF-first marked superseded.
- **Verification:** docs-only changeset; no secret; HEAD unchanged.
- **Report:** local + gpt-handoff (`microservice-architecture-correction-before-db-write-ai-v0.1`).
- **Commit/push:** none (source); gpt-handoff publish only.
- **STOP:** after docs + handoff. Next is BFF planning, not persistence.

### Gate 2 — `BFF_API_GATEWAY_SKELETON_PLANNING_V0.1`
- **Purpose:** plan the BFF/Gateway project: how it preserves the existing `/api/claims/...` seam, aggregation shapes, command routing, correlation-id, where it initially delegates (existing read service/stubs).
- **Scope:** docs only.
- **Forbidden:** code/projects/DB/AI/Azure/commit/push/main.
- **DONE:** BFF planning doc (routes↔services map, composed DTOs, routing table, "owns no data" rule, local run model).
- **Verification:** doc completeness; docs-only.
- **Report:** local + gpt-handoff.
- **Commit/push:** none (gpt-handoff publish only).
- **STOP:** after plan + handoff.

### Gate 3 — `BFF_API_GATEWAY_SKELETON_IMPLEMENTATION_V0.1`
- **Purpose:** implement the BFF/Gateway skeleton; frontend keeps calling one surface; BFF delegates to the existing read path / stubs; **no UI rewrite**.
- **Scope:** `server/**` (new BFF project) + minimal config; preserve frontend contracts; tests.
- **Forbidden:** service DBs; write business logic in BFF; DeepSeek; Azure; UI behavior change; commit/push (separate gate).
- **DONE:** BFF builds; serves all 11 read contracts (delegating/aggregating); CORS `:5173`; aggregation contract tests; frontend build still PASS; no domain DB in BFF.
- **Verification:** `dotnet build`/`test`; BFF read smoke parity with current API; FE build; secret scan.
- **Report:** local + gpt-handoff.
- **Commit/push:** none here.
- **STOP:** after impl + verify + handoff.

### Gate 4 — `COMMIT_AND_PUSH_DEV_BFF_GATEWAY_SKELETON_ONLY`
- **Purpose:** commit accepted BFF skeleton; FF push `dev`.
- **Scope:** stage accepted BFF files; one commit; FF push `dev`.
- **Forbidden:** secrets; `main`; force-push; unrelated files.
- **DONE:** changeset = accepted BFF; green; one commit; FF push; `origin/main` unchanged.
- **Verification:** changeset + secret scan + FF + SHAs.
- **Report:** gpt-handoff.
- **Commit/push:** one commit + FF push `dev`.
- **STOP:** after push + verify + handoff.

### Gate 5 — `MICROSERVICE_SERVICE_SKELETONS_PLANNING_V0.1`
- **Purpose:** plan the 6 service project skeletons (Claims, CustomersPolicies, Documents, AIAnalysis, Approval, AuditCost): project layout, contracts, local orchestration (Aspire/compose, described), inter-service HTTP/event seams — no DB.
- **Scope:** docs only.
- **Forbidden:** code/DB/AI/Azure/commit/push/main.
- **DONE:** service-skeleton planning doc (per-service project plan + contracts + local run topology).
- **Verification:** doc completeness; docs-only.
- **Report:** local + gpt-handoff.
- **Commit/push:** none.
- **STOP:** after plan + handoff.

### Gate 6 — `MICROSERVICE_SERVICE_SKELETONS_IMPLEMENTATION_V0.1`
- **Purpose:** create the 6 service projects + contracts + local orchestration; stubbed read/command endpoints (no DB yet); BFF wired to call them.
- **Scope:** `server/**` (6 projects + contracts + AppHost/compose); tests.
- **Forbidden:** DB/EF/migrations; DeepSeek real calls; write persistence; Azure; commit/push.
- **DONE:** solution builds; services boot locally + health; BFF aggregates from services (stubbed data ok); contract tests; no shared DbContext; frontend build PASS.
- **Verification:** build/test; local boot smoke; BFF↔services contract tests; secret scan; FE build.
- **Report:** local + gpt-handoff.
- **Commit/push:** none here.
- **STOP:** after impl + verify + handoff.

### Gate 7 — `COMMIT_AND_PUSH_DEV_MICROSERVICE_SKELETONS_ONLY`
- **Purpose:** commit accepted service skeletons; FF push `dev`.
- **Scope:** stage accepted service-skeleton files; one commit; FF push.
- **Forbidden:** secrets; `main`; force-push; unrelated files.
- **DONE:** changeset = accepted skeletons; green; one commit; FF push; `origin/main` unchanged.
- **Verification:** changeset + secret scan + FF + SHAs.
- **Report:** gpt-handoff.
- **Commit/push:** one commit + FF push `dev`.
- **STOP:** after push + verify + handoff.

### Gate 8 — `MICROSERVICE_PERSISTENCE_PLANNING_V0.1`
- **Purpose:** plan per-service persistence: schema-per-service inside `InsuranceAIPlatform` (Local Phase 1), DbContext + migrations per service, connection-string/secret strategy, seed ownership (200 users in CustomersPolicies; CLM-1006 split per owner), DB-boundary guard.
- **Scope:** docs only.
- **Forbidden:** DB/EF/migrations/seed; code; Azure; commit/push/main.
- **DONE:** persistence planning doc (per-service schema map, migration naming, conn-string strategy, seed plan, guard, reconciliations from §5 of the correction).
- **Verification:** doc completeness; docs-only.
- **Report:** local + gpt-handoff.
- **Commit/push:** none.
- **STOP:** after plan + handoff.

### Gate 9 — `MICROSERVICE_SQLSERVER_EFCORE_PERSISTENCE_IMPLEMENTATION_V0.1`
- **Purpose:** add EF Core + SQL Server **per service** (schema-per-service first); per-service DbContext + migrations; seed 200 test users (CustomersPolicies) + golden CLM-1006 across owners; swap stubs for EF-backed reads behind unchanged contracts.
- **Scope:** `server/**` per-service EF code, packages, migrations, local DB creation, seeders, design-time factory with boundary assert.
- **Forbidden:** shared DbContext; cross-service joins; write endpoints (next gate); DeepSeek; Azure; touching `DevDept`; committing secrets; commit/push.
- **DONE:** each service migrates its schema to local `InsuranceAIPlatform`; **`COUNT(TestUsers)=200`**; all 11 BFF-aggregated reads byte-identical to current; per-service + boundary-guard tests green; no cross-service DB access.
- **Verification:** build/test; `ef database update` per service; seed-count=200; read parity smoke; DevDept-untouched proof; secret scan.
- **Report:** local + gpt-handoff.
- **Commit/push:** none here.
- **STOP:** after impl + verify + handoff.

### Gate 10 — `COMMIT_AND_PUSH_DEV_MICROSERVICE_PERSISTENCE_ONLY`
- **Purpose:** commit accepted per-service persistence; FF push `dev`.
- **Scope:** stage accepted persistence files; one commit; FF push.
- **Forbidden:** secrets/conn-strings; `main`; force-push; unrelated files.
- **DONE:** changeset = accepted persistence; green; no secrets staged; one commit; FF push; `origin/main` unchanged.
- **Verification:** changeset + secret scan + FF + SHAs.
- **Report:** gpt-handoff.
- **Commit/push:** one commit + FF push `dev`.
- **STOP:** after push + verify + handoff.

### Gate 11 — `MICROSERVICE_WRITE_ACTIONS_IMPLEMENTATION_V0.1`
- **Purpose:** implement safe audited write commands across owning services (save approval draft + human approval-submit → Approval; request document → Documents; status transition → Claims), with validation, events/outbox, and mandatory audit events; enable the deferred frontend buttons behind the BFF.
- **Scope:** `server/**` commands + events/outbox + audit emission + tests; minimal `src/**` to enable deferred buttons via the facade.
- **Forbidden:** payout execution; real SMS/email; AI invoking submit; auto-approval; DeepSeek; Azure; commit/push.
- **DONE:** commands persist in the owning service + append audit; events flow to Audit & Cost; human-only submit enforced; deterministic state machine enforced; reads unchanged; FE build PASS.
- **Verification:** build/test (write + audit + event idempotency); write smoke; state-machine rejection tests; no-payout/no-message assertions; secret scan; FE build.
- **Report:** local + gpt-handoff.
- **Commit/push:** none here.
- **STOP:** after impl + verify + handoff.

### Gate 12 — `COMMIT_AND_PUSH_DEV_MICROSERVICE_WRITE_ACTIONS_ONLY`
- **Purpose:** commit accepted write actions; FF push `dev`.
- **Scope:** stage accepted write files; one commit; FF push.
- **Forbidden:** secrets; `main`; force-push; unrelated files.
- **DONE:** changeset = accepted write files; green; one commit; FF push; `origin/main` unchanged.
- **Verification:** changeset + secret scan + FF + SHAs.
- **Report:** gpt-handoff.
- **Commit/push:** one commit + FF push `dev`.
- **STOP:** after push + verify + handoff.

### Gate 13 — `AI_ANALYSIS_SERVICE_DEEPSEEK_GUARDRAILS_IMPLEMENTATION_V0.1`
- **Purpose:** implement AI Analysis Service: `IAiProvider` { Mock (default), DeepSeek (opt-in, disabled-by-default), Disabled }; structured outputs; run persistence + cache (no page-load calls); guardrails INV-1..6; cost/token/audit emission; explicit mock fallback.
- **Scope:** `server/**` AI service + provider adapters + guardrails + tests; minimal `src/**` to surface advisory output honestly; model names `deepseek-v4-flash`/`-pro`; env-var **name** `DEEPSEEK_API_KEY` only.
- **Forbidden:** **reading/logging/printing `DEEPSEEK_API_KEY`**; committing any key; real calls by default; AI deciding/approving/asserting-fraud; Azure; commit/push.
- **DONE:** provider abstraction + 3 impls; default `mock`; **DeepSeek-disabled-by-default test passes**; runs cached (no duplicate calls on reload); every run emits audit+cost+runId/traceId; honest labels; fallback records `fallbackReason`; no secret in repo/logs; guardrail tests pass; FE advisory copy preserved.
- **Verification:** provider + disabled-by-default + guardrail + cache + audit/cost tests; secret scan (key + conn string); FE build.
- **Report:** local + gpt-handoff.
- **Commit/push:** none here.
- **STOP:** after impl + verify + handoff. No real calls enabled by default. No key exposure.

### Gate 14 — `COMMIT_AND_PUSH_DEV_AI_ANALYSIS_SERVICE_ONLY`
- **Purpose:** commit accepted AI Analysis Service; FF push `dev`.
- **Scope:** stage accepted AI files; one commit; FF push.
- **Forbidden:** secrets/keys; `main`; force-push; unrelated files.
- **DONE:** changeset = accepted AI files; green; **no `DEEPSEEK_API_KEY` value anywhere**; one commit; FF push; `origin/main` unchanged.
- **Verification:** changeset + secret scan (key + conn) + FF + SHAs.
- **Report:** gpt-handoff.
- **Commit/push:** one commit + FF push `dev`.
- **STOP:** after push + verify + handoff.

### Gate 15 — `FULL_LOCAL_MICROSERVICE_VERIFICATION_BEFORE_SLAVA_MANUAL_V0.1`
- **Purpose:** full local-machine verification across all services + BFF + frontend before the owner manual walkthrough.
- **Scope:** read-only verification + report; start/stop local services + FE for smoke; no source changes.
- **Forbidden:** behavior changes; source commit; real AI calls; Azure.
- **DONE:** all services build/test green; per-service migrate clean; **TestUsers=200**; BFF aggregates all 11 reads; 4 write commands smoke + audit/events; AI mock smoke + DeepSeek-disabled-by-default confirmed; audit trace `BLOCK` for CLM-1006; secret scan clean; FE build + backend-mode + mock-mode smoke; documented evidence + known-issues.
- **Verification:** the above captured as command outputs.
- **Report:** local + gpt-handoff.
- **Commit/push:** none (gpt-handoff publish only).
- **STOP:** after verification + handoff. Hand to owner. No Azure.

### Gate 16 — `PRE_AZURE_FULL_LOCAL_MANUAL_ACCEPTANCE_CHECKPOINT`
- **Purpose:** owner manually walks all 11 screens locally and records ACCEPT or a defect list. **Owner-driven; agent only prepares + records.**
- **Scope:** prepare run instructions; record findings; no code changes.
- **Forbidden:** agent self-accepting; behavior changes; Azure; source commit.
- **DONE:** owner verdict captured (ACCEPT / defect list); defects triaged into gate 17.
- **Verification:** owner sign-off recorded.
- **Report:** local + gpt-handoff.
- **Commit/push:** none.
- **STOP:** after verdict. No Azure without ACCEPT.

### Gate 17 — `POST_MANUAL_ACCEPTANCE_FIX_BATCH_V0.1`
- **Purpose:** implement triaged fixes from the manual checkpoint (local only), within the recorded defect list.
- **Scope:** `src/**`/`server/**` fixes strictly per the defect list; tests; followed by a commit/push gate if needed.
- **Forbidden:** new features beyond defects; Azure; secrets; `main`.
- **DONE:** each defect fixed + verified; build/test green; boundaries/guardrails intact; no scope creep.
- **Verification:** per-defect evidence; full build/test; secret scan.
- **Report:** local + gpt-handoff.
- **Commit/push:** implement here; commit/push via dedicated follow-up gate.
- **STOP:** after fixes + verify + handoff.

### Gate 18 — `AZURE_MICROSERVICE_DEPLOYMENT_PLANNING_V0.1`
- **Purpose:** plan the minimal Azure deployment **mapping** (Container Apps per service, Azure SQL per service/pool, Service Bus/Event Grid, Blob, Key Vault, App Insights), cost ceiling, secret strategy, rollback — planning only.
- **Scope:** docs only.
- **Forbidden:** creating Azure resources; secrets; code; source commit.
- **DONE:** Azure plan doc (topology, SKU/cost ceiling, Key Vault secret strategy incl. `DEEPSEEK_API_KEY`, migration-to-cloud, rollback, deploy gate sequence).
- **Verification:** doc completeness.
- **Report:** local + gpt-handoff.
- **Commit/push:** none.
- **STOP:** after plan + handoff. No resources created.

### Gate 19 — `AZURE_MINIMAL_MICROSERVICE_DEPLOYMENT_IMPLEMENTATION_V0.1`
- **Purpose:** provision the minimal Azure footprint and deploy the services + BFF; secrets via Key Vault only; map service boundaries to Azure resources (no rewrite).
- **Scope:** Azure resources per the plan; cloud config; deploy from `dev`.
- **Forbidden:** secrets in repo; over-provisioning beyond ceiling; `main` promotion; real PII.
- **DONE:** services reachable on Azure; per-service Azure SQL migrated + seeded (200 users); AI mode via cloud config (default mock/disabled); health green; no secrets in repo.
- **Verification:** cloud smoke (health/read/write/AI-mode); cost check; secret scan.
- **Report:** local + gpt-handoff.
- **Commit/push:** infra/config committed without secrets; `dev` only.
- **STOP:** after deploy + verify + handoff. No `main`.

### Gate 20 — `POST_AZURE_FINAL_VERIFICATION_V0.1`
- **Purpose:** verify the deployed microservices end-to-end; confirm release readiness.
- **Scope:** read-only cloud + local verification + report.
- **Forbidden:** behavior changes; secrets; `main` promotion (next gate).
- **DONE:** cloud health/read/write/AI-guardrail smoke; secret scan; cost within ceiling; portfolio-quality checklist; go/no-go.
- **Verification:** documented evidence.
- **Report:** local + gpt-handoff.
- **Commit/push:** none.
- **STOP:** after verification + handoff. Await release decision.

### Gate 21 — `MAIN_PROMOTION_PORTFOLIO_RELEASE_V0.1`
- **Purpose:** promote verified `dev` → `main` as the stable portfolio release (the **only** gate allowed to touch `main`, owner-authorized).
- **Scope:** FF / reviewed merge `dev`→`main`; release notes; optional tag.
- **Forbidden:** force-push; secrets; unreviewed/unauthorized promotion; history rewrite.
- **DONE:** `main` at the verified commit; release notes; portfolio links updated; `dev`/`main` consistent.
- **Verification:** pre-promotion full verification green; post-promotion `origin/main` SHA recorded; no secrets.
- **Report:** local + gpt-handoff.
- **Commit/push:** the only `main`-touching gate, owner-authorized only.
- **STOP:** after promotion + verify + handoff. Arc complete.

---

## Ordering rationale
BFF first (2–4) so the frontend keeps one stable surface while the inside changes. Service skeletons (5–7) establish boundaries + contracts before any DB. Persistence (8–10) is per-service (schema-per-service), not a shared store. Writes (11–12) need persistence + audit. AI (13–14) is last of the build phases, isolated in its own service. Full local verification (15) gates the owner checkpoint (16); fixes (17) close the loop locally. Azure (18–20) is a mapping after local acceptance. `main` promotion (21) is final and owner-authorized only.

## Relationship to the superseded single-backend sequence
`LOCAL_COMPLETION_GATE_SEQUENCE_V0.1.md` is **superseded**. Its `SQLSERVER_EFCORE_PERSISTENCE_IMPLEMENTATION_V0.1` is replaced by the microservice persistence gates (8–10), which come **after** BFF + service skeletons. The immediate next gate is **`BFF_API_GATEWAY_SKELETON_PLANNING_V0.1`** (gate 2).
