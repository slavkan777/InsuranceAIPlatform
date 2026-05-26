import { delay, put, takeLatest } from 'redux-saga/effects';
import {
  aiAnalysisFailed,
  aiAnalysisProgressed,
  aiAnalysisSucceeded,
  runAiAnalysis,
} from './aiReviewSlice';

function* runAiAnalysisWorker() {
  try {
    const steps = [12, 28, 46, 64, 82, 100];
    for (const value of steps) {
      yield delay(350);
      yield put(aiAnalysisProgressed(value));
    }
    yield put(aiAnalysisSucceeded());
  } catch (err) {
    yield put(aiAnalysisFailed('Mock AI run failed'));
  }
}

export function* aiReviewSaga() {
  yield takeLatest(runAiAnalysis.type, runAiAnalysisWorker);
}
