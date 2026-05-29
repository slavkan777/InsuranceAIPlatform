import { useAppDispatch, useAppSelector } from '@/app/hooks';
import { selectLocale, setLocale, type Locale } from '@/features/i18n/i18nSlice';
import { messages, type Messages } from './messages';

/**
 * Product-wide i18n hook.
 *
 * English is the default for first-time visitors; the chosen locale persists in
 * localStorage (see i18nSlice). Returns the active message catalog (`t`), the
 * current `locale`, and a `setLocale` setter wired to the EN / UA switcher.
 *
 * Usage: `const { t } = useI18n();` then `t.sidebar.dashboard`, etc.
 */
export function useI18n(): {
  locale: Locale;
  t: Messages;
  setLocale: (locale: Locale) => void;
} {
  const locale = useAppSelector(selectLocale);
  const dispatch = useAppDispatch();
  const t: Messages = messages[locale];
  return { locale, t, setLocale: (next: Locale) => dispatch(setLocale(next)) };
}
