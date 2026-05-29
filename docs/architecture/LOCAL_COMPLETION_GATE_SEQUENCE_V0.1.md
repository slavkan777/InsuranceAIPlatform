# Local Completion Gate Sequence — V0.1

> ## ⚠️ SUPERSEDED BY MICROSERVICE LOCAL GATE SEQUENCE (2026-05-27)
> This single-backend gate sequence is **SUPERSEDED** by [`MICROSERVICE_LOCAL_GATE_SEQUENCE_V0.1.md`](MICROSERVICE_LOCAL_GATE_SEQUENCE_V0.1.md) following the microservice architecture correction.
> - **`SQLSERVER_EFCORE_PERSISTENCE_IMPLEMENTATION_V0.1` is NO LONGER the immediate next gate** and is replaced by per-service persistence gates that come **after** BFF + service skeletons.
> - The immediate next gate is **`BFF_API_GATEWAY_SKELETON_PLANNING_V0.1`**.
> - Use this document only for historical reference; follow the microservice sequence for execution.

Machine-readable, ordered gate contracts from the current state (`dev` @ `f8df2b6`, read integration + action honesty done) through local completion, owner manual acceptance, Azure, and `main` portfolio release. Companion to `LOCAL_COMPLETION_ARCHITECTURE_PLAN_BEFORE_DB_WRITE_AI_V0.1.md`.

**Conventions**
- Implementation and commit/push are **separate gates** (project rule G7: implement → verify → commit-only → push-only).
- Every gate: synthetic data only · no secrets in repo · no `DEEPSEEK_API_KEY` read/log/print · `main` untouched until gate 13 · no force-push · independent Opus inspector before any commit/push or "done".
- `dev` = integration branch; `main` = stable portfolio branch.
- DB name is always `InsuranceAIPlatform`; `DevDept` is never touched.

---

## Gate 1 — `SQLSERVER_EFCORE_PERSISTENCE_IMPLEMENTATION_V0.1`
- **Purpose:** Add EF Core + SQL Server; create local DB `InsuranceAIPlatform`; `InsuranceAiDbContext` + 24 entity configs (23 existing + `TestUsers`); `InitialCreate` migration; idempotent seeders (golden CLM-1006 graph + bulk tier incl. exactly 200 `TestUsers`); swap `InMemoryClaimReadService` for the EF-backed `IClaimReadRepository` behind unchanged interfaces.
- **Allowed:** `server/**` EF code, EF Core SQL Server packages, migrations, local DB creation, seeders, design-time factory with DB-name assert, user-secrets/env connection string.
- **Forbidden:** changing read DTO shapes; write endpoints; AI provider; Azure; touching `DevDept`; committing secrets/connection strings; `src/**` behavior change; commit/push (separate gate).
- **DONE:** packages added; DbContext + configs; migration applies to fresh `InsuranceAIPlatform`; seed runs idempotently; `COUNT(TestUsers)=200`; all 11 read endpoints return byte-identical golden shapes; existing tests green; new EF/seed/DB-name-guard tests pass; reconciliations from plan §5 applied (CustomerId `CUST-4421`, conn key `ConnectionStrings:InsuranceAIPlatform`).
- **Verification:** `dotnet build` 0/0; `dotnet test` green; `dotnet ef database update` clean; `SELECT COUNT(*) FROM TestUsers`=200; read smoke; secret scan clean; DevDept-untouched proof.
- **Report:** local report + gpt-handoff (`sqlserver-efcore-persistence-implementation-v0.1`).
- **Commit/push rule:** none here.
- **STOP:** after implementation + verification + handoff. No commit/push. No write endpoints. No AI. No Azure.

## Gate 2 — `COMMIT_AND_PUSH_DEV_SQLSERVER_EFCORE_PERSISTENCE_ONLY`
- **Purpose:** Commit the accepted persistence work and fast-forward push `dev`.
- **Allowed:** stage accepted `server/**` (+ gitignored config note) only; one commit; FF push `dev`.
- **Forbidden:** new behavior; secrets; `main` push/merge/PR; force-push; bundling unrelated files.
- **DONE:** changeset = accepted persistence files; build/test green; no secrets/connection strings staged; one commit on `dev`; FF push; `origin/main` unchanged.
- **Verification:** changeset review; secret scan; FF check; pre/post `origin/dev` + `origin/main` SHAs.
- **Report:** gpt-handoff (`commit-and-push-dev-sqlserver-efcore-persistence-only`).
- **Commit/push rule:** exactly one commit + FF push `dev`.
- **STOP:** after push + verify + handoff. No `main`.

## Gate 3 — `WRITE_ACTIONS_BACKEND_IMPLEMENTATION_V0.1`
- **Purpose:** Implement the four safe audited write commands (save approval draft, request missing document, append audit event, human approval-submit) with validation, deterministic status transitions, and mandatory audit events.
- **Allowed:** `server/**` write endpoints + command handlers + validation + audit append + write tests; minimal `src/**` to enable the deferred buttons behind the facade.
- **Forbidden:** payout execution; real SMS/email; AI invoking approval-submit; auto-approval; AI provider; Azure; commit/push.
- **DONE:** 4 endpoints; DTO validation; state machine enforced; every write appends an `AuditEvent`; payout/messaging absent; write smoke + audit assertions pass; reads unchanged; frontend build PASS.
- **Verification:** `dotnet test` (write + audit tests) green; write smoke; state-machine rejection tests; secret scan; FE build.
- **Report:** local + gpt-handoff (`write-actions-backend-implementation-v0.1`).
- **Commit/push rule:** none here.
- **STOP:** after implementation + verification + handoff. No commit/push. No payout/messaging. No AI. No Azure.

## Gate 4 — `COMMIT_AND_PUSH_DEV_WRITE_ACTIONS_ONLY`
- **Purpose:** Commit accepted write-actions work; FF push `dev`.
- **Allowed:** stage accepted write files; one commit; FF push `dev`.
- **Forbidden:** secrets; `main`; force-push; unrelated files.
- **DONE:** changeset = accepted write files; green; one commit; FF push; `origin/main` unchanged.
- **Verification:** changeset + secret scan + FF + SHAs.
- **Report:** gpt-handoff (`commit-and-push-dev-write-actions-only`).
- **Commit/push rule:** one commit + FF push `dev`.
- **STOP:** after push + verify + handoff. No `main`.

## Gate 5 — `AI_PROVIDER_LOCAL_INTEGRATION_AND_GUARDRAILS_V0.1`
- **Purpose:** Implement `IAiProvider` with `MockAiProvider` (default), `DeepSeekAiProvider` (opt-in, disabled by default), `DisabledAiProvider`; provider-mode config; guardrails (INV-1..6); cost/token/audit metadata; structured JSON output; mock fallback with `fallbackReason`.
- **Allowed:** `server/**` AI abstraction + adapters + guardrail enforcement + provider tests; minimal `src/**` to surface advisory output honestly; DeepSeek model names `deepseek-v4-flash`/`deepseek-v4-pro`; env-var name `DEEPSEEK_API_KEY` referenced only.
- **Forbidden:** **reading/logging/printing `DEEPSEEK_API_KEY`**; committing any key; real calls by default; AI deciding/approving/asserting fraud; Azure; commit/push.
- **DONE:** provider abstraction + 3 implementations; default mode `mock`; **DeepSeek-disabled-by-default test passes (no HTTP without explicit mode)**; guardrails enforced server-side; every run records audit + cost + runId/traceId; honest labels; fallback records `fallbackReason`; no secret in repo/logs; FE advisory copy preserved.
- **Verification:** provider unit tests; disabled-by-default test; guardrail tests (no auto-approve / no fraud-as-fact); audit+cost assertions; secret scan (incl. key-name value never present); FE build.
- **Report:** local + gpt-handoff (`ai-provider-local-integration-and-guardrails-v0.1`).
- **Commit/push rule:** none here.
- **STOP:** after implementation + verification + handoff. No commit/push. No real calls enabled by default. No key exposure. No Azure.

## Gate 6 — `COMMIT_AND_PUSH_DEV_AI_PROVIDER_GUARDRAILS_ONLY`
- **Purpose:** Commit accepted AI-provider/guardrails work; FF push `dev`.
- **Allowed:** stage accepted AI files; one commit; FF push `dev`.
- **Forbidden:** secrets/keys; `main`; force-push; unrelated files.
- **DONE:** changeset = accepted AI files; green; **no `DEEPSEEK_API_KEY` value anywhere**; one commit; FF push; `origin/main` unchanged.
- **Verification:** changeset + secret scan (key + conn string) + FF + SHAs.
- **Report:** gpt-handoff (`commit-and-push-dev-ai-provider-guardrails-only`).
- **Commit/push rule:** one commit + FF push `dev`.
- **STOP:** after push + verify + handoff. No `main`.

## Gate 7 — `FULL_LOCAL_MACHINE_VERIFICATION_BEFORE_SLAVA_MANUAL_V0.1`
- **Purpose:** Full local-machine verification that the app is ready for the owner's manual walkthrough.
- **Allowed:** read-only verification + a verification report; start/stop local backend + frontend for smoke; no source changes.
- **Forbidden:** behavior changes; commit of source; real AI calls; Azure.
- **DONE:** backend build/test green; EF migrate clean; **TestUsers count = 200**; all 11 read endpoints smoke; all 4 write commands smoke + audit; AI mock smoke + DeepSeek-disabled-by-default confirmed; secret scan clean; frontend build + backend-mode + mock-mode smoke; documented checklist with evidence; known-issues list.
- **Verification:** the above, captured as command outputs in the report.
- **Report:** local + gpt-handoff (`full-local-machine-verification-before-slava-manual-v0.1`).
- **Commit/push rule:** none (verification only); gpt-handoff publish allowed.
- **STOP:** after verification + handoff. Hand to owner for manual acceptance. No Azure.

## Gate 8 — `PRE_AZURE_FULL_LOCAL_MANUAL_ACCEPTANCE_CHECKPOINT`
- **Purpose:** Owner manually walks all 11 screens locally (backend + mock modes), exercises safe writes + AI advisory, and records acceptance or a defect list. **Owner-driven; agent only prepares + records.**
- **Allowed:** prepare run instructions; record owner findings into a defect list; no code changes.
- **Forbidden:** agent self-accepting; behavior changes; Azure; commit of source.
- **DONE:** owner verdict captured (ACCEPT or defect list); defect list triaged into the next gate.
- **Verification:** owner sign-off recorded; screenshots optional.
- **Report:** local + gpt-handoff (`pre-azure-full-local-manual-acceptance-checkpoint`).
- **Commit/push rule:** none (acceptance record only).
- **STOP:** after recording owner verdict. Do not proceed to Azure without ACCEPT.

## Gate 9 — `POST_MANUAL_ACCEPTANCE_FIX_BATCH_V0.1`
- **Purpose:** Implement the triaged fixes from the manual acceptance defect list (local only).
- **Allowed:** `src/**`/`server/**` fixes strictly within the recorded defect list; tests; followed by a separate commit/push gate if needed.
- **Forbidden:** new features beyond the defect list; Azure; secrets; `main`.
- **DONE:** each defect fixed + verified; build/test green; reads/writes/AI guardrails intact; no scope creep.
- **Verification:** per-defect evidence; full build/test; secret scan.
- **Report:** local + gpt-handoff (`post-manual-acceptance-fix-batch-v0.1`).
- **Commit/push rule:** implement here; commit/push via a dedicated follow-up gate.
- **STOP:** after fixes + verification + handoff.

## Gate 10 — `AZURE_DEPLOYMENT_PLANNING_V0.1`
- **Purpose:** Plan a minimal Azure deployment (compute, managed SQL, config/secret management, cost ceiling) — **planning only**.
- **Allowed:** planning docs only.
- **Forbidden:** creating Azure resources; secrets; code changes; commit of source.
- **DONE:** Azure plan doc (topology, SKU/cost ceiling, secret strategy via Key Vault/app settings, migration-to-cloud approach, rollback); gate sequence to deploy.
- **Verification:** doc completeness review.
- **Report:** local + gpt-handoff (`azure-deployment-planning-v0.1`).
- **Commit/push rule:** none (planning); gpt-handoff publish allowed.
- **STOP:** after plan + handoff. No Azure resources created.

## Gate 11 — `AZURE_MINIMAL_DEPLOYMENT_IMPLEMENTATION_V0.1`
- **Purpose:** Provision the minimal Azure footprint and deploy the local app; secrets via Azure config/Key Vault only.
- **Allowed:** Azure resource creation per the plan; cloud config; deploy from `dev`.
- **Forbidden:** secrets in repo; over-provisioning beyond the cost ceiling; `main` promotion; real PII.
- **DONE:** app reachable on Azure; managed SQL migrated + seeded (200 users); AI mode controlled by cloud config (default mock/disabled); health green; no secrets in repo.
- **Verification:** cloud smoke (health, read, write, AI mode), cost check, secret scan.
- **Report:** local + gpt-handoff (`azure-minimal-deployment-implementation-v0.1`).
- **Commit/push rule:** infra/config committed without secrets; `dev` only.
- **STOP:** after deploy + verify + handoff. No `main` promotion.

## Gate 12 — `POST_AZURE_FINAL_VERIFICATION_V0.1`
- **Purpose:** Verify the deployed app end-to-end and confirm release readiness.
- **Allowed:** read-only cloud + local verification + report.
- **Forbidden:** behavior changes; secrets; `main` promotion (next gate).
- **DONE:** cloud health/read/write/AI-guardrail smoke; secret scan; cost within ceiling; portfolio-quality checklist; go/no-go.
- **Verification:** documented evidence.
- **Report:** local + gpt-handoff (`post-azure-final-verification-v0.1`).
- **Commit/push rule:** none (verification).
- **STOP:** after verification + handoff. Await release decision.

## Gate 13 — `MAIN_PROMOTION_PORTFOLIO_RELEASE_V0.1`
- **Purpose:** Promote the verified `dev` to `main` as the stable portfolio release.
- **Allowed:** fast-forward / reviewed merge `dev` → `main` per branch policy; release notes; tag if desired.
- **Forbidden:** force-push; secrets; unreviewed promotion; rewriting history.
- **DONE:** `main` updated to the verified commit; release notes; portfolio links updated; `dev` and `main` consistent.
- **Verification:** pre-promotion full verification green; post-promotion `origin/main` SHA recorded; no secrets.
- **Report:** local + gpt-handoff (`main-promotion-portfolio-release-v0.1`).
- **Commit/push rule:** the **only** gate permitted to update `main`, and only with explicit owner authorization.
- **STOP:** after promotion + verify + handoff. Project local-completion → release arc complete.

---

## Ordering rationale
Persistence (1–2) precedes writes (3–4) because audited writes need a durable store and audit tables. AI (5–6) follows writes so the approval/audit surfaces exist for AI to annotate (advisory). Full local verification (7) gates the owner manual checkpoint (8); fixes (9) close the loop locally. Azure (10–12) is deferred until local acceptance. `main` promotion (13) is last and owner-authorized only.
