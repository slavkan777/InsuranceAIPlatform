import { all, fork } from 'redux-saga/effects';
import { aiReviewSaga } from '@/features/aiReview/aiReviewSaga';
import { approvalSaga } from '@/features/approval/approvalSaga';
import { demoSaga } from '@/features/demo/demoSaga';
import { documentsSaga } from '@/features/documents/documentsSaga';

export function* rootSaga() {
  yield all([
    fork(aiReviewSaga),
    fork(approvalSaga),
    fork(demoSaga),
    fork(documentsSaga),
  ]);
}
