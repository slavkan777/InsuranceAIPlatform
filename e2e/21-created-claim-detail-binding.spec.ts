import { test, expect } from '@playwright/test';
import { login } from './helpers/auth';

/**
 * PostManualV4 regression — created claim detail binding.
 *
 * Slava found that creating a new claim from the UI navigates to
 * /claims/CLM-1032 (correct id) but the detail page renders
 * CLM-1006 — Роберт Джонсон — Toyota Camry 2021 (CLM-1006 fallback / stale
 * Redux state). All prior tests passed because they only asserted the URL,
 * never the rendered detail content.
 *
 * This regression:
 *   1. Creates a customer + claim with UNIQUE markers (timestamp + random tag).
 *   2. Captures the new CLM-#### id from the URL after submit.
 *   3. Opens /claims/{createdId} freshly (via list row click — closest to
 *      manual flow).
 *   4. Asserts the workspace header / breadcrumb / description carry the
 *      CREATED customer + vehicle + VIN + description.
 *   5. Asserts that CLM-1006 / Роберт Джонсон / Toyota Camry are NOT rendered
 *      as the page's primary claim subject (CLM-1006 may appear in unrelated
 *      copy on other pages but never as the active claim's id).
 *   6. Re-opens the same id directly via URL — same assertions hold.
 *   7. Drives "Передати на перевірку" / "Підготувати рішення" / "Відкрити
 *      збір документів" buttons and confirms they navigate within the created
 *      claim's nested routes (no hardcoded /claims/CLM-1006/... jumps).
 *
 * This spec FAILS on the pre-fix behaviour (header shows CLM-1006 — Роберт
 * Джонсон) and PASSES on the post-fix behaviour.
 */
test.describe('Created claim detail binding (PostManualV4 regression)', () => {
  test('created claim renders its own customer/vehicle/VIN/description — not CLM-1006', async ({
    page,
  }) => {
    await page.goto('/');
    await page.evaluate(() => window.localStorage.removeItem('iap.auth.demo.v1'));
    await login(page);

    const stamp = Date.now().toString().slice(-7);
    const tag = Math.random().toString(36).slice(2, 6).toUpperCase();
    const customerName = `E2E Detail Customer ${stamp}-${tag}`;
    const vehicleLabel = `E2E Detail Vehicle ${stamp}-${tag}`;
    const vehicleVin = `VIN-DETAIL-${stamp}`;
    const description = `Detail binding regression ${stamp}-${tag}`;
    const locationLabel = `E2E sandbox, Київ ${stamp}`;

    // --- Step 1: create the customer ---
    await page.goto('/customers');
    await page.locator('[data-testid=create-customer-open]').click();
    await page.locator('[data-testid=create-customer-fullName]').fill(customerName);
    await page
      .locator('[data-testid=create-customer-email]')
      .fill(`detail-${stamp}@synthetic.invalid`);
    await page.locator('[data-testid=create-customer-submit]').click();
    await expect(page.locator('[data-testid=create-customer-fullName]')).toBeHidden({
      timeout: 5000,
    });
    await expect(page.locator('[data-testid=customers-table]')).toContainText(customerName, {
      timeout: 10000,
    });
    const customerRow = page
      .locator('[data-testid^="customer-row-"]')
      .filter({ hasText: customerName })
      .first();
    const customerId = (await customerRow.locator('td').first().textContent())?.trim() ?? '';
    expect(customerId, 'allocated CUST-T#### id').toMatch(/^CUST-T\d{4}$/);

    // --- Step 2: create the claim with unique markers ---
    await page.goto('/claims');
    await page.locator('[data-testid=new-claim-open]').click();
    await page.locator('[data-testid=new-claim-customerId]').fill(customerId);
    await page.locator('[data-testid=new-claim-customerName]').fill(customerName);
    await page.locator('[data-testid=new-claim-vehicle]').fill(vehicleLabel);
    await page.locator('[data-testid=new-claim-vehicleVin]').fill(vehicleVin);
    await page.locator('[data-testid=new-claim-location]').fill(locationLabel);
    // Description textarea: locate by placeholder (no testid by spec — minimal change).
    await page.getByPlaceholder(/Короткий опис обставин/).fill(description);
    await page.locator('[data-testid=new-claim-submit]').click();

    // Submission navigates to /claims/{createdId}.
    await page.waitForURL(/\/claims\/CLM-\d+$/, { timeout: 15000 });
    const createdId = page.url().match(/\/claims\/(CLM-\d+)$/)![1];
    expect(createdId, 'expected backend-allocated CLM-#### id').not.toMatch(/CLM-MOCK-/);
    expect(createdId, 'created id should not be the golden CLM-1006').not.toBe('CLM-1006');

    // --- Step 3: assert workspace header carries created id, not CLM-1006 ---
    // Wait for the per-route loadClaimDetail saga to resolve.
    await expect(page.locator('[data-testid=claim-header-title]')).toContainText(createdId, {
      timeout: 10000,
    });
    await expect(page.locator('[data-testid=claim-header-title]')).toContainText(customerName, {
      timeout: 10000,
    });
    // Negative assertions: must NOT be the CLM-1006 customer/vehicle.
    const headerText =
      (await page.locator('[data-testid=claim-header-title]').textContent()) ?? '';
    expect(
      /CLM-1006/.test(headerText),
      'header must not show CLM-1006 for a created claim',
    ).toBe(false);
    expect(
      /Роберт Джонсон/.test(headerText),
      'header must not show the CLM-1006 customer for a created claim',
    ).toBe(false);

    // --- Step 4: assert breadcrumb (ClaimShell) carries created data ---
    await expect(page.locator('[data-testid=claim-shell-id]')).toHaveText(createdId);
    await expect(page.locator('[data-testid=claim-shell-customer]')).toContainText(customerName, {
      timeout: 10000,
    });
    await expect(page.locator('[data-testid=claim-shell-vehicle]')).toContainText(vehicleLabel);

    // --- Step 5: assert workspace description tile carries submitted fields ---
    await expect(page.locator('[data-testid=claim-detail-customer]')).toContainText(customerName);
    await expect(page.locator('[data-testid=claim-detail-vehicle]')).toContainText(vehicleLabel);
    await expect(page.locator('[data-testid=claim-detail-vin]')).toContainText(vehicleVin);
    await expect(page.locator('[data-testid=claim-detail-event-type]')).toContainText(/ДТП/);
    await expect(page.locator('[data-testid=claim-detail-location]')).toContainText(locationLabel);
    await expect(page.locator('[data-testid=claim-detail-description-text]')).toContainText(
      description,
    );

    // Sandbox notice present for non-CLM-1006 claims — confirms no rich
    // CLM-1006 fixtures (Timeline, damagePhotos, stoInvoiceLines) leaked in.
    await expect(page.locator('[data-testid=claim-detail-sandbox-notice]')).toBeVisible();

    // --- Step 6: list search + re-open from list (mimics manual flow) ---
    await page.goto('/claims');
    await expect(page.locator(`[data-testid=claim-row-${createdId}]`)).toBeVisible({
      timeout: 10000,
    });
    await page.locator('[data-testid=claims-search]').fill(createdId);
    await page.waitForTimeout(300);
    expect(await page.locator('[data-testid^="claim-row-"]').count()).toBe(1);
    await page.locator(`[data-testid=claim-row-${createdId}]`).click();
    await expect(page).toHaveURL(new RegExp(`/claims/${createdId}$`));
    await expect(page.locator('[data-testid=claim-header-title]')).toContainText(createdId, {
      timeout: 10000,
    });
    await expect(page.locator('[data-testid=claim-detail-customer]')).toContainText(customerName);

    // --- Step 7: bottom-rail buttons navigate WITHIN the created claim ---
    await page.locator('[data-testid=open-ai-evidence]').click();
    await expect(page).toHaveURL(new RegExp(`/claims/${createdId}/ai-evidence$`));
    expect(page.url(), 'must not jump to CLM-1006 ai-evidence').not.toContain('CLM-1006');
    await page.goto(`/claims/${createdId}`);
    await page.locator('[data-testid=open-approval]').click();
    await expect(page).toHaveURL(new RegExp(`/claims/${createdId}/approval$`));
    expect(page.url(), 'must not jump to CLM-1006 approval').not.toContain('CLM-1006');
    await page.goto(`/claims/${createdId}`);
    await page.locator('[data-testid=open-documents-collection]').click();
    await expect(page).toHaveURL(new RegExp(`/claims/${createdId}/documents$`));
    expect(page.url(), 'must not jump to CLM-1006 documents').not.toContain('CLM-1006');
  });

  test('CLM-1006 still renders its own golden data (no regression)', async ({ page }) => {
    await page.goto('/');
    await page.evaluate(() => window.localStorage.removeItem('iap.auth.demo.v1'));
    await login(page);
    await page.goto('/claims/CLM-1006');

    // Header must still show CLM-1006 + Роберт Джонсон.
    await expect(page.locator('[data-testid=claim-header-title]')).toContainText('CLM-1006', {
      timeout: 10000,
    });
    await expect(page.locator('[data-testid=claim-header-title]')).toContainText(
      /Роберт Джонсон/,
      { timeout: 10000 },
    );
    // Breadcrumb carries CLM-1006 + golden customer + vehicle.
    await expect(page.locator('[data-testid=claim-shell-id]')).toHaveText('CLM-1006');
    await expect(page.locator('[data-testid=claim-shell-customer]')).toContainText(
      /Роберт Джонсон/,
    );
    await expect(page.locator('[data-testid=claim-shell-vehicle]')).toContainText(/Toyota Camry/);
    // Description tile is the rich golden one, not the sandbox notice.
    await expect(page.locator('[data-testid=claim-detail-sandbox-notice]')).toHaveCount(0);
    // Bottom-rail buttons still navigate within CLM-1006.
    await page.locator('[data-testid=open-ai-evidence]').click();
    await expect(page).toHaveURL(/\/claims\/CLM-1006\/ai-evidence$/);
  });
});
