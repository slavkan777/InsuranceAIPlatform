import { createSlice, type PayloadAction } from '@reduxjs/toolkit';

type DraftStatus = 'idle' | 'saving' | 'saved' | 'sending' | 'sent' | 'failed';

interface ApprovalState {
  selectedDecision: 'approve' | 'request' | 'reject' | 'escalate' | null;
  reviewerNotes: string;
  checklist: Record<string, boolean>;
  draftStatus: DraftStatus;
  draftMessage?: string;
}

const initialState: ApprovalState = {
  selectedDecision: 'request',
  reviewerNotes:
    'Запрошуємо клієнта надати фото пошкодження заднього бампера. AI confidence 78%.',
  checklist: {
    coverage: true,
    'docs-reviewed': true,
    risk: true,
    'docs-missing': true,
    amount: false,
    expert: false,
  },
  draftStatus: 'idle',
};

const slice = createSlice({
  name: 'approval',
  initialState,
  reducers: {
    setDecision(state, action: PayloadAction<ApprovalState['selectedDecision']>) {
      state.selectedDecision = action.payload;
    },
    setNotes(state, action: PayloadAction<string>) {
      state.reviewerNotes = action.payload;
    },
    toggleChecklistItem(state, action: PayloadAction<string>) {
      state.checklist[action.payload] = !state.checklist[action.payload];
    },
    saveDraft(state) {
      state.draftStatus = 'saving';
      state.draftMessage = undefined;
    },
    draftSaved(state) {
      state.draftStatus = 'saved';
      state.draftMessage = 'Чернетку збережено. Доступна в audit trail.';
    },
    sendRequestToCustomer(state) {
      state.draftStatus = 'sending';
      state.draftMessage = undefined;
    },
    requestSent(state) {
      state.draftStatus = 'sent';
      state.draftMessage = 'Запит надіслано клієнту через SMS + email.';
    },
    draftFailed(state, action: PayloadAction<string>) {
      state.draftStatus = 'failed';
      state.draftMessage = action.payload;
    },
  },
});

export const {
  setDecision,
  setNotes,
  toggleChecklistItem,
  saveDraft,
  draftSaved,
  sendRequestToCustomer,
  requestSent,
  draftFailed,
} = slice.actions;
export default slice.reducer;
