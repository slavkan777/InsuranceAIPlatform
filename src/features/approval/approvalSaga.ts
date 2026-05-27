import { call, put, takeLatest } from 'redux-saga/effects';
import { insuranceApi } from '@/api/insuranceApi';
import {
  draftFailed,
  draftSaved,
  requestSent,
  saveDraft,
  sendRequestToCustomer,
} from './approvalSlice';

const CLAIM_ID = 'CLM-1006';

// Workflow worker: persist the reviewer's approval DRAFT (never an autonomous decision)
// through the API facade. Write operations stay mock in both modes (no backend endpoint).
function* saveApprovalDraftWorker() {
  try {
    yield call([insuranceApi, insuranceApi.saveApprovalDraft], CLAIM_ID, {
      claimId: CLAIM_ID,
    });
    yield put(draftSaved());
  } catch (err) {
    yield put(draftFailed('Не вдалось зберегти чернетку.'));
  }
}

function* sendCustomerRequestWorker() {
  try {
    yield call([insuranceApi, insuranceApi.sendCustomerRequest], CLAIM_ID);
    yield put(requestSent());
  } catch (err) {
    yield put(draftFailed('Не вдалось надіслати запит клієнту.'));
  }
}

export function* approvalSaga() {
  yield takeLatest(saveDraft.type, saveApprovalDraftWorker);
  yield takeLatest(sendRequestToCustomer.type, sendCustomerRequestWorker);
}
