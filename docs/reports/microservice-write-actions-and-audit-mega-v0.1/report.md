# Microservice Write Actions & Audit (Mega) — V0.1 — Report

**Gate:** `MICROSERVICE_WRITE_ACTIONS_AND_AUDIT_MEGA_V0.1` · **Date:** 2026-05-27
**Type:** bounded implementation — human-controlled write commands + audit + outbox behind BFF/service boundaries. No AI/Azure/payout/messaging/upload/broker/secrets, no source commit/push.

## Current state
- Source: branch `dev` @ `1fc1774`; `origin/main` `69e6731` untouched. **No commit this gate** — HEAD still `1fc1774`; changes uncommitted.
- DB: `InsuranceAIPlatform` (LocalDB) — two new migrations applied; 200 synthetic users + `CLM-1006` intact.

## Commands implemented (4, human-controlled, BFF-only)
| Command | POST route | Service | Audit | Outbox |
|---|---|---|---|---|
| SaveApprovalDraft | `/api/claims/{id}/approval-draft` | Approval | `ApprovalDraftSaved` | `ApprovalDraftSaved` |
| SubmitHumanDecision | `/api/claims/{id}/human-decision` | Approval | `HumanDecisionSubmitted` | `HumanDecisionSubmitted` + `ClaimStatusTransitionRequested` |
| RequestMissingDocument | `/api/claims/{id}/missing-document-requests` | Documents | `MissingDocumentRequested` | `MissingDocumentRequested` |
| CreateDocumentMetadataPlaceholder | `/api/claims/{id}/document-metadata` | Documents | `DocumentMetadataCreated` | `DocumentMetadataCreated` |

## BFF orchestration / boundaries
`ClaimCommandsController` injects only `IApprovalService` / `IDocumentsService` / `IAuditCostService` (no DbContext). Flow: correlation id → synthetic actor (`demo.adjuster@insuranceai.local`) → owning-service write → `AppendAuditAsync` + `WriteOutboxAsync` → `CommandResult`. Services never reference each other; DB-backed impls are singletons via `IDbContextFactory`; the 6 skeleton health contributors are preserved.

## Human-control / safety rules
Decision ∈ {ApproveForReview, RejectForReview, NeedsMoreInformation, RequestDocuments}; invalid → `400 INVALID_DECISION`. AI cannot decide. **No payout execution** (`RecommendedPayout` is a synthetic display amount only). No customer message sent. No binary upload. Invalid claim id → safe error. Idempotency-Key header dedups outbox (duplicate → existing id + warning).

## Audit + outbox
`AuditEvent` extended (nullable `CorrelationId`/`Actor`/`ActionType`/`OccurredAtUtc`/`MetadataJson`); new `OutboxMessage` (audit_cost): EventType/ClaimId/OccurredAtUtc/CorrelationId/PayloadJson(sanitized)/Processed=false/ProcessedAtUtc?/Error?/IdempotencyKey?. Every command appends audit + writes outbox (one AuditCostDbContext SaveChanges). No broker.

## DB / migrations
`AddOutboxAndCommandAudit` (AuditCost), `AddMissingDocumentRequests` (Documents) — applied live. No Approval/Claims migration. DevDept untouched.

## Verification
| Check | Result |
|---|---|
| Backend build | **0 warnings, 0 errors** |
| Backend tests | **75 passed / 0 failed** (53 prior + 22 new; InMemory factory, no live-DB dependency) |
| Frontend build | **107 modules**, PASS (additive `src/api/` only) |
| Live command smoke | 4 POSTs → safe `CommandResult` (audit/outbox ids populated, correlation echoed); invalid decision → 400 |
| DB | 200 synthetic users exact; `CLM-1006` present; `OutboxMessages` + `MissingDocumentRequests` exist; AuditEvent command columns present; outbox rows written |
| Write endpoints | exactly **4** (`[HttpPost]`); none for Put/Patch/Delete/upload |
| Provider/Azure/messaging SDK | none; no AI provider call; `DEEPSEEK_API_KEY` never read |
| Boundaries | BFF injects no DbContext; no service-to-service refs |
| Source commit/push | none; `main` untouched (`69e6731`); HEAD `1fc1774` |

## Files
- **New:** BuildingBlocks `{CommandResult, ActorContext(+CommandActors), HumanDecisions}.cs`; AuditCost `{OutboxMessage, PersistenceAuditCostService}.cs` (+ AuditEvent/DbContext/extensions/interface/skeleton edits); Approval `PersistenceApprovalService.cs` (+ interface/skeleton/extensions edits); Documents `{MissingDocumentRequest, PersistenceDocumentsService}.cs` (+ DbContext/interface/skeleton/extensions edits); `Controllers/ClaimCommandsController.cs`; 2 migrations; `Tests/{CommandTestWebApplicationFactory, ClaimCommandTests}.cs` (22 tests).
- **Modified:** `Program.cs` (persistence wiring + connection resolution); `src/api/{backendInsuranceApi.ts, insuranceApi.types.ts}` (command client, additive).

## Deferred / limitations
Owning-service write and audit/outbox write are two separate transactions (true aggregate-atomic outbox deferred); broker/Azure mapping deferred; DB-backed read migration deferred (Option A); claim status not mutated (outbox event only); AI provider deferred.

## Next safe step
`COMMIT_AND_PUSH_DEV_MICROSERVICE_WRITE_ACTIONS_AND_AUDIT_ONLY` — commit exactly this scope to `dev`, fast-forward push `dev` only; `main` untouched; no force.
