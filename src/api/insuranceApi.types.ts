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
