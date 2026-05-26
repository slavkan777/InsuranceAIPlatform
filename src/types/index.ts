export type ClaimStatus =
  | 'В роботі'
  | 'Збір документів'
  | 'AI-обробка'
  | 'Високий ризик'
  | 'Готова'
  | 'Завершено';

export type RiskLevel = 'Низький' | 'Середній' | 'Високий';

export type AiStatus =
  | 'AI-перевірено'
  | 'Потрібна перевірка'
  | 'Очікує документи'
  | 'Обробляється'
  | 'Готова';

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
