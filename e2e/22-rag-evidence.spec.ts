import { test, expect } from '@playwright/test';
import * as path from 'path';
import { login } from './helpers/auth';

/**
 * Spec 22 — Claim Evidence Intelligence (RAG) panel.
 *
 * Mode: mock API (VITE_INSURANCE_API_MODE=mock via .env.local).
 * No backend / Azure / secrets required.
 *
 * Pass labels follow the InsuranceAIPlatform QA convention:
 *   MECHANICAL_PASS   — element renders / navigation succeeds
 *   SEMANTIC_PASS     — interaction produces meaningful content
 *   PERSISTENCE_PASS  — state survives / audit data present
 *   NEGATIVE_PASS     — prohibited content is absent
 *
 * Screenshots are written to test-results/rag-*.png at key steps.
 */

const SS = (name: string) =>
  path.join('test-results', `rag-${name}.png`);

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

/** Navigate to the AI Evidence sub-route of a claim. */
async function gotoAiEvidence(page: Parameters<typeof login>[0], claimId: string) {
  await page.goto('/');
  await page.evaluate(() => window.localStorage.removeItem('iap.auth.demo.v1'));
  await login(page);
  await page.goto(`/claims/${claimId}/ai-evidence`);
  await expect(page).toHaveURL(new RegExp(`/claims/${claimId}/ai-evidence$`));
}

// ---------------------------------------------------------------------------
// Suite: MECHANICAL_PASS — panel renders + buttons present
// ---------------------------------------------------------------------------

test.describe('22 RAG panel — MECHANICAL_PASS', () => {
  test('CLM-1006: "Claim Evidence Intelligence" panel + all use-case buttons render', async ({ page }) => {
    await gotoAiEvidence(page, 'CLM-1006');

    // Panel itself
    const panel = page.locator('[data-testid=rag-panel]');
    await expect(panel).toBeVisible();

    // Panel title
    await expect(panel).toContainText('Claim Evidence Intelligence');

    // Idle state placeholder
    await expect(page.locator('[data-testid=rag-state-idle]')).toBeVisible();

    // All six use-case buttons present
    for (const useCase of ['coverage', 'missing_docs', 'risk', 'similar', 'summary', 'custom']) {
      await expect(
        page.locator(`[data-testid=rag-btn-${useCase}]`),
      ).toBeVisible();
    }

    await page.screenshot({ path: SS('01-mechanical-panel-render') });
  });
});

// ---------------------------------------------------------------------------
// Suite: SEMANTIC_PASS — clicking buttons returns content + advisory banner
// ---------------------------------------------------------------------------

test.describe('22 RAG panel — SEMANTIC_PASS', () => {
  test.beforeEach(async ({ page }) => {
    await gotoAiEvidence(page, 'CLM-1006');
  });

  test('"Check policy coverage" → answer card + citations + advisory banner', async ({ page }) => {
    await page.locator('[data-testid=rag-btn-coverage]').click();

    // Wait for answer card (mock delay ~350 ms)
    const answerCard = page.locator('[data-testid=rag-answer-card]');
    await expect(answerCard).toBeVisible({ timeout: 10000 });

    // Advisory banner must always be present
    const banner = page.locator('[data-testid=rag-advisory-banner]');
    await expect(banner).toBeVisible();
    await expect(banner).toContainText('AI advisory only');
    await expect(banner).toContainText('human');

    // Answer text must be non-empty
    const answerText = page.locator('[data-testid=rag-answer-text]');
    await expect(answerText).toBeVisible();
    const text = await answerText.textContent();
    expect(text?.trim().length).toBeGreaterThan(0);

    // Citations — mock returns 2 rows
    const citationsTable = page.locator('[data-testid=rag-citations-table]');
    await expect(citationsTable).toBeVisible();
    const rows = citationsTable.locator('tbody tr');
    await expect(rows).toHaveCount(2);

    // Provider mode chip shows "Mock"
    await expect(page.locator('[data-testid=rag-meta-provider]')).toContainText('Mock');

    await page.screenshot({ path: SS('02-semantic-coverage-answer') });
  });

  test('"Find missing documents" → answer renders', async ({ page }) => {
    await page.locator('[data-testid=rag-btn-missing_docs]').click();

    const answerCard = page.locator('[data-testid=rag-answer-card]');
    await expect(answerCard).toBeVisible({ timeout: 10000 });

    // Advisory banner visible
    await expect(page.locator('[data-testid=rag-advisory-banner]')).toBeVisible();

    // Answer must contain some text
    const text = await page.locator('[data-testid=rag-answer-text]').textContent();
    expect(text?.trim().length).toBeGreaterThan(0);

    await page.screenshot({ path: SS('03-semantic-missing-docs') });
  });

  test('"Explain risk" → answer renders', async ({ page }) => {
    await page.locator('[data-testid=rag-btn-risk]').click();

    const answerCard = page.locator('[data-testid=rag-answer-card]');
    await expect(answerCard).toBeVisible({ timeout: 10000 });

    await expect(page.locator('[data-testid=rag-advisory-banner]')).toBeVisible();

    const text = await page.locator('[data-testid=rag-answer-text]').textContent();
    expect(text?.trim().length).toBeGreaterThan(0);

    await page.screenshot({ path: SS('04-semantic-risk-answer') });
  });
});

// ---------------------------------------------------------------------------
// Suite: NEGATIVE_PASS — prohibited content absent
// ---------------------------------------------------------------------------

test.describe('22 RAG panel — NEGATIVE_PASS', () => {
  test.beforeEach(async ({ page }) => {
    await gotoAiEvidence(page, 'CLM-1006');
  });

  test('"Explain risk" answer does NOT contain fraud-accusation words', async ({ page }) => {
    await page.locator('[data-testid=rag-btn-risk]').click();
    await expect(page.locator('[data-testid=rag-answer-card]')).toBeVisible({ timeout: 10000 });

    const answerText = await page.locator('[data-testid=rag-answer-text]').textContent() ?? '';
    // Prohibited fraud-accusation vocabulary (case-insensitive)
    const fraudWords = ['шахрай', 'шахрайств', 'fraud', 'fraudulent'];
    for (const word of fraudWords) {
      expect(answerText.toLowerCase()).not.toContain(word.toLowerCase());
    }

    await page.screenshot({ path: SS('05-negative-no-fraud-word') });
  });

  test('"Find similar claims" returns claim-level cards only — NO evidence snippets/answer text', async ({ page }) => {
    await page.locator('[data-testid=rag-btn-similar]').click();

    // Wait for similar-claims section (mock delay ~280 ms)
    const similarSection = page.locator('[data-testid=rag-similar-claims]');
    await expect(similarSection).toBeVisible({ timeout: 10000 });

    // Mock returns 3 cards (CLM-1008, CLM-1010, CLM-1015)
    await expect(page.locator('[data-testid=rag-similar-card-CLM-1008]')).toBeVisible();
    await expect(page.locator('[data-testid=rag-similar-card-CLM-1010]')).toBeVisible();
    await expect(page.locator('[data-testid=rag-similar-card-CLM-1015]')).toBeVisible();

    // Cards must show claimId + score — these are claim-level fields
    await expect(similarSection).toContainText('CLM-1008');
    await expect(similarSection).toContainText('Similarity');

    // Confirm the similar-claims section is NOT the ask answer card —
    // there must be NO rag-answer-card rendered alongside the similar results.
    await expect(page.locator('[data-testid=rag-answer-card]')).not.toBeVisible();

    // No citation table (evidence text from another claim must not appear)
    await expect(page.locator('[data-testid=rag-citations-table]')).not.toBeVisible();

    await page.screenshot({ path: SS('06-negative-similar-no-evidence') });
  });
});

// ---------------------------------------------------------------------------
// Suite: PERSISTENCE_PASS — audit / reload
// ---------------------------------------------------------------------------

test.describe('22 RAG panel — PERSISTENCE_PASS', () => {
  // PERSISTENCE_PASS: RagAuditHistoryPanel dispatches fetchAuditHistory on mount,
  // calling ragAudit() which returns deterministic mock rows (fixed createdAtUtc).
  // On page.reload() the component mounts again, dispatches fetchAuditHistory again,
  // and the mock returns the same deterministic rows — history survives reload.
  test('audit history panel renders rows and persists across page reload (MOCK)', async ({ page }) => {
    await gotoAiEvidence(page, 'CLM-1006');

    // Wait for the audit history container to appear (dispatched on mount)
    const auditContainer = page.locator('[data-testid=rag-audit-history]');
    await expect(auditContainer).toBeVisible({ timeout: 10000 });

    // Must contain at least 1 audit row before reload
    const rowsBefore = auditContainer.locator('[data-testid=rag-audit-row]');
    await expect(rowsBefore).toHaveCount(2);

    await page.screenshot({ path: SS('08-persistence-before-reload') });

    // Full page reload
    await page.reload();
    await page.waitForLoadState('networkidle');

    // PERSISTENCE_PASS: history must still show >=1 row after reload
    const auditContainerAfter = page.locator('[data-testid=rag-audit-history]');
    await expect(auditContainerAfter).toBeVisible({ timeout: 10000 });

    const rowsAfter = auditContainerAfter.locator('[data-testid=rag-audit-row]');
    await expect(rowsAfter).toHaveCount(2);

    await page.screenshot({ path: SS('09-persistence-after-reload') });
  });
});

// ---------------------------------------------------------------------------
// Suite: Evidence Intelligence Stack (RagInfrastructureStackPanel)
// ---------------------------------------------------------------------------

test.describe('22 RAG — Evidence Intelligence Stack', () => {
  test.beforeEach(async ({ page }) => {
    await gotoAiEvidence(page, 'CLM-1006');
  });

  test('MECHANICAL_PASS: container + all 3 layer rows visible for CLM-1006', async ({ page }) => {
    const container = page.locator('[data-testid=rag-infra-stack]');
    await expect(container).toBeVisible({ timeout: 10000 });

    await expect(page.locator('[data-testid=rag-infra-sql]')).toBeVisible();
    await expect(page.locator('[data-testid=rag-infra-index]')).toBeVisible();
    await expect(page.locator('[data-testid=rag-infra-vector]')).toBeVisible();
    await expect(page.locator('[data-testid=rag-infra-runtime]')).toBeVisible();

    await page.screenshot({ path: SS('10-infra-mechanical') });
  });

  test('SEMANTIC_PASS: sql status=healthy + counts, index 50/50, runtime=disabled', async ({ page }) => {
    // Wait for the panel to finish loading (infra is auto-fetched on mount)
    await expect(page.locator('[data-testid=rag-infra-sql]')).toBeVisible({ timeout: 10000 });

    // SQL layer: status badge text must contain "healthy" and counts must include 50
    const sqlStatus = page.locator('[data-testid=rag-infra-sql-status]');
    await expect(sqlStatus).toContainText('healthy');
    const sqlRow = page.locator('[data-testid=rag-infra-sql]');
    await expect(sqlRow).toContainText('50');

    // Index layer: 50/50 embedded
    const indexStatus = page.locator('[data-testid=rag-infra-index-status]');
    await expect(indexStatus).toContainText('healthy');
    const indexRow = page.locator('[data-testid=rag-infra-index]');
    await expect(indexRow).toContainText('50');

    // Runtime layer: badge must read "disabled"
    const runtimeStatus = page.locator('[data-testid=rag-infra-runtime-status]');
    await expect(runtimeStatus).toContainText('disabled');

    await page.screenshot({ path: SS('11-infra-semantic') });
  });

  test('ACTION: click Reindex → panel still shows index healthy 50/50 (no crash)', async ({ page }) => {
    // Wait for initial render
    await expect(page.locator('[data-testid=rag-infra-stack]')).toBeVisible({ timeout: 10000 });
    await expect(page.locator('[data-testid=rag-infra-index]')).toBeVisible();

    // Click reindex
    await page.locator('[data-testid=rag-infra-reindex-btn]').click();

    // Panel must still show index row with healthy + 50 after the mock reindex completes
    const indexRow = page.locator('[data-testid=rag-infra-index]');
    await expect(indexRow).toBeVisible({ timeout: 10000 });
    await expect(page.locator('[data-testid=rag-infra-index-status]')).toContainText('healthy');
    await expect(indexRow).toContainText('50');

    await page.screenshot({ path: SS('12-infra-reindex-action') });
  });

  test('NEGATIVE_PASS: runtime row reads disabled/mock; does NOT claim a live model is running', async ({ page }) => {
    await expect(page.locator('[data-testid=rag-infra-runtime]')).toBeVisible({ timeout: 10000 });

    const runtimeRow = page.locator('[data-testid=rag-infra-runtime]');
    const runtimeText = await runtimeRow.textContent() ?? '';

    // Must contain "disabled" or "mock"
    const hasDisabledOrMock =
      runtimeText.toLowerCase().includes('disabled') ||
      runtimeText.toLowerCase().includes('mock');
    expect(hasDisabledOrMock).toBe(true);

    // Must NOT contain words that imply a live paid model is active
    // "available" as the runtime status or "live" would be false advertising
    const runtimeStatusText = await page.locator('[data-testid=rag-infra-runtime-status]').textContent() ?? '';
    expect(runtimeStatusText.toLowerCase()).not.toBe('available');
    expect(runtimeStatusText.toLowerCase()).not.toBe('live');

    await page.screenshot({ path: SS('13-infra-negative-no-live-model') });
  });
});

// ---------------------------------------------------------------------------
// Suite: Cross-claim isolation — CLM-1009 must not show CLM-1006 evidence
// ---------------------------------------------------------------------------

test.describe('22 RAG panel — cross-claim isolation', () => {
  test('CLM-1009 coverage answer contains CLM-1009 claimId, not CLM-1006 evidence', async ({ page }) => {
    // Open CLM-1009 RAG panel
    await page.goto('/');
    await page.evaluate(() => window.localStorage.removeItem('iap.auth.demo.v1'));
    await login(page);
    await page.goto('/claims/CLM-1009/ai-evidence');
    await expect(page).toHaveURL(/\/claims\/CLM-1009\/ai-evidence$/);

    await page.locator('[data-testid=rag-btn-coverage]').click();
    await expect(page.locator('[data-testid=rag-answer-card]')).toBeVisible({ timeout: 10000 });

    // The mock answer embeds the claimId — assert CLM-1009 is there
    const answerText = await page.locator('[data-testid=rag-answer-text]').textContent() ?? '';
    expect(answerText).toContain('CLM-1009');

    // Check citation document IDs — all must reference CLM-1009, not CLM-1006.
    // Mock builds documentId as `doc-${claimId}-*` so we can check directly.
    const docIds = await page.locator('[data-testid=rag-citations-table] tbody tr td:nth-child(2)').allTextContents();
    for (const docId of docIds) {
      expect(docId).not.toContain('CLM-1006');
      expect(docId).toContain('CLM-1009');
    }

    // Also assert no CLM-1006 answer text leaks into the answer area
    expect(answerText).not.toContain('doc-CLM-1006');

    await page.screenshot({ path: SS('07-cross-claim-clm1009-isolation') });
  });
});
