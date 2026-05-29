import { test, expect } from '@playwright/test';
import { login } from './helpers/auth';

/**
 * Reload / persistence (scenario J).
 *
 * Create a customer + claim, then reload the page (hard navigation) and
 * confirm the rows survive — verifying actual DB persistence through the
 * browser, not just the in-memory React state.
 */
test.describe('Persistence (reload survives)', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await page.evaluate(() => window.localStorage.removeItem('iap.auth.demo.v1'));
    await login(page);
  });

  test('created customer survives full page reload', async ({ page }) => {
    await page.goto('/customers');
    const stamp = Date.now().toString().slice(-6);
    const name = `Synthetic Persist ${stamp}`;
    await page.locator('[data-testid=create-customer-open]').click();
    await page.locator('[data-testid=create-customer-fullName]').fill(name);
    await page.locator('[data-testid=create-customer-submit]').click();
    await expect(page.locator('[data-testid=create-customer-fullName]')).toBeHidden({
      timeout: 5000,
    });
    // Verify visible BEFORE reload.
    await expect(page.locator('[data-testid=customers-table]')).toContainText(name, {
      timeout: 10000,
    });

    // Pull the new id from the table row.
    const newRow = page
      .locator('[data-testid^="customer-row-"]')
      .filter({ hasText: name })
      .first();
    const newId = (await newRow.locator('td').first().textContent())?.trim();
    expect(newId).toMatch(/^CUST-T\d{4}$/);

    // Reload the page (full HTTP round-trip).
    await page.reload();
    await page.locator('[data-testid=customers-search]').fill(newId!);
    await expect(page.locator(`[data-testid="customer-row-${newId}"]`)).toBeVisible({
      timeout: 10000,
    });
  });

  test('created claim survives full page reload + is openable', async ({ page }) => {
    await page.goto('/claims');
    await expect(page.locator('[data-testid^="claim-row-"]').first()).toBeVisible({
      timeout: 10000,
    });
    const stamp = Date.now().toString().slice(-6);
    const vehicle = `Camry Persist ${stamp}`;
    await page.locator('[data-testid=new-claim-open]').click();
    await page.locator('[data-testid=new-claim-vehicle]').fill(vehicle);
    await page.locator('[data-testid=new-claim-location]').fill('Local sandbox · persistence');
    await page.locator('[data-testid=new-claim-submit]').click();
    await page.waitForURL(/\/claims\/CLM-\d+$/);
    const url = page.url();
    const newClaimId = url.match(/\/claims\/(CLM-\d+)$/)![1];

    // Reload + go back to list + search by id.
    await page.reload();
    await page.goto('/claims');
    await page.locator('[data-testid=claims-search]').fill(newClaimId);
    await page.waitForTimeout(400);
    await expect(page.locator(`[data-testid=claim-row-${newClaimId}]`)).toBeVisible({
      timeout: 10000,
    });

    // Open it.
    await page.locator(`[data-testid=claim-row-${newClaimId}]`).click();
    await expect(page).toHaveURL(new RegExp(`/claims/${newClaimId}$`));
  });
});
