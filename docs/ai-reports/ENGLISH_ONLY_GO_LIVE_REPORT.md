# InsuranceAIPlatform — English-Only Go-Live Report

GATE_ID / REQUEST_ID: `IAP-LEASE15-AZURE-ENGLISH-ONLY-GO-LIVE-2026-07-25` (macro lease; leases 1–15 of the English-only program)

- Producer: Corporate Claude (bounded producer). Acceptance gate: Codex Corp (independent). Owner: Slava — authorized every protected action in this gate explicitly (deploy, in-container DB migration, commits, push).
- Date: 2026-07-25
- Supersedes the state described in `CURRENT_STATE_INSPECT_REPORT.md` (2026-06-10) where they conflict.

## 1. VERDICT

The live demo is now **English-only end to end — UI, API payloads, Azure SQL demo data, and the LangChain sidecar** — verified on the live URL in a real signed-in browser: **0 rendered Cyrillic characters** on dashboard, claims list, customer directory, claim workspace (CLM-1006), and AI-evidence routes, and **0 Cyrillic / 0 literal `\u04` escapes** in `/api/claims`. The interview-readiness language goal of this program is met. Still demo-grade, not production-grade: API remains anonymous, outbox has no dispatcher (unchanged, out of scope for this gate).

## 2. WHAT CHANGED (2026-07-25, leases 1–15)

**Language migration (leases 1–11, local, Codex-ACCEPTED):** contract-code pattern (English codes as domain values + display-label maps; `ClaimContractCodes`, `src/utils/claimContract.ts`); frontend locale hard-locked to `en` (persisted `uk` coerced; switcher inert); RAG corpus translated with retrieval-keyword consistency; sidecar keyword logic translated (16/16 tests); six versioned, idempotent, UPDATE-only backfills `english-only-*` v1–v6 recorded in `dbo.DataBackfillHistory`; `e2e/23-english-only.spec.ts` asserts zero rendered Cyrillic with no allowlist.

**Deploy + data (lease 15, this gate):**

| Item | Before | After |
|---|---|---|
| API image / revision | `adv-ai-2259946-20260608211714` / `iap-demo-api--0000005` | `ghcr.io/slavkan777/insuranceai-api:fb65b365f1f4` / **`iap-demo-api--0000007`** (0/2 replicas preserved) |
| SPA bundle | `index-f4pJt1G8.js` (EN/UA switcher, UA labels) | **`index-DahxAhVr.js`** (English-only, live API baked in) |
| Sidecar | `insuranceai-langchain-sidecar:adv-ai-2259946…` (UA keyword logic) | `…:e1f9c568…` / **`iap-langchain-sidecar--0000002`** (English) |
| Azure SQL demo data | Ukrainian (1539 Cyrillic chars in `/api/claims` alone) | **0 Cyrillic / 0 `\u04`** — migrator ran in-container (`az containerapp exec`, secret never read); v4: 43 upd, v5: 46 upd, v6: 7 upd + 6 chunks re-embedded, 0 skipped |
| Workflow | disabled skeleton | manual GHCR build (`confirm=BUILD_IMAGE`); note: dispatchable only once the file lands on `main` |

**Product fixes shipped this gate:**
- `e1f9c56` — `HybridClaimReadService` degraded claim reads to the in-memory seed list when the DB is unreachable (was: every claim request → 500 on a no-SQL container; found by lease 15's pre-deploy probe of the exact deployed configuration). 7 regression tests.
- `fb65b36` — `english-only-demo-residue-v6` backfill for interactively-created rows (`CLM-1025` «Бампер»/«Тойота» → Bumper/Toyota; 6 uploaded-doc evidence chunks → truthful neutral text with hash/token/embedding recomputed in place). 15 tests.

**Tests:** 188 (June baseline) → **276 passed / 0 failed / 0 skipped**. Playwright: five-spec backend gate 34 passed; `22-rag-evidence` under its declared mock config 12 passed. Sidecar pytest 16/16.

## 3. EVIDENCE

- Live browser sweep (signed in as demo user, Playwright, screenshots in session scratchpad `l15/live-*.png`): dashboard 0, `/claims` 0, `/customers` 0, `/claims/CLM-1006` 0, AI-evidence 0 Cyrillic.
- Live API: `/api/claims` len 7749, 0 Cyrillic, 0 `\u04`; `/health` 200; CORS preflight/GET green for SWA origin and localhost.
- Migrator output captured live from `az containerapp exec` (v1–v5 idempotent on rerun; v6 first run 7/6/0, checkpoint recorded).
- Full lease-by-lease evidence chain: `C:\Users\DEVELOPER\Codex\CorporateClaudeBridge\InsuranceAIPlatform\LEASE1..15_CHECKPOINT.md` + `LEASE11_CODEX_AUDIT.md` (outside the repo by design; local-only paths).
- Branch `rag/local-foundation-mega-v0.1` @ **`fb65b36`**, pushed; tree clean except two untracked owner `.docx` (preserved throughout, per program rule).

## 4. BOUNDARIES HELD

Existing Azure target only (`rg-iap-demo`, 13 resources — count unchanged; no new/paid resource); UPDATE-only data migrations, previous revisions/images retained for rollback; no merge to `main`, no force-push; no secret value read, printed, or committed (SWA token env-only; migrator inherited the container's own connection string); Ukrainian intentionally retained in Git history, `docs/**` archives, and the two owner `.docx` files.

## 5. RISKS / OPEN ITEMS

1. **Anonymous write API** — unchanged; still the top product risk (previous report §8.1).
2. **Cold start** — unchanged (`minReplicas=0`): first load after idle can take ~10–20 s; warm the demo before an interview.
3. **`main` staleness** — now 44 commits ahead on the feature branch; the GHCR workflow only becomes dispatchable when its file reaches `main`.
4. Ukrainian remains, by explicit owner decision, in Git history and `docs/**` archives — do not "fix" without a new gate.

## 6. NEXT SAFE STEP

Codex Corp independent audit of lease 15 (diff `5dbbc4e..fb65b36` + this report + live URL). After acceptance: decide the `main` merge/PR strategy (also unlocks `workflow_dispatch`), then the API-protection gate recommended in the previous report.
