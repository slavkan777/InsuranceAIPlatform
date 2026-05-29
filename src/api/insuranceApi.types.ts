// Request/result contracts for the LOCAL mock API boundary.
// These shapes are the seam a future .NET backend will satisfy — see
// docs/architecture/FRONTEND_BACKEND_CONTRACT_READINESS_V0.1.md.
// There is intentionally NO network, NO base URL, NO fetch/axios here.

export interface ApprovalDraftInput {
  claimId: string;
  decision?: 'approve' | 'request' | 'reject' | 'escalate' | null;
  notes?: string;
}

/** Generic acknowledgement returned by mock write operations. */
export interface MockApiAck {
  ok: true;
  savedAt: string;
  /** Always present so callers/readers know this never hit a real service. */
  note: string;
}

export type ApprovalDraftResult = MockApiAck;
export type CustomerRequestResult = MockApiAck;

export interface MockAiRunResult {
  runId: string;
  status: 'succeeded' | 'failed';
}

// ---------------------------------------------------------------------------
// Command result shapes — returned by the 4 BFF write endpoints.
// Human-controlled only; no payout, no customer message, no upload.
// ---------------------------------------------------------------------------

/**
 * Canonical result from any BFF command endpoint (POST /api/claims/{id}/...).
 * Maps to C# CommandResult record.
 */
export interface CommandResult {
  success: boolean;
  commandId: string;
  claimId: string;
  status: string | null;
  auditEventId: number | null;
  outboxMessageId: number | null;
  correlationId: string;
  message: string;
  warnings: string[];
}

/** Body for POST /api/claims/{claimId}/approval-draft */
export interface SaveApprovalDraftBody {
  currentDecision?: string | null;
  notes?: string | null;
}

/** Body for POST /api/claims/{claimId}/human-decision */
export interface SubmitHumanDecisionBody {
  /** Must be one of: ApproveForReview | RejectForReview | NeedsMoreInformation | RequestDocuments */
  decision: string;
  notes?: string | null;
}

/** Body for POST /api/claims/{claimId}/missing-document-requests */
export interface RequestMissingDocumentBody {
  documentTitle: string;
  reason?: string | null;
}

/** Body for POST /api/claims/{claimId}/document-metadata */
export interface CreateDocumentMetadataBody {
  kind: string;
  title: string;
  docType?: string | null;
}

/** Body for POST /api/claims/{claimId}/documents/upload (DB-backed text content). */
export interface UploadDocumentContentBody {
  kind: string;
  title: string;
  docType?: string | null;
  /** Plain text content stored in nvarchar(max). No binary, no blob. */
  content: string;
}

/** Body for POST /api/claims (create synthetic claim row). */
export interface CreateClaimBody {
  customerId?: string | null;
  customerName?: string | null;
  policy?: string | null;
  policyId?: string | null;
  vehicle: string;
  vehicleVin?: string | null;
  eventType: string;
  /** ISO date "YYYY-MM-DD". */
  eventDate: string;
  location: string;
  description?: string | null;
}

/** Returned by POST /api/claims. */
export interface CreateClaimResult extends CommandResult {
  customerId: string;
  customer: string;
  vehicle: string;
}

/** Body for POST /api/claims/{claimId}/payout-simulation (DB-only). */
export interface CreatePayoutSimulationBody {
  amount: number;
  deductible: number;
  currency?: string | null;
  /** "Human" | "AI-advisory" | "Hybrid" */
  decisionSource?: string | null;
  sourceAiRunId?: string | null;
  notes?: string | null;
}

/** Returned by POST /api/claims/{claimId}/payout-simulation. */
export interface PayoutSimulationResultDto extends CommandResult {
  simulationId: number;
  amount: number;
  deductible: number;
  netPayoutAmount: number;
  currency: string;
  decisionSource: string;
  decisionActor: string;
  sourceAiRunId?: string | null;
  /** Always true — schema-level guarantee that no real money transfer occurred. */
  simulationOnly: true;
}

/** Returned by GET /api/claims/{claimId}/payout-simulations. */
export interface PayoutSimulationSummaryDto {
  id: number;
  claimId: string;
  status: string;
  amount: number;
  deductible: number;
  netPayoutAmount: number;
  currency: string;
  decisionSource: string;
  decisionActor: string;
  sourceAiRunId?: string | null;
  notes?: string | null;
  correlationId: string;
  createdAtUtc: string;
  confirmedAtUtc?: string | null;
  simulationOnly: boolean;
}

/** Returned by GET /api/customers (paginated synthetic directory). */
export interface CustomerSummaryDto {
  id: string;
  fullName: string;
  email: string;
  phone: string;
  addressLine: string;
  customerSince: string; // YYYY-MM-DD
  previousClaimsCount: number;
  isSynthetic: boolean;
}

export interface CustomerListResultDto {
  total: number;
  page: number;
  pageSize: number;
  items: CustomerSummaryDto[];
}

/** Body for POST /api/customers (create a new synthetic customer). */
export interface CreateCustomerBody {
  fullName: string;
  email?: string | null;
  phone?: string | null;
  addressLine?: string | null;
  customerSince?: string | null; // YYYY-MM-DD
}

/** Returned by POST /api/customers (create synthetic customer). */
export interface CreateCustomerResult {
  success: boolean;
  commandId: string;
  customerId: string;
  fullName: string;
  email: string;
  phone: string;
  addressLine: string;
  customerSince: string;
  isSynthetic: boolean;
  message: string;
}

// ---------------------------------------------------------------------------
// AI Analysis types — advisory only; IsAdvisoryOnly is always true.
// AI cannot approve payout, reject claim, accuse fraud, send customer messages,
// or change claim status. Human decision is always final.
// ---------------------------------------------------------------------------

export interface AiAnalysisFindingDto {
  id: string;
  category: string;
  text: string;
  severity: string; // "ok" | "warn" | "danger"
}

export interface AiAnalysisEvidenceDto {
  id: string;
  source: string;
  note: string;
  confidence: number;
}

export interface AiAnalysisRiskDto {
  id: string;
  label: string;
  weight: number;
}

export interface AiAnalysisGuardrailsDto {
  advisoryOnly: true;
  requiresHumanReview: true;
  canApprovePayout: false;
  canRejectClaim: false;
  canAccuseFraudFinal: false;
  canSendCustomerMessage: false;
  canChangeClaimStatus: false;
}

export interface AiAnalysisCostTraceDto {
  tokens: number;
  estimatedCost: number;
  currencyCode: string;
}

export interface AiAnalysisRecommendedActionDto {
  action: string;
  rationale: string;
  confidenceScore: number;
}

/**
 * BFF DTO for AI analysis results. isAdvisoryOnly is always true.
 * Notice is always "AI output is advisory only — human decision is final."
 */
export interface AiAnalysisDto {
  runId: string;
  claimId: string;
  providerMode: string;
  modelName: string;
  status: string; // "succeeded" | "blocked_unsafe" | "claim_not_found"
  summaryText: string;
  recommendedAction: AiAnalysisRecommendedActionDto;
  policyCoverageExplanation: string;
  riskLevel: string; // "low" | "moderate" | "high" | "unknown"
  confidenceScore: number;
  findings: AiAnalysisFindingDto[];
  evidence: AiAnalysisEvidenceDto[];
  risks: AiAnalysisRiskDto[];
  guardrails: AiAnalysisGuardrailsDto;
  costTrace: AiAnalysisCostTraceDto;
  correlationId: string;
  createdAtUtc: string; // ISO DateTime
  isAdvisoryOnly: true;
  notice: string;
}

/** Empty body for POST /api/claims/{claimId}/ai-analysis/run */
export interface AiAnalysisRunRequestDto {
  // intentionally empty — no parameters needed
}

// ---------------------------------------------------------------------------
// AI Decision (local-demo only) — records an auditable AI decision from the
// latest analysis run. Never authorises payout/reject/fraud/status change,
// never sends customer messages. Audit + outbox only.
// ---------------------------------------------------------------------------

/** Body for POST /api/claims/{claimId}/ai-decision */
export interface RecordAiDecisionBody {
  /** Optional human notes attached to the AI decision record. */
  notes?: string | null;
}

/** Returned by POST /api/claims/{claimId}/ai-decision — extends CommandResult with AI decision fields. */
export interface AiDecisionRecordedResult extends CommandResult {
  /** The AI run this decision was derived from. */
  aiRunId: string;
  /** Mode of the provider that produced the run (Mock | DeepSeek | Disabled). */
  providerMode: string;
  /** Model identifier. */
  modelName: string;
  /** AI's recommended action (advisory only). */
  recommendedAction: string;
  /** Risk level reported by the run. */
  riskLevel: string;
  /** Confidence score (0–100). */
  confidenceScore: number;
  /** Always true — kept explicit for clarity in the UI/audit. */
  isAdvisoryOnly: true;
  /** Source attribution marker — always "AI" for this endpoint. */
  source: 'AI';
}
