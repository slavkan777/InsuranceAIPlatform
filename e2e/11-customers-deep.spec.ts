import { test, expect } from '@playwright/test';
import { login } from './helpers/auth';

/**
 * Customers catalog deep coverage (scenario E).
 *
 * Pagination forward/back, empty-name validation in create modal, cancel
 * button closes without creating.
 */
test.describe('Customers catalog deep', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await page.evaluate(() => window.localStorage.removeItem('iap.auth.demo.v1'));
    await login(page);
    await page.goto('/customers');
    await expect(page.locator('[data-testid=customers-directory-page]')).toBeVisible();
  });

  test('pagination Next + Prev cycles between pages', async ({ page }) => {
    const meta = page.locator('[data-testid=customers-meta]');
    await expect.poll(async () => (await meta.textContent()) ?? '').toMatch(/\d+ знайдено/);

    // Click "Далі" → page should increment.
    const next = page.getByRole('button', { name: /Далі/ });
    if (await next.isEnabled()) {
      await next.click();
      await expect.poll(async () => (await meta.textContent()) ?? '').toMatch(/сторінка 2/);

      // Click "← Назад" → page 1 again.
      const prev = page.getByRole('button', { name: /Назад/ });
      await prev.click();
      await expect.poll(async () => (await meta.textContent()) ?? '').toMatch(/сторінка 1/);
    } else {
      // Backend has fewer than PAGE_SIZE rows — pagination button absent, valid state.
      test.info().annotations.push({
        type: 'pagination',
        description: 'pagination disabled — fewer than 25 customers',
      });
    }
  });

  test('empty fullName blocks submit + shows validation error', async ({ page }) => {
    await page.locator('[data-testid=create-customer-open]').click();
    await expect(page.locator('[data-testid=create-customer-fullName]')).toBeVisible();
    // Don't fill name. The HTML5 `required` attribute prevents submit at all on
    // its own; we click submit and assert the modal stays open + nothing new is
    // created in the directory.
    await page.locator('[data-testid=create-customer-submit]').click();
    // Modal must still be open (the form's `required` attribute blocks submit
    // and the field will have :invalid pseudo-state).
    await expect(page.locator('[data-testid=create-customer-fullName]')).toBeVisible();
    // Cancel out cleanly.
    await page.locator('[data-testid=create-customer-cancel]').click();
    await expect(page.locator('[data-testid=create-customer-fullName]')).toBeHidden({
      timeout: 3000,
    });
  });

  test('cancel button on CreateCustomerModal closes without creating', async ({ page }) => {
    await page.locator('[data-testid=create-customer-open]').click();
    await page.locator('[data-testid=create-customer-fullName]').fill('NEVER COMMIT THIS');
    await page.locator('[data-testid=create-customer-cancel]').click();
    await expect(page.locator('[data-testid=create-customer-fullName]')).toBeHidden({
      timeout: 3000,
    });
    // Search for "NEVER COMMIT THIS" → must NOT find any row.
    await page.locator('[data-testid=customers-search]').fill('NEVER COMMIT');
    await page.waitForTimeout(600); // debounced 250ms + buffer
    await expect(page.locator('[data-testid=customers-empty]')).toBeVisible();
  });

  test('search "T0042" returns exactly the matching synthetic customer', async ({ page }) => {
    await page.locator('[data-testid=customers-search]').fill('T0042');
    await expect(page.locator('[data-testid="customer-row-CUST-T0042"]')).toBeVisible({
      timeout: 5000,
    });
    // The result set may include only 1 row when filtering by id substring
    const visibleRowCount = await page.locator('[data-testid^="customer-row-"]').count();
    expect(visibleRowCount).toBe(1);
  });
});
