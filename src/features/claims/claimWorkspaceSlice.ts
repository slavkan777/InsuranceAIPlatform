import { createSlice, type PayloadAction } from '@reduxjs/toolkit';
import type { ClaimDetail, DocumentChecklistItem, DamagePhoto, RiskFactor, ExtractedEntity, AuditRow, CostLine } from '@/types';
import type { ClaimsApiMode } from './claimsSlice';

type ActiveSection =
  | 'overview'
  | 'documents'
  | 'ai-evidence'
  | 'risks'
  | 'approval'
  | 'audit'
  | 'policy'
  | 'customer';

type WorkflowStep =
  | 'registration'
  | 'documents'
  | 'ai-analysis'
  | 'risk-review'
  | 'human-decision'
  | 'completion';

// ---------------------------------------------------------------------------
// Sub-resource data shapes (typed to match both mock and backend mappers)
// ---------------------------------------------------------------------------

export interface AiFinding {
  id: string;
  text: string;
  detail: string;
  tone: 'warn' | 'good' | 'danger';
}

export interface ModelConfidenceItem {
  id: string;
  label: string;
  value: number;
}

export interface AiEvidenceData {
  findings: AiFinding[];
  evidence: string[];
  modelConfidence: ModelConfidenceItem[];
  extractedEntities: ExtractedEntity[];
}

export interface PipelineStep {
  id: string;
  label: string;
  status: 'done' | 'warn' | 'risk' | 'pending';
  duration: string;
}

export interface RiskReviewData {
  score: number;
  threshold: number;
  factors: RiskFactor[];
  pipeline: PipelineStep[];
}

export interface PolicyCoverageBlock {
  id: string;
  title: string;
  limit: string;
  deductible: string;
}

export interface PolicyData {
  blocks: PolicyCoverageBlock[];
  validation: string[];
}

export interface PreviousClaim {
  id: string;
  label: string;
  date: string;
  amount: string;
}

export interface CommunicationEntry {
  channel: string;
  topic: string;
  when: string;
}

export interface CustomerVehicleData {
  previousClaims: PreviousClaim[];
  communicationHistory: CommunicationEntry[];
}

export interface ApprovalOption {
  value: string;
  label: string;
  recommended: boolean;
  description: string | null;
}

export interface ApprovalReadData {
  claimId: string;
  currentDecision: string | null;
  notes: string | null;
  savedAt: string | null;
  submitted: boolean;
  submittedAt: string | null;
  availableOptions: ApprovalOption[];
  aiRecommendation: string | null;
  recommendedPayout: number;
}

export interface AuditData {
  runId: string;
  traceId: string;
  model: string;
  tokens: number;
  cost: number;
  durationSec: number;
  events: AuditRow[];
  distribution: CostLine[];
}

// ---------------------------------------------------------------------------
// Sub-resource async state wrapper
// ---------------------------------------------------------------------------

interface AsyncResource<T> {
  data: T | null;
  loading: boolean;
  error: string | null;
  apiMode: ClaimsApiMode;
}

function initAsync<T>(): AsyncResource<T> {
  return { data: null, loading: false, error: null, apiMode: 'mock' };
}

// ---------------------------------------------------------------------------
// Slice state
// ---------------------------------------------------------------------------

interface ClaimWorkspaceState {
  activeSection: ActiveSection;
  workflowStep: WorkflowStep;
  // --- async claim detail load ---
  claimDetail: ClaimDetail | null;
  loading: boolean;
  error: string | null;
  apiMode: ClaimsApiMode;
  // --- sub-resources (loaded after claimDetail) ---
  documents: AsyncResource<DocumentChecklistItem[]>;
  photos: AsyncResource<DamagePhoto[]>;
  aiEvidence: AsyncResource<AiEvidenceData>;
  risks: AsyncResource<RiskReviewData>;
  policy: AsyncResource<PolicyData>;
  customerVehicle: AsyncResource<CustomerVehicleData>;
  approvalRead: AsyncResource<ApprovalReadData>;
  audit: AsyncResource<AuditData>;
}

const initialState: ClaimWorkspaceState = {
  activeSection: 'overview',
  workflowStep: 'human-decision',
  claimDetail: null,
  loading: false,
  error: null,
  apiMode: 'mock',
  documents: initAsync(),
  photos: initAsync(),
  aiEvidence: initAsync(),
  risks: initAsync(),
  policy: initAsync(),
  customerVehicle: initAsync(),
  approvalRead: initAsync(),
  audit: initAsync(),
};

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

type SubKey = keyof Pick<
  ClaimWorkspaceState,
  'documents' | 'photos' | 'aiEvidence' | 'risks' | 'policy' | 'customerVehicle' | 'approvalRead' | 'audit'
>;

// ---------------------------------------------------------------------------
// Slice
// ---------------------------------------------------------------------------

const slice = createSlice({
  name: 'claimWorkspace',
  initialState,
  reducers: {
    setSection(state, action: PayloadAction<ActiveSection>) {
      state.activeSection = action.payload;
    },
    setWorkflowStep(state, action: PayloadAction<WorkflowStep>) {
      state.workflowStep = action.payload;
    },
    // --- async claim detail load ---
    loadClaimDetail(state, _action: PayloadAction<string>) {
      state.loading = true;
      state.error = null;
    },
    claimDetailLoaded(
      state,
      action: PayloadAction<{ detail: ClaimDetail; mode: ClaimsApiMode }>,
    ) {
      state.loading = false;
      state.claimDetail = action.payload.detail;
      state.apiMode = action.payload.mode;
      state.error = null;
    },
    claimDetailFailed(
      state,
      action: PayloadAction<{ error: string; fallback: ClaimDetail | null }>,
    ) {
      state.loading = false;
      state.claimDetail = action.payload.fallback;
      state.error = action.payload.error;
      state.apiMode = 'mock-fallback';
    },
    // --- generic sub-resource actions ---
    subResourceRequested(state, action: PayloadAction<SubKey>) {
      state[action.payload].loading = true;
      state[action.payload].error = null;
    },
    subResourceLoaded<K extends SubKey>(
      state: ClaimWorkspaceState,
      action: PayloadAction<{ key: K; data: ClaimWorkspaceState[K]['data']; mode: ClaimsApiMode }>,
    ) {
      const { key, data, mode } = action.payload;
      (state[key] as AsyncResource<unknown>).loading = false;
      (state[key] as AsyncResource<unknown>).data = data;
      (state[key] as AsyncResource<unknown>).apiMode = mode;
      (state[key] as AsyncResource<unknown>).error = null;
    },
    subResourceFailed<K extends SubKey>(
      state: ClaimWorkspaceState,
      action: PayloadAction<{ key: K; error: string; fallback: ClaimWorkspaceState[K]['data'] }>,
    ) {
      const { key, error, fallback } = action.payload;
      (state[key] as AsyncResource<unknown>).loading = false;
      (state[key] as AsyncResource<unknown>).data = fallback;
      (state[key] as AsyncResource<unknown>).error = error;
      (state[key] as AsyncResource<unknown>).apiMode = 'mock-fallback';
    },
  },
});

export const {
  setSection,
  setWorkflowStep,
  loadClaimDetail,
  claimDetailLoaded,
  claimDetailFailed,
  subResourceRequested,
  subResourceLoaded,
  subResourceFailed,
} = slice.actions;
export default slice.reducer;
