# AI Agent Factory Plan — Architecture Review Report

- Reviewed document: `docs/ai-workflows/AI_AGENT_FACTORY_PLAN_REVIEW_REQUEST.md`
- Review type: read-only architectural opinion (no code, no implementation, no workflow changes)
- Reviewer: external architecture reviewer
- Date: 2026-06-08

## VERDICT

Sound direction, wrong starting point. The plan describes a process this team **already runs in practice**: the per-project handoff bridge (bounded gate request → routing lock → bounded execution → evidence-rich report → independent audit → owner acceptance) has been exercised over ~10 real gates on this repository. The risk is not the ideas — it is writing an *idealized second system* next to the *practiced* one. Approve V0.1 only as a **distillation of the existing bridge process**, at roughly half the proposed surface.

## WHAT IS GOOD

- **Review gates + evidence verification** — already battle-tested here: routing lock, DONE STATE, FORBIDDEN list, smoke matrix, per-item evidence (file/line, command + exit code, counts), recorded rollback target. Codifying them is pure win.
- **Recurse/debug loop with a failure log** — the one genuinely new artifact. Failures currently live in chat transcripts and session notes; a durable per-project `ai-learnings/` log makes them searchable and reusable.
- **Produce / audit / accept separation** — three distinct seats (producing agent builds and tests; auditing reviewer verifies evidence; owner accepts) is the load-bearing safety property of the current process. Making it explicit and universal is right.
- **Project-level AI operating doc** — solves a real problem: today project rules live in gate prompts and conversation context, invisible to a fresh session or a different agent.

## WHAT IS OVERENGINEERED

- **Six roles.** The real topology is **three seats**. Builder / Tester / Evidence Verifier are one agent in one session — the tester *is* the builder collecting evidence, and a separate "Evidence Verifier" duplicates the Reviewer, whose entire job is verifying evidence. Model Architect → Builder → Tester as *phases* of the producing seat, not as agents with handoffs nobody actually performs.
- **Four workflows.** FEATURE_WORKFLOW vs TASK_WORKFLOW is the same skeleton at two sizes — one parametrized GATE workflow covers both. REVIEW_WORKFLOW already exists as the audit step of every gate. Two workflows suffice: **GATE** and **RECURSE_DEBUG**.
- **Five pilot documents.** A separate `ai-agents/README.md` plus two standalone prompt files is documentation sprawl on day one. Prompts belong *inside* the operating doc until they stabilize. A prompt file named after a specific AI model version bakes a vendor snapshot into the process — keep committed prompts model-agnostic.

## MISSING PIECES

1. **Migration story** — the plan never mentions the existing bridge process; the factory must absorb it, not coexist with it (two operating models guarantee drift).
2. **Precedence rule** — gate request vs project doc vs global knowledge-base doc: which wins on conflict. Current practice is gate > project > global; write it down.
3. **Exemption clause** — when *not* to use the factory (trivial or inspect-only work). Uniformly applied ceremony is how process adoption dies.
4. **Enforcement seam** — documents do not execute themselves; mark which parts are enforced by templates/hooks (gate template, report format) and which are convention only.
5. **Failure-log entry format** — without a five-line template (symptom / root cause / fix / resulting rule), `ai-learnings/` becomes a junk drawer.

## BETTER IMPLEMENTATION ORDER

1. Extract the **existing** gate format, report format, and three-seat model from the last 2–3 real gates (the recent sidecar deployment-and-smoke gate is a complete worked example) into the global factory doc.
2. Add the RECURSE_DEBUG loop and the failure-log entry template to the same doc.
3. Pilot on this repository with **one** `PROJECT_AI_OPERATING_SYSTEM.md` (pointer to the global doc + project specifics: branch policy, forbidden targets, smoke conventions) plus `ai-learnings/LOG.md`.
4. Run 2–3 real gates under it; fold lessons back into the doc.
5. Only then template the minimal structure for other projects.

## MINIMUM VIABLE VERSION

Three files total.

- Global: one factory doc — three seats, one gate template, the recurse/debug loop, the failure-log format, the precedence rule, and the exemption clause.
- Per project: one operating doc + one learnings log.

Everything else is deferred until a real gate demands it.

## RISKS

- **Two sources of truth** — a global knowledge-base copy and repo docs drift apart; the executing agent reads the repo and the gate, the reviewer reads the knowledge base. Mitigate with the precedence rule and by linking instead of copying.
- **Process theater** — roles and workflows nobody instantiates rot quickly and erode trust in the documents that do matter.
- **Ceremony tax** — without the exemption clause, the factory taxes exactly the small fast iterations where this setup performs best.
- **Stale committed prompts** — model-specific prompts age badly; review them like code, or keep them out of the repo.

## NEXT SAFE STEP

Draft the global factory doc as a **descriptive** write-up of the bridge process as actually practiced (using the recent deployment-and-smoke gate as the worked example), pass it through its own docs-only review gate, and defer all per-project rollout until it survives that audit. No code, no new roles, fully reversible.
