import type { Page } from '@playwright/test';

/** Demo credentials — visible to the operator under the login form by design. */
export const DEMO = {
  login: 'demo@insurance.local',
  password: 'Demo123!',
} as const;

/**
 * Performs the demo-login flow on the given page. After this returns the
 * page is authenticated and sitting at `/` (dashboard). Centralised so each
 * spec can call `await login(page)` instead of duplicating the form interaction.
 *
 * Pre-condition: page is freshly opened (no existing session).
 */
export async function login(page: Page): Promise<void> {
  await page.goto('/');
  // Either we already land on /login, or RequireAuth redirects us there.
  await page.waitForURL(/\/login$/);
  await page.locator('[data-testid=login-input]').fill(DEMO.login);
  await page.locator('[data-testid=login-password]').fill(DEMO.password);
  await page.locator('[data-testid=login-submit]').click();
  // Wait for redirect away from /login (RequireAuth pushes to "/" or the
  // originally requested route).
  await page.waitForURL((u) => !u.toString().includes('/login'));
}

/**
 * Clicks the logout button in the TopBar. Pre-condition: page is
 * authenticated. After this returns the page is back at `/login`.
 */
export async function logout(page: Page): Promise<void> {
  await page.locator('[data-testid=logout-button]').click();
  await page.waitForURL(/\/login$/);
}
