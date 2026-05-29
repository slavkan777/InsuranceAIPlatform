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

export default function LoginPage() {
  const dispatch = useAppDispatch();
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
    <div className="min-h-screen bg-gradient-to-br from-brand-50 via-white to-ai-50 flex items-center justify-center px-4" data-testid="login-page">
      <div className="w-full max-w-md">
        <div className="text-center mb-6">
          <div className="inline-flex w-14 h-14 rounded-2xl bg-gradient-to-br from-brand-500 to-brand-800 text-white items-center justify-center shadow-lg mb-3">
            <Icon name="shield" size={28} />
          </div>
          <h1 className="text-2xl font-bold text-ink-900">
            InsuranceAIPlatform
          </h1>
          <p className="text-sm text-ink-500 mt-1">
            Auto Claim AI Workbench · Local demo
          </p>
        </div>

        <form
          onSubmit={handleSubmit}
          className="card card-pad space-y-4 bg-white shadow-card"
        >
          <div>
            <label className="block text-xs font-semibold text-ink-700 uppercase tracking-wide mb-1.5">
              Логін
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
              Пароль
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
              {loginError}
            </div>
          ) : null}

          <button
            type="submit"
            data-testid="login-submit"
            className="w-full inline-flex items-center justify-center gap-2 px-4 py-2.5 rounded-xl bg-brand-600 hover:bg-brand-800 text-white text-sm font-semibold shadow-[0_4px_14px_rgba(37,99,235,0.35)] transition-colors"
          >
            <Icon name="check" size={15} />
            Увійти
          </button>

          <div className="border-t border-ink-100 pt-3">
            <p className="text-[11px] font-semibold uppercase tracking-wider text-ink-500 mb-1.5">
              Підказка демо-доступу
            </p>
            <div className="text-xs font-mono bg-ink-50 border border-ink-200 rounded-lg p-2.5 space-y-1">
              <div>
                <span className="text-ink-500">Логін: </span>
                <span className="text-ink-900 font-semibold">
                  {DEMO_CREDENTIALS.login}
                </span>
              </div>
              <div>
                <span className="text-ink-500">Пароль: </span>
                <span className="text-ink-900 font-semibold">
                  {DEMO_CREDENTIALS.password}
                </span>
              </div>
            </div>
            <p className="text-[10px] text-ink-400 mt-2 leading-snug">
              Локальна демо-автентифікація. Без зовнішнього провайдера ідентичності,
              без Azure AD, без реальних персональних даних. Сесія зберігається в
              localStorage браузера.
            </p>
          </div>
        </form>
      </div>
    </div>
  );
}
