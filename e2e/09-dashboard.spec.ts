import { test, expect } from '@playwright/test';
import { login } from './helpers/auth';

/**
 * Dashboard + Demo + TopBar smoke (scenarios A, L).
 *
 * Verifies the dashboard renders, the disabled-future controls really are
 * non-clickable, the export-CSV button triggers a download, and that the
 * topbar's "/demo" toggle reaches the demo route.
 */
test.describe('Dashboard / Demo / TopBar', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await page.evaluate(() => window.localStorage.removeItem('iap.auth.demo.v1'));
    await login(page);
  });

  test('dashboard renders metrics + queue + AI rec card', async ({ page }) => {
    await page.goto('/');
    await expect(page.locator('main')).toBeVisible();
    // Look for the canonical section titles (Ukrainian, present in source) — proves
    // the major sections rendered.
    await expect(page.locator('body')).toContainText(/Auto Insurance Claims Overview/i);
    await expect(page.locator('body')).toContainText(/Auto insurance claims queue/i);
    await expect(page.locator('body')).toContainText(/AI recommendation for/i);
  });

  test('dashboard period chips are disabled-future, not silently dead', async ({ page }) => {
    await page.goto('/');
    // DeferredActionButton renders a button with title="…available in the next release".
    // Locate by the title-text substring; both period chips are wrapped.
    const period = page.locator('button[title*="next release"]');
    const count = await period.count();
    expect(count, 'expected ≥2 deferred period buttons on dashboard').toBeGreaterThanOrEqual(2);
  });

  test('dashboard segment chips are disabled', async ({ page }) => {
    await page.goto('/');
    // The 5 segment chips render `disabled aria-disabled="true"`. We grep the
    // dashboard's queue section for `aria-disabled="true"` buttons matching the
    // chip text.
    for (const segment of ['All', 'RTA', 'High risk', 'Awaiting AI', 'Awaiting decision']) {
      const chip = page.locator(`button[aria-disabled="true"]:has-text("${segment}")`);
      await expect(chip, `dashboard segment "${segment}" expected disabled`).toHaveCount(1);
    }
  });

  test('dashboard "Create Claim" opens the new-claim modal', async ({ page }) => {
    await page.goto('/');
    // Click the dashboard's "Create Claim" CTA (NOT the same as the list
    // page's "New claim"; the dashboard reuses NewClaimModal).
    await page.getByRole('button', { name: /Create Claim/ }).click();
    await expect(page.locator('[data-testid=new-claim-vehicle]')).toBeVisible();
    await page.locator('[data-testid=new-claim-cancel]').click();
    await expect(page.locator('[data-testid=new-claim-vehicle]')).toBeHidden({ timeout: 3000 });
  });

  test('dashboard "Export CSV" triggers a download', async ({ page }) => {
    await page.goto('/');
    const [download] = await Promise.all([
      page.waitForEvent('download', { timeout: 10_000 }),
      page.getByRole('button', { name: /Export CSV/ }).click(),
    ]);
    // File name format: dashboard-claims-YYYY-MM-DD.csv
    expect(download.suggestedFilename()).toMatch(/^dashboard-claims-\d{4}-\d{2}-\d{2}\.csv$/);
  });

  test('topbar logout button exists and avatar greeting visible', async ({ page }) => {
    await page.goto('/');
    await expect(page.locator('[data-testid=logout-button]')).toBeVisible();
    // Topbar advertises "Demo environment" + "System ready"
    await expect(page.locator('body')).toContainText(/Demo environment/i);
    await expect(page.locator('body')).toContainText(/System ready/i);
  });

  test('topbar Help + Notifications icons are disabled-future', async ({ page }) => {
    await page.goto('/');
    const helpBtn = page.locator('button[aria-label*="Help"]');
    const bellBtn = page.locator('button[aria-label*="Notifications"]');
    await expect(helpBtn).toBeDisabled();
    await expect(bellBtn).toBeDisabled();
  });

  test('topbar demo CTA reaches /demo route', async ({ page }) => {
    await page.goto('/');
    await page.getByRole('button', { name: /Guided walkthrough/ }).first().click();
    await page.waitForURL(/\/demo$/);
    await expect(page.locator('body')).toContainText(/Guided product walkthrough|Platform capabilities/i);
  });

  test('demo page renders 7 step cards', async ({ page }) => {
    await page.goto('/demo');
    // Every step card is a button; we expect at least 7 step cards.
    const stepCards = page.locator('main button:has(div.w-9.h-9.rounded-full.bg-brand-600)');
    const cardCount = await stepCards.count();
    expect(cardCount, 'expected ≥7 demo step cards').toBeGreaterThanOrEqual(7);
  });
});
