# Frontend Saga Workflows — V0.1

## Ownership statement
Redux-Saga owns **multi-step / cancellable workflow orchestration** and is the designated **side-effect / future-API seam**. It does **not** own styling, simple tab/checkbox toggles, or any final business authority (no payout, no autonomous approval/rejection). `rootSaga` (`src/app/rootSaga.ts`) forks the watchers.

## Watcher / worker table
| Watcher (export) | Worker | Action listened | Effects / mock behavior | Future backend replacement |
|---|---|---|---|---|
| `aiReviewSaga` | `runAiAnalysisWorker` | `runAiAnalysis` | `takeLatest`; stepped progress 12→100 via `delay`+`put` | `POST /api/claims/{id}/ai-analysis/run` (poll/stream) |
| `documentsSaga` | `requestMissingPhotoWorker` | `requestMissingPhoto` | `takeLatest`; ~900ms mock SMS+email | `POST /api/claims/{id}/documents/request` |
| `approvalSaga` | `saveApprovalDraftWorker` | `saveDraft` | `takeLatest`; `call(mockInsuranceApi.saveApprovalDraft)` | `POST /api/claims/{id}/approval-draft` |
| `approvalSaga` | `sendCustomerRequestWorker` | `sendRequestToCustomer` | `takeLatest`; `call(mockInsuranceApi.sendCustomerRequest)` | `POST /api/claims/{id}/customer-request` |
| `demoSaga` | `startDemoWorker` / `stopDemoWorker` | `startDemo` / `stopDemo` | auto-advance steps 1→7 (`select` + `delay`), cancellable | n/a (client-only guided demo) |

## Async / mock effects
All async is local: `delay()` and `call(mockInsuranceApi.*)`. There is no `fetch`/`axios`/WebSocket. The approval workers already route through the mock API seam, so they become real by swapping the API implementation only.

## Future backend replacement path
Replace `mockInsuranceApi` methods with a real typed `.NET` client (same async signatures). Watchers/workers, action contracts, and reducers stay unchanged. Add retry/backoff and cancellation as needed (sagas are the right home for that).

## What Saga must NOT do
CSS/layout; trivial toggles (tab/checkbox/filter — those live in slice reducers); final claim approval/rejection authority; payout decisions; hidden business decisions.

## Interview explanation
> "I use Saga for orchestration and side effects, not for trivial state. The AI run, document request, approval draft/send, and the guided demo are multi-step or cancellable flows — that's where Saga earns its place. Today they call a typed mock API; tomorrow that same call hits a .NET endpoint and nothing else changes. Trivial UI toggles stay in Toolkit reducers."
