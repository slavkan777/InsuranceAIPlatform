import { test, expect } from '@playwright/test';
import { login } from './helpers/auth';

/**
 * Negative / adversarial UI checks (scenario K).
 *
 * - Unknown route → catch-all redirects to /
 * - Required field empty → modal stays open
 * - No "real payment" wording anywhere on the workspace
 */
test.describe('Negative / adversarial', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await page.evaluate(() => window.localStorage.removeItem('iap.auth.demo.v1'));
    await login(page);
  });

  test('unknown route redirects to dashboard via catch-all', async ({ page }) => {
    await page.goto('/this-route-does-not-exist-12345');
    // Router has `<Route path="*" element={<Navigate to="/" replace />}`.
    await page.waitForURL((u) => /\/(?:|$)/.test(u.toString()) && !u.toString().includes('login'));
    await expect(page).toHaveURL('http://localhost:5173/');
  });

  test('NewClaimModal: empty vehicle blocks submit', async ({ page }) => {
    await page.goto('/claims');
    await page.locator('[data-testid=new-claim-open]').click();
    // Fill location only; leave vehicle empty.
    await page.locator('[data-testid=new-claim-vehicle]').fill('');
    await page.locator('[data-testid=new-claim-location]').fill('Київ');
    await page.locator('[data-testid=new-claim-submit]').click();
    // Modal stays open (HTML5 required on vehicle).
    await expect(page.locator('[data-testid=new-claim-vehicle]')).toBeVisible();
    await page.locator('[data-testid=new-claim-cancel]').click();
  });

  test('unknown claim id does NOT 500 — workspace gracefully degrades', async ({ page }) => {
    await page.goto('/claims/CLM-NEVER-EXISTS');
    // We don't crash. Either UI falls back to goldenClaim display or to an
    // error/empty UI; in any case `<main>` mounts.
    await expect(page.locator('main')).toBeVisible({ timeout: 5000 });
  });

  test('no "real payment" / "real transfer" copy anywhere in the workspace', async ({ page }) => {
    const routes = [
      '/',
      '/claims',
      '/customers',
      '/claims/CLM-1006',
      '/claims/CLM-1006/documents',
      '/claims/CLM-1006/ai-evidence',
      '/claims/CLM-1006/approval',
      '/claims/CLM-1006/audit',
    ];
    for (const r of routes) {
      await page.goto(r);
      const txt = ((await page.locator('body').textContent()) ?? '').toLowerCase();
      expect(txt.includes('real payment'), `${r} must not say "real payment"`).toBe(false);
      expect(txt.includes('real transfer'), `${r} must not say "real transfer"`).toBe(false);
      expect(txt.includes('funds transferred'), `${r} must not say "funds transferred"`).toBe(false);
    }
  });
});
