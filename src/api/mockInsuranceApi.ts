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
  ApprovalDraftInput,
  ApprovalDraftResult,
  CustomerRequestResult,
  MockAiRunResult,
} from './insuranceApi.types';

const MOCK_NOTE = 'local mock · synthetic data · no network';

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
    return claimRows;
  },
  async getClaimById(_claimId: string): Promise<ClaimDetail> {
    return goldenClaim;
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
      model: 'Azure OpenAI',
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
};

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
