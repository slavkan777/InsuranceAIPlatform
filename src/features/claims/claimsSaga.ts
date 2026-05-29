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
  aiPipelineSteps,
  auditTrail,
  communicationHistory,
  costDistribution,
  damagePhotos,
  documentsChecklist,
  evidenceTabs,
  extractedEntities,
  keyFindings,
  modelConfidence,
  policyCoverageBlocks,
  policyValidation,
  previousClaims,
  riskFactors,
} from '@/data/mock/claim-1006';
import {
  loadClaimsQueue,
  claimsQueueLoaded,
  claimsQueueFailed,
  loadClaimsSummary,
  claimsSummaryLoaded,
  claimsSummaryFailed,
  type ClaimsSummaryData,
} from './claimsSlice';
import {
  loadClaimDetail,
  claimDetailLoaded,
  claimDetailFailed,
  subResourceRequested,
  subResourceLoaded,
  subResourceFailed,
} from './claimWorkspaceSlice';
import type { PayloadAction } from '@reduxjs/toolkit';

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

function errMsg(err: unknown, fallback: string): string {
  if (err instanceof BackendApiError) return `[${err.code}] ${err.message}`;
  if (err instanceof Error) return err.message;
  return fallback;
}

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
    yield put(claimsQueueFailed({ error: errMsg(err, 'Unknown error loading claims'), fallbackList: claimRows }));
  }
}

// ---------------------------------------------------------------------------
// Load claims summary (dashboard)
// ---------------------------------------------------------------------------

const MOCK_SUMMARY: ClaimsSummaryData = {
  totalActive: 53,
  pendingReview: 5,
  highRisk: 7,
  avgSlaRemainingHours: 18,
  processedToday: 48,
  aiAnalysisRunning: 14,
};

function* loadClaimsSummaryWorker() {
  try {
    const summary: Awaited<ReturnType<typeof insuranceApi.getClaimsSummary>> = yield call(
      [insuranceApi, insuranceApi.getClaimsSummary],
    );
    yield put(claimsSummaryLoaded({ summary, mode: API_MODE }));
  } catch (err) {
    yield put(claimsSummaryFailed({ error: errMsg(err, 'Unknown error loading summary'), fallback: MOCK_SUMMARY }));
  }
}

// ---------------------------------------------------------------------------
// Load claim detail by id + all sub-resources
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
    // Only fall back to the rich goldenClaim mock for the golden id itself.
    // For DB-created claims this used to fall back to nothing (`null`) which
    // is correct — surface the failure honestly, do NOT leak CLM-1006 data.
    const fallback = claimId === 'CLM-1006' ? goldenClaim : null;
    yield put(claimDetailFailed({ error: errMsg(err, 'Unknown error loading claim detail'), fallback }));
  }

  // Sub-resource fallbacks: CLM-1006 (the demo claim) keeps the rich mock
  // fixtures so its demo page survives a backend hiccup. For any other claim
  // we fall back to HONEST EMPTY structures — leaking CLM-1006 documents /
  // photos / AI findings into a different claim's tab was part of the
  // PostManualV4 bug surface area.
  const useGoldenFixtures = claimId === 'CLM-1006';

  yield* loadSubResource(
    'documents',
    () => insuranceApi.getClaimDocuments(claimId),
    useGoldenFixtures ? documentsChecklist : [],
  );
  yield* loadSubResource(
    'photos',
    () => insuranceApi.getClaimPhotos(claimId),
    useGoldenFixtures ? damagePhotos : [],
  );
  yield* loadSubResource(
    'aiEvidence',
    () => insuranceApi.getAiAnalysis(claimId),
    useGoldenFixtures
      ? {
          findings: keyFindings.map((f, i) => ({ id: `f-${i}`, text: f.text, detail: f.detail, tone: f.tone })),
          evidence: evidenceTabs,
          modelConfidence,
          extractedEntities,
        }
      : { findings: [], evidence: [], modelConfidence: [], extractedEntities: [] },
  );
  yield* loadSubResource(
    'risks',
    () => insuranceApi.getRiskReview(claimId),
    useGoldenFixtures
      ? {
          score: goldenClaim.riskScore,
          threshold: 60,
          factors: riskFactors,
          pipeline: aiPipelineSteps,
        }
      : { score: 0, threshold: 60, factors: [], pipeline: [] },
  );
  yield* loadSubResource(
    'policy',
    () => insuranceApi.getPolicyCoverage(claimId),
    useGoldenFixtures
      ? { blocks: policyCoverageBlocks, validation: policyValidation }
      : { blocks: [], validation: [] },
  );
  yield* loadSubResource(
    'customerVehicle',
    () => insuranceApi.getCustomerVehicleContext(claimId),
    useGoldenFixtures
      ? { previousClaims, communicationHistory }
      : { previousClaims: [], communicationHistory: [] },
  );
  yield* loadSubResource(
    'approvalRead',
    () => insuranceApi.getClaimApproval(claimId),
    useGoldenFixtures
      ? {
          claimId,
          currentDecision: 'request',
          notes: 'Запрошуємо клієнта надати фото пошкодження заднього бампера. AI confidence 78%.',
          savedAt: null,
          submitted: false,
          submittedAt: null,
          availableOptions: [
            { value: 'approve', label: 'Погодити виплату', recommended: false, description: 'Якщо ризики прийнятні' },
            { value: 'request', label: 'Запросити дані', recommended: true, description: 'Рекомендовано AI' },
            { value: 'reject', label: 'Відхилити', recommended: false, description: 'З обґрунтуванням' },
            { value: 'escalate', label: 'Передати старшому', recommended: false, description: 'Ескалація' },
          ],
          aiRecommendation: 'Запросити додаткове фото перед погодженням виплати',
          recommendedPayout: goldenClaim.recommendedPayout,
        }
      : {
          claimId,
          currentDecision: null,
          notes: null,
          savedAt: null,
          submitted: false,
          submittedAt: null,
          availableOptions: [],
          aiRecommendation: null,
          recommendedPayout: 0,
        },
  );
  yield* loadSubResource(
    'audit',
    () => insuranceApi.getAuditTrace(claimId),
    useGoldenFixtures
      ? {
          runId: goldenClaim.runId,
          traceId: goldenClaim.traceId,
          model: 'local-mock-v0.1',
          tokens: goldenClaim.tokens,
          cost: goldenClaim.cost,
          durationSec: goldenClaim.durationSec,
          events: auditTrail,
          distribution: costDistribution,
        }
      : {
          runId: '',
          traceId: '',
          model: '',
          tokens: 0,
          cost: 0,
          durationSec: 0,
          events: [],
          distribution: [],
        },
  );
}

type SubKey = 'documents' | 'photos' | 'aiEvidence' | 'risks' | 'policy' | 'customerVehicle' | 'approvalRead' | 'audit';

// Generic sub-resource loader generator
function* loadSubResource(
  key: SubKey,
  apiFn: () => Promise<unknown>,
  fallback: unknown,
): Generator {
  yield put(subResourceRequested(key));
  try {
    const data: unknown = yield call(apiFn);
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    yield put(subResourceLoaded({ key, data: data as any, mode: API_MODE }));
  } catch (err) {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    yield put(subResourceFailed({ key, error: errMsg(err, `Unknown error loading ${key}`), fallback: fallback as any }));
  }
}

// ---------------------------------------------------------------------------
// Watcher
// ---------------------------------------------------------------------------

export function* claimsSaga() {
  yield takeLatest(loadClaimsQueue.type, loadClaimsQueueWorker);
  yield takeLatest(loadClaimsSummary.type, loadClaimsSummaryWorker);
  yield takeLatest(loadClaimDetail.type, loadClaimDetailWorker);
}
