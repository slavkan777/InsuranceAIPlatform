import type { RootState } from '@/app/store';

export const selectActiveWorkspaceSection = (s: RootState) => s.claimWorkspace.activeSection;
export const selectWorkflowStep = (s: RootState) => s.claimWorkspace.workflowStep;
export const selectClaimDetail = (s: RootState) => s.claimWorkspace.claimDetail;
export const selectClaimDetailLoading = (s: RootState) => s.claimWorkspace.loading;
export const selectClaimDetailError = (s: RootState) => s.claimWorkspace.error;
export const selectClaimDetailApiMode = (s: RootState) => s.claimWorkspace.apiMode;

// Sub-resource selectors
export const selectWorkspaceDocuments = (s: RootState) => s.claimWorkspace.documents.data;
export const selectWorkspacePhotos = (s: RootState) => s.claimWorkspace.photos.data;
export const selectWorkspaceAiEvidence = (s: RootState) => s.claimWorkspace.aiEvidence.data;
export const selectWorkspaceRisks = (s: RootState) => s.claimWorkspace.risks.data;
export const selectWorkspacePolicy = (s: RootState) => s.claimWorkspace.policy.data;
export const selectWorkspaceCustomerVehicle = (s: RootState) => s.claimWorkspace.customerVehicle.data;
export const selectWorkspaceApprovalRead = (s: RootState) => s.claimWorkspace.approvalRead.data;
export const selectWorkspaceAudit = (s: RootState) => s.claimWorkspace.audit.data;

// Loading/error states
export const selectWorkspaceDocumentsLoading = (s: RootState) => s.claimWorkspace.documents.loading;
export const selectWorkspaceAiEvidenceLoading = (s: RootState) => s.claimWorkspace.aiEvidence.loading;
export const selectWorkspaceRisksLoading = (s: RootState) => s.claimWorkspace.risks.loading;
export const selectWorkspacePolicyLoading = (s: RootState) => s.claimWorkspace.policy.loading;
export const selectWorkspaceCustomerVehicleLoading = (s: RootState) => s.claimWorkspace.customerVehicle.loading;
export const selectWorkspaceApprovalReadLoading = (s: RootState) => s.claimWorkspace.approvalRead.loading;
export const selectWorkspaceAuditLoading = (s: RootState) => s.claimWorkspace.audit.loading;
