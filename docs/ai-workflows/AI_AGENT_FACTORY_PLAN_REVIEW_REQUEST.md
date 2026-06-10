# AI Agent Factory Plan Review Request

Purpose: ask an external AI reviewer for a read-only architectural opinion about the proposed universal AI Agent Factory process for current and future projects.

## Context

We want to introduce a lightweight AI-assisted development operating model across all projects.

The useful ideas are:

- agent role separation;
- phased workflows;
- review gates;
- evidence verification;
- recurse/debug loop with a failure log;
- improve/learning step;
- project-level AI operating docs.

## Proposed plan

1. Add a global AIKB document:
   `00_GLOBAL_AI_ENGINEERING_OS/AI_AGENT_FACTORY_UNIVERSAL_V0.1.md`

2. Define standard roles:
   - Architect
   - Builder
   - Reviewer
   - Tester
   - Evidence Verifier
   - Improver

3. Define standard workflows:
   - FEATURE_WORKFLOW
   - TASK_WORKFLOW
   - REVIEW_WORKFLOW
   - RECURSE_DEBUG_WORKFLOW

4. Apply first to InsuranceAIPlatform:
   - `docs/ai-workflows/PROJECT_AI_OPERATING_SYSTEM.md`
   - `docs/ai-agents/README.md`
   - `docs/ai-learnings/README.md`
   - `docs/ai-workflows/FABLE_ARCHITECTURE_REVIEW_PROMPT.md`
   - `docs/ai-workflows/EVIDENCE_VERIFICATION_PROMPT.md`

5. Later apply the minimal structure to all existing and future projects.

## Review questions

1. Is the plan structurally sound?
2. What is overengineered for the current stage?
3. What should be implemented first?
4. What should be postponed?
5. What makes this useful across all projects?
6. What is the minimum viable version?
7. What mistakes should be avoided?
8. What is the next safe step?

## Required answer format

- VERDICT
- WHAT IS GOOD
- WHAT IS OVERENGINEERED
- MISSING PIECES
- BETTER IMPLEMENTATION ORDER
- MINIMUM VIABLE VERSION
- RISKS
- NEXT SAFE STEP

Keep the answer practical and concise. This is an opinion/review request only.
