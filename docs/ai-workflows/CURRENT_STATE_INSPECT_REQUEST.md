# Current State Inspect Request

GATE_ID / REQUEST_ID: insuranceai-current-state-inspect-2026-06-10

## PROMPT HANDOFF

Prompt origin:

Git file: `slavkan777/InsuranceAIPlatform:docs/ai-workflows/CURRENT_STATE_INSPECT_REQUEST.md@rag/local-foundation-mega-v0.1`

Claude must read:

Repository: `slavkan777/InsuranceAIPlatform`
Branch: `rag/local-foundation-mega-v0.1`

Required files:

- `docs/ai-workflows/PROJECT_AI_OPERATING_SYSTEM.md`
- `docs/ai-workflows/PROJECT_AI_OPERATING_SYSTEM_AUDIT_FIXES.md`
- `docs/ai-workflows/PROJECT_AI_OPERATING_SYSTEM_ACCEPTANCE_RECORD.md`
- `docs/ai-learnings/LOG.md`

Claude may inspect other repository files as needed, read-only, to determine the current project state.

Claude must compare against:

Repository: `slavkan777/ai-kb`
Branch: default branch
Files:

- `00_GLOBAL_AI_ENGINEERING_OS/AI_ENGINEERING_BRIDGE_UNIVERSAL_V0.1.md`
- `00_GLOBAL_AI_ENGINEERING_OS/AI_BRIDGE_ROUTING_LOCK_RULE.md`

Claude must write report to:

Repository: `slavkan777/InsuranceAIPlatform`
Branch: `rag/local-foundation-mega-v0.1`
File: `docs/ai-reports/CURRENT_STATE_INSPECT_REPORT.md`

Commit/push:

Allowed for the report file only.

Expected final output from Claude:

- current branch
- files read
- report file written
- commit hash
- push status

## ROUTING LOCK

Target repository:

`slavkan777/InsuranceAIPlatform`

Target branch:

`rag/local-foundation-mega-v0.1`

Allowed action:

inspect-only + one docs-only report file

Allowed target file:

`docs/ai-reports/CURRENT_STATE_INSPECT_REPORT.md`

Forbidden actions:

- Do not modify application code.
- Do not modify runtime configuration.
- Do not modify deployment/Azure files.
- Do not modify AI provider code/config.
- Do not modify existing operating docs or learning log.
- Do not create files other than the target report.
- Do not change unrelated files.

## Task

Perform the first real inspect-only gate under the accepted project operating baseline.

Goal:

Determine the current real state of InsuranceAIPlatform without changing code.

Inspect:

- project structure;
- solution/project files;
- existing docs and reports;
- current branch context if available;
- application modules/components visible in the repo;
- test/smoke scripts if present;
- AI/RAG-related implementation if present;
- known boundaries from project operating docs and addendum;
- recent reports and learnings relevant to current state.

Do not invent state. If something is not verified from repository evidence, mark it as `NOT VERIFIED`.

## Required report format

Create:

`docs/ai-reports/CURRENT_STATE_INSPECT_REPORT.md`

Report sections:

1. VERDICT
2. CURRENT STATE
3. CURRENT GATE
4. ROUTING LOCK VERIFIED
5. WHAT EXISTS
6. WHAT IS MISSING / NOT VERIFIED
7. PROJECT BOUNDARIES
8. RISKS
9. EVIDENCE
10. RECOMMENDED NEXT SAFE STEP

## Specific questions to answer

1. What is the repository's current technical shape?
2. What project operating files are now active?
3. What app/runtime areas appear to exist?
4. What AI/RAG/LLM-related parts appear to exist, if any?
5. What tests or smoke paths are available, if any?
6. What must not be claimed yet?
7. What should the next real gate be?

## Output rules

- Use evidence-first language.
- Use `VERIFIED`, `PARTIALLY VERIFIED`, or `NOT VERIFIED` where relevant.
- Do not claim production readiness unless there is repository evidence.
- Do not claim real users, real traffic, real managed LLM quality, compliance readiness, calibrated confidence, or automated CI/CD unless directly evidenced.
- Keep the report practical and concise.

After writing the report:

- commit it
- push it
- show current branch
- show files read
- show report file written
- show commit hash
- show push status

Commit message:

`docs: add current state inspect report`
