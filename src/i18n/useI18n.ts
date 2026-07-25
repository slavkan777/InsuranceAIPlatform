import { useAppSelector } from '@/app/hooks';
import { selectLocale, type Locale } from '@/features/i18n/i18nSlice';
import { messages, type Messages } from './messages';

/**
 * Product-wide i18n hook.
 *
 * The product is English-only: there is no locale switcher and the catalog
 * resolves to English. The hook keeps returning `locale` so call sites and
 * accessible names stay stable.
 *
 * Usage: `const { t } = useI18n();` then `t.sidebar.dashboard`, etc.
 */
export function useI18n(): {
  locale: Locale;
  t: Messages;
} {
  const locale = useAppSelector(selectLocale);
  const t: Messages = messages[locale];
  return { locale, t };
}
