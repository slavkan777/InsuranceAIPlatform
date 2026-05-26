import type { RootState } from '@/app/store';

export const selectActiveWorkspaceSection = (s: RootState) => s.claimWorkspace.activeSection;
export const selectWorkflowStep = (s: RootState) => s.claimWorkspace.workflowStep;
