# InsuranceAIPlatform — Project AI Operating System Migration Audit

GATE_ID / REQUEST_ID: insuranceai-project-os-migration-audit-2026-06-10

- Prompt origin: pasted by owner, not stored in Git
- Audited artifacts: `docs/ai-workflows/PROJECT_AI_OPERATING_SYSTEM.md` (262 lines) + `docs/ai-learnings/LOG.md` (140 lines), migration commits `3ad2e4c` + `423bd04`
- Compared against: ai-kb `00_GLOBAL_AI_ENGINEERING_OS/AI_ENGINEERING_BRIDGE_UNIVERSAL_V0.1.md` (V0.1.1 @ `e12e90a`) and `AI_BRIDGE_ROUTING_LOCK_RULE.md` (227 lines, ACTIVE)
- Audit type: independent docs-only audit (auditing seat; the migration artifacts were produced by the coordinator seat, not by this auditor)
- Date: 2026-06-10

## 1. VERDICT

**APPROVE WITH REQUIRED FIXES — no blockers.** The migration correctly instantiates the global V0.1.1 baseline for this project (precedence, three-bucket floor, routing lock, prompt handoff, four modes, formats with GATE_ID, single-home statements) and stayed strictly docs-only (verified by commit diff). Three small required fixes remain — an incomplete Sacred Floor copy, missing fields in the default routing template (ironically including the *local workspace* field, whose absence is the log's own first lesson), and missing project-specific data boundaries. All are ≤5-line edits for a follow-up docs-only gate.

## 2. VERIFIED STRENGTHS

- **Docs-only migration verified** (Q8): `git diff --name-only 0403aa7..423bd04` = exactly `docs/ai-workflows/PROJECT_AI_OPERATING_SYSTEM.md` + `docs/ai-learnings/LOG.md`. No code, Azure, provider, or config files touched.
- **Correct inheritance** (Q1): global baseline pinned by name+version (operating doc line 9), full precedence chain restated verbatim (line 17), Sacred Floor absolute/owner-waivable buckets copied faithfully (lines 29–39), BLOCKED protocol (line 43), report/audit formats include `GATE_ID / REQUEST_ID` and the LOW-confidence consequence (lines 166, 183, 194), recurse rule includes the default 3-attempt limit (line 232).
- **Prompt Handoff present and usable** (Q3): all seven required fields (lines 79–87), both origin conventions (lines 89–95), plus the strong "must not infer report destinations from relative paths alone" rule (line 97) — matching the routing rule's prompt-placement protocol.
- **Project-local homes correct** (Q4): `docs/ai-workflows|ai-reports|ai-learnings` split (lines 214–218) matches the routing rule's typical paths exactly; "Global/cross-project rules belong in ai-kb, not here" (line 220); repo=how-to-work / ai-kb=durable-state split with a no-duplication rule (lines 19–23).
- **Learning log is real and properly guarded** (Q6): the three entries describe genuine incidents from recent work (workspace/report-destination ambiguity — observed twice during the review-report cycle; overclaim risk — mirrors the architecture review's must-not-claim list). Entry evidence citations check out: `AI_ENGINEERING_BRIDGE_UNIVERSAL_V0.1_ACCEPTANCE_RECORD.md` **exists** on ai-kb origin/main (verified via ls-tree). The log carries the anti-state-ledger guard (line 20) and the promotion rule (lines 14–18).
- **This gate itself validated the format**: the PROMPT HANDOFF + ROUTING LOCK blocks used to commission this audit follow the new templates and were sufficient to execute without inference.

## 3. BLOCKERS

None. (The items below are required but small, and none weakens the floor normatively — the Sacred Floor binds at rank 1 from the global doc regardless of the project copy's completeness.)

## 4. DRIFT RISKS

1. **Rule-copy drift (main risk):** the operating doc copies global content nearly verbatim — floor buckets (§2), handoff fields (§4), task modes (§5), both report formats (§6), evidence examples (§7), recurse rule (§9). The version pin (line 9) makes the copies traceable, but every global bump (V0.2+) now requires manual re-sync of ~6 sections. The single-home rule formally covers *durable state*, not rule text, so this is permitted — but it is the doc's largest maintenance liability. Mitigation (optional now, recommended for V0.2): trim copied sections to one-line links + project deltas, or add an explicit re-sync rule ("on global version bump, re-sync §2/§4–§7/§9 or re-pin").
2. **Retroactive gate IDs in the log:** entries 2–3 cite gate IDs (`bridge-report-routing-cleanup-2026-06-10`, `insuranceai-overclaim-control-2026-06-10`) that did not run as separate gates — the lessons are real but were consolidated retrospectively. Harmless, but future entries should either reference the actual gate that produced the lesson or be marked `retrospective`.
3. **No state duplicated** (Q7 core check): neither file carries project profile/current-state content — the durable-state single-home rule is respected today. ✅

## 5. OVERENGINEERING

Minimal — the migration correctly resisted extra files (2 files + the pre-existing `docs/ai-reports/` only). The only excess is the rule-copy volume noted above (§4.1): the operating doc is 262 lines where ~150 (project deltas + links) would carry the same force with less drift surface.

## 6. REQUIRED FIXES

1. **Complete the Sacred Floor copy** (operating doc §2): the global floor has **three** buckets; the project copy lists two and omits `Default blocked state` (unverified production/AI/security/compliance claims — global line 80) while §2 claims the floor "applies here without exception". §7 line 210 covers the substance, but the floor section itself must either include all three buckets or replace the copies with "see global §2" — a misleadingly partial floor copy in the highest-traffic doc is the worst place for an omission.
2. **Complete the default Routing Lock template** (§3): against the routing rule's required template (rule lines 153–184) it is missing `Local workspace` (e.g., `C:/Projects/InsuranceAIPlatform`), `Forbidden repositories` (default: ai-kb, gpt-handoff, any TwinCore repo), and the `Stop condition` line. The omission of *local workspace* is exactly the failure mode the log's first lesson documents — the template should embody its own lesson.
3. **Add project-specific data/provider boundaries** (§3 or §10): the boundaries every real gate on this project repeats are absent from the operating doc — do not mutate seeded claims `CLM-1006/1007/1012`; synthetic data only (`E2E-*` prefixes, no real PII); no paid LLM providers / no new Azure AI resources without an explicit gate. These belong in the project doc precisely so future gates can reference instead of restate them.

## 7. OPTIONAL IMPROVEMENTS

- Seed the log with 1–2 **technical** lessons from recent real gates (current entries are all process-level): e.g., the live demo is auth-gated so UI smoke must log in first (localStorage rehydration allows deep-link after login), and the registry env-token vs credential-helper mismatch discovered during the sidecar deployment. Both already have evidence in `docs/ai-reports/` and the deployed-gate reports.
- Name the protected branches explicitly (`main`, `dev`) in §3 instead of the generic "main/protected branch".
- Add the re-sync-on-version-bump rule or trim rule-copies to links (see Drift Risk 1).
- Mark consolidated log entries as `retrospective` (see Drift Risk 2).

## 8. OWNER ACCEPTANCE READINESS

**READY WITH REQUIRED FIXES.** Nothing blocks acceptance of the migration as the project baseline; the three required fixes are fidelity/completeness edits (≤5 lines each) suitable for one follow-up docs-only gate — acceptance now with a fix gate scheduled, or one combined fix-then-accept cycle, are both safe. FINAL CONFIDENCE: **HIGH** — every checked claim verified against file+line evidence and commit diffs; both compared baselines read in full; no boundary violations found (this gate touched only the allowed report file).

## 9. NEXT SAFE STEP

Owner accepts the migration baseline; one docs-only fix gate applies the three required fixes (floor third bucket, routing-template fields, project data boundaries) plus optional technical log seeds; then run the first small **real** gate (inspect-only or docs-only) under the operating system — exactly as the operating doc's own §11 plans.
