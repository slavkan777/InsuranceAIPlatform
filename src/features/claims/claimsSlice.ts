import { createSlice, type PayloadAction } from '@reduxjs/toolkit';
import { claimRows } from '@/data/mock/claims';
import type { ClaimRow } from '@/types';

/** 'mock-fallback' = backend unreachable; fell back to mock data with degraded indicator */
export type ClaimsApiMode = 'mock' | 'backend' | 'mock-fallback';

export interface ClaimsSummaryData {
  totalActive: number;
  pendingReview: number;
  highRisk: number;
  avgSlaRemainingHours: number;
  processedToday: number;
  aiAnalysisRunning: number;
}

interface ClaimsState {
  list: ClaimRow[];
  selectedId: string;
  search: string;
  filters: {
    status: string;
    risk: string;
    eventType: string;
    aiStatus: string;
    date: string;
  };
  segment: 'Усі' | 'ДТП' | 'Високий ризик' | 'Чекає AI' | 'Чекає рішення';
  // --- async load state (queue) ---
  loading: boolean;
  error: string | null;
  apiMode: ClaimsApiMode;
  // --- async load state (summary) ---
  summary: ClaimsSummaryData | null;
  summaryLoading: boolean;
  summaryError: string | null;
  summaryApiMode: ClaimsApiMode;
}

const initialState: ClaimsState = {
  list: claimRows,
  selectedId: 'CLM-1006',
  search: '',
  filters: {
    status: 'Усі',
    risk: 'Усі',
    eventType: 'ДТП',
    aiStatus: 'Усі',
    date: '7 днів',
  },
  segment: 'Усі',
  loading: false,
  error: null,
  apiMode: 'mock',
  summary: null,
  summaryLoading: false,
  summaryError: null,
  summaryApiMode: 'mock',
};

const claimsSlice = createSlice({
  name: 'claims',
  initialState,
  reducers: {
    setSelected(state, action: PayloadAction<string>) {
      state.selectedId = action.payload;
    },
    setSearch(state, action: PayloadAction<string>) {
      state.search = action.payload;
    },
    setFilter(
      state,
      action: PayloadAction<{ key: keyof ClaimsState['filters']; value: string }>,
    ) {
      state.filters[action.payload.key] = action.payload.value;
    },
    setSegment(state, action: PayloadAction<ClaimsState['segment']>) {
      state.segment = action.payload;
    },
    // --- async load actions for claims queue ---
    loadClaimsQueue(state) {
      state.loading = true;
      state.error = null;
    },
    claimsQueueLoaded(
      state,
      action: PayloadAction<{ list: ClaimRow[]; mode: ClaimsApiMode }>,
    ) {
      state.loading = false;
      state.list = action.payload.list;
      state.apiMode = action.payload.mode;
      state.error = null;
    },
    claimsQueueFailed(
      state,
      action: PayloadAction<{ error: string; fallbackList: ClaimRow[] }>,
    ) {
      state.loading = false;
      state.list = action.payload.fallbackList;
      state.error = action.payload.error;
      state.apiMode = 'mock-fallback';
    },
    // --- async load actions for dashboard summary ---
    loadClaimsSummary(state) {
      state.summaryLoading = true;
      state.summaryError = null;
    },
    claimsSummaryLoaded(
      state,
      action: PayloadAction<{ summary: ClaimsSummaryData; mode: ClaimsApiMode }>,
    ) {
      state.summaryLoading = false;
      state.summary = action.payload.summary;
      state.summaryApiMode = action.payload.mode;
      state.summaryError = null;
    },
    claimsSummaryFailed(
      state,
      action: PayloadAction<{ error: string; fallback: ClaimsSummaryData }>,
    ) {
      state.summaryLoading = false;
      state.summary = action.payload.fallback;
      state.summaryError = action.payload.error;
      state.summaryApiMode = 'mock-fallback';
    },
  },
});

export const {
  setSelected,
  setSearch,
  setFilter,
  setSegment,
  loadClaimsQueue,
  claimsQueueLoaded,
  claimsQueueFailed,
  loadClaimsSummary,
  claimsSummaryLoaded,
  claimsSummaryFailed,
} = claimsSlice.actions;
export default claimsSlice.reducer;
