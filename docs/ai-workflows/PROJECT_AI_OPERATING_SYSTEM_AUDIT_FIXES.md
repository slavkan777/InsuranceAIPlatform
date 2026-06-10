# InsuranceAIPlatform — Project AI Operating System Audit Fixes Addendum

GATE_ID / REQUEST_ID: insuranceai-project-os-audit-fixes-2026-06-10

Status: ACTIVE ADDENDUM
Applies to: `docs/ai-workflows/PROJECT_AI_OPERATING_SYSTEM.md`
Audit source: `docs/ai-reports/PROJECT_AI_OPERATING_SYSTEM_MIGRATION_AUDIT_REPORT.md`

## Purpose

This addendum records the required audit fixes for the InsuranceAIPlatform project operating system migration.

It is active until the fixes are folded into `PROJECT_AI_OPERATING_SYSTEM.md`.

## 1. Sacred Floor fix

The project operating system inherits the full global Sacred Floor from:

`slavkan777/ai-kb:00_GLOBAL_AI_ENGINEERING_OS/AI_ENGINEERING_BRIDGE_UNIVERSAL_V0.1.md#2-sacred-floor-and-three-seats`

Project-local rule:

- absolute rules remain absolute;
- owner-waivable rules require explicit per-instance owner approval;
- default blocked state applies to unverified production, AI, platform, readiness, and compliance-style claims.

## 2. Routing Lock fix

Default project routing must include these fields in addition to the existing template:

```text
Local workspace, if applicable:
`C:/Projects/InsuranceAIPlatform`

Forbidden repositories unless explicitly approved:
- `slavkan777/ai-kb`
- `slavkan777/gpt-handoff`
- unrelated repositories

Stop condition:
If active workspace, repository, branch, or target path does not match this lock, stop before writing files.
```

## 3. Project-specific boundaries

These boundaries apply unless a current owner-approved gate explicitly overrides them.

Data/demo boundaries:

- synthetic demo data only by default;
- do not place real-world confidential data in prompts, tests, screenshots, reports, logs, or seeded data;
- E2E/test-generated entities should use recognizable prefixes such as `E2E-*` when practical;
- do not mutate seeded/demo claims `CLM-1006`, `CLM-1007`, or `CLM-1012` unless the gate explicitly says so and includes rollback.

Provider/deployment boundaries:

- no paid LLM providers without an explicit gate;
- no new Azure AI resources without an explicit gate;
- no AI provider/config switch without evidence and owner approval;
- no Azure/deployment changes unless task mode is explicitly `deployment`.

Claim boundaries:

- do not claim real production traffic, real-user validation, managed LLM quality, calibrated confidence, compliance readiness, or automated CI/CD unless there is direct evidence.

## 4. Status

This addendum satisfies the three required audit fixes identified in the migration audit.

Next safe step:

- either fold this addendum into `PROJECT_AI_OPERATING_SYSTEM.md`, or
- keep this addendum as the active project-specific fix layer and run a narrow diff-check.
