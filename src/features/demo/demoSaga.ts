import { call, delay, put, select, takeLatest } from 'redux-saga/effects';
import type { RootState } from '@/app/store';
import { insuranceApi, API_MODE, BackendApiError } from '@/api/insuranceApi';
import { demoSteps } from '@/data/mock/claim-1006';
import {
  advanceDemo,
  demoScenarioFailed,
  demoScenarioLoaded,
  loadDemoScenario,
  setHighlightRoute,
  startDemo,
  stopDemo,
} from './demoSlice';

// ---------------------------------------------------------------------------
// Load demo scenario
// ---------------------------------------------------------------------------

function* loadDemoScenarioWorker() {
  try {
    const scenario: Awaited<ReturnType<typeof insuranceApi.getDemoScenario>> = yield call(
      [insuranceApi, insuranceApi.getDemoScenario],
    );
    yield put(demoScenarioLoaded({ scenario, mode: API_MODE }));
  } catch (err) {
    const message =
      err instanceof BackendApiError
        ? `[${err.code}] ${err.message}`
        : err instanceof Error
          ? err.message
          : 'Unknown error loading demo scenario';
    yield put(demoScenarioFailed({ error: message, fallback: demoSteps }));
  }
}

// ---------------------------------------------------------------------------
// Demo playback
// ---------------------------------------------------------------------------

function* startDemoWorker(): Generator<unknown, void, RootState> {
  let active = true;
  while (active) {
    const state = (yield select()) as RootState;
    active = state.demo.active;
    if (!active) break;
    const step = state.demo.currentStep;
    // Use loaded scenario if available, fall back to static mock
    const steps = state.demo.scenario ?? demoSteps;
    const cfg = steps.find((s) => s.step === step);
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
  yield takeLatest(loadDemoScenario.type, loadDemoScenarioWorker);
  yield takeLatest(startDemo.type, startDemoWorker);
  yield takeLatest(stopDemo.type, stopDemoWorker);
}
