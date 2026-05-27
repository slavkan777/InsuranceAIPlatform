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
