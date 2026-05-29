import { all, fork, put } from 'redux-saga/effects';
import { aiReviewSaga } from '@/features/aiReview/aiReviewSaga';
import { approvalSaga } from '@/features/approval/approvalSaga';
import { demoSaga } from '@/features/demo/demoSaga';
import { documentsSaga } from '@/features/documents/documentsSaga';
import { claimsSaga } from '@/features/claims/claimsSaga';
import { loadClaimsQueue, loadClaimsSummary } from '@/features/claims/claimsSlice';
import { loadDemoScenario } from '@/features/demo/demoSlice';

export function* rootSaga() {
  yield all([
    fork(aiReviewSaga),
    fork(approvalSaga),
    fork(demoSaga),
    fork(documentsSaga),
    fork(claimsSaga),
  ]);
  // Kick off initial data loads that don't depend on a route param. Per-claim
  // detail is loaded by ClaimShell's useEffect on route mount — boot-time
  // `loadClaimDetail('CLM-1006')` was removed because it locked Redux state
  // on CLM-1006 forever, causing every claim detail page to render CLM-1006
  // data regardless of URL (PostManualV4 bug).
  yield put(loadClaimsQueue());
  yield put(loadClaimsSummary());
  yield put(loadDemoScenario());
}
