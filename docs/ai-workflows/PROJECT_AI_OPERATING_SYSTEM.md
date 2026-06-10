# InsuranceAIPlatform — Project AI Operating System

GATE_ID / REQUEST_ID: insuranceai-project-os-migration-2026-06-10

Status: ACTIVE PROJECT BASELINE
Project: InsuranceAIPlatform
Repository: `slavkan777/InsuranceAIPlatform`
Primary working branch: `rag/local-foundation-mega-v0.1`
Global baseline: `AI Engineering Bridge Universal V0.1.1`

## 1. Purpose

This document defines how AI-assisted work is performed inside the InsuranceAIPlatform repository.

It is the project-local operating layer under the global baseline:

`Sacred Floor > Current Gate > Project Operating Doc > Global AIKB Rule > Historical AIKB Context > Old Chat Memory`

The project repo is the home for how to work here.

The ai-kb is the home for durable project profile/state/ledger knowledge.

Do not duplicate durable project state in this repo unless the current gate explicitly explains why and names the canonical copy.

## 2. Sacred Floor

The global Sacred Floor applies here without exception.

Absolute / no-waiver:

- no AI self-acceptance;
- no exposure of secrets, credentials, private data, or PII;
- no bypassing security/privacy boundaries.

Owner-waivable per specific instance only:

- push to main/protected branch;
- destructive action with backup/rollback;
- shipping with partially verified claims when owner accepts residual risk.

If a gate conflicts with the Sacred Floor, stop and report:

`BLOCKED: gate conflicts with Sacred Floor`

## 3. Routing Lock for this repo

Every file-affecting task must state a Routing Lock.

Default project routing:

```text
ROUTING LOCK

Target repository:
`slavkan777/InsuranceAIPlatform`

Target branch:
`rag/local-foundation-mega-v0.1`

Allowed action:
inspect-only | docs-only | small-code | deployment

Allowed project AI paths:
- `docs/ai-workflows/...`
- `docs/ai-reports/...`
- `docs/ai-learnings/...`

Forbidden unless explicitly approved:
- application code changes
- Azure/deployment changes
- AI provider changes
- secret/config changes
- repo restructuring
- main/protected branch pushes
```

## 4. Prompt Handoff requirement

Every prompt handed to Claude/Codex/Fable must say:

- prompt origin;
- repository and branch to read from;
- file to read;
- repository and branch to write to;
- expected report path;
- whether commit/push is allowed;
- required final status output.

If the prompt is pasted manually:

`Prompt origin: pasted by owner, not stored in Git.`

If stored in Git:

`Prompt origin: Git file: owner/repo:path@branch.`

Claude/Codex/Fable must not infer report destinations from relative paths alone.

## 5. Task modes

### Inspect-only

Allowed:

- read files;
- analyze architecture/state;
- produce report.

Forbidden:

- file edits;
- commits;
- deployment.

### Docs-only

Allowed:

- project operating docs;
- reports;
- learning log updates;
- AI workflow/request files.

Forbidden:

- application code changes;
- runtime config changes;
- Azure/deployment changes.

### Small-code

Allowed only with explicit current gate.

Required:

- narrow scope;
- file list;
- test/smoke command;
- evidence report;
- rollback note.

Forbidden:

- broad refactor;
- unrelated cleanup;
- provider/deployment changes unless explicitly approved.

### Deployment

Allowed only with explicit deployment gate.

Required:

- precheck;
- target resource/revision;
- smoke result;
- rollback target;
- report;
- audit if high impact.

## 6. Project report format

Producing reports should use:

```text
GATE_ID / REQUEST_ID
CURRENT STATE
GATE
ROUTING LOCK
WHAT CHANGED
FILES CHANGED
TESTS / SMOKE
EVIDENCE
RISKS
BOUNDARIES CHECK
ROLLBACK
NEXT SAFE STEP
```

Audit reports should use:

```text
GATE_ID / REQUEST_ID
VERIFIED CLAIMS
NOT VERIFIED CLAIMS
OVERSTATED CLAIMS
BOUNDARY VIOLATIONS
RISKS
CORRECTED SUMMARY
FINAL CONFIDENCE: HIGH / MEDIUM / LOW
NEXT SAFE STEP
```

LOW confidence means no acceptance; owner chooses redo, descope, or reject.

## 7. Evidence defaults

Default rule:

`No evidence -> NOT VERIFIED`

Evidence examples:

- code claim: file path, diff, test command, exit code, commit hash;
- architecture claim: file/module/config path, ADR/doc/diagram;
- AI/RAG claim: provider mode, RAG/citation path, claim-scoping test, fallback behavior, eval/smoke;
- deployment claim: resource/revision, URL, smoke result, rollback target;
- security claim: auth/config/policy path, secret handling, negative test.

Never overclaim production, AI, security, compliance, or user/business traction without evidence.

## 8. Project-local homes

Project-local AI files:

- `docs/ai-workflows/...` = project gates, request files, operating docs;
- `docs/ai-reports/...` = durable project reports/audits;
- `docs/ai-learnings/LOG.md` = project-local learning log.

Global/cross-project rules belong in ai-kb, not here.

## 9. Recurse/debug rule

Failure loop:

`ATTEMPT -> TEST -> FAILURE LOG -> DIFFERENT APPROACH -> RETEST`

Rules:

- do not repeat failed fixes;
- explain why the new attempt differs;
- default limit is 3 attempts if gate gives no limit;
- then stop and report.

## 10. Current migration status

This file is the project-local operating baseline created during the first pilot under the accepted global bridge baseline.

Mode:

`docs-only migration gate`

Created together with:

`docs/ai-learnings/LOG.md`

Forbidden in this migration gate:

- application code;
- Azure;
- AI provider changes;
- repo restructuring;
- deployment.

## 11. Next safe step

Run one small real gate under this project operating system.

Recommended first real gate type:

`inspect-only` or `docs-only`
