# Project AI Operating System Audit Fixes — Diff-Check Request

GATE_ID / REQUEST_ID: insuranceai-project-os-audit-fixes-diff-check-2026-06-10

## PROMPT HANDOFF

Prompt origin:

Git file: `slavkan777/InsuranceAIPlatform:docs/ai-workflows/PROJECT_AI_OPERATING_SYSTEM_AUDIT_FIXES_DIFF_CHECK_REQUEST.md@rag/local-foundation-mega-v0.1`

Claude must read:

Repository: `slavkan777/InsuranceAIPlatform`
Branch: `rag/local-foundation-mega-v0.1`
Files:

- `docs/ai-workflows/PROJECT_AI_OPERATING_SYSTEM.md`
- `docs/ai-workflows/PROJECT_AI_OPERATING_SYSTEM_AUDIT_FIXES.md`
- `docs/ai-reports/PROJECT_AI_OPERATING_SYSTEM_MIGRATION_AUDIT_REPORT.md`

Claude must compare against:

Repository: `slavkan777/ai-kb`
Branch: default branch
Files:

- `00_GLOBAL_AI_ENGINEERING_OS/AI_ENGINEERING_BRIDGE_UNIVERSAL_V0.1.md`
- `00_GLOBAL_AI_ENGINEERING_OS/AI_BRIDGE_ROUTING_LOCK_RULE.md`

Claude must write report to:

Repository: `slavkan777/InsuranceAIPlatform`
Branch: `rag/local-foundation-mega-v0.1`
File: `docs/ai-reports/PROJECT_AI_OPERATING_SYSTEM_AUDIT_FIXES_DIFF_CHECK_REPORT.md`

Commit/push:

Allowed

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

docs-only diff-check report

Allowed target file:

`docs/ai-reports/PROJECT_AI_OPERATING_SYSTEM_AUDIT_FIXES_DIFF_CHECK_REPORT.md`

Allowed read files:

- `docs/ai-workflows/PROJECT_AI_OPERATING_SYSTEM.md`
- `docs/ai-workflows/PROJECT_AI_OPERATING_SYSTEM_AUDIT_FIXES.md`
- `docs/ai-reports/PROJECT_AI_OPERATING_SYSTEM_MIGRATION_AUDIT_REPORT.md`

Forbidden actions:

- Do not modify application code.
- Do not modify Azure/deployment files.
- Do not modify AI provider code/config.
- Do not modify existing project operating docs.
- Do not modify existing learning log.
- Do not create extra files beyond the target report.
- Do not change unrelated files.

## Task

Perform a narrow diff-check only.

Check whether the addendum file resolves the required fixes from the migration audit:

1. Sacred Floor copy issue: third bucket / default blocked state is covered.
2. Routing Lock template issue: local workspace, forbidden repositories, and stop condition are covered.
3. Project-specific data/provider boundaries are covered.
4. Addendum does not contradict the main project operating document.
5. Addendum is acceptable as an active fix layer until folded into the main project operating document.
6. Migration remains docs-only.

Do not perform a full second audit.
Do not suggest large redesign unless a required fix is still missing.

Create report:

`docs/ai-reports/PROJECT_AI_OPERATING_SYSTEM_AUDIT_FIXES_DIFF_CHECK_REPORT.md`

## Output format

1. VERDICT
2. FIXES VERIFIED
3. FIXES NOT VERIFIED
4. REMAINING BLOCKERS
5. DRIFT / ADDENDUM RISKS
6. OWNER ACCEPTANCE READINESS
7. NEXT SAFE STEP

After writing the report:

- commit it
- push it
- show current branch
- show files read
- show report file written
- show commit hash
- show push status

Commit message:

`docs: add diff-check for project AI operating system audit fixes`
