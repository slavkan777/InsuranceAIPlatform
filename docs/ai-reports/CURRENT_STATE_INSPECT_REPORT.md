# InsuranceAIPlatform — Current State Inspect Report

GATE_ID / REQUEST_ID: insuranceai-current-state-inspect-2026-06-10

- Prompt origin: Git file: `slavkan777/InsuranceAIPlatform:docs/ai-workflows/CURRENT_STATE_INSPECT_REQUEST.md@rag/local-foundation-mega-v0.1` (`8782bac`)
- Mode: inspect-only + this one report file; no code/config/docs modified
- Date: 2026-06-10

## 1. VERDICT

InsuranceAIPlatform is a **working, live-deployed portfolio demo** with an unusually complete safety/evidence story, now operating under an **accepted project AI operating layer**. VERIFIED today: full .NET suite **188/188 green** (fresh run), live Azure API + SWA respond 200, and the LangChain advisory endpoint returns claim-scoped citations live. It is **demo-ready, NOT production-ready**: the API is anonymous, CI/CD is a disabled skeleton, the outbox has no dispatcher, and the AI runs deterministic/mock by default. No state is invented below; every claim carries a label.

## 2. CURRENT STATE

- Branch `rag/local-foundation-mega-v0.1` @ `8782bac`, working tree clean; **41 commits ahead of `origin/main`, 0 behind** (main = pre-RAG demo baseline; all RAG/sidecar/bridge work lives only on this branch). VERIFIED (`git rev-list --count`).
- Project AI operating layer **ACCEPTED 2026-06-10**: operating doc + audit-fixes addendum + learning log, with acceptance record and two verification reports. VERIFIED (`docs/ai-workflows/PROJECT_AI_OPERATING_SYSTEM_ACCEPTANCE_RECORD.md`).
- Live demo (probed today, read-only): API `/health` → 200 `Healthy` (0.19s warm); SWA root → 200; `POST /api/claims/CLM-1006/advanced-ai-review` → `framework=langchain, providerMode=Deterministic, advisoryOnly=true, confidence=95, 6 citations all CLM-1006-scoped`. VERIFIED (HTTP responses).
- Cold-start observed: the first `/health` probe timed out at 20s (HTTP 000), the retry answered in 0.19s — consistent with the API's scale-to-zero configuration (minReplicas=0 per deployment reports). PARTIALLY VERIFIED (behavior observed today; replica config from the deployment gate report, not re-queried via Azure CLI in this gate).

## 3. CURRENT GATE

`insuranceai-current-state-inspect-2026-06-10` — first real inspect-only gate under the accepted baseline; allowed artifact: this report only; commit/push allowed for the report file only.

## 4. ROUTING LOCK VERIFIED

- Workspace `C:/Projects/InsuranceAIPlatform` = clone of `slavkan777/InsuranceAIPlatform` (git remote), branch `rag/local-foundation-mega-v0.1` matches the lock. VERIFIED.
- Forbidden targets untouched: no application code, runtime config, Azure files, provider config, operating docs, or learning log modified; only the allowed report file created. VERIFIED (`git status` before commit shows only the report).

## 5. WHAT EXISTS

**Frontend** (VERIFIED — paths in repo): React 18 + TypeScript + Vite + Tailwind (`src/`, `vite.config.ts`); Redux Toolkit + redux-saga (`src/app/store.ts`, `src/app/rootSaga.ts`, sagas in `src/features/{aiReview,approval,claims}`); router with claim workspace pages; i18n EN/UA; demo login gate (`src/features/auth/authSlice.ts` — client-side localStorage, publicly shown demo credentials); API facade with mock↔backend switch (`src/api/insuranceApi.ts`); Advanced AI Review panel (`src/components/claim/AdvancedAiReviewPanel.tsx`); **23 Playwright e2e specs** (`e2e/*.spec.ts`) + two Playwright configs (backend + mock).

**Backend** (VERIFIED): .NET 9 modular monolith — `InsuranceAIPlatform.Api` + `BuildingBlocks` + 6 `Services.*` (Claims, CustomersPolicies, Documents, AiAnalysis, Approval, AuditCost) + `DbMigrator` + `Tests` (`server/InsuranceAIPlatform.sln`). Command endpoints write domain row + audit + outbox with correlation ids and idempotency keys (`Api/Controllers/ClaimCommandsController.cs`). Config-driven CORS. **Tests: 188/188 passed, fresh Release run today.** VERIFIED (command output).

**AI/RAG** (VERIFIED in code; runtime modes labelled): provider seam `IAiProvider` with Mock / DeepSeek-real / DeepSeek-disabled (`Services.AiAnalysis/Providers/`); guardrails incl. advisory-only evaluator (`Guardrails/`); RAG with deterministic local-hash embeddings, strictly claim-scoped chunk retrieval, ingestion of uploaded docs into evidence chunks, grounded generators (Mock + LocalLlama seam), eval-questions harness (`Services.AiAnalysis/Rag/...`, `Tests/Rag*Tests.cs`); Qdrant adapter and Ollama client exist as **disabled seams**.

**LangChain sidecar** (VERIFIED): FastAPI + LangChain advisory analytics service (`ai-sidecars/langchain-claim-analytics/app.py` + Dockerfile), deterministic by default, optional local Ollama via env; consumed by a feature-flagged .NET endpoint (`Api/Controllers/AdvancedAiReviewController.cs`, flag default OFF in code; ON in the live demo per deployment report + live behavior verified today).

**Azure/infra**: live Container Apps API + internal sidecar app + SWA + Azure SQL — **live behavior VERIFIED today via HTTP**; resource/revision identities (`iap-demo-api--0000005`, `iap-langchain-sidecar--nqj6jlh`) per the deployment-and-smoke gate report (PARTIALLY VERIFIED in this gate — not re-queried via az). Bicep skeleton (`infra/`), **deploy workflow exists but is a disabled validate-only skeleton** (`.github/workflows/azure-deploy-demo.yml`). Azure runbooks under `docs/architecture/azure/`.

**Operating layer** (VERIFIED, all active): `PROJECT_AI_OPERATING_SYSTEM.md` + `..._AUDIT_FIXES.md` (active addendum) + `..._ACCEPTANCE_RECORD.md` + `docs/ai-learnings/LOG.md` (3 entries) + 5 reports in `docs/ai-reports/` (incl. this one) + request files in `docs/ai-workflows/`.

## 6. WHAT IS MISSING / NOT VERIFIED

- **API authentication/authorization: absent** — controllers are anonymous; the UI login is client-side only. VERIFIED absent (no auth middleware in `Api/Program.cs`).
- **CI/CD: not operational** — workflow is a manual, validate-only skeleton with deploy steps commented out. VERIFIED (`.github/workflows/azure-deploy-demo.yml`).
- **Outbox dispatcher: absent** — outbox rows are written, no `BackgroundService`/`IHostedService` consumes them. VERIFIED absent (grep over `server/`).
- **Real users / real traffic / business validation: NOT VERIFIED** (no repository evidence; must not be claimed).
- **Managed/real LLM quality: NOT VERIFIED** — default providers are Mock/Deterministic; DeepSeek/Ollama are key-/env-gated seams. Calibrated confidence: NOT VERIFIED (heuristic numbers).
- **Compliance/security readiness: NOT VERIFIED** (and contradicted by the anonymous API).
- **Production readiness: NOT VERIFIED** — must not be claimed (see boundaries).
- main branch does not contain the RAG/sidecar/bridge work (41 commits unmerged) — merge/PR strategy NOT yet defined in any doc. VERIFIED gap.

## 7. PROJECT BOUNDARIES

Active boundaries (operating doc §3/§10 + addendum §2–3, all VERIFIED as accepted): no main/protected-branch push; no app-code/Azure/provider/secret changes outside an explicit gate; forbidden repos by default (ai-kb, gpt-handoff); workspace lock `C:/Projects/InsuranceAIPlatform`; seeded claims `CLM-1006/1007/1012` immutable; synthetic data only (`E2E-*`); no paid LLM providers / new Azure AI resources; claim boundaries (no production/users/LLM-quality/compliance/CI-CD claims without evidence — applied throughout this report).

## 8. RISKS

1. **Anonymous write API on a public demo** — anyone with the FQDN can create claims/upload documents (synthetic-data blast radius; seeded demo rows protected only by convention). Highest product risk.
2. **Cold-start UX** — scale-to-zero API: first request after idle can exceed 20s (observed today); a demo viewer may see a hung first load.
3. **Merge debt** — 41 commits on one long-lived feature branch; `main` increasingly stale as the "demo baseline".
4. **Two-layer operating docs** — main doc + addendum must be read together until folded (accepted, planned).
5. **Sidecar fixed cost** — always-on internal app (minReplicas=1 per deployment report) accrues small constant cost; flip to 0 only with the cold-start fallback caveat.
6. **Synthetic E2E leftovers** — claims `CLM-1026..1032` from prior smoke gates remain in the demo data (harmless, labelled synthetic).

## 9. EVIDENCE

- Git: `git branch --show-current`; `git log -1` = `8782bac`; `git rev-list --count origin/main..HEAD` = 41 / reverse 0; clean `git status`.
- Tests: `dotnet test InsuranceAIPlatform.sln -c Release` → `Passed! Failed: 0, Passed: 188, Total: 188` (today).
- Live HTTP (today): `/health` → 200 `{"status":"Healthy",...}` (0.19s; first attempt timed out at 20s → cold start); SWA root → 200 (760 B shell); `POST .../CLM-1006/advanced-ai-review` → `langchain / Deterministic / advisoryOnly=true / confidence 95 / 6 citations, all CLM-1006`.
- Files: paths cited inline in §5; operating layer files + acceptance record read in full; ai-kb baseline V0.1.1 @ `e12e90a` + routing-lock rule (227 lines) read in full this session.
- Deployment identities (revisions, replica counts): per `gpt-handoff` deployment-and-smoke gate report + `docs/architecture/azure/` runbooks — labelled PARTIALLY VERIFIED here (behavior re-verified live; identities not re-queried).

## 10. RECOMMENDED NEXT SAFE STEP

Two candidates, in order:
1. **Docs-only consolidation gate** (smallest): fold the addendum into `PROJECT_AI_OPERATING_SYSTEM.md` (restoring the literal "security" in the default-blocked bucket), trim the copied global sections to links, and seed `LOG.md` with 1–2 technical lessons already evidenced in reports (auth-gated UI smoke; registry credential-helper mismatch; cold-start probe from this gate).
2. **First small-code gate** (highest product value, small blast radius): add basic API protection for write endpoints (API key or rate limit) on the feature branch — directly addresses Risk 1 without touching the demo's read paths — with tests and a standard evidence report.

GitHub handoff ready. Tell GPT: отчёт.
