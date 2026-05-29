import { test, expect } from '@playwright/test';
import { login } from './helpers/auth';

/**
 * UI resilience (scenario L).
 *
 * - Cancel each major modal cleanly closes without committing
 * - Rapid navigation across routes does not crash the app
 * - Disabled-future controls are non-clickable
 */
test.describe('UI resilience', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await page.evaluate(() => window.localStorage.removeItem('iap.auth.demo.v1'));
    await login(page);
  });

  test('cancel NewClaimModal does not create a row', async ({ page }) => {
    // Race-safe variant: rather than comparing row counts (other tests
    // running serially in this same suite create claims and shift the count),
    // we fill a UNIQUE never-saved vehicle string and verify that searching for
    // that string after cancelling returns the empty-results state.
    await page.goto('/claims');
    const uniqueMarker = `NEVER_SAVED_CANCEL_${Date.now()}_${Math.random().toString(36).slice(2, 6)}`;
    await page.locator('[data-testid=new-claim-open]').click();
    await page.locator('[data-testid=new-claim-vehicle]').fill(uniqueMarker);
    await page.locator('[data-testid=new-claim-cancel]').click();
    await expect(page.locator('[data-testid=new-claim-vehicle]')).toBeHidden({ timeout: 3000 });
    // Trigger a queue refresh + search by unique marker.
    await page.reload();
    await page.locator('[data-testid=claims-search]').fill(uniqueMarker);
    await page.waitForTimeout(400);
    await expect(page.locator('[data-testid=claims-empty]')).toBeVisible();
  });

  test('cancel CreateCustomerModal does not create a row', async ({ page }) => {
    await page.goto('/customers');
    await expect(page.locator('[data-testid=customers-meta]')).toBeVisible();
    const initialMeta = (await page.locator('[data-testid=customers-meta]').textContent()) ?? '';
    await page.locator('[data-testid=create-customer-open]').click();
    await page.locator('[data-testid=create-customer-fullName]').fill('NEVER SAVED CANCEL');
    await page.locator('[data-testid=create-customer-cancel]').click();
    await expect(page.locator('[data-testid=create-customer-fullName]')).toBeHidden({
      timeout: 3000,
    });
    // Search the would-be name; the empty-state must render.
    await page.locator('[data-testid=customers-search]').fill('NEVER SAVED CANCEL');
    await page.waitForTimeout(600);
    await expect(page.locator('[data-testid=customers-empty]')).toBeVisible();
    // Restore initial state to keep tests independent.
    await page.locator('[data-testid=customers-search]').fill('');
    void initialMeta; // captured but not asserted-against (total may grow over runs)
  });

  test('rapid navigation across 6 routes does not crash', async ({ page }) => {
    const routes = [
      '/',
      '/claims',
      '/customers',
      '/claims/CLM-1006',
      '/claims/CLM-1006/audit',
      '/demo',
    ];
    for (const r of routes) {
      await page.goto(r);
    }
    // After the final route is loaded, the app must still render `<main>`.
    await expect(page.locator('main')).toBeVisible();
  });

  test('sidebar disabled-future entries are not clickable (Транспортні засоби, Налаштування)', async ({
    page,
  }) => {
    await page.goto('/');
    // The disabled items render as <span>, not <a>; clicking them must not
    // change the URL.
    const startUrl = page.url();
    const vehiclesItem = page.locator('aside').locator('span:has-text("Транспортні засоби")').first();
    await vehiclesItem.click();
    expect(page.url(), 'URL must not change when clicking disabled sidebar item').toBe(startUrl);
    const settingsItem = page.locator('aside').locator('span:has-text("Налаштування")').first();
    await settingsItem.click();
    expect(page.url(), 'URL must not change when clicking disabled sidebar item').toBe(startUrl);
  });

  test('topbar Help + Notifications buttons are disabled, not destructive on click', async ({
    page,
  }) => {
    await page.goto('/');
    const help = page.locator('button[aria-label*="Довідка"]');
    const bell = page.locator('button[aria-label*="Сповіщення"]');
    // The framework refuses to dispatch click() to a disabled element; just
    // assert disabled is true.
    await expect(help).toBeDisabled();
    await expect(bell).toBeDisabled();
  });
});
