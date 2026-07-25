import type {
  AiStatusCode,
  ClaimStatusCode,
  EventTypeCode,
  RiskLevelCode,
} from '@/types';

/**
 * Claim contract helper — code normalization + English display labels.
 *
 * Rules:
 *   - Codes are the domain values. All filtering / segment / tone logic compares CODES.
 *   - Labels are display-only and must never be used in logic.
 *   - An unrecognised value normalizes to `null` (explicitly unknown). It is never
 *     coerced into a real state; it renders with a muted tone and keeps its raw text.
 *
 * Legacy Ukrainian values that may still sit in the database are normalized to codes
 * SERVER-SIDE, at the API boundary (see `ClaimContractCodes` +
 * `HybridClaimReadService`). That keeps the compatibility mapping off the wire and out
 * of the browser bundle, so no Cyrillic ships to the client.
 */

export type PillTone = 'danger' | 'warn' | 'good' | 'info' | 'muted';

/** Sentinel meaning "no filtering on this field". Shared by every filter dropdown. */
export const FILTER_ALL = 'All';

/** Segment chips on the claims list. UI-level grouping, expressed as codes. */
export type ClaimSegmentCode =
  | 'All'
  | 'Accident'
  | 'HighRisk'
  | 'AwaitingAi'
  | 'AwaitingDecision';

// ---------------------------------------------------------------------------
// Status
// ---------------------------------------------------------------------------

const STATUS_CODES: readonly ClaimStatusCode[] = [
  'New',
  'InProgress',
  'CollectingDocuments',
  'AiProcessing',
  'HighRisk',
  'Ready',
  'Completed',
];

export const CLAIM_STATUS_LABELS: Readonly<Record<ClaimStatusCode, string>> = {
  New: 'New',
  InProgress: 'In progress',
  CollectingDocuments: 'Collecting documents',
  AiProcessing: 'AI processing',
  HighRisk: 'High risk',
  Ready: 'Ready',
  Completed: 'Completed',
};

/** Normalize a wire/mock claim status to its code, or `null` when unrecognised. */
export function toClaimStatusCode(raw: string | null | undefined): ClaimStatusCode | null {
  const v = (raw ?? '').trim();
  if (!v) return null;
  if ((STATUS_CODES as readonly string[]).includes(v)) return v as ClaimStatusCode;
  return null;
}

/** English label for display. Unknown values keep their raw text so nothing is lost. */
export function claimStatusLabel(raw: string | null | undefined): string {
  const code = toClaimStatusCode(raw);
  return code ? CLAIM_STATUS_LABELS[code] : (raw ?? '').trim();
}

export function claimStatusTone(raw: string | null | undefined): PillTone {
  switch (toClaimStatusCode(raw)) {
    case 'HighRisk':
      return 'danger';
    case 'Ready':
      return 'good';
    case 'AiProcessing':
      return 'info';
    case 'CollectingDocuments':
      return 'warn';
    default:
      return 'muted';
  }
}

// ---------------------------------------------------------------------------
// Risk
// ---------------------------------------------------------------------------

const RISK_CODES: readonly RiskLevelCode[] = ['Undetermined', 'Low', 'Medium', 'High'];

export const RISK_LEVEL_LABELS: Readonly<Record<RiskLevelCode, string>> = {
  Undetermined: 'Undetermined',
  Low: 'Low',
  Medium: 'Medium',
  High: 'High',
};

/** Normalize a wire/mock risk level to its code, or `null` when unrecognised. */
export function toRiskLevelCode(raw: string | null | undefined): RiskLevelCode | null {
  const v = (raw ?? '').trim();
  if (!v) return null;
  if ((RISK_CODES as readonly string[]).includes(v)) return v as RiskLevelCode;
  return null;
}

export function riskLevelLabel(raw: string | null | undefined): string {
  const code = toRiskLevelCode(raw);
  return code ? RISK_LEVEL_LABELS[code] : (raw ?? '').trim();
}

export function riskLevelTone(raw: string | null | undefined): PillTone {
  switch (toRiskLevelCode(raw)) {
    case 'High':
      return 'danger';
    case 'Medium':
      return 'warn';
    case 'Low':
      return 'good';
    // 'Undetermined' and unrecognised values stay muted — never presented as a safe "good".
    default:
      return 'muted';
  }
}

// ---------------------------------------------------------------------------
// AI status
// ---------------------------------------------------------------------------

const AI_STATUS_CODES: readonly AiStatusCode[] = [
  'AwaitingAi',
  'AiVerified',
  'NeedsReview',
  'AwaitingDocuments',
  'Processing',
  'Ready',
];

export const AI_STATUS_LABELS: Readonly<Record<AiStatusCode, string>> = {
  AwaitingAi: 'Awaiting AI',
  AiVerified: 'AI verified',
  NeedsReview: 'Needs review',
  AwaitingDocuments: 'Awaiting documents',
  Processing: 'Processing',
  Ready: 'Ready',
};

/** Normalize a wire/mock AI status to its code, or `null` when unrecognised. */
export function toAiStatusCode(raw: string | null | undefined): AiStatusCode | null {
  const v = (raw ?? '').trim();
  if (!v) return null;
  if ((AI_STATUS_CODES as readonly string[]).includes(v)) return v as AiStatusCode;
  return null;
}

export function aiStatusLabel(raw: string | null | undefined): string {
  const code = toAiStatusCode(raw);
  return code ? AI_STATUS_LABELS[code] : (raw ?? '').trim();
}

export function aiStatusTone(raw: string | null | undefined): PillTone {
  switch (toAiStatusCode(raw)) {
    case 'AiVerified':
      return 'good';
    case 'NeedsReview':
      return 'warn';
    case 'Processing':
      return 'info';
    default:
      return 'muted';
  }
}

// ---------------------------------------------------------------------------
// Event type
// ---------------------------------------------------------------------------

const EVENT_TYPE_CODES: readonly EventTypeCode[] = [
  'RoadAccident',
  'Parking',
  'Collision',
  'Damage',
  'Glass',
  'Theft',
];

export const EVENT_TYPE_LABELS: Readonly<Record<EventTypeCode, string>> = {
  RoadAccident: 'Road accident',
  Parking: 'Parking',
  Collision: 'Collision',
  Damage: 'Damage',
  Glass: 'Glass',
  Theft: 'Theft',
};

/** Normalize a wire/mock event type to its code, or `null` when unrecognised. */
export function toEventTypeCode(raw: string | null | undefined): EventTypeCode | null {
  const v = (raw ?? '').trim();
  if (!v) return null;
  if ((EVENT_TYPE_CODES as readonly string[]).includes(v)) return v as EventTypeCode;
  return null;
}

export function eventTypeLabel(raw: string | null | undefined): string {
  const code = toEventTypeCode(raw);
  return code ? EVENT_TYPE_LABELS[code] : (raw ?? '').trim();
}

// ---------------------------------------------------------------------------
// SLA
// ---------------------------------------------------------------------------

/**
 * Canonical token for a breached SLA. The backend emits it verbatim
 * (`HybridClaimReadService.FormatSla`) and the mock layer mirrors it, so the
 * overdue check never depends on a translated display string.
 */
export const SLA_OVERDUE = 'Overdue';

/** True when the SLA string represents a breached deadline. */
export function isSlaOverdue(sla: string | null | undefined): boolean {
  return (sla ?? '').trim() === SLA_OVERDUE;
}
