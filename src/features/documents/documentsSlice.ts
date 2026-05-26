import { createSlice, type PayloadAction } from '@reduxjs/toolkit';

type ReviewStatus = 'idle' | 'requesting' | 'sent' | 'failed';

interface DocumentsState {
  selectedDocumentId: string;
  reviewedIds: Record<string, boolean>;
  missingPhotoFlag: boolean;
  reviewStatus: ReviewStatus;
  reviewMessage?: string;
}

const initialState: DocumentsState = {
  selectedDocumentId: 'police',
  reviewedIds: {
    application: true,
    police: true,
    'photo-front': true,
    'photo-side': true,
    'policy-terms': true,
  },
  missingPhotoFlag: true,
  reviewStatus: 'idle',
};

const slice = createSlice({
  name: 'documents',
  initialState,
  reducers: {
    selectDocument(state, action: PayloadAction<string>) {
      state.selectedDocumentId = action.payload;
    },
    toggleReviewed(state, action: PayloadAction<string>) {
      state.reviewedIds[action.payload] = !state.reviewedIds[action.payload];
    },
    requestMissingPhoto(state) {
      state.reviewStatus = 'requesting';
      state.reviewMessage = undefined;
    },
    requestMissingPhotoSucceeded(state, action: PayloadAction<string>) {
      state.reviewStatus = 'sent';
      state.reviewMessage = action.payload;
    },
    requestMissingPhotoFailed(state, action: PayloadAction<string>) {
      state.reviewStatus = 'failed';
      state.reviewMessage = action.payload;
    },
  },
});

export const {
  selectDocument,
  toggleReviewed,
  requestMissingPhoto,
  requestMissingPhotoSucceeded,
  requestMissingPhotoFailed,
} = slice.actions;
export default slice.reducer;
