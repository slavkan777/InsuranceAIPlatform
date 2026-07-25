// ---------------------------------------------------------------------------
// Claim contract: stable English codes
// ---------------------------------------------------------------------------
// These are the canonical API/domain values. The product is English-only and the
// backend normalizes any legacy Ukrainian values still present in the database to
// codes at the API boundary, so the browser only ever sees codes.
//
// All *logic* normalizes to a code first via `@/utils/claimContract`; display labels
// live there too and must never be compared against.
// ---------------------------------------------------------------------------

export type ClaimStatusCode =
  | 'New'
  | 'InProgress'
  | 'CollectingDocuments'
  | 'AiProcessing'
  | 'HighRisk'
  | 'Ready'
  | 'Completed';

/** Claim status as carried on the wire. */
export type ClaimStatus = ClaimStatusCode;

export type RiskLevelCode = 'Undetermined' | 'Low' | 'Medium' | 'High';

/** Risk level as carried on the wire. */
export type RiskLevel = RiskLevelCode;

export type EventTypeCode =
  | 'RoadAccident'
  | 'Parking'
  | 'Collision'
  | 'Damage'
  | 'Glass'
  | 'Theft';

export type AiStatusCode =
  | 'AwaitingAi'
  | 'AiVerified'
  | 'NeedsReview'
  | 'AwaitingDocuments'
  | 'Processing'
  | 'Ready';

/** AI status as carried on the wire. */
export type AiStatus = AiStatusCode;

export interface ClaimRow {
  id: string;
  customer: string;
  vehicle: string;
  eventType: string;
  status: ClaimStatus;
  documentsCount: string;
  aiStatus: AiStatus;
  risk: RiskLevel;
  sla: string;
  nextAction: string;
  updated: string;
}

export interface DamagePhoto {
  id: string;
  label: string;
  confidence?: number;
  missing?: boolean;
}

export interface DocumentChecklistItem {
  id: string;
  label: string;
  detail?: string;
  status: 'ok' | 'warn' | 'missing';
}

export interface RiskFactor {
  id: string;
  label: string;
  contribution: number;
}

export interface CostLine {
  id: string;
  label: string;
  value: string;
}

export interface ExtractedEntity {
  field: string;
  value: string;
  source: string;
  confidence: number;
}

export interface AuditRow {
  time: string;
  actor: string;
  action: string;
  result: 'OK' | 'WARN' | 'BLOCK';
}

export interface DemoStep {
  step: number;
  title: string;
  caption: string;
  pdfRef: string;
  route: string;
}

export interface ClaimDetail {
  id: string;
  customer: string;
  customerId: string;
  vehicle: string;
  vehicleVin: string;
  policy: string;
  policyId: string;
  eventType: string;
  eventDate: string;
  location: string;
  description: string;
  status: ClaimStatus;
  risk: RiskLevel;
  riskScore: number;
  confidence: number;
  slaDeadline: string;
  documentsReceived: number;
  documentsTotal: number;
  missingDocument: string;
  estimate: number;
  expectedBenchmark: number;
  deductible: number;
  recommendedPayout: number;
  traceId: string;
  runId: string;
  tokens: number;
  cost: number;
  durationSec: number;
}
