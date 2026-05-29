import { test, expect } from '@playwright/test';
import { login } from './helpers/auth';

/**
 * TEST 12 — Safety negative checks.
 *
 * Verifies the local sandbox never claims to perform a real-world side
 * effect (real payout / real customer message / real Azure call / leaked
 * secret) and that the obvious "real action" labels are absent from the
 * primary route surfaces a reviewer walks.
 */
test.describe('Safety invariants', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await page.evaluate(() => window.localStorage.removeItem('iap.auth.demo.v1'));
    await login(page);
  });

  const routesToScan = [
    '/',
    '/claims',
    '/customers',
    '/claims/CLM-1006',
    '/claims/CLM-1006/documents',
    '/claims/CLM-1006/ai-evidence',
    '/claims/CLM-1006/approval',
    '/claims/CLM-1006/audit',
  ] as const;

  const forbiddenSubstrings = [
    'sk-ant-', // Anthropic API key prefix
    'sk-or-',  // OpenRouter
    'sk-proj-', // OpenAI project key
    'DEEPSEEK_API_KEY', // env var name should not leak into UI
    'real payout executed',
    'transferred funds',
    'sent email to customer',
    'sent sms to customer',
  ] as const;

  for (const route of routesToScan) {
    test(`no real-action / no-secret leak on ${route}`, async ({ page }) => {
      await page.goto(route);
      await page.waitForLoadState('networkidle').catch(() => {});
      const body = (await page.locator('body').textContent()) ?? '';
      const lower = body.toLowerCase();
      for (const needle of forbiddenSubstrings) {
        expect(
          lower.includes(needle.toLowerCase()),
          `expected ${route} to NOT contain '${needle}'`,
        ).toBe(false);
      }
    });
  }

  test('TopBar advertises local sandbox, not "real demo"', async ({ page }) => {
    await page.goto('/');
    await expect(page.locator('body')).toContainText(/Local Sandbox/i);
  });

  test('No path to /payout or /customer-messages from the UI', async ({ page }) => {
    // We don't grep every anchor in every page (false-positive prone); we just
    // confirm those forbidden endpoints don't appear as button labels or as
    // any visible link href.
    await page.goto('/claims/CLM-1006');
    const html = await page.content();
    expect(html).not.toContain('/payout"');
    expect(html).not.toContain('/customer-messages"');
  });
});
