// Audit / cost governance contracts (frontend DTO-like). Telemetry is observability, not decoration.
import type { AuditRow, CostLine } from './index';

export type { AuditRow, CostLine } from './index';

/** One row in the audit trail. */
export type AuditEvent = AuditRow;

export interface TokenUsage {
  tokens: number;
}

export interface ModelTrace {
  model: string;
  traceId: string;
  runId: string;
}

export interface CostTrace {
  cost: number;
  durationSec: number;
  distribution: CostLine[];
}

export interface AuditRun extends ModelTrace, TokenUsage {
  cost: number;
  durationSec: number;
  success: boolean;
  events: AuditEvent[];
}
