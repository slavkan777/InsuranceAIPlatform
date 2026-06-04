/**
 * Playwright config variant — mock-only mode for the RAG e2e suite.
 *
 * Differences from the main playwright.config.ts:
 *   - Only starts the Vite dev server (no .NET backend).
 *   - Forces VITE_INSURANCE_API_MODE=mock via the command env so the UI
 *     uses the in-memory mock API regardless of .env.development.
 *   - Single worker, Chromium only — same as the main config.
 *
 * Usage:
 *   npx playwright test 22-rag-evidence --config playwright.mock.config.ts --reporter=list
 */
import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './e2e',
  fullyParallel: false,
  forbidOnly: !!process.env.CI,
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
      // Vite dev server. .env.local sets VITE_INSURANCE_API_MODE=mock,
      // overriding .env.development (backend). No backend needed.
      command: 'npm run dev -- --host 127.0.0.1 --port 5173 --strictPort',
      url: 'http://localhost:5173',
      reuseExistingServer: !process.env.CI,
      timeout: 90_000,
      stdout: 'pipe',
      stderr: 'pipe',
    },
  ],
});
