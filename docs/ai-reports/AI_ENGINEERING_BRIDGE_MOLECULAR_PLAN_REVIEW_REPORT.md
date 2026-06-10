# AI Engineering Bridge (Universal V0.1) — Molecular Plan Review Report

- Reviewed document: `docs/ai-workflows/AI_ENGINEERING_BRIDGE_MOLECULAR_PLAN_REVIEW_REQUEST.md`
- Review type: read-only molecular architectural evaluation (no code, no implementation, no workflow changes)
- Reviewer: external architecture reviewer
- Date: 2026-06-10

## 1. VERDICT

Approvable direction; trim before committing. This version passes the key test the request itself sets: it **strengthens the practiced bridge instead of building a second system** — the workflow, the seats, the evidence discipline, and the audit format all match what ~10 real gates on this repository already do. Two structural defects must be fixed before the first commit: the **precedence rule has no non-overridable safety floor**, and **project context/state gets two homes** (repo + ai-kb), which guarantees drift. With those fixed and the surface cut by roughly one third, commit it.

## 2. WHAT IS STRUCTURALLY RIGHT

- **Three seats** match the real topology; the audit categories (`VERIFIED / NOT VERIFIED / OVERSTATED / BOUNDARY VIOLATIONS`) mirror how audits actually behave — `OVERSTATED CLAIMS` is the single most valuable category in practice.
- **Evidence matrix by claim type** (code / architecture / AI-RAG / Azure / security) with the `NOT VERIFIED` default codifies exactly what made past reports auditable: provider mode, rollback target, smoke result, and claim-scoping test are the right exemplars.
- **Precedence chain exists** and correctly ranks `Old Chat Memory` last.
- **Exemption clause with both lists** (exempt vs required) — and putting *CV/interview claims* and *AI-capability claims* on the "required" side is exactly right; those are where overclaiming happens.
- **Explicitly postponed** section (fleet, dashboard, daemon, six roles, model-specific prompts) — deferral is stated, not implied.
- **Learning log: one file, five-line entry format, evidence field** — minimal and durable.
- "**No coding before BOOTSTRAP_GATE is accepted**" — a cheap, high-leverage guard.

## 3. WHAT IS STILL OVERENGINEERED

- **Eighteen sections in the global doc.** A V0.1 constitution with 18 headings becomes a 1500-line document nobody re-reads — the doc must obey the leanness it preaches. Target ≤ ~300 lines and ~8 sections (merge purpose/scope/non-goals; merge drift-prevention into the precedence section; see section 7).
- **"Maturity levels"** — process-framework ceremony for a one-owner, two-AI-seat operation. Delete; "postponed infrastructure" already covers growth.
- **Seven task modes.** Four are real (inspect-only, docs-only, small-code, deployment) because each has distinct *boundaries*. "Large-feature-planning" is inspect+docs; "CV/market positioning" and "architecture review" are *content types* of docs-only/inspect-only, not modes. Every named mode invites per-mode rules and therefore ceremony. Keep four; list the rest as examples.
- **Five-file new-project bootstrap.** `PROJECT_CONTEXT.md` + `CURRENT_STATE.md` + the operating doc overlap ~80% at day 0. Worse: ai-kb already maintains `PROJECT_PROFILE.md` / `CURRENT_STATE.md` / `TASK_LEDGER.md` per project — in-repo copies create a **second home for the same facts**.
- **Mandatory two-gate migration ritual per project** (×4 projects = 8 ceremony gates). Migrate lazily — see section 8.

## 4. WHAT IS MISSING

1. **A non-overridable floor under the precedence rule.** As written, `Current Gate > everything` means a gate could authorize printing a secret, self-acceptance, or PII exposure. Add: *sacred rules (no self-acceptance, secret handling, PII, no-main-push, backup-before-destructive) outrank even the gate; a gate demanding their violation → STOP + report BLOCKED.* This is the most important fix in the whole plan.
2. **Change control for the global doc itself** — V0.1 → V0.2 must pass through its own docs-only gate plus audit in ai-kb; otherwise the constitution mutates silently.
3. **Canonical home for reports** — the handoff-bridge paths *and* repo `docs/ai-reports/` both exist today. Define: bridge = transport + audit channel (latest + runs, pruned); repo = durable per-project archive (optional, per gate). This ambiguity is live drift — it already occurred during recent gates.
4. **Audit independence stated explicitly** — the producing seat never audits its own gate. It is the system's core safety property; one sentence, currently absent.
5. **Gate traceability convention** — a REQUEST_ID linking gate → report → commit hash → learning entry (already practiced, not written down).
6. **Connector-failure fallback** — the bridge depends on the reviewer seat's write access, which has already failed once (a safety-filter block); write down the practiced fallback: *the owner pasting the gate in chat is a valid gate origin*.
7. **Personal-vault scope** — the personal knowledge vault is listed as a target, but it is governed by its own memory/configuration rules. Mark it exempt (own-rules), not bridge-governed.

## 5. WHAT SHOULD BE REMOVED

- Maturity levels.
- CV/positioning and architecture-review as named task modes (fold into docs-only/inspect-only).
- `PROJECT_CONTEXT.md` + `CURRENT_STATE.md` from the in-repo bootstrap set — one home per fact: **repo = how to work here** (operating doc, learnings); **ai-kb = what state it is in** (profile, current state, ledger).
- "Drift prevention" as a standalone section (it *is* the precedence rule plus the single-home rule).
- The fixed two-gate migration ritual.

## 6. RISKS

- **Gate-overrides-safety** (until the floor exists) — highest severity.
- **Dual-home drift** between repo and ai-kb for context/state — highest likelihood.
- **Constitution bloat** — gates start restating everything, and the global doc becomes a dead letter.
- **Confidence-score numerology**: `FINAL CONFIDENCE SCORE 1-10` without anchors is noise. Define three anchor points (e.g., 9 = all critical claims independently verified; 5 = core verified, periphery not; 2 = report contradicted by evidence) or drop the number and keep verdict categories.
- **Exemption creep** ("simple question" stretching). Add the practiced tiebreaker: *if the output reaches an external observer or triggers an irreversible action, the bridge applies regardless of task size.*

## 7. BETTER FIRST COMMIT CONTENT

Same repository, path, and commit message as proposed; ~250–300 lines, eight sections:

1. Purpose, scope, non-goals (one section)
2. Three seats + **sacred floor** (including no-self-acceptance)
3. Precedence rule (floor on top, old-chat-memory last)
4. Gate workflow + four task modes + exemption clause with the external-observer tiebreaker
5. Evidence matrix + `NOT VERIFIED` default
6. Report format + audit format (+ confidence anchors, or no number)
7. Recurse/debug loop + learning-log entry format
8. Migration (lazy), bootstrap (two files + one directory), report homes, global-doc change control, postponed list

Close with a **link to one real worked gate** (the recent sidecar deployment-and-smoke gate) as the canonical example — a pointer to reality beats 500 lines of abstract prose.

## 8. OLD PROJECT MIGRATION STRATEGY

Lazy and value-driven, not a campaign: migrate a project **when its next real task arrives** — that gate doubles as migration validation. Concretely:

- **InsuranceAIPlatform** — the only real migration: one docs-only gate creating the operating doc plus `docs/ai-learnings/LOG.md` seeded with 2–3 genuine recent learnings (the auth-gated-UI smoke lesson; the container-registry credential lesson).
- **Handoff repo** — transport infrastructure; needs only a README stating canonical paths, not bridge governance.
- **ai-kb** — the global doc's home; its migration *is* the first commit.
- **Personal vault** — exempt (own rules).

Net: one migration gate, one README, one commit, one exemption — instead of eight ritual gates.

## 9. NEW PROJECT BOOTSTRAP STRATEGY

Two files + one directory: `docs/ai-workflows/PROJECT_AI_OPERATING_SYSTEM.md` (with *Context* and *Current State* as living sections inside it), `docs/ai-learnings/LOG.md`, and `docs/ai-reports/`. The BOOTSTRAP_GATE fills the operating doc (branch policy, forbidden zones, test/smoke commands), creates the repo skeleton, and is owner-accepted before any code — keep that rule verbatim. The auditing seat creates the ai-kb `PROJECT_PROFILE.md` in the same gate, so each fact gets exactly one home. Split `CURRENT_STATE` into its own file only when it starts churning at a different rate than the rules.

## 10. FINAL MINIMUM VIABLE VERSION

Three files: the trimmed global doc in ai-kb (section 7) + InsuranceAIPlatform's operating doc + its seeded `LOG.md`. Plus two sentences of infrastructure clarification (handoff-repo README note; vault exemption). Nothing else until a real gate demands it.

## 11. NEXT SAFE STEP

Commit the section-7-shaped global doc to ai-kb through **its own docs-only gate with audit** (proving the change-control rule by using it), referencing the worked example; then run the single InsuranceAIPlatform migration gate (operating doc + seeded learnings log). Both steps are reversible, contain no code, and add no new process surface.
