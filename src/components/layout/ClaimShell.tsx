import { NavLink, Outlet, useParams } from 'react-router-dom';
import clsx from '@/utils/clsx';
import { goldenClaim } from '@/data/mock/claims';

const tabs = [
  { to: '', label: 'Робоче місце', end: true },
  { to: 'documents', label: 'Документи та фото' },
  { to: 'ai-evidence', label: 'AI-докази' },
  { to: 'risks', label: 'Ризики' },
  { to: 'approval', label: 'Погодження' },
  { to: 'audit', label: 'Audit & Cost' },
  { to: 'policy', label: 'Поліс' },
  { to: 'customer-vehicle', label: 'Клієнт + ТЗ' },
];

export function ClaimShell() {
  const { claimId } = useParams();
  const c = goldenClaim;
  const id = claimId || c.id;

  return (
    <div className="flex flex-col gap-5 min-w-0">
      <div className="flex flex-wrap items-center gap-3 text-sm">
        <NavLink to="/claims" className="text-ink-500 hover:text-brand-700">
          ← Повернутись до списку
        </NavLink>
        <span className="text-ink-300">/</span>
        <span className="font-semibold text-ink-900">{id}</span>
        <span className="text-ink-400">·</span>
        <span className="text-ink-600">{c.customer}</span>
        <span className="text-ink-400">·</span>
        <span className="text-ink-600">{c.vehicle}</span>
      </div>

      <nav className="bg-white border border-ink-100 rounded-xl px-1.5 py-1 shadow-card overflow-x-auto">
        <ul className="flex gap-1 min-w-max">
          {tabs.map((t) => (
            <li key={t.to || 'root'}>
              <NavLink
                to={t.to}
                end={t.end}
                className={({ isActive }) =>
                  clsx(
                    'inline-flex items-center px-3 py-2 rounded-lg text-sm font-medium transition-colors',
                    isActive
                      ? 'bg-brand-600 text-white shadow-card'
                      : 'text-ink-600 hover:bg-ink-100',
                  )
                }
              >
                {t.label}
              </NavLink>
            </li>
          ))}
        </ul>
      </nav>

      <Outlet />
    </div>
  );
}
