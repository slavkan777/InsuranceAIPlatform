import { createSlice, type PayloadAction } from '@reduxjs/toolkit';
import type { ClaimDetail } from '@/types';
import type { ClaimsApiMode } from './claimsSlice';

type ActiveSection =
  | 'overview'
  | 'documents'
  | 'ai-evidence'
  | 'risks'
  | 'approval'
  | 'audit'
  | 'policy'
  | 'customer';

type WorkflowStep =
  | 'registration'
  | 'documents'
  | 'ai-analysis'
  | 'risk-review'
  | 'human-decision'
  | 'completion';

interface ClaimWorkspaceState {
  activeSection: ActiveSection;
  workflowStep: WorkflowStep;
  // --- async claim detail load ---
  claimDetail: ClaimDetail | null;
  loading: boolean;
  error: string | null;
  apiMode: ClaimsApiMode;
}

const initialState: ClaimWorkspaceState = {
  activeSection: 'overview',
  workflowStep: 'human-decision',
  claimDetail: null,
  loading: false,
  error: null,
  apiMode: 'mock',
};

const slice = createSlice({
  name: 'claimWorkspace',
  initialState,
  reducers: {
    setSection(state, action: PayloadAction<ActiveSection>) {
      state.activeSection = action.payload;
    },
    setWorkflowStep(state, action: PayloadAction<WorkflowStep>) {
      state.workflowStep = action.payload;
    },
    // --- async claim detail load ---
    loadClaimDetail(state, _action: PayloadAction<string>) {
      state.loading = true;
      state.error = null;
    },
    claimDetailLoaded(
      state,
      action: PayloadAction<{ detail: ClaimDetail; mode: ClaimsApiMode }>,
    ) {
      state.loading = false;
      state.claimDetail = action.payload.detail;
      state.apiMode = action.payload.mode;
      state.error = null;
    },
    claimDetailFailed(
      state,
      action: PayloadAction<{ error: string; fallback: ClaimDetail | null }>,
    ) {
      state.loading = false;
      state.claimDetail = action.payload.fallback;
      state.error = action.payload.error;
      state.apiMode = 'mock-fallback';
    },
  },
});

export const {
  setSection,
  setWorkflowStep,
  loadClaimDetail,
  claimDetailLoaded,
  claimDetailFailed,
} = slice.actions;
export default slice.reducer;
