import { createSlice, type PayloadAction } from '@reduxjs/toolkit';

// -----------------------------------------------------------------------
// Product i18n — English default, Ukrainian opt-in
// -----------------------------------------------------------------------
// First-time visitors always get English. The user's manual choice (EN / UA)
// is persisted in localStorage and restored on the next visit. Browser /
// navigator language is deliberately NOT used to override the English default
// — the product ships English-first.
// -----------------------------------------------------------------------

export type Locale = 'en' | 'uk';

const STORAGE_KEY = 'iap.i18n.locale.v1';
const DEFAULT_LOCALE: Locale = 'en';

function loadLocale(): Locale {
  if (typeof window === 'undefined') return DEFAULT_LOCALE;
  try {
    const raw = window.localStorage.getItem(STORAGE_KEY);
    if (raw === 'en' || raw === 'uk') return raw;
  } catch {
    // ignore storage/security errors — fall back to the English default
  }
  return DEFAULT_LOCALE;
}

function persistLocale(locale: Locale) {
  if (typeof window === 'undefined') return;
  try {
    window.localStorage.setItem(STORAGE_KEY, locale);
  } catch {
    // ignore quota/security errors
  }
}

interface I18nState {
  locale: Locale;
}

const initialState: I18nState = { locale: loadLocale() };

const slice = createSlice({
  name: 'i18n',
  initialState,
  reducers: {
    setLocale(state, action: PayloadAction<Locale>) {
      state.locale = action.payload;
      persistLocale(action.payload);
    },
  },
});

export const { setLocale } = slice.actions;
export const selectLocale = (state: { i18n: I18nState }): Locale => state.i18n.locale;
export default slice.reducer;
