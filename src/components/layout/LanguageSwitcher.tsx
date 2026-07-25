/**
 * Language switcher — INERT.
 *
 * The product is English-only: the Ukrainian catalogs were removed from the
 * active build and the runtime locale is fixed to `en` (see
 * `features/i18n/i18nSlice.ts`, which also coerces any locale persisted by an
 * older browser). There is therefore no language choice to offer.
 *
 * The component is deliberately retained — unmounted and rendering nothing —
 * rather than removed, so the file stays tracked for a future re-introduction
 * of multi-language support. It has no call sites.
 */
export function LanguageSwitcher(_props: { className?: string }) {
  return null;
}
