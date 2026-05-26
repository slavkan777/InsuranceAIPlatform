# Frontend State Model — V0.1

Store: `configureStore` (`src/app/store.ts`), `thunk: false`, saga middleware concatenated. Typed `useAppDispatch` / `useAppSelector` in `src/app/hooks.ts`; `RootState` / `AppDispatch` exported.

## Slice ownership
| Slice | File | Owns | Kind |
|---|---|---|---|
| `claims` | `features/claims/claimsSlice.ts` | queue list, selectedId, search, filters (status/risk/eventType/aiStatus/date), segment | domain-view |
| `claimWorkspace` | `features/claims/claimWorkspaceSlice.ts` | active workspace section, workflow step | UI |
| `documents` | `features/documents/documentsSlice.ts` | selected document, reviewed-ids map, missing-evidence flag, review status/message | domain-view + workflow |
| `aiReview` | `features/aiReview/aiReviewSlice.ts` | mock AI run status, progress %, selected evidence, confidence filter | workflow + UI |
| `approval` | `features/approval/approvalSlice.ts` | draft decision, reviewer notes, checklist, draft status/message | human-draft (never authority) |
| `demo` | `features/demo/demoSlice.ts` | active flag, current step, highlight route | UI/workflow |

## Actions / reducers summary
- `claims`: setSelected, setSearch, setFilter, setSegment.
- `claimWorkspace`: setSection, setWorkflowStep.
- `documents`: selectDocument, toggleReviewed, requestMissingPhoto (+succeeded/failed).
- `aiReview`: runAiAnalysis (+progressed/succeeded/failed), setSelectedEvidence, setConfidenceFilter.
- `approval`: setDecision, setNotes, toggleChecklistItem, saveDraft/draftSaved, sendRequestToCustomer/requestSent, draftFailed.
- `demo`: startDemo, stopDemo, advanceDemo, setDemoStep, setHighlightRoute.

## Selectors summary
Per-feature `*Selectors.ts` provide typed reads — e.g. `selectClaimsQueue`, `selectSelectedClaimId`, `selectActiveClaim`, `selectClaimsFilters`, `selectSelectedDocumentId`, `selectMissingEvidenceFlag`, `selectAiRunStatus`, `selectSelectedAiEvidence`, `selectApprovalDecision`, `selectReviewerNotes`, `selectDemoIsPlaying`, `selectDemoCurrentStep`. Pages should prefer these over deep `state.x.y` reads.

## Intentionally NOT in Redux
Styling/layout; component-local ephemeral state (hover, local input focus); the source-of-truth mock data (lives in `data/mock`, exposed via the mock API); **final claim authority** (approval is a human-controlled draft only).

## Future server-state plan
When the backend lands, **server cache state** (claim lists, claim details, AI results) should move to **RTK Query or TanStack Query**, leaving Redux slices for genuine client/UI state (filters, selections, drafts, demo). The current slices already isolate that client state, so the migration is additive.
