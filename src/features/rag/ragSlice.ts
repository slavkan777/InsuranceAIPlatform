import { createSlice, type PayloadAction } from '@reduxjs/toolkit';
import type { RagAnswerDto, RagAuditEntryDto, RagInfrastructureStatus, RagSimilarClaimsResultDto } from '@/api/insuranceApi.types';

type AskStatus = 'idle' | 'loading' | 'succeeded' | 'failed';
type AuditStatus = 'idle' | 'loading' | 'ready' | 'error';
type InfraStatus = 'idle' | 'loading' | 'ready' | 'error';

export interface RagState {
  /** Current loading status for the ask call. */
  askStatus: AskStatus;
  /** The last RAG answer returned by the backend. */
  lastAnswer: RagAnswerDto | null;
  /** Error message when askStatus === 'failed'. */
  askError: string | undefined;
  /** The currently selected useCase button (drives which button appears active). */
  activeUseCase: string | null;
  /** Custom question text typed by the user when useCase === 'custom'. */
  customQuestion: string;
  /** Loading status for the similar-claims call. */
  similarClaimsStatus: AskStatus;
  /** The last similar-claims result. */
  similarClaims: RagSimilarClaimsResultDto | null;
  /** Error message when similarClaimsStatus === 'failed'. */
  similarClaimsError: string | undefined;
  /** Persisted audit history rows re-hydrated from the backend on mount. */
  auditHistory: RagAuditEntryDto[];
  /** Loading status for the audit-history fetch. */
  auditStatus: AuditStatus;
  /** Error message when auditStatus === 'error'. */
  auditError: string | undefined;
  /** The last infrastructure stack status fetched. */
  infrastructure: RagInfrastructureStatus | undefined;
  /** Loading status for the infrastructure fetch / reindex call. */
  infraStatus: InfraStatus;
  /** Error message when infraStatus === 'error'. */
  infraError: string | undefined;
}

const initialState: RagState = {
  askStatus: 'idle',
  lastAnswer: null,
  askError: undefined,
  activeUseCase: null,
  customQuestion: '',
  similarClaimsStatus: 'idle',
  similarClaims: null,
  similarClaimsError: undefined,
  auditHistory: [],
  auditStatus: 'idle',
  auditError: undefined,
  infrastructure: undefined,
  infraStatus: 'idle',
  infraError: undefined,
};

const slice = createSlice({
  name: 'rag',
  initialState,
  reducers: {
    /**
     * Dispatch to trigger the saga. Saga reads claimId + question + useCase
     * from the action payload.
     */
    askRag: {
      reducer(
        state,
        _action: PayloadAction<{ claimId: string; question: string; useCase: string }>,
      ) {
        state.askStatus = 'loading';
        state.askError = undefined;
      },
      prepare(claimId: string, question: string, useCase: string) {
        return { payload: { claimId, question, useCase } };
      },
    },

    ragAnswerReceived(state, action: PayloadAction<RagAnswerDto>) {
      state.askStatus = 'succeeded';
      state.lastAnswer = action.payload;
    },

    ragAskFailed(state, action: PayloadAction<string>) {
      state.askStatus = 'failed';
      state.askError = action.payload;
    },

    ragReset(state) {
      state.askStatus = 'idle';
      state.lastAnswer = null;
      state.askError = undefined;
      state.activeUseCase = null;
      state.similarClaimsStatus = 'idle';
      state.similarClaims = null;
      state.similarClaimsError = undefined;
    },

    /**
     * Dispatch to trigger the similar-claims saga. Saga reads claimId + topK
     * from the action payload.
     */
    fetchSimilarClaims: {
      reducer(
        state,
        _action: PayloadAction<{ claimId: string; topK: number }>,
      ) {
        state.similarClaimsStatus = 'loading';
        state.similarClaimsError = undefined;
      },
      prepare(claimId: string, topK = 5) {
        return { payload: { claimId, topK } };
      },
    },

    similarClaimsReceived(state, action: PayloadAction<RagSimilarClaimsResultDto>) {
      state.similarClaimsStatus = 'succeeded';
      state.similarClaims = action.payload;
    },

    similarClaimsFailed(state, action: PayloadAction<string>) {
      state.similarClaimsStatus = 'failed';
      state.similarClaimsError = action.payload;
    },

    setActiveUseCase(state, action: PayloadAction<string | null>) {
      state.activeUseCase = action.payload;
    },

    setCustomQuestion(state, action: PayloadAction<string>) {
      state.customQuestion = action.payload;
    },

    /**
     * Dispatch on mount to trigger the audit-history saga.
     * Saga reads claimId from the payload.
     */
    fetchAuditHistory: {
      reducer(
        state,
        _action: PayloadAction<{ claimId: string }>,
      ) {
        state.auditStatus = 'loading';
        state.auditError = undefined;
      },
      prepare(claimId: string) {
        return { payload: { claimId } };
      },
    },

    auditHistoryReceived(state, action: PayloadAction<RagAuditEntryDto[]>) {
      state.auditStatus = 'ready';
      state.auditHistory = action.payload;
    },

    auditHistoryFailed(state, action: PayloadAction<string>) {
      state.auditStatus = 'error';
      state.auditError = action.payload;
    },

    /**
     * Dispatch to trigger the infrastructure saga.
     * Saga reads claimId from the payload.
     */
    fetchInfrastructure: {
      reducer(
        state,
        _action: PayloadAction<{ claimId: string }>,
      ) {
        state.infraStatus = 'loading';
        state.infraError = undefined;
      },
      prepare(claimId: string) {
        return { payload: { claimId } };
      },
    },

    infrastructureReceived(state, action: PayloadAction<RagInfrastructureStatus>) {
      state.infraStatus = 'ready';
      state.infrastructure = action.payload;
    },

    infrastructureFailed(state, action: PayloadAction<string>) {
      state.infraStatus = 'error';
      state.infraError = action.payload;
    },

    /**
     * Dispatch to trigger the reindex saga.
     * Saga reads claimId from the payload; on success dispatches infrastructureReceived.
     */
    reindexInfrastructure: {
      reducer(
        state,
        _action: PayloadAction<{ claimId: string }>,
      ) {
        state.infraStatus = 'loading';
        state.infraError = undefined;
      },
      prepare(claimId: string) {
        return { payload: { claimId } };
      },
    },
  },
});

export const {
  askRag,
  ragAnswerReceived,
  ragAskFailed,
  ragReset,
  setActiveUseCase,
  setCustomQuestion,
  fetchSimilarClaims,
  similarClaimsReceived,
  similarClaimsFailed,
  fetchAuditHistory,
  auditHistoryReceived,
  auditHistoryFailed,
  fetchInfrastructure,
  infrastructureReceived,
  infrastructureFailed,
  reindexInfrastructure,
} = slice.actions;

export default slice.reducer;
