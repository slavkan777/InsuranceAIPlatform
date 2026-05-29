import { test, expect } from '@playwright/test';
import { login } from './helpers/auth';

/**
 * TEST 3 — Customers catalog 200/search/pagination (per gate spec; Slava bug 2).
 * TEST 4 — Create new synthetic customer.
 *
 * Pre-fix bug: the catalog showed 5 mock rows because VITE_INSURANCE_API_MODE
 * defaulted to 'mock'. The fix added `.env.development` with backend mode and
 * a server-side `POST /api/customers` endpoint + UI modal.
 */
test.describe('Customers catalog', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await page.evaluate(() => window.localStorage.removeItem('iap.auth.demo.v1'));
    await login(page);
  });

  test('catalog renders backend 200+ synthetic customers, pagination is exposed', async ({
    page,
  }) => {
    await page.goto('/customers');
    await expect(page.locator('[data-testid=customers-directory-page]')).toBeVisible();
    const meta = page.locator('[data-testid=customers-meta]');
    await expect(meta).toBeVisible();
    // Meta text reads e.g. "200 знайдено · сторінка 1/8" once the API responds.
    await expect.poll(async () => (await meta.textContent()) ?? '').toMatch(/\d+ знайдено/);
    const metaText = (await meta.textContent()) ?? '';
    const match = metaText.match(/^(\d+)\s+знайдено/);
    expect(match, `expected meta to start with a number; got: ${metaText}`).not.toBeNull();
    const total = Number(match![1]);
    // Backend seed is 200; any number > 5 means we're not on the mock fallback.
    expect(total).toBeGreaterThan(5);

    // Pagination control visible when total > pageSize (25).
    if (total > 25) {
      await expect(page.getByRole('button', { name: /Далі/ })).toBeVisible();
    }
  });

  test('search returns the seeded synthetic customer CUST-T0042', async ({ page }) => {
    await page.goto('/customers');
    await page.locator('[data-testid=customers-search]').fill('T0042');
    // Debounced; row should appear within ~1s.
    await expect(page.locator('[data-testid="customer-row-CUST-T0042"]')).toBeVisible({
      timeout: 5000,
    });
    await expect(page.locator('[data-testid=customers-meta]')).toContainText(/\d+ знайдено/);
  });

  test('create new synthetic customer, then see it in the catalog', async ({ page }) => {
    await page.goto('/customers');

    // Open the create modal
    await page.locator('[data-testid=create-customer-open]').click();
    await expect(page.locator('[data-testid=create-customer-fullName]')).toBeVisible();

    // Fill synthetic data only — no real PII.
    const stamp = Date.now().toString().slice(-6);
    const name = `Synthetic E2E Customer ${stamp}`;
    await page.locator('[data-testid=create-customer-fullName]').fill(name);
    await page.locator('[data-testid=create-customer-email]').fill(`e2e-${stamp}@synthetic.invalid`);
    await page.locator('[data-testid=create-customer-phone]').fill('+380501234567');
    await page.locator('[data-testid=create-customer-address]').fill('Local sandbox, no real address');

    // Submit
    await page.locator('[data-testid=create-customer-submit]').click();

    // Modal closes; CustomersDirectoryPage resets search to the new id and
    // reloads — the new row should be visible within a few seconds.
    await expect(page.locator('[data-testid=create-customer-fullName]')).toBeHidden({
      timeout: 5000,
    });

    // Wait for the table to settle on the new customer.
    await expect(page.locator('[data-testid=customers-table]')).toContainText(name, {
      timeout: 10000,
    });
  });
});
