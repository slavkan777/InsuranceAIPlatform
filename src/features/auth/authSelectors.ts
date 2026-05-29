import type { RootState } from '@/app/store';

export const selectIsAuthenticated = (s: RootState) => s.auth.isAuthenticated;
export const selectAuthUser = (s: RootState) => s.auth.user;
export const selectLoginError = (s: RootState) => s.auth.loginError;
