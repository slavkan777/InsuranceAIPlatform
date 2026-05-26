import { call, put, takeLatest } from 'redux-saga/effects';
import { mockInsuranceApi } from '@/api/mockInsuranceApi';
import {
  draftFailed,
  draftSaved,
  requestSent,
  saveDraft,
  sendRequestToCustomer,
} from './approvalSlice';

const CLAIM_ID = 'CLM-1006';

// Workflow worker: persist the reviewer's approval DRAFT (never an autonomous decision)
// through the mock API seam. Swapping mockInsuranceApi for a real .NET client later
// requires no change here.
function* saveApprovalDraftWorker() {
  try {
    yield call(mockInsuranceApi.saveApprovalDraft, CLAIM_ID, { claimId: CLAIM_ID });
    yield put(draftSaved());
  } catch (err) {
    yield put(draftFailed('Не вдалось зберегти чернетку.'));
  }
}

function* sendCustomerRequestWorker() {
  try {
    yield call(mockInsuranceApi.sendCustomerRequest, CLAIM_ID);
    yield put(requestSent());
  } catch (err) {
    yield put(draftFailed('Не вдалось надіслати запит клієнту.'));
  }
}

export function* approvalSaga() {
  yield takeLatest(saveDraft.type, saveApprovalDraftWorker);
  yield takeLatest(sendRequestToCustomer.type, sendCustomerRequestWorker);
}
