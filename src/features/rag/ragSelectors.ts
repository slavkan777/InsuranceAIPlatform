import type { RootState } from '@/app/store';

export const selectRagAskStatus = (s: RootState) => s.rag.askStatus;
export const selectRagLastAnswer = (s: RootState) => s.rag.lastAnswer;
export const selectRagAskError = (s: RootState) => s.rag.askError;
export const selectRagActiveUseCase = (s: RootState) => s.rag.activeUseCase;
export const selectRagCustomQuestion = (s: RootState) => s.rag.customQuestion;

export const selectSimilarClaimsStatus = (s: RootState) => s.rag.similarClaimsStatus;
export const selectSimilarClaims = (s: RootState) => s.rag.similarClaims;
export const selectSimilarClaimsError = (s: RootState) => s.rag.similarClaimsError;

export const selectRagAuditHistory = (s: RootState) => s.rag.auditHistory;
export const selectRagAuditStatus = (s: RootState) => s.rag.auditStatus;

export const selectRagInfrastructure = (s: RootState) => s.rag.infrastructure;
export const selectRagInfraStatus = (s: RootState) => s.rag.infraStatus;
