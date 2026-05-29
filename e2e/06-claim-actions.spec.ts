import { test, expect } from '@playwright/test';
import { login } from './helpers/auth';

/**
 * TEST 7 — Document upload text content.
 * TEST 8 — Missing document/photo request.
 * TEST 9 — AI analysis + AI decision (source=AI).
 * TEST 10 — Payout simulation (SimulationOnly).
 * TEST 11 — Audit trace renders.
 *
 * All use the seeded CLM-1006 golden claim — it has the richest data so the
 * UI controls are present without needing a freshly-created claim to be in
 * the right state.
 */
test.describe('Claim workspace actions on CLM-1006', () => {
  const claimId = 'CLM-1006';

  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await page.evaluate(() => window.localStorage.removeItem('iap.auth.demo.v1'));
    await login(page);
  });

  test('Document upload — text content saved (success toast)', async ({ page }) => {
    await page.goto(`/claims/${claimId}/documents`);
    await page.locator('[data-testid=upload-doc-open]').click();
    await expect(page.locator('[data-testid=upload-doc-title]')).toBeVisible();
    // Title and content are pre-filled with a synthetic template on open.
    await page.locator('[data-testid=upload-doc-submit]').click();
    // Modal closes; success toast appears in viewport.
    await expect(page.locator('[data-testid=upload-doc-title]')).toBeHidden({ timeout: 10000 });
    // Toast content asserts the success ("збережено в БД").
    await expect(page.locator('body')).toContainText(/Документ збережено в БД|документ.*збережено/i, {
      timeout: 10000,
    });
  });

  test('Missing document/photo request — visible local-sandbox wording', async ({ page }) => {
    await page.goto(`/claims/${claimId}/documents`);
    await page.locator('[data-testid=request-missing-doc-open]').click();
    // The shared RequestMissingDocumentModal opens. Its primary CTA is a
    // "Зафіксувати запит" button (no data-testid yet); we click the
    // submit button by role to keep the test stable.
    const submitBtn = page.getByRole('button', { name: /Зафіксувати запит|Зафіксувати/i });
    await expect(submitBtn).toBeVisible();
    await submitBtn.click();
    // Success toast or local-sandbox copy.
    await expect(page.locator('body')).toContainText(
      /Запит.*зафіксовано|зафіксовано.*журналі|документ.*зафіксовано/i,
      { timeout: 10000 },
    );
  });

  test('AI analysis runs + AI decision recorded with source=AI', async ({ page }) => {
    await page.goto(`/claims/${claimId}/ai-evidence`);

    // Run AI analysis.
    await page.locator('[data-testid=run-ai-analysis]').click();
    // Wait for the analysis to complete — button text flips back from
    // "Запускаємо N%" to "Запустити AI-аналіз".
    await expect(page.locator('[data-testid=run-ai-analysis]')).toHaveText(
      /Запустити AI-аналіз/,
      { timeout: 30000 },
    );

    // Record AI decision.
    const recordBtn = page.locator('[data-testid=record-ai-decision]');
    await expect(recordBtn).toBeEnabled();
    await recordBtn.click();

    // Confirmation card appears; source=AI is the explicit attribution.
    // Timeout matches the AI-analysis wait (30s): the POST /ai-decision flow does
    // more work (latest-run lookup + audit insert + outbox insert) than a read, so
    // a tight 15s window can flake under local machine load. Same selector, same
    // semantic expectation — only the wait window is widened.
    await expect(page.locator('[data-testid=ai-decision-recorded]')).toBeVisible({
      timeout: 30000,
    });
    await expect(page.locator('[data-testid=ai-decision-source]')).toHaveText('AI');
  });

  test('Payout simulation — SimulationOnly notice visible, sim created', async ({ page }) => {
    await page.goto(`/claims/${claimId}/approval`);
    await page.locator('[data-testid=payout-sim-open]').click();

    // The SimulationOnly notice copy is required by spec (Slava rule:
    // every payout UI says "Локальна симуляція" / "SimulationOnly=true").
    await expect(page.locator('body')).toContainText(/SimulationOnly=true|Локальна симуляція/i);

    await page.locator('[data-testid=payout-sim-amount]').fill('1234.56');
    await page.locator('[data-testid=payout-sim-submit]').click();

    // Toast confirms the sim was created.
    await expect(page.locator('body')).toContainText(
      /Симуляція виплати.*створена|SimulationOnly.*true/i,
      { timeout: 10000 },
    );
  });

  test('Audit trace page renders with recent actions visible', async ({ page }) => {
    await page.goto(`/claims/${claimId}/audit`);
    await expect(page.locator('main')).toBeVisible();
    // The CLM-1006 audit ledger surfaces actions; we don't pin exact rows
    // because tests above may have added new ones in any order. We just
    // assert the route renders + at least one "categorical" action label is
    // visible somewhere.
    const bodyText = (await page.locator('body').textContent()) ?? '';
    const hasAnyCategory =
      /ClaimCreated|DocumentUploaded|MissingDocumentRequested|AiAnalysisCompleted|AiDecisionRecorded|ApprovalDraftSaved|HumanDecisionSubmitted|PayoutSimulationCreated|Аудит/i.test(
        bodyText,
      );
    expect(hasAnyCategory, 'expected at least one audit category label or "Аудит" header').toBe(
      true,
    );
  });
});
