import { test, expect } from '@playwright/test';
import { login } from './helpers/auth';

/**
 * Risks & Checks page (scenario C — part of the workspace tab walk).
 *
 * Renders + the 3 nav buttons at the bottom move to expected sub-routes.
 */
test.describe('Risks & Checks', () => {
  const claimId = 'CLM-1006';

  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await page.evaluate(() => window.localStorage.removeItem('iap.auth.demo.v1'));
    await login(page);
    await page.goto(`/claims/${claimId}/risks`);
  });

  test('renders risk score, factors, and governance panels', async ({ page }) => {
    await expect(page.locator('body')).toContainText(/Risks & checks|Risk score/i);
    await expect(page.locator('body')).toContainText(/Risk factors/i);
    await expect(page.locator('body')).toContainText(/Auto-approval BLOCKED/);
  });

  test('"Open evidence" → ai-evidence', async ({ page }) => {
    await page.getByRole('button', { name: /Open evidence/ }).click();
    await page.waitForURL(/\/claims\/CLM-1006\/ai-evidence$/);
  });

  test('"Request data" → documents', async ({ page }) => {
    await page.getByRole('button', { name: /Request data/ }).click();
    await page.waitForURL(/\/claims\/CLM-1006\/documents$/);
  });

  test('"Send for approval" → approval', async ({ page }) => {
    await page.getByRole('button', { name: /Send for approval/ }).click();
    await page.waitForURL(/\/claims\/CLM-1006\/approval$/);
  });

  test('safety: no "real fraud" / "real legal" wording', async ({ page }) => {
    const txt = ((await page.locator('body').textContent()) ?? '').toLowerCase();
    expect(txt.includes('real fraud')).toBe(false);
    expect(txt.includes('legal action triggered')).toBe(false);
  });
});
