// AI / evidence domain contracts (frontend DTO-like). The AI layer is advisory only.
export type { ExtractedEntity } from './index';

/** 0–100 model confidence. */
export type ConfidenceScore = number;

export type AiRunStatus = 'idle' | 'running' | 'succeeded' | 'failed';

/** Current build runs in mock/demo only; `real` is a future backend-gated mode. */
export type AiProviderMode = 'mock' | 'demo' | 'real';

/** A document/source the AI cites as evidence. */
export type EvidenceSource = string;

export interface AiFinding {
  id?: string;
  text: string;
  detail: string;
  tone: 'danger' | 'warn' | 'good';
}

export interface AiRecommendation {
  action: string;
  rationale: string;
  confidence: ConfidenceScore;
  /** Always true in this product — the model proposes, the human decides. */
  advisoryOnly: true;
}

export interface ModelConfidenceBar {
  id: string;
  label: string;
  value: ConfidenceScore;
}
