import { test, expect } from '@playwright/test';
import { login } from './helpers/auth';

/**
 * TEST 2 — Sidebar active state (per gate spec; Slava bug 1).
 *
 * Pre-fix bug: navigating to `/claims/CLM-1006` highlighted BOTH
 * "Автострахові випадки" (`/claims`) AND "Робоче місце випадку"
 * (`/claims/CLM-1006`) because the first link lacked `end:true` and
 * therefore prefix-matched any nested route.
 *
 * The fix added `end:true` on every NavLink (Sidebar.tsx). This test
 * asserts that for every realistic route, exactly ONE sidebar item is
 * `.sidebar-link-active`.
 */
test.describe('Sidebar', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await page.evaluate(() => window.localStorage.removeItem('iap.auth.demo.v1'));
    await login(page);
  });

  for (const path of [
    '/',
    '/claims',
    '/customers',
    '/claims/CLM-1006',
    '/claims/CLM-1006/documents',
    '/claims/CLM-1006/ai-evidence',
    '/claims/CLM-1006/approval',
    '/claims/CLM-1006/audit',
  ] as const) {
    test(`exactly one active sidebar link for ${path}`, async ({ page }) => {
      await page.goto(path);
      // Wait until the route is committed.
      await page.waitForURL(new RegExp(escapeRegExp(path) + '$'));
      const activeCount = await page.locator('.sidebar-link-active').count();
      expect(activeCount, `expected exactly one active sidebar link for ${path}`).toBe(1);
    });
  }
});

function escapeRegExp(s: string): string {
  return s.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
}
