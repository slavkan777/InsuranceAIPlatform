import { test, expect } from '@playwright/test';
import { login } from './helpers/auth';

/**
 * Claim workspace in-page navigation buttons (scenario C).
 *
 * `ClaimWorkspacePage` (the index sub-route at `/claims/CLM-1006`) has 4
 * navigation buttons at the bottom + 1 inside the right rail. We click
 * each and assert the URL settles.
 */
test.describe('Claim workspace deep navigation', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await page.evaluate(() => window.localStorage.removeItem('iap.auth.demo.v1'));
    await login(page);
  });

  test('"Open document collection" navigates to documents sub-route', async ({ page }) => {
    await page.goto('/claims/CLM-1006');
    await page.getByRole('button', { name: /Open document collection/ }).click();
    await page.waitForURL(/\/claims\/CLM-1006\/documents$/);
  });

  test('"Send for review" navigates to ai-evidence', async ({ page }) => {
    await page.goto('/claims/CLM-1006');
    await page.getByRole('button', { name: /Send for review/ }).click();
    await page.waitForURL(/\/claims\/CLM-1006\/ai-evidence$/);
  });

  test('"Prepare decision" navigates to approval', async ({ page }) => {
    await page.goto('/claims/CLM-1006');
    await page.getByRole('button', { name: /Prepare decision/ }).click();
    await page.waitForURL(/\/claims\/CLM-1006\/approval$/);
  });

  test('"Back to list" navigates to /claims', async ({ page }) => {
    await page.goto('/claims/CLM-1006');
    await page.getByRole('button', { name: /Back to list/ }).click();
    await page.waitForURL(/\/claims$/);
  });
});
