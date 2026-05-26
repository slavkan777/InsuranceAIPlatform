import { delay, put, select, takeLatest } from 'redux-saga/effects';
import type { RootState } from '@/app/store';
import { advanceDemo, setHighlightRoute, startDemo, stopDemo } from './demoSlice';
import { demoSteps } from '@/data/mock/claim-1006';

function* startDemoWorker(): Generator<unknown, void, RootState> {
  let active = true;
  while (active) {
    const state = (yield select()) as RootState;
    active = state.demo.active;
    if (!active) break;
    const step = state.demo.currentStep;
    const cfg = demoSteps.find((s) => s.step === step);
    yield put(setHighlightRoute(cfg?.route));
    if (step >= 7) break;
    yield delay(1200);
    const next = (yield select()) as RootState;
    if (!next.demo.active) break;
    yield put(advanceDemo());
  }
}

function* stopDemoWorker() {
  yield put(setHighlightRoute(undefined));
}

export function* demoSaga() {
  yield takeLatest(startDemo.type, startDemoWorker);
  yield takeLatest(stopDemo.type, stopDemoWorker);
}
