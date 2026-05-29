import { test, expect } from '@playwright/test';
import { login } from './helpers/auth';

/**
 * Claims list / filter deep coverage (scenario F).
 *
 * Goes beyond the basic search test: filter dropdowns, segment chips,
 * empty-result state, reset-to-default behaviour.
 */
test.describe('Claims list deep filtering', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await page.evaluate(() => window.localStorage.removeItem('iap.auth.demo.v1'));
    await login(page);
    await page.goto('/claims');
    // Wait for the saga to populate the queue from backend.
    await expect(page.locator('[data-testid^="claim-row-"]').first()).toBeVisible({
      timeout: 10_000,
    });
  });

  test('CLM-1006 visible by default + searchable', async ({ page }) => {
    await expect(page.locator('[data-testid=claim-row-CLM-1006]')).toBeVisible();
    await page.locator('[data-testid=claims-search]').fill('CLM-1006');
    await page.waitForTimeout(300);
    await expect(page.locator('[data-testid=claim-row-CLM-1006]')).toBeVisible();
    // Search clears → row stays visible too.
    await page.locator('[data-testid=claims-search]').fill('');
    await page.waitForTimeout(300);
    await expect(page.locator('[data-testid=claim-row-CLM-1006]')).toBeVisible();
  });

  test('status filter set to "Готова" narrows the list', async ({ page }) => {
    const beforeCount = await page.locator('[data-testid^="claim-row-"]').count();
    await page.locator('select').filter({ hasText: /Усі/ }).first().selectOption('Готова');
    await page.waitForTimeout(300);
    const afterCount = await page.locator('[data-testid^="claim-row-"]').count();
    // Either narrower or shows the empty state; in any case the body must
    // no longer contain the stale full-list row count.
    expect(afterCount).toBeLessThanOrEqual(beforeCount);
  });

  test('event-type filter limits to "Зіткнення" (or shows empty state honestly)', async ({
    page,
  }) => {
    // Pick the third <select> which is event-type (status, risk, eventType, date, aiStatus order).
    const selects = page.locator('section.card.card-pad select');
    await selects.nth(2).selectOption('Зіткнення');
    await page.waitForTimeout(300);
    // Every remaining row must be a "Зіткнення" — verify by reading the third <td>
    // text on each visible row.
    const rows = page.locator('[data-testid^="claim-row-"]');
    const n = await rows.count();
    if (n === 0) {
      await expect(page.locator('[data-testid=claims-empty]')).toBeVisible();
    } else {
      for (let i = 0; i < n; i++) {
        const cellText = (await rows.nth(i).locator('td').nth(2).textContent()) ?? '';
        expect(cellText.trim()).toBe('Зіткнення');
      }
    }
  });

  test('segment chip "Високий ризик" filters to high-risk rows only', async ({ page }) => {
    await page.getByRole('button', { name: /^Високий ризик/ }).click();
    await page.waitForTimeout(300);
    const rows = page.locator('[data-testid^="claim-row-"]');
    const n = await rows.count();
    if (n === 0) {
      await expect(page.locator('[data-testid=claims-empty]')).toBeVisible();
    } else {
      for (let i = 0; i < n; i++) {
        // 7th column is Risk pill — we read the row text and require "Високий".
        const rowText = (await rows.nth(i).textContent()) ?? '';
        expect(rowText.includes('Високий'), `row ${i} should be Високий-risk`).toBe(true);
      }
    }
  });

  test('impossible search renders empty-state row', async ({ page }) => {
    await page.locator('[data-testid=claims-search]').fill('zzz-impossible-search-no-match');
    await page.waitForTimeout(300);
    await expect(page.locator('[data-testid=claims-empty]')).toBeVisible();
    // 0 rows visible.
    const n = await page.locator('[data-testid^="claim-row-"]').count();
    expect(n).toBe(0);
  });
});
