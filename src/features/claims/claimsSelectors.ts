import type { RootState } from '@/app/store';
import type { ClaimId } from '@/types/insurance';

export const selectClaimsState = (s: RootState) => s.claims;
export const selectClaimsQueue = (s: RootState) => s.claims.list;
export const selectSelectedClaimId = (s: RootState) => s.claims.selectedId;
export const selectClaimsFilters = (s: RootState) => s.claims.filters;
export const selectClaimsSearch = (s: RootState) => s.claims.search;
export const selectClaimsSegment = (s: RootState) => s.claims.segment;

export const selectClaimById = (id: ClaimId) => (s: RootState) =>
  s.claims.list.find((c) => c.id === id);

export const selectActiveClaim = (s: RootState) =>
  s.claims.list.find((c) => c.id === s.claims.selectedId);
