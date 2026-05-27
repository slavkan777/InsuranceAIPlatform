# Microservice Write Actions & Audit — Implementation V0.1

**Gate:** `MICROSERVICE_WRITE_ACTIONS_AND_AUDIT_MEGA_V0.1` · **Branch:** `dev` @ `1fc1774` (no commit in this gate) · **Date:** 2026-05-27
**Type:** bounded implementation — human-controlled write/command workflows + audit append + transactional outbox records behind the BFF + service boundaries. No AI provider, no Azure, no payout, no customer messaging, no binary upload, no broker, no source commit/push.
**Status:** implemented; build + tests + frontend + live command smoke all PASS.

## Status / purpose
Introduce the first safe write surface: four human-controlled commands routed through the BFF, each writing to its owning service's DB, with an audit event + outbox event recorded for every command. Human authority is absolute; AI is never a decision-maker; no money moves and no message is sent.

## Source state before implementation
`dev` @ `1fc1774` (`feat: add microservice persistence and synthetic seed`); `origin/main` `69e6731` untouched. Six service-owned schemas/DbContexts + 200 synthetic users + `CLM-1006` seeded; reads served in-memory (Option A); no write endpoints existed.

## Command surface (BFF-facing, 4 commands)
| Command | Route (POST) | Owning service | Audit action | Outbox event(s) |
|---|---|---|---|---|
| Save approval draft | `/api/claims/{claimId}/approval-draft` | Approval | `ApprovalDraftSaved` | `ApprovalDraftSaved` |
| Submit human decision | `/api/claims/{claimId}/human-decision` | Approval | `HumanDecisionSubmitted` | `HumanDecisionSubmitted` + `ClaimStatusTransitionRequested` |
| Request missing document | `/api/claims/{claimId}/missing-document-requests` | Documents | `MissingDocumentRequested` | `MissingDocumentRequested` |
| Create document metadata placeholder | `/api/claims/{claimId}/document-metadata` | Documents | `DocumentMetadataCreated` | `DocumentMetadataCreated` |

All are `[HttpPost]` (the only four write verbs in the codebase — verified). Commands go through the BFF only; the frontend never calls a service directly.

## Service ownership
- **Approval** owns the approval draft + human-decision workflow state (`ApprovalDraft`, `ApprovalDecisionOption`). `SaveDraftAsync` / `SubmitDecisionAsync`.
- **Documents** owns missing-document requests (`MissingDocumentRequest`, new) + document metadata placeholders (`ClaimDocument`). `RequestMissingDocumentAsync` / `CreateMetadataPlaceholderAsync`.
- **Audit & Cost** owns the append-only audit trail + the transactional outbox (`OutboxMessage`, new; `AuditEvent` extended). `AppendAuditAsync` / `WriteOutboxAsync`.
- **Claims** is **not mutated** — a human decision emits a `ClaimStatusTransitionRequested` outbox event (deterministic decision→requested-status), no claim-row write (safest option).
- **AI Analysis** is untouched and provider-free in this gate.

## Human-control rules (deterministic)
- Only a human decision can submit; allowed set: `ApproveForReview`, `RejectForReview`, `NeedsMoreInformation`, `RequestDocuments` (validated; anything else → safe `400 INVALID_DECISION`). AI cannot approve/reject/finalize (no AI path in commands).
- **No payout is ever executed** — `RecommendedPayout` is a synthetic display amount only, never disbursed.
- Customer messages remain internal/local placeholders — no SMS/email is sent.
- Invalid claim id → safe error; raw exceptions are never returned to the client.
- Synthetic actor only: `demo.adjuster@insuranceai.local` / "Synthetic Adjuster" (`ActorType=human`).

## Audit behavior
Every command appends an `AuditEvent` (extended with nullable `CorrelationId`, `Actor`, `ActionType`, `OccurredAtUtc`, `MetadataJson`). Payloads are sanitized — no secrets, no real PII. Append is owned by Audit & Cost and invoked by the BFF after the owning-service write.

## Outbox / event behavior
`OutboxMessage` (schema `audit_cost`): `Id`, `EventType`, `ClaimId` (aggregate id), `OccurredAtUtc`, `CorrelationId`, `PayloadJson` (sanitized), `Processed` (false), `ProcessedAtUtc?`, `Error?`, `IdempotencyKey?`. Records cross-service facts for a future broker/Azure mapping — **no broker integration** in this gate. Audit + outbox are written together in one `AuditCostDbContext` SaveChanges (atomic together). Idempotency: an `Idempotency-Key` header dedups on the outbox key — a duplicate returns the existing id + a warning, no double-write.

## BFF orchestration (boundary-respecting)
`ClaimCommandsController` injects only service interfaces (`IApprovalService`, `IDocumentsService`, `IAuditCostService`) — **never a DbContext**. Per command: resolve correlation id (middleware) → build synthetic `ActorContext` → call the owning service's write (its own DbContext) → `AppendAuditAsync` + `WriteOutboxAsync` → return `CommandResult { Success, CommandId, ClaimId, Status, AuditEventId, OutboxMessageId, CorrelationId, Message, Warnings }`. Services never reference each other; the BFF is the only orchestrator. DB-backed service impls are registered as singletons via `IDbContextFactory` (no scoped-in-singleton issue); the skeleton health contributors are preserved (still 6).

## DB migrations
Two service-owned migrations, applied live to `InsuranceAIPlatform`: `AddOutboxAndCommandAudit` (AuditCost — OutboxMessages table + AuditEvent columns) and `AddMissingDocumentRequests` (Documents). No Approval migration (ApprovalDraft already had the needed fields); no Claims migration (no mutation). No shared/monolithic migration; DevDept untouched. Post-apply: 200 synthetic users still exact; `CLM-1006` present.

## Frontend / API integration
Additive only: typed command client functions + body/`CommandResult` types added under `src/api/` (`backendInsuranceApi.ts`, `insuranceApi.types.ts`). No component/page rewrite; no payout/upload/message buttons enabled; existing screens unchanged. UI button wiring deferred (honest — reported). `npm run build` PASS (107 modules).

## Tests added/updated
22 new command tests via a `CommandTestWebApplicationFactory` that swaps the SqlServer DbContexts to EF-InMemory (so `dotnet test` needs no live DB): draft-save success + audit/outbox; valid human decision succeeds (Submitted=true); invalid decision → 400; missing-document request + audit/outbox; metadata placeholder + audit/outbox; idempotency safe-duplicate; `CommandResult` shape; no-AI-provider/no-egress; command controller injects services not a DbContext; preserved read routes + boundary tests still pass. **Total 75 passed / 0 failed** (53 prior + 22). Existing 53 stay DB-free.

## Verification results
- Backend build: **0 warnings, 0 errors**. Backend tests: **75 passed / 0 failed**.
- Frontend build: **107 modules**, PASS. Live command smoke: the 4 POSTs returned safe structured `CommandResult` (e.g. `ApprovalDraftSaved`, auditEventId/outboxMessageId populated, correlation id echoed); invalid decision → `400 INVALID_DECISION`.
- DB: 200 synthetic users exact; `CLM-1006` present; `audit_cost.OutboxMessages` + `documents.MissingDocumentRequests` exist; AuditEvent command columns present; outbox rows written.

## Safety scan
Exactly 4 write endpoints (all human-controlled POST); no payout execution; no SMS/email/customer send; no binary/file upload; no AI provider call; no provider/Azure/Twilio/SendGrid SDK package; no Azure; no broker; `DEEPSEEK_API_KEY` never read (only a "never used" doc-comment); no DevDept; no real PII (`@example.invalid`); no secret/password connection string; BFF injects no DbContext; no service-to-service references; no source commit/push; `main` untouched (`69e6731`); HEAD `1fc1774`.

## Deferred work
True transactional outbox within each aggregate's own context (currently the owning-service write and the audit/outbox write are two separate transactions — acceptable local-demo); broker/Azure Service Bus mapping; DB-backed read migration (Option A still in force); claim-status mutation; AI Analysis provider (mock default; DeepSeek opt-in/disabled). All later gates.

## Next gate
`COMMIT_AND_PUSH_DEV_MICROSERVICE_WRITE_ACTIONS_AND_AUDIT_ONLY` — commit exactly this write/audit/outbox scope to `dev`, fast-forward push `dev` only; `main` untouched; no force.

## Stop boundaries
Write commands + audit + outbox only. No commit/push, no AI provider, no Azure, no payout, no customer messaging, no binary upload, no broker, no `main` change, no merge, no force push.
