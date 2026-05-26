import type { RootState } from '@/app/store';

export const selectAiRunStatus = (s: RootState) => s.aiReview.status;
export const selectAiProgress = (s: RootState) => s.aiReview.progressPct;
export const selectSelectedAiEvidence = (s: RootState) => s.aiReview.selectedEvidence;
export const selectAiConfidenceFilter = (s: RootState) => s.aiReview.confidenceFilter;
export const selectAiLastError = (s: RootState) => s.aiReview.lastError;
