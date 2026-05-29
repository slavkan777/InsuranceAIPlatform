import { test, expect } from '@playwright/test';
import { login } from './helpers/auth';

/**
 * Zero-to-end flow — the Slava "real reviewer walks the product" scenario.
 *
 * This is the single chained test that proves the whole sandbox works
 * end-to-end through the browser:
 *   1. login
 *   2. create a synthetic customer
 *   3. create a claim for that new customer
 *   4. open the claim → assert it's openable, not CLM-MOCK-*
 *   5. (CLM-1006 is the rich-data claim, so subsequent rich-tab tests live
 *      in 06-claim-actions.spec.ts; a freshly created claim doesn't have
 *      seeded ai-evidence/policy/etc., so we don't drive those tabs here)
 *   6. logout
 */
test.describe('Zero-to-end browser walk', () => {
  test('full reviewer chain: login → create customer → create claim → open → logout', async ({
    page,
  }) => {
    await page.goto('/');
    await page.evaluate(() => window.localStorage.removeItem('iap.auth.demo.v1'));
    await login(page);

    // -------- 1. Create synthetic customer --------
    await page.goto('/customers');
    const stamp = Date.now().toString().slice(-6);
    const customerName = `Synthetic E2E Z2E ${stamp}`;
    await page.locator('[data-testid=create-customer-open]').click();
    await page.locator('[data-testid=create-customer-fullName]').fill(customerName);
    await page.locator('[data-testid=create-customer-email]').fill(`z2e-${stamp}@synthetic.invalid`);
    await page.locator('[data-testid=create-customer-submit]').click();
    await expect(page.locator('[data-testid=create-customer-fullName]')).toBeHidden({
      timeout: 5000,
    });
    await expect(page.locator('[data-testid=customers-table]')).toContainText(customerName, {
      timeout: 10000,
    });

    // Pull the newly allocated customer id from the table row.
    const newCustomerRow = page
      .locator('[data-testid^="customer-row-"]')
      .filter({ hasText: customerName })
      .first();
    const customerId = (await newCustomerRow.locator('td').first().textContent())?.trim();
    expect(customerId, 'expected backend-allocated CUST-T#### id').toMatch(/^CUST-T\d{4}$/);

    // -------- 2. Create claim for the new customer --------
    await page.goto('/claims');
    await page.locator('[data-testid=new-claim-open]').click();
    await page.locator('[data-testid=new-claim-customerId]').fill(customerId!);
    await page.locator('[data-testid=new-claim-customerName]').fill(customerName);
    const vehicleLabel = `Toyota Z2E ${stamp}`;
    await page.locator('[data-testid=new-claim-vehicle]').fill(vehicleLabel);
    await page.locator('[data-testid=new-claim-location]').fill('Local sandbox, Київ');
    await page.locator('[data-testid=new-claim-submit]').click();

    // Submission navigates to /claims/{newClaimId}. We need a real
    // CLM-#### id, not CLM-MOCK-*.
    await page.waitForURL(/\/claims\/CLM-\d+$/);
    const claimUrl = page.url();
    expect(claimUrl, `expected backend-allocated CLM-#### id`).not.toMatch(/CLM-MOCK-/);
    const newClaimMatch = claimUrl.match(/\/claims\/(CLM-\d+)$/);
    const newClaimId = newClaimMatch![1];

    // -------- 3. Back to /claims; new claim is searchable --------
    await page.goto('/claims');
    await expect(page.locator(`[data-testid=claim-row-${newClaimId}]`)).toBeVisible({
      timeout: 10000,
    });

    await page.locator('[data-testid=claims-search]').fill(newClaimId);
    await page.waitForTimeout(300);
    const visibleRows = await page.locator('[data-testid^="claim-row-"]').count();
    expect(visibleRows).toBe(1);

    // -------- 4. Open the new claim again from the list --------
    await page.locator(`[data-testid=claim-row-${newClaimId}]`).click();
    await expect(page).toHaveURL(new RegExp(`/claims/${newClaimId}$`));

    // -------- 5. Logout --------
    await page.locator('[data-testid=logout-button]').click();
    await page.waitForURL(/\/login$/);
  });
});
