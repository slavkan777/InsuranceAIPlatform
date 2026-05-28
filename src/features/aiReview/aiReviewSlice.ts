import { createSlice, type PayloadAction } from '@reduxjs/toolkit';
import type { AiAnalysisDto } from '@/api/insuranceApi.types';

type RunStatus = 'idle' | 'running' | 'succeeded' | 'failed';
type LoadStatus = 'idle' | 'loading' | 'succeeded' | 'failed';

interface AiReviewState {
  // Progress-animation status (legacy UX shape) — driven by saga steps.
  status: RunStatus;
  progressPct: number;

  // Real BFF data — populated by GET /api/claims/{id}/ai-analysis
  // and updated by POST .../ai-analysis/run.
  lastRun: AiAnalysisDto | null;
  lastRunStatus: LoadStatus;
  lastError?: string;

  // UI-only state (carried forward from prior version).
  selectedEvidence: string;
  confidenceFilter: number;
}

const initialState: AiReviewState = {
  status: 'idle',
  progressPct: 0,
  lastRun: null,
  lastRunStatus: 'idle',
  selectedEvidence: 'Поліцейський звіт',
  confidenceFilter: 70,
};

const slice = createSlice({
  name: 'aiReview',
  initialState,
  reducers: {
    // ----- Load latest run from BFF -----
    loadLatestAiAnalysis: {
      reducer(state, _action: PayloadAction<{ claimId: string }>) {
        state.lastRunStatus = 'loading';
        state.lastError = undefined;
      },
      prepare(claimId: string) {
        return { payload: { claimId } };
      },
    },
    aiAnalysisLoaded(state, action: PayloadAction<AiAnalysisDto | null>) {
      state.lastRunStatus = 'succeeded';
      state.lastRun = action.payload;
    },
    aiAnalysisLoadFailed(state, action: PayloadAction<string>) {
      state.lastRunStatus = 'failed';
      state.lastError = action.payload;
    },

    // ----- Run new AI analysis via BFF -----
    runAiAnalysis: {
      reducer(state, _action: PayloadAction<{ claimId: string }>) {
        state.status = 'running';
        state.progressPct = 0;
        state.lastError = undefined;
      },
      prepare(claimId: string) {
        return { payload: { claimId } };
      },
    },
    aiAnalysisProgressed(state, action: PayloadAction<number>) {
      state.progressPct = action.payload;
    },
    /** Saga payload: the persisted AiAnalysisDto from the BFF run endpoint. */
    aiAnalysisSucceeded(state, action: PayloadAction<AiAnalysisDto>) {
      state.status = 'succeeded';
      state.progressPct = 100;
      state.lastRun = action.payload;
      state.lastRunStatus = 'succeeded';
    },
    aiAnalysisFailed(state, action: PayloadAction<string>) {
      state.status = 'failed';
      state.lastError = action.payload;
    },

    // ----- UI-only state -----
    setSelectedEvidence(state, action: PayloadAction<string>) {
      state.selectedEvidence = action.payload;
    },
    setConfidenceFilter(state, action: PayloadAction<number>) {
      state.confidenceFilter = action.payload;
    },
  },
});

export const {
  loadLatestAiAnalysis,
  aiAnalysisLoaded,
  aiAnalysisLoadFailed,
  runAiAnalysis,
  aiAnalysisProgressed,
  aiAnalysisSucceeded,
  aiAnalysisFailed,
  setSelectedEvidence,
  setConfidenceFilter,
} = slice.actions;
export default slice.reducer;
