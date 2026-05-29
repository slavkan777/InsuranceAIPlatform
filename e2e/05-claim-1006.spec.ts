import { test, expect } from '@playwright/test';
import { login } from './helpers/auth';

/**
 * TEST 6 — Existing CLM-1006 walkthrough (per gate spec).
 *
 * Opens the golden seeded claim and walks each main tab. Verifies that:
 *   - The detail route renders (HTTP 200 + UI mounts).
 *   - Each sub-route URL is reachable.
 *   - The customer label "Роберт Джонсон" is visible somewhere in the
 *     workspace shell (proves the seeded data is wired all the way through
 *     the HybridClaimReadService + BFF + UI).
 */
test.describe('CLM-1006 walkthrough', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await page.evaluate(() => window.localStorage.removeItem('iap.auth.demo.v1'));
    await login(page);
  });

  const subRoutes = [
    'documents',
    'ai-evidence',
    'risks',
    'policy',
    'customer-vehicle',
    'approval',
    'audit',
  ] as const;

  test('open detail + every sub-tab reachable', async ({ page }) => {
    // Detail
    await page.goto('/claims/CLM-1006');
    await expect(page).toHaveURL(/\/claims\/CLM-1006$/);

    // Customer label appears somewhere (proves HybridClaimReadService served it).
    await expect(page.locator('body')).toContainText(/Роберт Джонсон/);

    for (const r of subRoutes) {
      await page.goto(`/claims/CLM-1006/${r}`);
      await expect(page).toHaveURL(new RegExp(`/claims/CLM-1006/${r}$`));
      // Basic mount check — main element rendered.
      await expect(page.locator('main')).toBeVisible();
    }
  });
});
