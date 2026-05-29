# Frontend i18n + Product Copy — V0.1

Product-wide internationalization for the Auto Insurance Claim AI Workbench SPA: **English by default**, **Ukrainian** via a visible switcher, and a copy rewrite that positions the app as a real B2B insurance claims operations platform (not a demo/portfolio artifact).

## Goal

- English is the default language for first-time visitors.
- A visible **EN / UA** switcher lets the user change language; the choice persists.
- The whole visible UI is covered (not just the landing page).
- All inaccurate / non-product copy is removed in both languages.

## Approach (lightweight, no new dependency)

No i18n library was added (keeps the bundle lean). The mechanism is a small typed message catalog + a Redux locale slice + a hook:

| Piece | File | Role |
|---|---|---|
| Locale state | `src/features/i18n/i18nSlice.ts` | `locale: 'en' \| 'uk'`; `setLocale`; **English default**; persists to `localStorage` (`iap.i18n.locale.v1`). Browser/`navigator` language deliberately does **not** override the English default. |
| Message catalog | `src/i18n/messages/*.ts` (+ `index.ts`) | One namespace file per area, each exporting `{ en, uk }`. |
| Hook | `src/i18n/useI18n.ts` | `const { t, locale, setLocale } = useI18n();` → `t.<namespace>.<key>`. |
| Switcher | `src/components/layout/LanguageSwitcher.tsx` | Compact `EN / UA` control. `role="group"`, `aria-pressed`, `aria-label`; keyboard/click; visible but not dominant. |

### Compile-time key parity

Each namespace enforces identical EN/UA keys at compile time:

```ts
const en = { key: 'English' };
type T = typeof en;
const uk: T = { key: 'Українською' }; // missing/extra key => TypeScript error
export const ns = { en, uk };
```

This makes a missing translation a build failure, not a silent runtime gap.

### Switcher placement

Top-right header, immediately left of the profile avatar in `TopBar` (on every authenticated page), and in the top-right strip of the sign-in screen (which has no avatar). Never inside hero content. English default; selection persisted.

## Default-language behavior

`i18nSlice.loadLocale()` reads `localStorage`; if absent (first-time visitor) it returns `'en'`. A manual switch writes the choice back. There is no `navigator.language` auto-detection — the product ships English-first by design.

## Coverage

| Area | Namespace | Component(s) |
|---|---|---|
| Sign-in / landing hero | `login` | `pages/LoginPage.tsx` (rebuilt as a product landing: hero + value bullets + sign-in card) |
| Left nav + system status | `sidebar` | `components/layout/Sidebar.tsx` |
| Top bar (search, badges, walkthrough, profile, sign-out) | `topbar` | `components/layout/TopBar.tsx` |
| Shared | `common` | app name / tagline / language label |
| Overview dashboard | `dashboard` | `pages/DashboardPage.tsx` |
| Claims list | `claimsList` | `pages/ClaimsListPage.tsx` |
| Claim workspace | `claimWorkspace` | `pages/ClaimWorkspacePage.tsx` |
| AI analysis & evidence | `aiEvidence` | `pages/AiEvidencePage.tsx` |
| Documents & photos | `documents` | `pages/DocumentsPhotosPage.tsx` |
| Risks & checks | `risks` | `pages/RisksChecksPage.tsx` |
| Human approval | `approval` | `pages/HumanApprovalPage.tsx` |
| Audit & cost | `audit` | `pages/AuditCostPage.tsx` |
| Policy & coverage | `policy` | `pages/PolicyCoveragePage.tsx` |
| Customer & vehicle | `customerVehicle` | `pages/CustomerVehiclePage.tsx` |
| Customer directory | `customers` | `pages/CustomersDirectoryPage.tsx` + `components/customers/CreateCustomerModal.tsx` |
| Claim breadcrumb + tabs | `claimShell` | `components/layout/ClaimShell.tsx` |
| Guided walkthrough | `demo` | `pages/DemoScenarioPage.tsx` |
| Modals + deferred-action button | `ui` | 6 claim/customer modals + `components/ui/DeferredActionButton.tsx` |

Also: `app/store.ts` registers the `i18n` reducer; `features/auth/authSlice.ts` now sets a stable error **code** (`'invalid'`) which the login page localizes; `package.json` description updated.

## Product copy direction

Old copy framed the app as a "walking skeleton / portfolio mockup" with an architecture-diagram and a "Portfolio message". That is **removed** and replaced with insurance-operations positioning:

- Hero: *AI-Assisted Insurance Claims Workbench* — review evidence, prioritize cases, detect risk signals, prepare auditable decisions.
- Walkthrough page: *Platform capabilities* (claim review workspace · AI evidence assistance · audit & governance · cloud operations) + an honest *Demo environment status* note.

### Forbidden product-tone terms (removed from visible UI)

`walking skeleton`, `portfolio`, `interview`, `Azure-ready`, `next phase` / `Наступна фаза`, `only frontend` / `лише frontend`, `Архітектура системи`, `Три шари`, `Portfolio message`. Verified absent in the built bundle (see deploy doc). (`Demo environment`, `next release`, `synthetic data` are allowed and used.)

## Known remaining strings (documented, deferred)

A category of **synthetic data-layer strings** was intentionally left for a follow-up pass and is **not** part of the localized UI chrome:

- `src/data/mock/claim-1006.ts` — golden-claim content: `keyFindings[].text/.detail`, `evidenceTabs[]`, `damagePhotos[].label`, `stoInvoiceLines[].label`, `modelConfidence[].label`, `extractedEntities[].field/value/source`, `keyRisks[]`.
- `src/data/mock/dashboard.ts` — fallback metric/label/legend text (`overviewMetrics`, `lifecyclePhases`, `auditToday`, `recentEvents`, `caseTypeBreakdown`, `processingTrend.series`).
- `src/pages/ClaimsListPage.tsx` filter `<option>` values + row `status/risk/aiStatus` values — these are **data values matched by filter / tone-derivation logic**, so translating the display text alone would break filtering. They need a value→label display map.

Why deferred: (1) on the live site the golden-claim panels are fed by the **API seed**, so localizing the frontend mock alone would not change live rendering; aligning them requires **backend seed changes**, which are out of scope for this frontend gate. (2) Filter/tone values are logic-coupled. Recommended for a `PRODUCT_I18N_QA_POLISH` pass (data-layer i18n + value→label maps + backend seed localization).

## Verification

- `tsc -b && vite build` → clean (key parity holds across all namespaces).
- Built bundle contains both EN and UA strings; no forbidden terms; no secrets.
- Live headless-Chromium smoke: English default on first load, UA switch flips the page, dashboard/nav English, walkthrough shows product copy with zero forbidden terms. See `docs/architecture/azure/AZURE_PRODUCT_I18N_EN_DEFAULT_UA_SWITCH_V0.1.md` and `docs/portfolio/screenshots/i18n-0{1..4}-*.png`.
