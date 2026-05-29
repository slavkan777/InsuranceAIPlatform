import { test, expect } from '@playwright/test';
import { login } from './helpers/auth';

/**
 * TEST 5 — Create claim for a synthetic customer (per gate spec; Slava bug 3 + 4).
 *
 * Pre-fix bugs:
 *   - Bug 3: new claim returned "CLM-MOCK-1001" because frontend was in mock
 *     mode by default. Fix: `.env.development` flips to backend mode.
 *   - Bug 4: search after creation returned nothing because
 *     ClaimsListPage.rows.map rendered the source rows unfiltered AND the
 *     queue was never reloaded after a create. Fix: useEffect dispatches
 *     loadClaimsQueue on mount + on modal close; filterClaimRows applied.
 */
test.describe('Claims list / create / search', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await page.evaluate(() => window.localStorage.removeItem('iap.auth.demo.v1'));
    await login(page);
  });

  test('claims list opens, default filters show all (not just ДТП)', async ({ page }) => {
    await page.goto('/claims');
    // After fix: default eventType filter is 'Усі' so existing seed rows render.
    // We expect more than zero rows; we don't pin the exact count because the
    // seed grows over the lifetime of the LocalDB instance.
    const rowCount = await page.locator('[data-testid^="claim-row-"]').count();
    expect(rowCount, 'expected at least one claim row on default filters').toBeGreaterThan(0);
  });

  test('create new claim → returned id is NOT CLM-MOCK-*', async ({ page }) => {
    await page.goto('/claims');
    await page.locator('[data-testid=new-claim-open]').click();
    await expect(page.locator('[data-testid=new-claim-vehicle]')).toBeVisible();

    // Synthetic body — no real PII.
    const stamp = Date.now().toString().slice(-6);
    const vehicleLabel = `Toyota E2E ${stamp}`;
    await page.locator('[data-testid=new-claim-vehicle]').fill(vehicleLabel);
    await page.locator('[data-testid=new-claim-location]').fill('Local sandbox, Київ');

    await page.locator('[data-testid=new-claim-submit]').click();

    // Submission navigates to /claims/{newClaimId}; URL should contain
    // CLM-#### (NOT CLM-MOCK-*). Wait for navigation.
    await page.waitForURL(/\/claims\/CLM-\d+$/);
    const url = page.url();
    expect(url, `expected backend-allocated CLM-#### id, got ${url}`).not.toMatch(/CLM-MOCK-/);

    // Pull the new id out of the URL for the next test.
    const match = url.match(/\/claims\/(CLM-\d+)$/);
    expect(match).not.toBeNull();
    const newClaimId = match![1];
    test.info().annotations.push({ type: 'createdClaimId', description: newClaimId });

    // Go back to /claims; the new row should now be in the list.
    await page.goto('/claims');
    await expect(page.locator(`[data-testid=claim-row-${newClaimId}]`)).toBeVisible({
      timeout: 10000,
    });

    // Search filter narrows to the new claim.
    await page.locator('[data-testid=claims-search]').fill(newClaimId);
    // Allow the controlled-input update to settle.
    await page.waitForTimeout(300);
    await expect(page.locator(`[data-testid=claim-row-${newClaimId}]`)).toBeVisible();
    const allVisibleRows = await page.locator('[data-testid^="claim-row-"]').count();
    expect(allVisibleRows, `expected only the matching new claim row to render`).toBe(1);
  });
});
