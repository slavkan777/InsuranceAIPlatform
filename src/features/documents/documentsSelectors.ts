import type { RootState } from '@/app/store';

export const selectSelectedDocumentId = (s: RootState) => s.documents.selectedDocumentId;
export const selectReviewedDocumentIds = (s: RootState) => s.documents.reviewedIds;
export const selectMissingEvidenceFlag = (s: RootState) => s.documents.missingPhotoFlag;
export const selectDocumentReviewStatus = (s: RootState) => s.documents.reviewStatus;
export const selectDocumentReviewMessage = (s: RootState) => s.documents.reviewMessage;
