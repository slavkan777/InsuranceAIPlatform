// Insurance domain contracts (frontend DTO-like). Canonical primitives live in `@/types`;
// this file groups them under the insurance domain and adds claim/policy/customer contracts.
import type {
  ClaimDetail,
  ClaimRow,
  DamagePhoto,
  DocumentChecklistItem,
  RiskFactor,
  RiskLevel,
} from './index';

export type { ClaimStatus, RiskLevel, AiStatus, ClaimRow, ClaimDetail, DamagePhoto, DocumentChecklistItem, RiskFactor } from './index';

export type ClaimId = string;
/** Full claim aggregate as the workspace consumes it. */
export type Claim = ClaimDetail;
/** Row projection for the claims queue/table. */
export type ClaimSummary = ClaimRow;
export type ClaimRiskLevel = RiskLevel;
export type ClaimPriority = 'low' | 'normal' | 'high' | 'critical';
export type ClaimDocument = DocumentChecklistItem;
export type ClaimPhoto = DamagePhoto;

export interface PolicyCoverage {
  id: string;
  title: string;
  limit: string;
  deductible: string;
}

export interface Customer {
  id: string;
  name: string;
  since: string;
  phoneMasked: string;
  emailMasked: string;
  address: string;
  riskProfile: string;
}

export interface Vehicle {
  label: string;
  vinMasked: string;
  mileageKm: string;
  color: string;
  year: string;
  insuredValue: string;
  riskCategory: string;
}

export interface RiskAssessment {
  score: number;
  threshold: number;
  level: ClaimRiskLevel;
  factors: RiskFactor[];
}

export interface DeterministicCheck {
  id: string;
  label: string;
  passed: boolean;
}

/** Governance gate: AI never auto-approves; human review is mandatory above thresholds. */
export interface HumanReviewRequirement {
  autoApprovalAllowed: boolean;
  humanReviewRequired: boolean;
  reasons: string[];
}
