// LOCAL mock API boundary — the single seam between UI/workflows and synthetic data.
//
// HARD BOUNDARY: every function here is local-only. No fetch, no axios, no WebSocket,
// no backend base URL, no API key, no real provider. Functions return synthetic data
// via Promise.resolve so they are drop-in replaceable by a real .NET client later
// WITHOUT changing the calling UI/sagas (same async signatures).
import { claimRows, goldenClaim } from '@/data/mock/claims';
import {
  aiPipelineSteps,
  auditTrail,
  communicationHistory,
  costDistribution,
  damagePhotos,
  documentsChecklist,
  evidenceTabs,
  extractedEntities,
  keyFindings,
  modelConfidence,
  policyCoverageBlocks,
  policyValidation,
  previousClaims,
  riskFactors,
  demoSteps,
} from '@/data/mock/claim-1006';
import type { ClaimDetail, ClaimRow, DamagePhoto, DocumentChecklistItem } from '@/types';
import type {
  AiAnalysisDto,
  AiDecisionRecordedResult,
  ApprovalDraftInput,
  ApprovalDraftResult,
  CommandResult,
  CreateClaimBody,
  CreateClaimResult,
  CreateCustomerBody,
  CreateCustomerResult,
  CreateDocumentMetadataBody,
  CreatePayoutSimulationBody,
  CustomerListResultDto,
  CustomerRequestResult,
  CustomerSummaryDto,
  MockAiRunResult,
  PayoutSimulationResultDto,
  PayoutSimulationSummaryDto,
  RecordAiDecisionBody,
  RequestMissingDocumentBody,
  SaveApprovalDraftBody,
  SubmitHumanDecisionBody,
  UploadDocumentContentBody,
} from './insuranceApi.types';

const MOCK_NOTE = 'local mock · synthetic data · no network';

/** In-memory counter for synthetic CLM-MOCK-#### ids used by mock-mode createClaim. */
let mockNextClaimNumber = 1000;
/** In-memory counter for synthetic CUST-MOCK-T#### ids used by mock-mode createCustomer. */
let mockNextCustomerNumber = 5;
/**
 * In-memory list of claims created via mock-mode createClaim. Used so that
 * mock-mode getClaimsQueue and getClaimById can serve a freshly-created mock
 * claim's detail with that claim's own id/customer/vehicle (no CLM-1006 leak).
 */
const mockExtraClaims: ClaimRow[] = [];
/**
 * Mock-mode in-memory directory. Seeded with 5 static rows; expanded each time
 * mock createCustomer is called (so the catalog visibly grows in mock mode too).
 */
const mockCustomerDirectory: CustomerSummaryDto[] = Array.from({ length: 5 }).map((_, i) => ({
  id: `CUST-MOCK-${(i + 1).toString().padStart(4, '0')}`,
  fullName: `Mock Customer ${i + 1}`,
  email: `mock${i + 1}@example.invalid`,
  phone: '+1-555-0100',
  addressLine: 'Local sandbox, no real address',
  customerSince: '2024-01-01',
  previousClaimsCount: i,
  isSynthetic: true,
}));

function delay(ms: number): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

/**
 * The mock insurance API. Read getters resolve immediately; mock write/run
 * operations use a small local delay to mirror the async shape a real client
 * would have (sagas already `yield call(...)` these).
 */
export const mockInsuranceApi = {
  // ---- reads (claim domain) ----
  async getClaimsQueue(): Promise<ClaimRow[]> {
    return [...mockExtraClaims, ...claimRows];
  },
  async getClaimById(claimId: string): Promise<ClaimDetail> {
    // CLM-1006 = the rich golden demo claim. Anything else: synthesize a bare
    // detail from the mock list row (or from in-memory mock-created entries),
    // keeping the requested claimId AS THE ID. Returning goldenClaim for every
    // id was part of the PostManualV4 bug: the detail page's `id === claimId`
    // guard would treat the data as belonging to a different claim and refuse
    // to render — or, before the guard existed, render Роберт Джонсон data.
    if (claimId === goldenClaim.id) return goldenClaim;
    const row =
      mockExtraClaims.find((r) => r.id === claimId) ??
      claimRows.find((r) => r.id === claimId);
    if (row) {
      return {
        ...goldenClaim,
        id: row.id,
        customer: row.customer,
        vehicle: row.vehicle,
        eventType: row.eventType,
        status: row.status,
        risk: row.risk,
        // The list row doesn't carry every detail field; keep the goldenClaim
        // defaults for the rest but override the visible identifying fields.
      };
    }
    // Unknown id: synthesize a minimal sandbox detail so the UI still has a
    // valid claim to render. Never leaks the CLM-1006 customer/vehicle.
    return {
      ...goldenClaim,
      id: claimId,
      customer: 'Synthetic Customer (mock)',
      customerId: 'CUST-MOCK-0000',
      vehicle: 'Synthetic Vehicle (mock)',
      vehicleVin: 'VIN ****0000',
      description: 'Локальний sandbox кейс (mock-mode).',
      location: 'Local sandbox',
      status: 'Новий' as ClaimDetail['status'],
      risk: 'Невизначений' as ClaimDetail['risk'],
    };
  },
  async getClaimDocuments(_claimId: string): Promise<DocumentChecklistItem[]> {
    return documentsChecklist;
  },
  async getClaimPhotos(_claimId: string): Promise<DamagePhoto[]> {
    return damagePhotos;
  },

  // ---- reads (AI / evidence) ----
  async getAiAnalysis(_claimId: string) {
    return {
      findings: keyFindings,
      evidence: evidenceTabs,
      modelConfidence,
      extractedEntities,
    };
  },
  async runMockAiAnalysis(_claimId: string): Promise<MockAiRunResult> {
    await delay(50);
    return { runId: goldenClaim.runId, status: 'succeeded' };
  },

  // ---- reads (risk / policy / context / audit) ----
  async getRiskReview(_claimId: string) {
    return {
      score: goldenClaim.riskScore,
      threshold: 60,
      factors: riskFactors,
      pipeline: aiPipelineSteps,
    };
  },
  async getPolicyCoverage(_claimId: string) {
    return { blocks: policyCoverageBlocks, validation: policyValidation };
  },
  async getCustomerVehicleContext(_claimId: string) {
    return { previousClaims, communicationHistory };
  },
  async getAuditTrace(_claimId: string) {
    return {
      runId: goldenClaim.runId,
      traceId: goldenClaim.traceId,
      model: 'local-mock-v0.1',
      tokens: goldenClaim.tokens,
      cost: goldenClaim.cost,
      durationSec: goldenClaim.durationSec,
      events: auditTrail,
      distribution: costDistribution,
    };
  },

  // ---- dashboard summary ----
  async getClaimsSummary() {
    return {
      totalActive: 53,
      pendingReview: 5,
      highRisk: 7,
      avgSlaRemainingHours: 18,
      processedToday: 48,
      aiAnalysisRunning: 14,
    };
  },

  // ---- approval read model ----
  async getClaimApproval(_claimId: string) {
    return {
      claimId: goldenClaim.id,
      currentDecision: 'request' as string | null,
      notes: 'Запрошуємо клієнта надати фото пошкодження заднього бампера. AI confidence 78%.' as string | null,
      savedAt: null as string | null,
      submitted: false,
      submittedAt: null as string | null,
      availableOptions: [
        { value: 'approve', label: 'Погодити виплату', recommended: false, description: 'Якщо ризики прийнятні' as string | null },
        { value: 'request', label: 'Запросити дані', recommended: true, description: 'Рекомендовано AI' as string | null },
        { value: 'reject', label: 'Відхилити', recommended: false, description: 'З обґрунтуванням' as string | null },
        { value: 'escalate', label: 'Передати старшому', recommended: false, description: 'Ескалація' as string | null },
      ],
      aiRecommendation: 'Запросити додаткове фото перед погодженням виплати' as string | null,
      recommendedPayout: goldenClaim.recommendedPayout,
    };
  },

  // ---- demo ----
  async getDemoScenario() {
    return demoSteps;
  },

  // ---- writes (human-controlled drafts only — never an autonomous decision) ----
  async saveApprovalDraft(
    _claimId: string,
    _draft: ApprovalDraftInput,
  ): Promise<ApprovalDraftResult> {
    await delay(500);
    return { ok: true, savedAt: new Date().toISOString(), note: MOCK_NOTE };
  },
  async sendCustomerRequest(_claimId: string): Promise<CustomerRequestResult> {
    await delay(700);
    return { ok: true, savedAt: new Date().toISOString(), note: MOCK_NOTE };
  },

  // ---- AI Analysis (advisory only) — synthetic stub mirroring the BFF DTO shape ----
  // In mock mode these return deterministic synthetic data so the UI exercises the
  // same advisory-only rendering pipeline as backend mode without any network call.

  async getClaimAiAnalysis(claimId: string): Promise<AiAnalysisDto | null> {
    return buildSyntheticAiAnalysisDto(claimId, 'corr-mock-get');
  },

  async runClaimAiAnalysis(claimId: string): Promise<AiAnalysisDto> {
    await delay(400);
    return buildSyntheticAiAnalysisDto(claimId, 'corr-mock-run');
  },

  // ---- BFF command mocks (mock-mode parity with backend command endpoints) ----
  // None of these talk to a DB; they return a synthetic CommandResult with success=true
  // so the UI can exercise the same code path it uses in backend mode.

  async saveApprovalDraftCommand(
    claimId: string,
    _body: SaveApprovalDraftBody,
    _idempotencyKey?: string,
  ): Promise<CommandResult> {
    await delay(180);
    return buildSyntheticCommandResult(claimId, 'DraftSaved', 'Чернетку збережено локально (mock).');
  },

  async submitHumanDecision(
    claimId: string,
    body: SubmitHumanDecisionBody,
    _idempotencyKey?: string,
  ): Promise<CommandResult> {
    await delay(220);
    const requestedStatus = mapDecisionToStatus(body.decision);
    return buildSyntheticCommandResult(
      claimId,
      requestedStatus,
      `Рішення «${body.decision}» зафіксовано локально (mock). Виплата не виконувалась.`,
    );
  },

  async requestMissingDocument(
    claimId: string,
    body: RequestMissingDocumentBody,
    _idempotencyKey?: string,
  ): Promise<CommandResult> {
    await delay(180);
    return buildSyntheticCommandResult(
      claimId,
      'MissingDocumentRequested',
      `Внутрішній запит на документ «${body.documentTitle}» зафіксовано (mock). Лист клієнту не надсилався.`,
    );
  },

  async createDocumentMetadata(
    claimId: string,
    body: CreateDocumentMetadataBody,
    _idempotencyKey?: string,
  ): Promise<CommandResult> {
    await delay(180);
    return buildSyntheticCommandResult(
      claimId,
      'MetadataCreated',
      `Метадані документа «${body.title}» збережено локально (mock). Файл не завантажувався.`,
    );
  },

  // ---- Synthetic-sandbox mocks (mock-mode parity with backend) ----
  async createClaim(
    body: CreateClaimBody,
    _idempotencyKey?: string,
  ): Promise<CreateClaimResult> {
    await delay(220);
    // Generate a synthetic CLM-MOCK-#### id and register the row in the
    // in-memory mock list so that mock-mode getClaimsQueue + getClaimById will
    // serve THIS claim's own data on subsequent reads. Previously the create
    // returned an id and then mock-mode getClaimById('CLM-MOCK-1001') would
    // resolve to goldenClaim, which the new detail-page guard rejects as a
    // mismatch — visible in mock-mode as a permanent "loading..." placeholder.
    mockNextClaimNumber += 1;
    const claimId = `CLM-MOCK-${mockNextClaimNumber.toString().padStart(4, '0')}`;
    const customer = body.customerName ?? 'Synthetic Customer 001';
    mockExtraClaims.unshift({
      id: claimId,
      customer,
      vehicle: body.vehicle,
      eventType: body.eventType,
      // Cast: backend uses 'Новий' / 'Очікує AI' / 'Невизначений' for fresh
      // synthetic claims (outside the formal union); mock mirrors that.
      status: 'Новий' as ClaimRow['status'],
      documentsCount: '0/0',
      aiStatus: 'Очікує AI' as ClaimRow['aiStatus'],
      risk: 'Невизначений' as ClaimRow['risk'],
      sla: '7 дн',
      nextAction: 'Зібрати документи',
      updated: 'щойно',
    });
    return {
      ...buildSyntheticCommandResult(claimId, 'Новий',
        `Mock-mode: створено локальний кейс ${claimId} (без БД).`),
      customerId: body.customerId ?? 'CUST-MOCK-0001',
      customer,
      vehicle: body.vehicle,
    };
  },

  async uploadDocumentContent(
    claimId: string,
    body: UploadDocumentContentBody,
    _idempotencyKey?: string,
  ): Promise<CommandResult> {
    await delay(180);
    return buildSyntheticCommandResult(
      claimId,
      'Uploaded',
      `Mock-mode: документ «${body.title}» збережено локально (${body.content.length} симв.).`,
    );
  },

  async createPayoutSimulation(
    claimId: string,
    body: CreatePayoutSimulationBody,
    _idempotencyKey?: string,
  ): Promise<PayoutSimulationResultDto> {
    await delay(200);
    const net = Math.max(0, body.amount - body.deductible);
    const base = buildSyntheticCommandResult(
      claimId,
      'DraftSimulated',
      `Mock-mode: симуляція виплати ${body.amount} ${body.currency ?? 'USD'} створена локально (без БД, без виплати).`,
    );
    return {
      ...base,
      simulationId: 0,
      amount: body.amount,
      deductible: body.deductible,
      netPayoutAmount: net,
      currency: body.currency ?? 'USD',
      decisionSource: body.decisionSource ?? 'Human',
      decisionActor: 'Synthetic Adjuster (mock)',
      sourceAiRunId: body.sourceAiRunId ?? null,
      simulationOnly: true,
    };
  },

  async listPayoutSimulations(_claimId: string): Promise<PayoutSimulationSummaryDto[]> {
    return [];
  },

  async listCustomers(
    search?: string | null,
    page = 1,
    pageSize = 25,
  ): Promise<CustomerListResultDto> {
    const rows = mockCustomerDirectory;
    const filtered = search
      ? rows.filter(
          (r) =>
            r.fullName.toLowerCase().includes(search.toLowerCase()) ||
            r.id.toLowerCase().includes(search.toLowerCase()) ||
            r.email.toLowerCase().includes(search.toLowerCase()),
        )
      : rows;
    const offset = Math.max(0, (page - 1) * pageSize);
    return {
      total: filtered.length,
      page,
      pageSize,
      items: filtered.slice(offset, offset + pageSize),
    };
  },

  async getCustomerById(customerId: string): Promise<CustomerSummaryDto | null> {
    return mockCustomerDirectory.find((c) => c.id === customerId) ?? null;
  },

  async createCustomer(
    body: CreateCustomerBody,
    _idempotencyKey?: string,
  ): Promise<CreateCustomerResult> {
    await delay(180);
    mockNextCustomerNumber += 1;
    const id = `CUST-MOCK-${mockNextCustomerNumber.toString().padStart(4, '0')}`;
    const since = body.customerSince ?? new Date().toISOString().slice(0, 10);
    const row: CustomerSummaryDto = {
      id,
      fullName: body.fullName.trim(),
      email: (body.email ?? '').trim() || `mock-${mockNextCustomerNumber}@example.invalid`,
      phone: (body.phone ?? '').trim() || '+1-555-0100',
      addressLine: (body.addressLine ?? '').trim() || 'Local sandbox · mock-mode',
      customerSince: since,
      previousClaimsCount: 0,
      isSynthetic: true,
    };
    mockCustomerDirectory.push(row);
    return {
      success: true,
      commandId: 'cmd-mock-' + Math.random().toString(36).slice(2, 10),
      customerId: row.id,
      fullName: row.fullName,
      email: row.email,
      phone: row.phone,
      addressLine: row.addressLine,
      customerSince: row.customerSince,
      isSynthetic: true,
      message: `Mock-mode: створено клієнта ${row.id} (без БД).`,
    };
  },

  async recordAiDecision(
    claimId: string,
    _body: RecordAiDecisionBody,
    _idempotencyKey?: string,
  ): Promise<AiDecisionRecordedResult> {
    await delay(160);
    const base = buildSyntheticCommandResult(
      claimId,
      'AiDecisionRecorded',
      'AI-рішення збережено локально (mock). Без виплати, без повідомлень клієнту.',
    );
    return {
      ...base,
      aiRunId: 'run_mock_local',
      providerMode: 'Mock',
      modelName: 'local-mock-v0.1',
      recommendedAction: 'Запросити фото заднього бампера перед остаточним рішенням.',
      riskLevel: 'moderate',
      confidenceScore: 78,
      isAdvisoryOnly: true,
      source: 'AI',
    };
  },
};

function buildSyntheticCommandResult(
  claimId: string,
  status: string,
  message: string,
): CommandResult {
  const commandId =
    'cmd-mock-' + Math.random().toString(36).slice(2, 10);
  return {
    success: true,
    commandId,
    claimId,
    status,
    auditEventId: null,
    outboxMessageId: null,
    correlationId: 'corr-mock-' + commandId.slice(-6),
    message,
    warnings: [],
  };
}

function mapDecisionToStatus(decision: string): string {
  switch (decision) {
    case 'ApproveForReview':
      return 'PendingApprovalReview';
    case 'RejectForReview':
      return 'PendingRejectionReview';
    case 'NeedsMoreInformation':
      return 'AwaitingInformation';
    case 'RequestDocuments':
      return 'AwaitingDocuments';
    default:
      return 'Unknown';
  }
}

function buildSyntheticAiAnalysisDto(claimId: string, correlationId: string): AiAnalysisDto {
  return {
    runId: 'run_mock_local',
    claimId,
    providerMode: 'Mock',
    modelName: 'local-mock-v0.1',
    status: 'succeeded',
    summaryText:
      'Локальний демо-аналіз: розбіжність кошторису з бенчмарком та неповний фотопакет. ' +
      'Усі прапорці порадницькі; рішення приймає людина.',
    recommendedAction: {
      action: 'Запросити фото заднього бампера перед остаточним рішенням.',
      rationale: 'AI advisory recommendation — human adjuster decides.',
      confidenceScore: 78,
    },
    policyCoverageExplanation:
      'Поліс POL-2025-AC-4421 покриває ДТП після франшизи $500; виплата у межах ліміту.',
    riskLevel: 'moderate',
    confidenceScore: 78,
    findings: keyFindings.map((f, i) => ({
      id: `f-${i + 1}`,
      category: 'Mock',
      text: f.text,
      severity: f.tone === 'danger' ? 'danger' : f.tone === 'warn' ? 'warn' : 'ok',
    })),
    evidence: extractedEntities.slice(0, 2).map((e, i) => ({
      id: `e-${i + 1}`,
      source: e.source,
      note: `${e.field}: ${e.value}`,
      confidence: e.confidence,
    })),
    risks: riskFactors.map((r, i) => ({
      id: `rs-${i + 1}`,
      label: r.label,
      weight: r.contribution,
    })),
    guardrails: {
      advisoryOnly: true,
      requiresHumanReview: true,
      canApprovePayout: false,
      canRejectClaim: false,
      canAccuseFraudFinal: false,
      canSendCustomerMessage: false,
      canChangeClaimStatus: false,
    },
    costTrace: {
      tokens: 4261,
      estimatedCost: 0.0187,
      currencyCode: 'USD',
    },
    correlationId,
    createdAtUtc: new Date().toISOString(),
    isAdvisoryOnly: true,
    notice: 'AI output is advisory only — human decision is final.',
  };
}

export type MockInsuranceApi = typeof mockInsuranceApi;
