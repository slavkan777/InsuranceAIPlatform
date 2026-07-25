import { createSlice } from '@reduxjs/toolkit';

// -----------------------------------------------------------------------
// Product i18n — English-only runtime
// -----------------------------------------------------------------------
// The product ships English only. The former EN/UA switcher has been removed
// and the Ukrainian catalogs are gone from the active build.
//
// Older browsers may still hold `uk` (or anything else) in the legacy
// localStorage key from a previous release. `loadLocale()` coerces ANY stored
// value to 'en' and rewrites the key, so a returning visitor can neither crash
// on a missing catalog nor restore a Ukrainian UI.
// -----------------------------------------------------------------------

export type Locale = 'en';

const STORAGE_KEY = 'iap.i18n.locale.v1';
const DEFAULT_LOCALE: Locale = 'en';

function loadLocale(): Locale {
  if (typeof window === 'undefined') return DEFAULT_LOCALE;
  try {
    // Any previously persisted value ('uk', a stale locale, or junk) is
    // migrated to English exactly once, on load.
    if (window.localStorage.getItem(STORAGE_KEY) !== DEFAULT_LOCALE) {
      window.localStorage.setItem(STORAGE_KEY, DEFAULT_LOCALE);
    }
  } catch {
    // ignore storage/security errors — English is the only outcome either way
  }
  return DEFAULT_LOCALE;
}

interface I18nState {
  locale: Locale;
}

const initialState: I18nState = { locale: loadLocale() };

const slice = createSlice({
  name: 'i18n',
  initialState,
  reducers: {},
});

export const selectLocale = (state: { i18n: I18nState }): Locale => state.i18n.locale;
export default slice.reducer;
