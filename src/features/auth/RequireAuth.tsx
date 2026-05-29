import type { ReactNode } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAppSelector } from '@/app/hooks';
import { selectIsAuthenticated } from './authSelectors';

/**
 * Route guard for the demo app. Unauthenticated users are redirected to /login
 * and the original path is remembered so they land back where they were after
 * successful demo-login. This is local/demo only — not a production auth guard.
 */
export function RequireAuth({ children }: { children: ReactNode }) {
  const isAuthenticated = useAppSelector(selectIsAuthenticated);
  const location = useLocation();

  if (!isAuthenticated) {
    return (
      <Navigate
        to="/login"
        replace
        state={{ from: location.pathname + location.search }}
      />
    );
  }

  return <>{children}</>;
}
