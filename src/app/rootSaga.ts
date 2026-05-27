import { all, fork, put } from 'redux-saga/effects';
import { aiReviewSaga } from '@/features/aiReview/aiReviewSaga';
import { approvalSaga } from '@/features/approval/approvalSaga';
import { demoSaga } from '@/features/demo/demoSaga';
import { documentsSaga } from '@/features/documents/documentsSaga';
import { claimsSaga } from '@/features/claims/claimsSaga';
import { loadClaimsQueue, loadClaimsSummary } from '@/features/claims/claimsSlice';
import { loadClaimDetail } from '@/features/claims/claimWorkspaceSlice';
import { loadDemoScenario } from '@/features/demo/demoSlice';

export function* rootSaga() {
  yield all([
    fork(aiReviewSaga),
    fork(approvalSaga),
    fork(demoSaga),
    fork(documentsSaga),
    fork(claimsSaga),
  ]);
  // Kick off initial data loads; sagas are already listening by this point
  yield put(loadClaimsQueue());
  yield put(loadClaimsSummary());
  yield put(loadClaimDetail('CLM-1006'));
  yield put(loadDemoScenario());
}
