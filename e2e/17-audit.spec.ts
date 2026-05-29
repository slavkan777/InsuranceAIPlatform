import { test, expect } from '@playwright/test';
import { login } from './helpers/auth';

/**
 * Audit & Cost page deep (scenario I).
 *
 * Verifies the audit trail table renders, the cost-distribution panel
 * renders, and the page does NOT advertise any forbidden action category.
 */
test.describe('Audit & Cost', () => {
  const claimId = 'CLM-1006';

  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await page.evaluate(() => window.localStorage.removeItem('iap.auth.demo.v1'));
    await login(page);
    await page.goto(`/claims/${claimId}/audit`);
    await expect(page.locator('main')).toBeVisible();
  });

  test('header + trace identifiers visible', async ({ page }) => {
    await expect(page.locator('body')).toContainText(/Аудит і витрати/);
    await expect(page.locator('body')).toContainText(/Run ID|Trace ID/);
  });

  test('audit trail table renders + cost distribution renders', async ({ page }) => {
    await expect(page.locator('table')).toHaveCount(1);
    await expect(page.locator('body')).toContainText(/Розподіл витрат/);
  });

  test('governance panel: auto-approval forbidden, human review mandatory', async ({ page }) => {
    await expect(page.locator('body')).toContainText(/НЕ ДОЗВОЛЕНО/);
    await expect(page.locator('body')).toContainText(/ОБОВ'ЯЗКОВА/);
  });

  test('no forbidden action category names on the audit page', async ({ page }) => {
    const body = ((await page.locator('body').textContent()) ?? '').toLowerCase();
    const forbidden = [
      'realpayouttransfer',
      'realemailsent',
      'realsmssent',
      'claimstatusforciblychanged',
      'fraudconfirmed',
    ];
    for (const term of forbidden) {
      expect(body.includes(term.toLowerCase()), `audit must not contain ${term}`).toBe(false);
    }
  });
});
