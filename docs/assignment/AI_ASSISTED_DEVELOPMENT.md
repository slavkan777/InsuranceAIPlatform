# AI-Assisted Development Notes

## Principle

AI tools were used as engineering assistants, not as final decision-makers.

The project is meant to show how AI coding tools can accelerate implementation while the human engineer remains responsible for architecture, scope, safety, code review and final trade-offs.

## Tools used

The workflow can use tools such as:

- ChatGPT / GPT for planning, architecture review, README drafting and critique.
- Claude Code / coding assistants for bounded implementation tasks.
- GitHub Copilot-style autocomplete for local coding acceleration.

The important part is not the specific tool, but the control process around it.

## What AI was allowed to do

AI was allowed to help with:

- decomposing the assignment;
- comparing architecture options;
- drafting API contract shapes;
- generating boilerplate candidates;
- reviewing README clarity;
- finding missing documentation sections;
- proposing test cases;
- identifying risks and edge cases;
- producing first-pass implementation scaffolding.

## What AI was not allowed to decide alone

AI was not allowed to make final decisions about:

- product safety boundaries;
- whether AI can approve/reject claims;
- whether to store or expose sensitive data;
- final architecture trade-offs;
- final README claims;
- whether code is correct enough to submit;
- whether a shortcut is acceptable.

Those decisions remain human-owned.

## Human review gates

Every AI-assisted change should pass these checks:

1. Does it match the assignment scope?
2. Does it keep the app runnable in local mock mode?
3. Does it preserve advisory-only AI behavior?
4. Does it avoid secrets and real PII?
5. Does it keep API boundaries explicit?
6. Does it introduce hidden coupling or unreviewed magic?
7. Does the README accurately describe what exists vs what is planned?
8. Does the code build?

## Repeatable workflow

Preferred workflow:

```text
1. Define the target outcome.
2. Ask AI for alternatives and risks.
3. Pick the simplest viable path.
4. Give the coding assistant a bounded task.
5. Review the diff manually.
6. Run build/tests.
7. Update documentation.
8. Record limitations honestly.
```

## Do's with AI coding assistants

- Give small bounded tasks.
- Provide existing file paths and contracts.
- Ask for a plan before implementation.
- Keep one architectural boundary per task.
- Review all generated code.
- Prefer boring, readable code over clever code.
- Ask for failure cases and test ideas.
- Keep a deterministic mock path for reviewers.

## Don'ts with AI coding assistants

- Do not let AI invent production claims that are not implemented.
- Do not accept generated security logic without review.
- Do not commit secrets or sample real PII.
- Do not let AI silently change product scope.
- Do not let AI create hidden autonomous decision behavior.
- Do not over-engineer only because the tool can generate code quickly.
- Do not submit README text that sounds impressive but is not true.

## Concrete application in this repo

The AI-assisted workflow was useful for:

- turning a polished insurance UI skeleton into an assignment-ready RAG narrative;
- identifying the mismatch between old README wording and newer API/RAG contracts;
- documenting mock vs backend mode;
- making RAG trade-offs explicit;
- adding productionization and observability notes;
- keeping limitations visible instead of hiding them.

The human-owned decisions were:

- keeping AI advisory-only;
- making claim scoping a hard boundary;
- prioritizing local deterministic review mode;
- treating production deployment as documented future work rather than pretending it is complete;
- keeping the solution simple enough to review under time constraints.

## What I would improve in a longer AI-assisted workflow

- Add stricter prompts/templates for code generation tasks.
- Add a generated-code review checklist in the repo.
- Add automated contract tests to prevent DTO drift.
- Add an evaluation dataset and require the AI assistant to run through it before accepting RAG changes.
- Add a changelog explaining which parts were human-designed and which were AI-assisted.
