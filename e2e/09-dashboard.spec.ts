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
    await expect(page.locator('body')).toContainText(/Огляд автострахових випадків/i);
    await expect(page.locator('body')).toContainText(/Черга автострахових випадків/i);
    await expect(page.locator('body')).toContainText(/AI-рекомендація для/i);
  });

  test('dashboard period chips are disabled-future, not silently dead', async ({ page }) => {
    await page.goto('/');
    // DeferredActionButton renders a button with title="…з'явиться у наступному релізі".
    // Locate by the title-text substring; both "Сьогодні" and "7 днів" are wrapped.
    const period = page.locator('button[title*="наступному релізі"]');
    const count = await period.count();
    expect(count, 'expected ≥2 deferred period buttons on dashboard').toBeGreaterThanOrEqual(2);
  });

  test('dashboard segment chips are disabled', async ({ page }) => {
    await page.goto('/');
    // The 5 segment chips render `disabled aria-disabled="true"`. We grep the
    // dashboard's queue section for `aria-disabled="true"` buttons matching the
    // chip text.
    for (const segment of ['Усі', 'ДТП', 'Високий ризик', 'Чекає AI', 'Чекає рішення']) {
      const chip = page.locator(`button[aria-disabled="true"]:has-text("${segment}")`);
      await expect(chip, `dashboard segment "${segment}" expected disabled`).toHaveCount(1);
    }
  });

  test('dashboard "Створити випадок" opens the new-claim modal', async ({ page }) => {
    await page.goto('/');
    // Click the dashboard's "Створити випадок" CTA (NOT the same as the list
    // page's "Новий випадок"; the dashboard reuses NewClaimModal).
    await page.getByRole('button', { name: /Створити випадок/ }).click();
    await expect(page.locator('[data-testid=new-claim-vehicle]')).toBeVisible();
    await page.locator('[data-testid=new-claim-cancel]').click();
    await expect(page.locator('[data-testid=new-claim-vehicle]')).toBeHidden({ timeout: 3000 });
  });

  test('dashboard "Експорт CSV" triggers a download', async ({ page }) => {
    await page.goto('/');
    const [download] = await Promise.all([
      page.waitForEvent('download', { timeout: 10_000 }),
      page.getByRole('button', { name: /Експорт CSV/ }).click(),
    ]);
    // File name format: dashboard-claims-YYYY-MM-DD.csv
    expect(download.suggestedFilename()).toMatch(/^dashboard-claims-\d{4}-\d{2}-\d{2}\.csv$/);
  });

  test('topbar logout button exists and avatar greeting visible', async ({ page }) => {
    await page.goto('/');
    await expect(page.locator('[data-testid=logout-button]')).toBeVisible();
    // Topbar advertises "Local Sandbox" + "Система готова"
    await expect(page.locator('body')).toContainText(/Local Sandbox/i);
    await expect(page.locator('body')).toContainText(/Система готова/i);
  });

  test('topbar Help + Notifications icons are disabled-future', async ({ page }) => {
    await page.goto('/');
    const helpBtn = page.locator('button[aria-label*="Довідка"]');
    const bellBtn = page.locator('button[aria-label*="Сповіщення"]');
    await expect(helpBtn).toBeDisabled();
    await expect(bellBtn).toBeDisabled();
  });

  test('topbar demo CTA reaches /demo route', async ({ page }) => {
    await page.goto('/');
    await page.getByRole('button', { name: /Приклад використання/ }).first().click();
    await page.waitForURL(/\/demo$/);
    await expect(page.locator('body')).toContainText(/Demo Scenario|Архітектура системи/i);
  });

  test('demo page renders 7 step cards', async ({ page }) => {
    await page.goto('/demo');
    // Every step card is a button; we expect at least 7 step cards.
    const stepCards = page.locator('main button:has(div.w-9.h-9.rounded-full.bg-brand-600)');
    const cardCount = await stepCards.count();
    expect(cardCount, 'expected ≥7 demo step cards').toBeGreaterThanOrEqual(7);
  });
});
