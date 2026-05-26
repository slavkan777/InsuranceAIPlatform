import { createSlice, type PayloadAction } from '@reduxjs/toolkit';

type RunStatus = 'idle' | 'running' | 'succeeded' | 'failed';

interface AiReviewState {
  status: RunStatus;
  progressPct: number;
  selectedEvidence: string;
  confidenceFilter: number;
  lastError?: string;
}

const initialState: AiReviewState = {
  status: 'succeeded',
  progressPct: 100,
  selectedEvidence: 'Поліцейський звіт',
  confidenceFilter: 70,
};

const slice = createSlice({
  name: 'aiReview',
  initialState,
  reducers: {
    runAiAnalysis(state) {
      state.status = 'running';
      state.progressPct = 0;
      state.lastError = undefined;
    },
    aiAnalysisProgressed(state, action: PayloadAction<number>) {
      state.progressPct = action.payload;
    },
    aiAnalysisSucceeded(state) {
      state.status = 'succeeded';
      state.progressPct = 100;
    },
    aiAnalysisFailed(state, action: PayloadAction<string>) {
      state.status = 'failed';
      state.lastError = action.payload;
    },
    setSelectedEvidence(state, action: PayloadAction<string>) {
      state.selectedEvidence = action.payload;
    },
    setConfidenceFilter(state, action: PayloadAction<number>) {
      state.confidenceFilter = action.payload;
    },
  },
});

export const {
  runAiAnalysis,
  aiAnalysisProgressed,
  aiAnalysisSucceeded,
  aiAnalysisFailed,
  setSelectedEvidence,
  setConfidenceFilter,
} = slice.actions;
export default slice.reducer;
