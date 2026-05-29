import { useState, type FormEvent } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '@/app/hooks';
import {
  submitLogin,
  clearLoginError,
  DEMO_CREDENTIALS,
} from '@/features/auth/authSlice';
import {
  selectIsAuthenticated,
  selectLoginError,
} from '@/features/auth/authSelectors';
import { Icon } from '@/components/ui/Icon';
import { LanguageSwitcher } from '@/components/layout/LanguageSwitcher';
import { useI18n } from '@/i18n/useI18n';

export default function LoginPage() {
  const dispatch = useAppDispatch();
  const { t } = useI18n();
  const isAuthenticated = useAppSelector(selectIsAuthenticated);
  const loginError = useAppSelector(selectLoginError);
  const location = useLocation();
  const [login, setLogin] = useState('');
  const [password, setPassword] = useState('');

  const fromPath = (location.state as { from?: string } | null)?.from ?? '/';

  if (isAuthenticated) {
    return <Navigate to={fromPath} replace />;
  }

  function handleSubmit(e: FormEvent) {
    e.preventDefault();
    dispatch(submitLogin({ login, password }));
  }

  function handleChange(setter: (v: string) => void) {
    return (v: string) => {
      setter(v);
      if (loginError) dispatch(clearLoginError());
    };
  }

  return (
    <div
      className="min-h-screen bg-gradient-to-br from-brand-50 via-white to-ai-50 flex flex-col"
      data-testid="login-page"
    >
      {/* Top strip: brand + language switcher (top-right) */}
      <div className="flex items-center justify-between px-6 lg:px-10 py-5">
        <div className="flex items-center gap-2.5">
          <div className="w-9 h-9 rounded-xl bg-gradient-to-br from-brand-500 to-brand-800 grid place-items-center text-white font-bold text-[13px] tracking-tight shadow-[0_4px_14px_rgba(37,99,235,0.4)]">
            IA
          </div>
          <span className="text-sm font-semibold text-ink-900 tracking-tight">
            {t.common.appName}
          </span>
        </div>
        <LanguageSwitcher />
      </div>

      {/* Main split: product hero (left) + sign-in card (right) */}
      <div className="flex-1 w-full max-w-6xl mx-auto px-6 lg:px-10 pb-12 grid lg:grid-cols-2 gap-10 items-center">
        {/* Product hero — communicates the platform, not a demo */}
        <div className="hidden lg:block">
          <p className="text-[11px] font-semibold uppercase tracking-[0.16em] text-brand-600">
            {t.login.heroEyebrow}
          </p>
          <h1 className="text-3xl xl:text-[2.5rem] font-bold text-ink-900 mt-3 leading-[1.12]">
            {t.login.heroTitle}
          </h1>
          <p className="text-ink-600 mt-4 text-base leading-relaxed max-w-xl">
            {t.login.heroSubtitle}
          </p>
          <ul className="mt-7 space-y-3">
            {t.login.valueBullets.map((bullet) => (
              <li key={bullet} className="flex items-start gap-3 text-sm text-ink-700">
                <span className="mt-0.5 inline-flex w-5 h-5 rounded-full bg-brand-100 text-brand-700 items-center justify-center shrink-0">
                  <Icon name="check" size={12} />
                </span>
                <span className="leading-snug">{bullet}</span>
              </li>
            ))}
          </ul>
        </div>

        {/* Sign-in card */}
        <div className="w-full max-w-md mx-auto lg:mx-0 lg:ml-auto">
          {/* Mobile-only condensed hero */}
          <div className="lg:hidden mb-6 text-center">
            <h1 className="text-xl font-bold text-ink-900 leading-tight">
              {t.login.heroTitle}
            </h1>
            <p className="text-sm text-ink-500 mt-1.5">{t.login.heroSubtitle}</p>
          </div>

          <form
            onSubmit={handleSubmit}
            className="card card-pad space-y-4 bg-white shadow-card"
          >
            <div className="mb-1">
              <h2 className="text-lg font-bold text-ink-900">{t.login.formTitle}</h2>
              <p className="text-xs text-ink-500 mt-0.5">{t.login.formSubtitle}</p>
            </div>

            <div>
              <label className="block text-xs font-semibold text-ink-700 uppercase tracking-wide mb-1.5">
                {t.login.emailLabel}
              </label>
              <input
                type="email"
                autoComplete="email"
                autoFocus
                required
                data-testid="login-input"
                value={login}
                onChange={(e) => handleChange(setLogin)(e.target.value)}
                placeholder={DEMO_CREDENTIALS.login}
                className="w-full px-3 py-2.5 rounded-lg border border-ink-200 bg-white text-sm focus-ring"
              />
            </div>
            <div>
              <label className="block text-xs font-semibold text-ink-700 uppercase tracking-wide mb-1.5">
                {t.login.passwordLabel}
              </label>
              <input
                type="password"
                autoComplete="current-password"
                required
                data-testid="login-password"
                value={password}
                onChange={(e) => handleChange(setPassword)(e.target.value)}
                placeholder="••••••••"
                className="w-full px-3 py-2.5 rounded-lg border border-ink-200 bg-white text-sm focus-ring"
              />
            </div>

            {loginError ? (
              <div
                role="alert"
                data-testid="login-error"
                className="rounded-lg border border-danger-200 bg-danger-50 text-danger-700 text-xs px-3 py-2"
              >
                {t.login.errorInvalid}
              </div>
            ) : null}

            <button
              type="submit"
              data-testid="login-submit"
              className="w-full inline-flex items-center justify-center gap-2 px-4 py-2.5 rounded-xl bg-brand-600 hover:bg-brand-800 text-white text-sm font-semibold shadow-[0_4px_14px_rgba(37,99,235,0.35)] transition-colors"
            >
              <Icon name="check" size={15} />
              {t.login.signInCta}
            </button>

            <div className="border-t border-ink-100 pt-3">
              <p className="text-[11px] font-semibold uppercase tracking-wider text-ink-500 mb-1.5">
                {t.login.demoHintTitle}
              </p>
              <div className="text-xs font-mono bg-ink-50 border border-ink-200 rounded-lg p-2.5 space-y-1">
                <div>
                  <span className="text-ink-500">{t.login.demoLoginLabel}: </span>
                  <span className="text-ink-900 font-semibold">
                    {DEMO_CREDENTIALS.login}
                  </span>
                </div>
                <div>
                  <span className="text-ink-500">{t.login.demoPasswordLabel}: </span>
                  <span className="text-ink-900 font-semibold">
                    {DEMO_CREDENTIALS.password}
                  </span>
                </div>
              </div>
              <p className="text-[10px] text-ink-400 mt-2 leading-snug">
                {t.login.demoNote}
              </p>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}
