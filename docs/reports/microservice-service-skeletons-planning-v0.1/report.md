# Microservice Service Skeletons Planning — V0.1 — Report

**Gate:** `MICROSERVICE_SERVICE_SKELETONS_PLANNING_V0.1` · **Date:** 2026-05-27
**Type:** planning-only — no source code, no projects, no `.csproj`/`.sln`, no DB/EF, no writes, no AI, no Azure, no source commit/push.

## Current state
- Source: branch `dev` @ `9f494a1` (BFF skeleton committed); `origin/dev` `9f494a1`; `origin/main` `69e6731` (unchanged). Source code clean (only prior-gate planning docs untracked).
- BFF (`current-api-as-bff`): existing API project is the gateway; 13 preserved read routes + 2 additive read-only BFF endpoints; correlation middleware; delegates to `InMemoryClaimReadService`. Build PASS, 22 tests PASS, frontend PASS.

## Why this gate
Make the six service boundaries behind the BFF explicit (as skeletons) before persistence/writes/AI, so later gates attach to real owners — without a DB or web-host sprawl.

## What this gate produced
2 architecture docs + this local report + AIKB light sync + sanitized gpt-handoff. No code/projects/DB/AI.

## Files created (source repo, local, uncommitted)
- `docs/architecture/MICROSERVICE_SERVICE_SKELETONS_PLANNING_V0.1.md` (all required sections).
- `docs/architecture/MICROSERVICE_SERVICE_SKELETONS_CONTRACT_MAP_V0.1.md` (6 services × 13 required columns + dependency direction + BFF→service delegation).
- `docs/reports/microservice-service-skeletons-planning-v0.1/report.md` (this).

## Chosen strategy
**Option C — Hybrid: per-service class-library skeletons, in-process behind the BFF.** Six `Services.*` class libs (interface + contracts + DI extension + health metadata) + a thin `BuildingBlocks` primitives lib; registered in the BFF DI; skeleton implementations delegate to the current read logic (response-identical). **No web hosts, no DB, no writes, no AI calls** in the skeleton. Rationale: real compile-time boundaries + clean dependency direction + Azure-mappable, without premature host/DB complexity. (Alternatives A = six web APIs now [premature], B = folders in current API [weak boundary] — both rejected.)

## Services planned (6)
Claims · Customers & Policies · Documents · AI Analysis · Approval · Audit & Cost — each with owns/does-not-own, skeleton artifacts, contracts, BFF dependency, future read/command/events, future data boundary, tests, and Azure mapping in the contract map.

## Structure for next gate
`server/` adds `InsuranceAIPlatform.BuildingBlocks` + `InsuranceAIPlatform.Services.{Claims,CustomersPolicies,Documents,AiAnalysis,Approval,AuditCost}` (7 new class-lib projects) to the existing `.sln`. Dependency direction: `Api`(BFF) → `Services.*` → `BuildingBlocks`; services never reference each other; BuildingBlocks holds primitives only (no domain/DTOs/DbContext).

## BFF delegation
Stage 1 (committed) BFF → read service. Stage 2 (next gate) BFF → six service interfaces (DI), delegating to the current read logic, response-identical. Stage 3 read ownership into services → Stage 4 per-service persistence → Stage 5 write/events/audit → Stage 6 AI Analysis provider.

## Verification (this gate)
- Only docs/report/handoff + AIKB changed; **no `src/**`, no `server/**` code, no `.csproj`/`.sln`, no package** (source `git status`: only untracked docs).
- No projects created; no DB/EF/migrations; no write endpoints; no AI provider call; no Azure.
- `DEEPSEEK_API_KEY` never read/printed/logged; no secret value; synthetic only (`CLM-1006`); no real PII.
- No source commit/push; `main` untouched (`69e6731`); source HEAD still `9f494a1`.
- Docs explicitly: frontend → BFF only; BFF owns no data/DB/AI; services own data later; no shared all-entity DbContext; no cross-service DB access; DeepSeek isolated in AI Analysis later; next gate `MICROSERVICE_SERVICE_SKELETONS_IMPLEMENTATION_V0.1`.

## AIKB light sync
`ai-kb` (`slavkan777/ai-kb`) updated: `CURRENT_STATE.md` (status `BFF_SKELETON_COMMITTED_TO_DEV`, BFF commit `9f494a1` recorded, next gate service-skeletons implementation, read-API counts corrected to 13 GET/13 tests), `TASK_LEDGER.md` (compact BFF-commit accepted entry), `ACTIVE_PROJECTS.md` (row + section reconciled).

## Next safe step
`MICROSERVICE_SERVICE_SKELETONS_IMPLEMENTATION_V0.1` — create the 7 class-lib projects (interfaces/contracts/DI/health), register in the BFF, delegate to current read logic, add DI/health/preserved-route tests. No DB/EF/writes/AI/Azure; no source commit/push (separate gate). Then `COMMIT_AND_PUSH_DEV_MICROSERVICE_SERVICE_SKELETONS_ONLY`.
