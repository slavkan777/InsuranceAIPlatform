/**
 * Backend API client — real fetch calls to the .NET read-only API.
 *
 * - Base URL: VITE_INSURANCE_API_BASE_URL (default http://localhost:5284)
 * - All JSON is camelCase (ASP.NET default serialiser).
 * - Non-2xx responses are parsed as ApiErrorResponse and re-thrown as typed errors.
 * - NO secrets, NO write endpoints, NO external services beyond localhost:5284.
 */

import type { ClaimDetail, ClaimRow, DocumentChecklistItem, DamagePhoto } from '@/types';
import type { MockAiRunResult } from './insuranceApi.types';

// ---------------------------------------------------------------------------
// Configuration
// ---------------------------------------------------------------------------

const BASE_URL: string =
  (import.meta.env as unknown as Record<string, string>).VITE_INSURANCE_API_BASE_URL ??
  'http://localhost:5284';

// ---------------------------------------------------------------------------
// Backend DTO types (camelCase from ASP.NET serialiser)
// ---------------------------------------------------------------------------

interface ApiErrorResponse {
  code: string;
  message: string;
  traceId: string;
}

/** Typed error surfaced from a backend 4xx/5xx. Never exposes raw stack. */
export class BackendApiError extends Error {
  constructor(
    public readonly code: string,
    message: string,
    public readonly status: number,
    public readonly traceId: string,
  ) {
    super(message);
    this.name = 'BackendApiError';
  }
}

// ---------------------------------------------------------------------------
// Fetch helper
// ---------------------------------------------------------------------------

async function apiFetch<T>(path: string): Promise<T> {
  const url = `${BASE_URL}${path}`;
  const res = await fetch(url);
  if (!res.ok) {
    let errBody: ApiErrorResponse | null = null;
    try {
      errBody = (await res.json()) as ApiErrorResponse;
    } catch {
      // Non-JSON error body
    }
    throw new BackendApiError(
      errBody?.code ?? 'HTTP_ERROR',
      errBody?.message ?? `HTTP ${res.status} from ${url}`,
      res.status,
      errBody?.traceId ?? '',
    );
  }
  return res.json() as Promise<T>;
}

/** POST a JSON body to a command endpoint; returns parsed response or throws BackendApiError. */
async function apiPost<TBody, TResult>(path: string, body: TBody, idempotencyKey?: string): Promise<TResult> {
  const url = `${BASE_URL}${path}`;
  const headers: Record<string, string> = { 'Content-Type': 'application/json' };
  if (idempotencyKey) headers['Idempotency-Key'] = idempotencyKey;
  const res = await fetch(url, {
    method: 'POST',
    headers,
    body: JSON.stringify(body),
  });
  if (!res.ok) {
    let errBody: ApiErrorResponse | null = null;
    try { errBody = (await res.json()) as ApiErrorResponse; } catch { /* non-JSON */ }
    throw new BackendApiError(
      errBody?.code ?? 'HTTP_ERROR',
      errBody?.message ?? `HTTP ${res.status} from ${url}`,
      res.status,
      errBody?.traceId ?? '',
    );
  }
  return res.json() as Promise<TResult>;
}

// ---------------------------------------------------------------------------
// Backend DTO shapes (camelCase; mirror the .cs record field names)
// ---------------------------------------------------------------------------

interface ClaimListItemDto {
  id: string;
  customer: string;
  vehicle: string;
  eventType: string;
  status: string;
  documentsCount: string;
  aiStatus: string;
  risk: string;
  sla: string;
  nextAction: string;
  updated: string; // ISO DateTimeOffset
}

interface ClaimSummaryDto {
  totalActive: number;
  pendingReview: number;
  highRisk: number;
  avgSlaRemainingHours: number;
  processedToday: number;
  aiAnalysisRunning: number;
}

interface ClaimDetailsDto {
  id: string;
  customer: string;
  customerId: string;
  vehicle: string;
  vehicleVin: string;
  policy: string;
  policyId: string;
  eventType: string;
  eventDate: string; // DateOnly → "YYYY-MM-DD"
  location: string;
  description?: string | null;
  status: string;
  risk: string;
  riskScore: number;
  confidence: number;
  slaDeadline: string; // ISO DateTimeOffset
  documentsReceived: number;
  documentsTotal: number;
  missingDocument?: string | null;
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

interface ClaimDocumentDto {
  id: string;
  label: string;
  detail?: string | null;
  status: string; // "ok" | "warn" | "missing"
  type: string;   // "document" | "photo"
  confidence?: number | null;
}

interface AiFindingDto {
  id: string;
  category: string;
  text: string;
  severity: string; // "warn" | "ok" | "danger"
}

interface EvidenceSourceDto {
  id: string;
  source: string;
  text: string;
  confidence: number;
}

interface ExtractedEntityDto {
  field: string;
  value: string;
  source: string;
  confidence: number;
}

interface ConfidenceBreakdownItemDto {
  stage: string;
  confidence: number;
}

interface AiEvidenceDto {
  runId: string;
  modelConfidence: number;
  findings: AiFindingDto[];
  evidence: EvidenceSourceDto[];
  extractedEntities: ExtractedEntityDto[];
  modelConfidenceBreakdown: ConfidenceBreakdownItemDto[];
}

interface RiskFactorDto {
  id: string;
  label: string;
  contribution: number;
}

interface PipelineStageDto {
  stage: string;
  status: string;
}

interface RiskAssessmentDto {
  score: number;
  threshold: number;
  level: string;
  factors: RiskFactorDto[];
  pipeline: PipelineStageDto[];
}

interface PolicyCoverageDto {
  id: string;
  label: string;
  limit: string;
  deductible: string;
  applicable: boolean;
  note?: string | null;
}

interface PolicyCheckResultDto {
  covered: boolean;
  coverageType: string;
  validationNotes: string[];
  exclusionTriggered: boolean;
}

interface PolicyDto {
  policyId: string;
  productName: string;
  coverageBlocks: PolicyCoverageDto[];
  validation: PolicyCheckResultDto;
}

interface CommunicationEntryDto {
  date: string; // DateOnly → "YYYY-MM-DD"
  channel: string;
  summary: string;
}

interface CustomerDto {
  customerId: string;
  fullName: string;
  previousClaimsCount: number;
  customerSince: string; // DateOnly
  communicationHistory: CommunicationEntryDto[];
}

interface VehicleDto {
  make: string;
  model: string;
  year: number;
  vin: string;
  color?: string | null;
  mileage?: number | null;
}

interface CustomerVehicleContextDto {
  customer: CustomerDto;
  vehicle: VehicleDto;
}

interface AuditEventDto {
  time: string;
  actor: string;
  action: string;
  result: string;
}

interface CostDistributionItemDto {
  stage: string;
  cost: number;
}

interface AuditTraceDto {
  runId: string;
  traceId: string;
  model: string;
  tokens: number;
  cost: number;
  durationSec: number;
  events: AuditEventDto[];
  costDistribution: CostDistributionItemDto[];
}

interface HumanDecisionOptionDto {
  value: string;
  label: string;
  recommended: boolean;
  description?: string | null;
}

interface ApprovalDraftDto {
  claimId: string;
  currentDecision?: string | null;
  notes?: string | null;
  savedAt?: string | null;
  submitted: boolean;
  submittedAt?: string | null;
  availableOptions: HumanDecisionOptionDto[];
  aiRecommendation?: string | null;
  recommendedPayout: number;
}

interface DemoStepDto {
  step: number;
  title: string;
  caption: string;
  pdfRef?: string | null;
  route: string;
}

interface DemoScenarioDto {
  steps: DemoStepDto[];
  goldenClaimId: string;
}

// ---------------------------------------------------------------------------
// Mappers — backend DTO → frontend types
// ---------------------------------------------------------------------------

function mapClaimListItem(dto: ClaimListItemDto): ClaimRow {
  return {
    id: dto.id,
    customer: dto.customer,
    vehicle: dto.vehicle,
    eventType: dto.eventType,
    // Cast to ClaimStatus — backend sends same Ukrainian strings as frontend union
    status: dto.status as ClaimRow['status'],
    documentsCount: dto.documentsCount,
    aiStatus: dto.aiStatus as ClaimRow['aiStatus'],
    risk: dto.risk as ClaimRow['risk'],
    sla: dto.sla,
    nextAction: dto.nextAction,
    // updated from backend is ISO DateTimeOffset; format as relative string for UI
    updated: formatRelativeTime(dto.updated),
  };
}

/** Format ISO DateTimeOffset as "N год" / "N хв" relative time for the UI table. */
function formatRelativeTime(iso: string): string {
  try {
    const diffMs = Date.now() - new Date(iso).getTime();
    const diffMin = Math.round(diffMs / 60000);
    if (diffMin < 60) return `${diffMin} хв`;
    const diffH = Math.round(diffMin / 60);
    if (diffH < 24) return `${diffH} год`;
    return `${Math.round(diffH / 24)} дн`;
  } catch {
    return iso;
  }
}

function mapClaimDetail(dto: ClaimDetailsDto): ClaimDetail {
  // eventDate from backend: "YYYY-MM-DD" (DateOnly) → reformat to "DD.MM.YYYY" for UI
  const eventDateFormatted = formatDateOnly(dto.eventDate);
  // slaDeadline from backend: ISO DateTimeOffset → human-readable deadline string
  const slaDeadline = formatSlaDeadline(dto.slaDeadline);
  return {
    id: dto.id,
    customer: dto.customer,
    customerId: dto.customerId,
    vehicle: dto.vehicle,
    vehicleVin: dto.vehicleVin,
    policy: dto.policy,
    policyId: dto.policyId,
    eventType: dto.eventType,
    eventDate: eventDateFormatted,
    location: dto.location,
    description: dto.description ?? '',
    status: dto.status as ClaimDetail['status'],
    risk: dto.risk as ClaimDetail['risk'],
    riskScore: dto.riskScore,
    confidence: dto.confidence,
    slaDeadline,
    documentsReceived: dto.documentsReceived,
    documentsTotal: dto.documentsTotal,
    missingDocument: dto.missingDocument ?? '',
    estimate: dto.estimate,
    expectedBenchmark: dto.expectedBenchmark,
    deductible: dto.deductible,
    recommendedPayout: dto.recommendedPayout,
    traceId: dto.traceId,
    runId: dto.runId,
    tokens: dto.tokens,
    cost: dto.cost,
    durationSec: dto.durationSec,
  };
}

function formatDateOnly(iso: string): string {
  // "2026-05-18" → "18.05.2026"
  try {
    const [y, m, d] = iso.split('-');
    return `${d}.${m}.${y}`;
  } catch {
    return iso;
  }
}

function formatSlaDeadline(iso: string): string {
  try {
    const dt = new Date(iso);
    const now = new Date();
    if (
      dt.getFullYear() === now.getFullYear() &&
      dt.getMonth() === now.getMonth() &&
      dt.getDate() === now.getDate()
    ) {
      return `Сьогодні до ${dt.getHours().toString().padStart(2, '0')}:${dt.getMinutes().toString().padStart(2, '0')}`;
    }
    return formatDateOnly(iso.split('T')[0]);
  } catch {
    return iso;
  }
}

function mapDocuments(dtos: ClaimDocumentDto[]): DocumentChecklistItem[] {
  return dtos
    .filter((d) => d.type === 'document' || d.status === 'missing')
    .map((d) => ({
      id: d.id,
      label: d.label,
      detail: d.detail ?? undefined,
      status: d.status as DocumentChecklistItem['status'],
    }));
}

function mapPhotos(dtos: ClaimDocumentDto[]): DamagePhoto[] {
  return dtos
    .filter((d) => d.type === 'photo' || d.id.startsWith('photo'))
    .map((d) => ({
      id: d.id.replace('photo-', ''),
      label: d.label.replace('Фото — ', '').replace('Фото — ', ''),
      confidence: d.confidence ?? undefined,
      missing: d.status === 'missing',
    }));
}

// ---------------------------------------------------------------------------
// Public API — same function names/signatures as mockInsuranceApi
// ---------------------------------------------------------------------------

export const backendInsuranceApi = {
  async getClaimsQueue(): Promise<ClaimRow[]> {
    const dtos = await apiFetch<ClaimListItemDto[]>('/api/claims');
    return dtos.map(mapClaimListItem);
  },

  async getClaimById(claimId: string): Promise<ClaimDetail> {
    const dto = await apiFetch<ClaimDetailsDto>(`/api/claims/${claimId}`);
    return mapClaimDetail(dto);
  },

  async getClaimDocuments(claimId: string): Promise<DocumentChecklistItem[]> {
    const dtos = await apiFetch<ClaimDocumentDto[]>(`/api/claims/${claimId}/documents`);
    return mapDocuments(dtos);
  },

  async getClaimPhotos(claimId: string): Promise<DamagePhoto[]> {
    const dtos = await apiFetch<ClaimDocumentDto[]>(`/api/claims/${claimId}/documents`);
    return mapPhotos(dtos);
  },

  async getAiAnalysis(claimId: string) {
    const dto = await apiFetch<AiEvidenceDto>(`/api/claims/${claimId}/ai-evidence`);
    // Map to the shape mockInsuranceApi.getAiAnalysis returns
    const findings = dto.findings.map((f) => ({
      id: f.id,
      text: f.text,
      detail: f.text,
      // backend severity "warn"/"ok"/"danger" maps to frontend AiFinding tone
      tone: (f.severity === 'warn' ? 'warn' : f.severity === 'ok' ? 'good' : 'danger') as
        | 'warn'
        | 'good'
        | 'danger',
    }));
    const evidence = dto.evidence.map((e) => e.source);
    const modelConfidence = dto.modelConfidenceBreakdown.map((b, i) => ({
      id: `conf-${i}`,
      label: b.stage,
      value: b.confidence,
    }));
    const extractedEntities = dto.extractedEntities.map((e) => ({
      field: e.field,
      value: e.value,
      source: e.source,
      confidence: e.confidence,
    }));
    return { findings, evidence, modelConfidence, extractedEntities };
  },

  /** No backend endpoint for this — keep it mock-only (write-ish trigger action). */
  async runMockAiAnalysis(_claimId: string): Promise<MockAiRunResult> {
    return { runId: 'run_backend_na', status: 'succeeded' };
  },

  async getRiskReview(claimId: string) {
    const dto = await apiFetch<RiskAssessmentDto>(`/api/claims/${claimId}/risks`);
    const factors = dto.factors.map((f) => ({
      id: f.id,
      label: f.label,
      contribution: f.contribution,
    }));
    const pipeline = dto.pipeline.map((p, i) => ({
      id: `stage-${i}`,
      label: p.stage,
      status: mapPipelineStatus(p.status),
      duration: '',
    }));
    return {
      score: dto.score,
      threshold: dto.threshold,
      factors,
      pipeline,
    };
  },

  async getPolicyCoverage(claimId: string) {
    const dto = await apiFetch<PolicyDto>(`/api/claims/${claimId}/policy`);
    const blocks = dto.coverageBlocks.map((b) => ({
      id: b.id,
      title: b.label,
      limit: b.limit,
      deductible: b.deductible,
    }));
    const validation = dto.validation.validationNotes;
    return { blocks, validation };
  },

  async getCustomerVehicleContext(claimId: string) {
    const dto = await apiFetch<CustomerVehicleContextDto>(
      `/api/claims/${claimId}/customer-vehicle`,
    );
    const previousClaims: { id: string; label: string; date: string; amount: string }[] = []; // backend doesn't expose previous-claims list separately
    const communicationHistory = dto.customer.communicationHistory.map((c) => ({
      channel: c.channel,
      topic: c.summary,
      when: formatDateOnly(c.date),
    }));
    return { previousClaims, communicationHistory };
  },

  async getAuditTrace(claimId: string) {
    const dto = await apiFetch<AuditTraceDto>(`/api/claims/${claimId}/audit`);
    const events = dto.events.map((e) => ({
      time: e.time,
      actor: e.actor,
      action: e.action,
      result: e.result as 'OK' | 'WARN' | 'BLOCK',
    }));
    const distribution = dto.costDistribution.map((d, i) => ({
      id: `dist-${i}`,
      label: d.stage,
      value: `$${d.cost.toFixed(4)}`,
    }));
    return {
      runId: dto.runId,
      traceId: dto.traceId,
      model: dto.model,
      tokens: dto.tokens,
      cost: dto.cost,
      durationSec: dto.durationSec,
      events,
      distribution,
    };
  },

  async getClaimsSummary() {
    const dto = await apiFetch<ClaimSummaryDto>('/api/claims/summary');
    return {
      totalActive: dto.totalActive,
      pendingReview: dto.pendingReview,
      highRisk: dto.highRisk,
      avgSlaRemainingHours: dto.avgSlaRemainingHours,
      processedToday: dto.processedToday,
      aiAnalysisRunning: dto.aiAnalysisRunning,
    };
  },

  async getClaimApproval(claimId: string) {
    const dto = await apiFetch<ApprovalDraftDto>(`/api/claims/${claimId}/approval`);
    return {
      claimId: dto.claimId,
      currentDecision: (dto.currentDecision ?? null) as string | null,
      notes: (dto.notes ?? null) as string | null,
      savedAt: (dto.savedAt ?? null) as string | null,
      submitted: dto.submitted,
      submittedAt: (dto.submittedAt ?? null) as string | null,
      availableOptions: dto.availableOptions.map((o) => ({
        value: o.value,
        label: o.label,
        recommended: o.recommended,
        description: (o.description ?? null) as string | null,
      })),
      aiRecommendation: (dto.aiRecommendation ?? null) as string | null,
      recommendedPayout: Number(dto.recommendedPayout),
    };
  },

  async getDemoScenario() {
    const dto = await apiFetch<DemoScenarioDto>('/api/demo/scenario');
    return dto.steps.map((s) => ({
      step: s.step,
      title: s.title,
      caption: s.caption,
      pdfRef: s.pdfRef ?? '',
      route: s.route,
    }));
  },

  // ---- legacy mock-compatible writes (kept for backward compat) ----
  async saveApprovalDraft(
    _claimId: string,
    _draft: import('./insuranceApi.types').ApprovalDraftInput,
  ): Promise<import('./insuranceApi.types').ApprovalDraftResult> {
    return { ok: true, savedAt: new Date().toISOString(), note: 'backend-mode: use saveApprovalDraftCommand instead' };
  },
  async sendCustomerRequest(
    _claimId: string,
  ): Promise<import('./insuranceApi.types').CustomerRequestResult> {
    return { ok: true, savedAt: new Date().toISOString(), note: 'backend-mode: write deferred' };
  },

  // ---- BFF command endpoints (human-controlled; no payout, no customer msg, no upload) ----

  /**
   * POST /api/claims/{claimId}/approval-draft
   * Upserts the human adjuster's draft decision. Submitted stays false.
   */
  async saveApprovalDraftCommand(
    claimId: string,
    body: import('./insuranceApi.types').SaveApprovalDraftBody,
    idempotencyKey?: string,
  ): Promise<import('./insuranceApi.types').CommandResult> {
    return apiPost(`/api/claims/${claimId}/approval-draft`, body, idempotencyKey);
  },

  /**
   * POST /api/claims/{claimId}/human-decision
   * Submits the human decision (Submitted=true). Decision must be in the allowed set.
   * No payout, no customer message.
   */
  async submitHumanDecision(
    claimId: string,
    body: import('./insuranceApi.types').SubmitHumanDecisionBody,
    idempotencyKey?: string,
  ): Promise<import('./insuranceApi.types').CommandResult> {
    return apiPost(`/api/claims/${claimId}/human-decision`, body, idempotencyKey);
  },

  /**
   * POST /api/claims/{claimId}/missing-document-requests
   * Records an internal missing-document request. NO customer message sent.
   */
  async requestMissingDocument(
    claimId: string,
    body: import('./insuranceApi.types').RequestMissingDocumentBody,
    idempotencyKey?: string,
  ): Promise<import('./insuranceApi.types').CommandResult> {
    return apiPost(`/api/claims/${claimId}/missing-document-requests`, body, idempotencyKey);
  },

  /**
   * POST /api/claims/{claimId}/document-metadata
   * Creates a document metadata placeholder row. NO binary upload, NO blob.
   */
  async createDocumentMetadata(
    claimId: string,
    body: import('./insuranceApi.types').CreateDocumentMetadataBody,
    idempotencyKey?: string,
  ): Promise<import('./insuranceApi.types').CommandResult> {
    return apiPost(`/api/claims/${claimId}/document-metadata`, body, idempotencyKey);
  },

  // ---- AI Analysis endpoints (advisory only; never authorises payout/reject/fraud/status change) ----

  /**
   * GET /api/claims/{claimId}/ai-analysis
   * Returns the latest AI analysis run for a claim, or null if none exists (404 → null).
   * AI output is advisory only — human decision is always final.
   */
  async getClaimAiAnalysis(claimId: string): Promise<import('./insuranceApi.types').AiAnalysisDto | null> {
    try {
      return await apiFetch<import('./insuranceApi.types').AiAnalysisDto>(`/api/claims/${claimId}/ai-analysis`);
    } catch (e) {
      if (e instanceof BackendApiError && e.status === 404) return null;
      throw e;
    }
  },

  /**
   * POST /api/claims/{claimId}/ai-analysis/run
   * Triggers a new AI analysis run for a claim. Returns the result DTO.
   * AI output is advisory only — human decision is always final.
   * Throws BackendApiError on 4xx/5xx.
   */
  async runClaimAiAnalysis(claimId: string): Promise<import('./insuranceApi.types').AiAnalysisDto> {
    return apiPost<import('./insuranceApi.types').AiAnalysisRunRequestDto, import('./insuranceApi.types').AiAnalysisDto>(
      `/api/claims/${claimId}/ai-analysis/run`,
      {},
    );
  },
};

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

function mapPipelineStatus(
  s: string,
): 'done' | 'warn' | 'risk' | 'pending' {
  switch (s.toUpperCase()) {
    case 'OK':
      return 'done';
    case 'WARN':
      return 'warn';
    case 'BLOCK':
      return 'risk';
    default:
      return 'pending';
  }
}
