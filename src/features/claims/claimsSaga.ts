/**
 * Sagas for async claim reads.
 *
 * In mock mode:  calls insuranceApi (which routes to mockInsuranceApi) — no network.
 * In backend mode: calls insuranceApi (which routes to backendInsuranceApi).
 *
 * On BackendApiError (e.g. backend unreachable): sets 'mock-fallback' mode +
 * surfaces error in state. Does NOT silently swap to mock without surfacing it.
 * On network/parse failure: same path.
 */

import { call, put, takeLatest } from 'redux-saga/effects';
import { insuranceApi, API_MODE, BackendApiError } from '@/api/insuranceApi';
import { claimRows, goldenClaim } from '@/data/mock/claims';
import {
  loadClaimsQueue,
  claimsQueueLoaded,
  claimsQueueFailed,
} from './claimsSlice';
import {
  loadClaimDetail,
  claimDetailLoaded,
  claimDetailFailed,
} from './claimWorkspaceSlice';
import type { PayloadAction } from '@reduxjs/toolkit';

// ---------------------------------------------------------------------------
// Load claims queue
// ---------------------------------------------------------------------------

function* loadClaimsQueueWorker() {
  try {
    const list: Awaited<ReturnType<typeof insuranceApi.getClaimsQueue>> = yield call(
      [insuranceApi, insuranceApi.getClaimsQueue],
    );
    yield put(claimsQueueLoaded({ list, mode: API_MODE }));
  } catch (err) {
    const message =
      err instanceof BackendApiError
        ? `[${err.code}] ${err.message}`
        : err instanceof Error
          ? err.message
          : 'Unknown error loading claims';
    // Fallback to static mock data; surface degraded mode
    yield put(claimsQueueFailed({ error: message, fallbackList: claimRows }));
  }
}

// ---------------------------------------------------------------------------
// Load claim detail by id
// ---------------------------------------------------------------------------

function* loadClaimDetailWorker(action: PayloadAction<string>) {
  const claimId = action.payload;
  try {
    const detail: Awaited<ReturnType<typeof insuranceApi.getClaimById>> = yield call(
      [insuranceApi, insuranceApi.getClaimById],
      claimId,
    );
    yield put(claimDetailLoaded({ detail, mode: API_MODE }));
  } catch (err) {
    const message =
      err instanceof BackendApiError
        ? `[${err.code}] ${err.message}`
        : err instanceof Error
          ? err.message
          : 'Unknown error loading claim detail';
    // Fallback to goldenClaim mock (only CLM-1006 has mock data)
    const fallback = claimId === 'CLM-1006' ? goldenClaim : null;
    yield put(claimDetailFailed({ error: message, fallback }));
  }
}

// ---------------------------------------------------------------------------
// Watcher
// ---------------------------------------------------------------------------

export function* claimsSaga() {
  yield takeLatest(loadClaimsQueue.type, loadClaimsQueueWorker);
  yield takeLatest(loadClaimDetail.type, loadClaimDetailWorker);
}
