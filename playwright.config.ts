import { defineConfig, devices } from '@playwright/test';

/**
 * Playwright config for the InsuranceAIPlatform local-sandbox E2E suite.
 *
 * Run mode:
 *   - One Chromium project (we target a single browser; this is a portfolio
 *     mockup, not a multi-browser product surface).
 *   - Single worker — tests touch the same LocalDB instance and mutate
 *     customers/claims; parallel workers would race on ID allocation and
 *     audit row ordering. Reliability > speed for this suite.
 *
 * Web servers:
 *   - Backend (.NET 9 API) at http://localhost:5284
 *   - Frontend (Vite dev) at http://localhost:5173
 *   - `reuseExistingServer: !process.env.CI` so an operator can leave them
 *     running across iterations while developing the tests.
 *
 * Reports:
 *   - HTML report into `playwright-report/` (open with `npx playwright
 *     show-report`).
 *   - Screenshots on failure into the report.
 *   - Trace on the first retry (cheap; gives a zip-shipped step-replay).
 */
export default defineConfig({
  testDir: './e2e',
  fullyParallel: false,
  forbidOnly: !!process.env.CI,
  // Local retries lifted 0 -> 1 so a rare transient flake auto-recovers AND
  // `trace: 'on-first-retry'` captures a step-replay for diagnosis. CI stays at 1.
  retries: 1,
  workers: 1,
  reporter: [
    ['list'],
    ['html', { open: 'never', outputFolder: 'playwright-report' }],
    ['json', { outputFile: 'playwright-report/results.json' }],
  ],
  use: {
    baseURL: 'http://localhost:5173',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
    actionTimeout: 15000,
    navigationTimeout: 30000,
  },
  expect: {
    timeout: 10000,
  },
  timeout: 60000,
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],
  webServer: [
    {
      // .NET BFF/API — listens on http://localhost:5284 by default
      // (see server/InsuranceAIPlatform.Api/Properties/launchSettings.json).
      command: 'dotnet run --project server/InsuranceAIPlatform.Api --no-launch-profile --urls http://localhost:5284',
      url: 'http://localhost:5284/api/bff/health',
      reuseExistingServer: !process.env.CI,
      timeout: 180_000,
      stdout: 'pipe',
      stderr: 'pipe',
    },
    {
      // Vite dev server. `.env.development` (committed) flips
      // VITE_INSURANCE_API_MODE=backend so the UI hits the real API.
      command: 'npm run dev -- --host 127.0.0.1 --port 5173 --strictPort',
      url: 'http://localhost:5173',
      reuseExistingServer: !process.env.CI,
      timeout: 90_000,
      stdout: 'pipe',
      stderr: 'pipe',
    },
  ],
});
