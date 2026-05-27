/**
 * Insurance API facade — picks backend or mock based on VITE_INSURANCE_API_MODE.
 *
 * Default: 'mock'  (build + dev always work even with no backend running)
 * Backend: set VITE_INSURANCE_API_MODE=backend in .env.local (never commit)
 *
 * Public function names are IDENTICAL to mockInsuranceApi so all sagas/callers
 * can import from '@/api/insuranceApi' without further changes.
 *
 * Mock-fallback: in backend mode, if a fetch fails because the backend is
 * unreachable, the call throws a BackendApiError — sagas catch it and set
 * error + apiMode:'mock-fallback' in state. It is NOT silently swapped.
 */

import { mockInsuranceApi } from './mockInsuranceApi';
import { backendInsuranceApi } from './backendInsuranceApi';

export type ApiMode = 'mock' | 'backend';

/** Runtime API mode — read once at module load. */
export const API_MODE: ApiMode = (() => {
  try {
    const v = (import.meta.env as unknown as Record<string, string>).VITE_INSURANCE_API_MODE;
    return v === 'backend' ? 'backend' : 'mock';
  } catch {
    return 'mock';
  }
})();

/** The active API implementation, selected by VITE_INSURANCE_API_MODE (default: mock). */
export const insuranceApi: typeof mockInsuranceApi =
  API_MODE === 'backend' ? backendInsuranceApi : mockInsuranceApi;

// Re-export the error class so sagas can instanceof-check without importing from backendInsuranceApi
export { BackendApiError } from './backendInsuranceApi';
