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
};

export type MockInsuranceApi = typeof mockInsuranceApi;
