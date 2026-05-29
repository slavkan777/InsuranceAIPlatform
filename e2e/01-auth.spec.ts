import { test, expect } from '@playwright/test';
import { DEMO, login, logout } from './helpers/auth';

/**
 * TEST 1 — Login/logout (per gate spec).
 *
 * Validates the demo-only auth flow:
 *   - Unauthenticated visits to `/` redirect to `/login` (RequireAuth).
 *   - The login form accepts the documented demo credentials.
 *   - After submit we land on `/` (Dashboard).
 *   - Logout clears localStorage and redirects back to `/login`.
 *
 * No real identity provider is involved — this is the
 * `localStorage:iap.auth.demo.v1` flow.
 */
test.describe('Auth', () => {
  test('redirect → login → dashboard → logout → login again', async ({ page }) => {
    // Pre-condition: clear any persisted demo session from a prior run.
    await page.goto('/');
    await page.evaluate(() => window.localStorage.removeItem('iap.auth.demo.v1'));
    await page.reload();

    // Step 1: unauthenticated visit lands on /login.
    await expect(page).toHaveURL(/\/login$/);
    await expect(page.locator('[data-testid=login-page]')).toBeVisible();

    // Step 2: bad creds → error banner, no redirect.
    await page.locator('[data-testid=login-input]').fill('wrong@example.com');
    await page.locator('[data-testid=login-password]').fill('NotARealPassword!');
    await page.locator('[data-testid=login-submit]').click();
    await expect(page.locator('[data-testid=login-error]')).toBeVisible();
    await expect(page).toHaveURL(/\/login$/);

    // Step 3: correct creds → redirect to /.
    await page.locator('[data-testid=login-input]').fill(DEMO.login);
    await page.locator('[data-testid=login-password]').fill(DEMO.password);
    await page.locator('[data-testid=login-submit]').click();
    await page.waitForURL((u) => !u.toString().includes('/login'));
    await expect(page).toHaveURL('http://localhost:5173/');

    // localStorage now carries the session marker.
    const persisted = await page.evaluate(() =>
      window.localStorage.getItem('iap.auth.demo.v1'),
    );
    expect(persisted).toBeTruthy();

    // Step 4: logout → /login + storage cleared.
    await logout(page);
    const after = await page.evaluate(() =>
      window.localStorage.getItem('iap.auth.demo.v1'),
    );
    expect(after).toBeNull();
  });

  test('login helper reaches dashboard cleanly', async ({ page }) => {
    await page.goto('/');
    await page.evaluate(() => window.localStorage.removeItem('iap.auth.demo.v1'));
    await login(page);
    await expect(page).toHaveURL('http://localhost:5173/');
    await expect(page.locator('[data-testid=sidebar]')).toBeVisible();
  });
});
