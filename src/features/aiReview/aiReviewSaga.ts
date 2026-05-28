import { call, delay, put, takeLatest, type SagaReturnType } from 'redux-saga/effects';
import type { PayloadAction } from '@reduxjs/toolkit';
import { insuranceApi } from '@/api/insuranceApi';
import type { AiAnalysisDto } from '@/api/insuranceApi.types';
import {
  aiAnalysisFailed,
  aiAnalysisLoadFailed,
  aiAnalysisLoaded,
  aiAnalysisProgressed,
  aiAnalysisSucceeded,
  loadLatestAiAnalysis,
  runAiAnalysis,
} from './aiReviewSlice';

/**
 * Fetch the latest persisted AI analysis run for a claim.
 * The BFF endpoint returns null when no run exists (404 path is mapped to null in the client).
 */
function* loadLatestWorker(action: PayloadAction<{ claimId: string }>) {
  try {
    const dto: SagaReturnType<typeof insuranceApi.getClaimAiAnalysis> = yield call(
      [insuranceApi, 'getClaimAiAnalysis'],
      action.payload.claimId,
    );
    yield put(aiAnalysisLoaded(dto ?? null));
  } catch (err) {
    const msg = err instanceof Error ? err.message : 'Failed to load AI analysis';
    yield put(aiAnalysisLoadFailed(msg));
  }
}

/**
 * Trigger a new AI analysis run via the BFF and persist the returned DTO.
 * A short progress animation is intentionally retained for UX while the BFF call is in flight.
 */
function* runAiAnalysisWorker(action: PayloadAction<{ claimId: string }>) {
  try {
    // UX progress animation in parallel with the real call.
    const animationSteps = [12, 28, 46, 64];
    for (const value of animationSteps) {
      yield delay(180);
      yield put(aiAnalysisProgressed(value));
    }

    const dto: AiAnalysisDto = yield call(
      [insuranceApi, 'runClaimAiAnalysis'],
      action.payload.claimId,
    );
    yield put(aiAnalysisProgressed(95));
    yield delay(120);
    yield put(aiAnalysisSucceeded(dto));
  } catch (err) {
    const msg = err instanceof Error ? err.message : 'AI analysis run failed';
    yield put(aiAnalysisFailed(msg));
  }
}

export function* aiReviewSaga() {
  yield takeLatest(loadLatestAiAnalysis.type, loadLatestWorker);
  yield takeLatest(runAiAnalysis.type, runAiAnalysisWorker);
}
