import { call, put, takeLatest, type SagaReturnType } from 'redux-saga/effects';
import type { PayloadAction } from '@reduxjs/toolkit';
import { insuranceApi } from '@/api/insuranceApi';
import {
  askRag,
  ragAnswerReceived,
  ragAskFailed,
  fetchSimilarClaims,
  similarClaimsReceived,
  similarClaimsFailed,
  fetchAuditHistory,
  auditHistoryReceived,
  auditHistoryFailed,
  fetchInfrastructure,
  infrastructureReceived,
  infrastructureFailed,
  reindexInfrastructure,
} from './ragSlice';

function* askRagWorker(
  action: PayloadAction<{ claimId: string; question: string; useCase: string }>,
) {
  try {
    const { claimId, question, useCase } = action.payload;
    const dto: SagaReturnType<typeof insuranceApi.ragAsk> = yield call(
      [insuranceApi, 'ragAsk'],
      claimId,
      { question, useCase },
    );
    yield put(ragAnswerReceived(dto));
  } catch (err) {
    const msg = err instanceof Error ? err.message : 'RAG ask failed';
    yield put(ragAskFailed(msg));
  }
}

function* fetchSimilarClaimsWorker(
  action: PayloadAction<{ claimId: string; topK: number }>,
) {
  try {
    const { claimId, topK } = action.payload;
    const dto: SagaReturnType<typeof insuranceApi.ragSimilarClaims> = yield call(
      [insuranceApi, 'ragSimilarClaims'],
      claimId,
      topK,
    );
    yield put(similarClaimsReceived(dto));
  } catch (err) {
    const msg = err instanceof Error ? err.message : 'Similar claims fetch failed';
    yield put(similarClaimsFailed(msg));
  }
}

function* fetchAuditHistoryWorker(
  action: PayloadAction<{ claimId: string }>,
) {
  try {
    const { claimId } = action.payload;
    const rows: SagaReturnType<typeof insuranceApi.ragAudit> = yield call(
      [insuranceApi, 'ragAudit'],
      claimId,
      10,
    );
    yield put(auditHistoryReceived(rows));
  } catch (err) {
    const msg = err instanceof Error ? err.message : 'Audit history fetch failed';
    yield put(auditHistoryFailed(msg));
  }
}

function* fetchInfrastructureWorker(
  action: PayloadAction<{ claimId: string }>,
) {
  try {
    const { claimId } = action.payload;
    const dto: SagaReturnType<typeof insuranceApi.ragInfrastructure> = yield call(
      [insuranceApi, 'ragInfrastructure'],
      claimId,
    );
    yield put(infrastructureReceived(dto));
  } catch (err) {
    const msg = err instanceof Error ? err.message : 'Infrastructure fetch failed';
    yield put(infrastructureFailed(msg));
  }
}

function* reindexInfrastructureWorker(
  action: PayloadAction<{ claimId: string }>,
) {
  try {
    const { claimId } = action.payload;
    const dto: SagaReturnType<typeof insuranceApi.ragReindex> = yield call(
      [insuranceApi, 'ragReindex'],
      claimId,
    );
    yield put(infrastructureReceived(dto));
  } catch (err) {
    const msg = err instanceof Error ? err.message : 'Reindex failed';
    yield put(infrastructureFailed(msg));
  }
}

export function* ragSaga() {
  yield takeLatest(askRag.type, askRagWorker);
  yield takeLatest(fetchSimilarClaims.type, fetchSimilarClaimsWorker);
  yield takeLatest(fetchAuditHistory.type, fetchAuditHistoryWorker);
  yield takeLatest(fetchInfrastructure.type, fetchInfrastructureWorker);
  yield takeLatest(reindexInfrastructure.type, reindexInfrastructureWorker);
}
