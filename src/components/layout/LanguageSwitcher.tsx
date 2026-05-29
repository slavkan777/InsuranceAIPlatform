import type { Locale } from '@/features/i18n/i18nSlice';
import { useI18n } from '@/i18n/useI18n';

const OPTIONS: { code: Locale; label: string }[] = [
  { code: 'en', label: 'EN' },
  { code: 'uk', label: 'UA' },
];

/**
 * Compact EN / UA language switcher. Sits in the top-right header near the
 * profile control (and on the sign-in screen). Visible but not dominant;
 * keyboard- and click-operable with a clear selected state. The choice
 * persists in localStorage via the i18n slice.
 */
export function LanguageSwitcher({ className = '' }: { className?: string }) {
  const { locale, setLocale, t } = useI18n();
  return (
    <div
      role="group"
      aria-label={t.topbar.languageAria}
      className={
        'inline-flex items-center rounded-lg border border-ink-200 bg-ink-50 p-0.5 ' + className
      }
    >
      {OPTIONS.map((o) => {
        const active = locale === o.code;
        return (
          <button
            key={o.code}
            type="button"
            onClick={() => setLocale(o.code)}
            aria-pressed={active}
            className={
              'px-2 py-1 rounded-md text-[11px] font-semibold uppercase tracking-wide transition-colors focus-ring ' +
              (active
                ? 'bg-white text-brand-700 shadow-sm'
                : 'text-ink-500 hover:text-ink-700')
            }
          >
            {o.label}
          </button>
        );
      })}
    </div>
  );
}
