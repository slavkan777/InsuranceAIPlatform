import type { RootState } from '@/app/store';

export const selectActiveWorkspaceSection = (s: RootState) => s.claimWorkspace.activeSection;
export const selectWorkflowStep = (s: RootState) => s.claimWorkspace.workflowStep;
export const selectClaimDetail = (s: RootState) => s.claimWorkspace.claimDetail;
export const selectClaimDetailLoading = (s: RootState) => s.claimWorkspace.loading;
export const selectClaimDetailError = (s: RootState) => s.claimWorkspace.error;
export const selectClaimDetailApiMode = (s: RootState) => s.claimWorkspace.apiMode;
