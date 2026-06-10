# InsuranceAIPlatform — AI Learnings Log

GATE_ID / REQUEST_ID: insuranceai-project-os-migration-2026-06-10

Status: ACTIVE PROJECT LEARNING LOG
Repository: `slavkan777/InsuranceAIPlatform`
Branch: `rag/local-foundation-mega-v0.1`
Global baseline: `AI Engineering Bridge Universal V0.1.1`

## Purpose

This file stores project-local lessons learned from real gates.

Promotion rule:

- one-time lesson -> this project log;
- repeated across two or more projects -> candidate for ai-kb global rule;
- owner accepted -> global rule update.

Do not use this file as a general project-state ledger. Durable project state belongs in ai-kb unless a current gate explicitly says otherwise.

---

## 2026-06-10 — Project AI Operating System migration

Gate / Request ID:

`insuranceai-project-os-migration-2026-06-10`

Symptom:

AI tools were creating or expecting files in different repositories because relative paths such as `docs/ai-reports/...` were not tied to an explicit repository, branch, and report destination.

Root cause:

The workflow did not yet require a Routing Lock and Prompt Handoff for every file-affecting task. Active workspace could silently decide where files were written.

Fix / decision:

Adopted `AI Engineering Bridge Universal V0.1.1` as global baseline and created this project-local operating system. Every future file-affecting task must specify target repository, branch, allowed paths, forbidden paths, prompt origin, and report destination.

Rule for next time:

Never hand Claude/Codex/Fable a prompt without a `PROMPT HANDOFF`. Never use a relative path alone as a target.

Evidence:

- ai-kb global baseline accepted: `AI_ENGINEERING_BRIDGE_UNIVERSAL_V0.1_ACCEPTANCE_RECORD.md`
- project operating doc: `docs/ai-workflows/PROJECT_AI_OPERATING_SYSTEM.md`

---

## 2026-06-10 — Report destination ambiguity

Gate / Request ID:

`bridge-report-routing-cleanup-2026-06-10`

Symptom:

A review report was expected in one place while Claude worked from another active repository/workspace.

Root cause:

The prompt told Claude what to do, but did not always explicitly state where the prompt was stored, what repository to read from, and where the report must be committed.

Fix / decision:

Added Prompt Handoff protocol to the global routing rule and project operating workflow.

Rule for next time:

Every prompt handoff must include:

- prompt origin;
- repository/branch/file to read;
- repository/branch/file to write report;
- commit/push permission;
- expected final output.

Evidence:

- ai-kb: `00_GLOBAL_AI_ENGINEERING_OS/AI_BRIDGE_ROUTING_LOCK_RULE.md`
- project operating doc: `docs/ai-workflows/PROJECT_AI_OPERATING_SYSTEM.md`

---

## 2026-06-10 — Overclaim prevention for AI/RAG/CV claims

Gate / Request ID:

`insuranceai-overclaim-control-2026-06-10`

Symptom:

AI/RAG/platform/CV claims can easily become stronger than the actual evidence, especially around production-readiness, security, managed LLMs, and real-user/business traction.

Root cause:

Without evidence categories and constrained claim language, reports and CV wording can drift from verified implementation into marketing overclaim.

Fix / decision:

The project operating system now requires evidence for code, architecture, AI/RAG, deployment, and security claims. If evidence is missing, the claim must be marked `NOT VERIFIED`.

Rule for next time:

For InsuranceAIPlatform, do not claim production traffic, real LLM quality, managed provider readiness, security/compliance readiness, calibrated confidence, or automated CI/CD unless there is direct evidence.

Evidence:

- global baseline: `AI_ENGINEERING_BRIDGE_UNIVERSAL_V0.1.md`
- project operating doc evidence section: `docs/ai-workflows/PROJECT_AI_OPERATING_SYSTEM.md`

---

## Template for future entries

```text
## YYYY-MM-DD — Gate name

Gate / Request ID:
...

Symptom:
...

Root cause:
...

Fix / decision:
...

Rule for next time:
...

Evidence:
...
```
