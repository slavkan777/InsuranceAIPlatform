import type { RootState } from '@/app/store';

export const selectApprovalDecision = (s: RootState) => s.approval.selectedDecision;
export const selectReviewerNotes = (s: RootState) => s.approval.reviewerNotes;
export const selectApprovalChecklist = (s: RootState) => s.approval.checklist;
export const selectApprovalDraftStatus = (s: RootState) => s.approval.draftStatus;
export const selectApprovalDraftMessage = (s: RootState) => s.approval.draftMessage;
