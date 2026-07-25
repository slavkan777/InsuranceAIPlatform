import { test, expect } from '@playwright/test';
import { login } from './helpers/auth';

/**
 * English-only product guard (Lease 4).
 *
 * Walks every route reachable from the navigation, plus the sign-in screen, the
 * new-claim modal and the guided walkthrough, and asserts that the RENDERED page
 * body contains zero Cyrillic characters.
 *
 * Why rendered text and not source: compatibility maps that translate legacy
 * Ukrainian database values into English labels are allowed to exist server-side,
 * but they must never reach the screen. This spec is the proof of that boundary —
 * there is no allowlist for rendered text.
 */

const CYRILLIC = /\p{Script=Cyrillic}/u;

/** Returns the Cyrillic characters currently rendered in <body>, if any. */
async function renderedCyrillic(page: import('@playwright/test').Page): Promise<string[]> {
  const text = await page.locator('body').innerText();
  return [...new Set(text.match(/\p{Script=Cyrillic}+/gu) ?? [])];
}

async function expectNoCyrillic(page: import('@playwright/test').Page, where: string) {
  const hits = await renderedCyrillic(page);
  expect(hits, `${where} must render no Cyrillic, found: ${JSON.stringify(hits)}`).toEqual([]);
}

const CLAIM = 'CLM-1006';

const ROUTES: [string, string][] = [
  ['/', 'dashboard'],
  ['/claims', 'claims list'],
  ['/customers', 'customer directory'],
  ['/demo', 'guided walkthrough'],
  [`/claims/${CLAIM}`, 'claim workspace'],
  [`/claims/${CLAIM}/documents`, 'documents & photos'],
  [`/claims/${CLAIM}/ai-evidence`, 'AI checks'],
  [`/claims/${CLAIM}/risks`, 'risks & checks'],
  [`/claims/${CLAIM}/approval`, 'human approval'],
  [`/claims/${CLAIM}/audit`, 'audit & cost'],
  [`/claims/${CLAIM}/policy`, 'policy & coverage'],
  [`/claims/${CLAIM}/customer-vehicle`, 'customer & vehicle'],
];

test.describe('English-only rendered product', () => {
  test('sign-in screen renders no Cyrillic', async ({ page }) => {
    await page.goto('/');
    await page.waitForURL(/\/login$/);
    await expect(page.locator('[data-testid=login-submit]')).toBeVisible();
    await expectNoCyrillic(page, 'login page');
  });

  test('the locale switcher is gone and a persisted uk locale is coerced to en', async ({
    page,
  }) => {
    // Simulate a returning visitor whose browser still holds the old locale.
    await page.goto('/');
    await page.evaluate(() => window.localStorage.setItem('iap.i18n.locale.v1', 'uk'));
    await page.reload();
    await page.waitForURL(/\/login$/);

    // The stored value must have been migrated, not merely ignored.
    const stored = await page.evaluate(() =>
      window.localStorage.getItem('iap.i18n.locale.v1'),
    );
    expect(stored, 'persisted locale must be coerced to "en"').toBe('en');

    // No UA switch anywhere, and the page still renders English.
    await expect(page.getByRole('button', { name: /^UA$/ })).toHaveCount(0);
    await expectNoCyrillic(page, 'login page after uk-locale migration');
  });

  for (const [route, name] of ROUTES) {
    test(`${name} (${route}) renders no Cyrillic`, async ({ page }) => {
      await login(page);
      await page.goto(route);
      // Let the saga-backed views settle before sampling the DOM.
      await expect(page.locator('main')).toBeVisible();
      await page.waitForTimeout(400);
      await expectNoCyrillic(page, `${name} (${route})`);
    });
  }

  test('new-claim modal renders no Cyrillic', async ({ page }) => {
    await login(page);
    await page.goto('/claims');
    await page.locator('[data-testid=new-claim-open]').click();
    await expect(page.locator('[data-testid=new-claim-vehicle]')).toBeVisible();
    await expectNoCyrillic(page, 'new-claim modal');
  });

  test('event-type options in the new-claim modal are English labels', async ({ page }) => {
    await login(page);
    await page.goto('/claims');
    await page.locator('[data-testid=new-claim-open]').click();
    await expect(page.locator('[data-testid=new-claim-vehicle]')).toBeVisible();

    const options = await page
      .locator('select')
      .filter({ hasText: /Road accident/ })
      .first()
      .locator('option')
      .allTextContents();

    expect(options.length, 'expected the event-type dropdown to be populated').toBeGreaterThan(0);
    for (const o of options) {
      expect(CYRILLIC.test(o), `event-type option "${o}" must be English`).toBe(false);
    }
    expect(options).toContain('Road accident');
  });
});
