import { createSlice, type PayloadAction } from '@reduxjs/toolkit';

/**
 * Transient action feedback — toast/inline messages shown after a user action
 * succeeds, fails, or is blocked. State is in-memory only and dismissable.
 */

export type FeedbackTone = 'success' | 'warning' | 'error' | 'info';

export interface FeedbackToast {
  id: string;
  tone: FeedbackTone;
  title: string;
  detail?: string;
  /** ISO timestamp of when this toast was created (for sorting/expiry). */
  createdAt: string;
}

interface UiFeedbackState {
  toasts: FeedbackToast[];
}

const initialState: UiFeedbackState = {
  toasts: [],
};

let toastCounter = 0;

const slice = createSlice({
  name: 'uiFeedback',
  initialState,
  reducers: {
    pushToast(
      state,
      action: PayloadAction<{
        tone: FeedbackTone;
        title: string;
        detail?: string;
      }>,
    ) {
      toastCounter += 1;
      state.toasts.push({
        id: `toast-${Date.now()}-${toastCounter}`,
        tone: action.payload.tone,
        title: action.payload.title,
        detail: action.payload.detail,
        createdAt: new Date().toISOString(),
      });
    },
    dismissToast(state, action: PayloadAction<string>) {
      state.toasts = state.toasts.filter((t) => t.id !== action.payload);
    },
    clearToasts(state) {
      state.toasts = [];
    },
  },
});

export const { pushToast, dismissToast, clearToasts } = slice.actions;
export default slice.reducer;
