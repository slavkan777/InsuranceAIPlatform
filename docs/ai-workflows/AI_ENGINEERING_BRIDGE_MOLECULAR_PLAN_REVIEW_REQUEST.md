# AI Engineering Bridge — Molecular Plan Review Request

Purpose: ask an external AI reviewer for a detailed read-only architectural evaluation of the proposed Universal AI Engineering Bridge plan.

This request is intentionally review-only. It must not create or modify implementation files.

## Context

We do not want to build a new overengineered agent factory. We want to strengthen the AI-assisted bridge process that already exists in practice across our projects.

Current practiced bridge process:

`REQUEST -> GATE -> PLAN -> EXECUTION -> EVIDENCE -> AUDIT -> OWNER ACCEPTANCE -> LEARNING`

Target scope:

- InsuranceAIPlatform
- ai-kb
- gpt-handoff
- claude-vault
- future projects

## Core idea

One global AIKB document defines universal rules.
Each project has a small project-level operating document.
Each task starts with a current gate.

## Proposed global file

Repository: `slavkan777/ai-kb`

File:

`00_GLOBAL_AI_ENGINEERING_OS/AI_ENGINEERING_BRIDGE_UNIVERSAL_V0.1.md`

The file should define:

- purpose
- scope
- non-goals
- three-seat model
- precedence rule
- gate workflow
- task modes
- recurse/debug workflow
- evidence matrix
- report format
- audit format
- learning log format
- exemption clause
- existing project migration
- new project bootstrap
- drift prevention
- maturity levels
- postponed infrastructure

## Three-seat model

### Producing Seat

Understands, plans, executes, tests, collects evidence, and writes the report.

### Auditing Seat

Verifies claims, evidence, diff, tests, boundaries, risks, security/privacy, and overclaims.

### Owner Acceptance Seat

Slava/Alexey approve, reject, stop, or define the next gate.

## Precedence rule

`Current Gate > Project Operating Doc > Global AIKB Rule > Historical AIKB Context > Old Chat Memory`

## Universal Gate workflow

`GATE -> PLAN -> EXECUTE -> TEST/SMOKE -> REPORT -> AUDIT -> ACCEPT -> LEARN`

## Task modes inside one Gate workflow

- inspect-only
- docs-only
- small-code
- large-feature-planning
- deployment
- CV/market positioning
- architecture review

## Recurse/debug workflow

`ATTEMPT -> TEST -> FAILURE LOG -> DIFFERENT APPROACH -> RETEST`

Rule: do not repeat failed fixes.

## Evidence matrix

Every important claim must have evidence.

Examples:

- code claim: file path, diff, test command, exit code, commit hash
- architecture claim: file path, module path, config path, ADR/diagram
- AI/RAG claim: provider mode, RAG path, citation path, claim-scoping test, fallback/eval result
- Azure/deploy claim: resource name, revision, URL, smoke result, rollback target
- security claim: auth/config/policy/secret handling/negative test

If no evidence: `NOT VERIFIED`.

## Report format

- CURRENT STATE
- GATE
- WHAT CHANGED
- FILES CHANGED
- TESTS / SMOKE
- EVIDENCE
- RISKS
- BOUNDARIES CHECK
- ROLLBACK
- NEXT SAFE STEP

## Audit format

- VERIFIED CLAIMS
- NOT VERIFIED CLAIMS
- OVERSTATED CLAIMS
- BOUNDARY VIOLATIONS
- RISKS
- CORRECTED SUMMARY
- FINAL CONFIDENCE SCORE 1-10

## Learning log

Use one project file first:

`docs/ai-learnings/LOG.md`

Entry format:

- Symptom
- Root cause
- Fix / decision
- Rule for next time
- Evidence

## Exemption clause

Full bridge is not needed for:

- translation
- short text
- simple questions
- screenshot interpretation
- rough brainstorming
- one-off wording

Full bridge is required for:

- code
- deploy
- security/privacy
- architecture decisions
- production-readiness claims
- AI/RAG/LLM capability claims
- CV/interview claims
- new project bootstrap

## Existing project migration

For each existing project:

- discover repo purpose
- discover active branch
- discover main/protected branch
- discover current state/handoff docs
- discover forbidden zones
- discover tests/smoke commands
- create `docs/ai-workflows/PROJECT_AI_OPERATING_SYSTEM.md`
- create `docs/ai-learnings/LOG.md`
- run one docs-only gate
- run one real small gate

## New project bootstrap

Every new project starts with:

- `PROJECT_CONTEXT.md`
- `CURRENT_STATE.md`
- `docs/ai-workflows/PROJECT_AI_OPERATING_SYSTEM.md`
- `docs/ai-learnings/LOG.md`
- `docs/ai-reports/`

Rule: no coding before `BOOTSTRAP_GATE` is accepted.

## Explicitly postponed

- six separate agent roles
- four workflow files
- model-specific prompt standards
- Cloudflare fleet
- dashboard
- daemon
- tmux wrapper
- credential vault
- notification system
- mass rollout to all repos

## Proposed first commit

Repository:

`slavkan777/ai-kb`

File:

`00_GLOBAL_AI_ENGINEERING_OS/AI_ENGINEERING_BRIDGE_UNIVERSAL_V0.1.md`

Commit message:

`docs: add universal AI engineering bridge operating model`

## Review rules

- Review only.
- No file changes.
- No commits.
- No branches.
- No implementation.
- Be critical and practical.
- Check whether this strengthens the existing bridge process instead of creating a second system.

## Required output format

1. VERDICT
2. WHAT IS STRUCTURALLY RIGHT
3. WHAT IS STILL OVERENGINEERED
4. WHAT IS MISSING
5. WHAT SHOULD BE REMOVED
6. RISKS
7. BETTER FIRST COMMIT CONTENT
8. OLD PROJECT MIGRATION STRATEGY
9. NEW PROJECT BOOTSTRAP STRATEGY
10. FINAL MINIMUM VIABLE VERSION
11. NEXT SAFE STEP
