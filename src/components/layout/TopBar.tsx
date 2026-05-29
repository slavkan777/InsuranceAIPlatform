import { useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '@/app/hooks';
import { startDemo, stopDemo } from '@/features/demo/demoSlice';
import { logout } from '@/features/auth/authSlice';
import { selectAuthUser } from '@/features/auth/authSelectors';
import { setSearch } from '@/features/claims/claimsSlice';
import { pushToast } from '@/features/ui/uiFeedbackSlice';
import { Icon } from '@/components/ui/Icon';

export function TopBar() {
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const demoActive = useAppSelector((s) => s.demo.active);
  const user = useAppSelector(selectAuthUser);
  const [searchValue, setSearchValue] = useState('');

  function handleRunDemo() {
    if (demoActive) {
      dispatch(stopDemo());
    } else {
      dispatch(startDemo());
      navigate('/demo');
    }
  }

  function handleSearchSubmit(e: FormEvent) {
    e.preventDefault();
    const v = searchValue.trim();
    dispatch(setSearch(v));
    navigate('/claims');
  }

  function handleLogout() {
    dispatch(logout());
    dispatch(
      pushToast({
        tone: 'info',
        title: 'Сесію демо-доступу завершено.',
        detail: 'Локальний токен очищено.',
      }),
    );
    navigate('/login', { replace: true });
  }

  const initials = (user?.displayName ?? 'СК')
    .split(' ')
    .map((w) => w[0])
    .slice(0, 2)
    .join('')
    .toUpperCase();

  return (
    <header className="h-16 px-6 lg:px-8 border-b border-ink-200 bg-white/90 backdrop-blur flex items-center gap-4">
      <form onSubmit={handleSearchSubmit} className="flex-1 max-w-xl relative">
        <span className="absolute left-3 top-1/2 -translate-y-1/2 text-ink-400">
          <Icon name="search" size={17} />
        </span>
        <input
          type="search"
          value={searchValue}
          onChange={(e) => setSearchValue(e.target.value)}
          placeholder="Пошук за номером, авто, клієнтом... (Enter)"
          title="Натисніть Enter — пошук виконається у списку випадків"
          className="w-full pl-10 pr-14 py-2.5 rounded-xl bg-ink-50 border border-ink-200 text-sm focus-ring placeholder:text-ink-400"
        />
        <span className="absolute right-3 top-1/2 -translate-y-1/2 text-[10px] text-ink-400 font-mono px-1.5 py-0.5 rounded border border-ink-200 bg-white">
          ↵
        </span>
      </form>

      <div className="flex items-center gap-2.5">
        <span className="hidden md:inline-flex items-center gap-2 px-2.5 py-1 rounded-md bg-good-50 border border-good-200 text-good-700 text-[11px] font-semibold uppercase tracking-wider">
          <span className="w-1.5 h-1.5 rounded-full bg-good-500 shadow-[0_0_8px_rgba(16,185,129,0.6)]" />
          Система готова
        </span>
        <span className="hidden md:inline-flex items-center gap-1.5 px-2.5 py-1 rounded-md bg-ai-50 border border-ai-200 text-ai-800 text-[11px] font-semibold uppercase tracking-wider">
          Local Sandbox
        </span>
        <button
          onClick={handleRunDemo}
          className={
            demoActive
              ? 'inline-flex items-center justify-center gap-2 px-3.5 py-2 rounded-xl bg-danger-500 hover:bg-danger-600 text-white text-sm font-semibold shadow-card transition-colors'
              : 'inline-flex items-center justify-center gap-2 px-3.5 py-2 rounded-xl bg-brand-600 hover:bg-brand-800 text-white text-sm font-semibold shadow-[0_4px_14px_rgba(37,99,235,0.35)] transition-colors'
          }
        >
          <Icon name={demoActive ? 'shield' : 'play'} size={15} />
          {demoActive ? 'Зупинити демо' : 'Приклад використання'}
        </button>
        <button
          type="button"
          disabled
          title="Довідка з'явиться у наступному релізі"
          className="w-9 h-9 rounded-lg text-ink-400 grid place-items-center transition cursor-not-allowed opacity-60"
          aria-label="Довідка — поки що недоступна"
        >
          <Icon name="help" size={18} />
        </button>
        <button
          type="button"
          disabled
          title="Центр сповіщень з'явиться у наступному релізі"
          className="relative w-9 h-9 rounded-lg text-ink-400 grid place-items-center transition cursor-not-allowed opacity-60"
          aria-label="Сповіщення — поки що недоступні"
        >
          <span className="absolute top-0.5 right-0.5 w-4 h-4 grid place-items-center rounded-full bg-danger-500 text-white text-[10px] font-bold">
            3
          </span>
          <Icon name="bell" size={18} />
        </button>
        <div
          className="w-9 h-9 rounded-full bg-gradient-to-br from-brand-500 to-brand-800 grid place-items-center text-white text-sm font-semibold ring-1 ring-brand-300/30"
          title={user?.login ?? 'demo user'}
        >
          {initials}
        </div>
        <button
          type="button"
          data-testid="logout-button"
          onClick={handleLogout}
          title="Вихід з демо-сесії"
          aria-label="Вийти з демо-сесії"
          className="inline-flex items-center gap-1.5 px-2.5 py-1.5 rounded-lg border border-ink-200 hover:bg-ink-50 text-ink-700 text-xs font-semibold transition-colors"
        >
          <Icon name="logOut" size={14} />
          Вихід
        </button>
      </div>
    </header>
  );
}
