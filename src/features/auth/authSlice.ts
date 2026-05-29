import { createSlice, type PayloadAction } from '@reduxjs/toolkit';

// -----------------------------------------------------------------------
// Demo-local auth — NOT production
// -----------------------------------------------------------------------
// This is a local/demo authentication mechanism for the portfolio reviewer
// click-through. It does not use any production identity provider, no Azure AD,
// no Entra ID, no OAuth provider, no Key Vault. Credentials are hard-coded
// in the source (visible to the operator under the login form). The session
// is persisted in localStorage and is reversible via the logout control.
// -----------------------------------------------------------------------

/** Demo-only allowed credentials. Visible to reviewer under login form by design. */
export const DEMO_CREDENTIALS = {
  login: 'demo@insurance.local',
  password: 'Demo123!',
  displayName: 'Demo Adjuster',
};

interface AuthState {
  isAuthenticated: boolean;
  user: { login: string; displayName: string } | null;
  loginError: string | null;
}

const STORAGE_KEY = 'iap.auth.demo.v1';

function loadFromStorage(): AuthState {
  if (typeof window === 'undefined') return initialEmpty;
  try {
    const raw = window.localStorage.getItem(STORAGE_KEY);
    if (!raw) return initialEmpty;
    const parsed = JSON.parse(raw) as { login?: string; displayName?: string };
    if (parsed?.login && parsed?.displayName) {
      return {
        isAuthenticated: true,
        user: { login: parsed.login, displayName: parsed.displayName },
        loginError: null,
      };
    }
  } catch {
    // ignore parse errors — treat as logged out
  }
  return initialEmpty;
}

function persistToStorage(user: AuthState['user']) {
  if (typeof window === 'undefined') return;
  try {
    if (user) {
      window.localStorage.setItem(STORAGE_KEY, JSON.stringify(user));
    } else {
      window.localStorage.removeItem(STORAGE_KEY);
    }
  } catch {
    // ignore quota/security errors
  }
}

const initialEmpty: AuthState = {
  isAuthenticated: false,
  user: null,
  loginError: null,
};

const initialState: AuthState = loadFromStorage();

const slice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    submitLogin(
      state,
      action: PayloadAction<{ login: string; password: string }>,
    ) {
      const { login, password } = action.payload;
      if (
        login.trim().toLowerCase() === DEMO_CREDENTIALS.login &&
        password === DEMO_CREDENTIALS.password
      ) {
        state.isAuthenticated = true;
        state.user = {
          login: DEMO_CREDENTIALS.login,
          displayName: DEMO_CREDENTIALS.displayName,
        };
        state.loginError = null;
        persistToStorage(state.user);
      } else {
        state.isAuthenticated = false;
        state.user = null;
        state.loginError =
          'Невірний логін або пароль. Скористайтесь підказкою під формою (демо-режим).';
        persistToStorage(null);
      }
    },
    logout(state) {
      state.isAuthenticated = false;
      state.user = null;
      state.loginError = null;
      persistToStorage(null);
    },
    clearLoginError(state) {
      state.loginError = null;
    },
  },
});

export const { submitLogin, logout, clearLoginError } = slice.actions;
export default slice.reducer;
