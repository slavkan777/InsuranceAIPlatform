import { NavLink } from 'react-router-dom';
import clsx from '@/utils/clsx';
import { Icon, type IconName } from '@/components/ui/Icon';
import { useI18n } from '@/i18n/useI18n';

interface NavItem {
  to: string;
  label: string;
  icon: IconName;
  end?: boolean;
  disabled?: boolean;
}

export function Sidebar() {
  const { t } = useI18n();

  // `end: true` is REQUIRED on every nav item that could otherwise prefix-match a
  // nested route. Without it, /claims would highlight at the same time as
  // /claims/CLM-1006/documents and the sidebar shows two active items (Slava bug 1).
  //
  // Each claim-detail sub-route also gets `end: true` so the parent "Claim workspace"
  // link doesn't activate twice when you navigate to /claims/CLM-1006/documents.
  const navItems: NavItem[] = [
    { to: '/', label: t.sidebar.navOverview, icon: 'grid', end: true },
    { to: '/claims', label: t.sidebar.navClaims, icon: 'list', end: true },
    { to: '/customers', label: t.sidebar.navCustomerDirectory, icon: 'users', end: true },
    { to: '/claims/CLM-1006', label: t.sidebar.navWorkspace, icon: 'layers', end: true },
    { to: '/claims/CLM-1006/documents', label: t.sidebar.navDocuments, icon: 'file', end: true },
    { to: '/claims/CLM-1006/ai-evidence', label: t.sidebar.navAiChecks, icon: 'cpu', end: true },
    { to: '/claims/CLM-1006/risks', label: t.sidebar.navRisks, icon: 'gauge', end: true },
    { to: '/claims/CLM-1006/approval', label: t.sidebar.navApproval, icon: 'userCheck', end: true },
    { to: '/claims/CLM-1006/audit', label: t.sidebar.navAudit, icon: 'receipt', end: true },
    { to: '/claims/CLM-1006/customer-vehicle', label: t.sidebar.navCustomerVehicle, icon: 'users', end: true },
    { to: '/claims/CLM-1006/policy', label: t.sidebar.navPolicies, icon: 'clipboard', end: true },
    { to: '#vehicles', label: t.sidebar.navVehicles, icon: 'car', disabled: true },
    { to: '#settings', label: t.sidebar.navSettings, icon: 'settings', disabled: true },
  ];

  const systemStatus = [
    { id: 'ui', label: t.sidebar.statusUiLabel, tone: 'good' as const, text: t.sidebar.statusUiText },
    { id: 'api', label: t.sidebar.statusApiLabel, tone: 'good' as const, text: t.sidebar.statusApiText },
    { id: 'db', label: t.sidebar.statusDataLabel, tone: 'good' as const, text: t.sidebar.statusDataText },
    { id: 'ai', label: t.sidebar.statusAiLabel, tone: 'demo' as const, text: t.sidebar.statusAiText },
    { id: 'storage', label: t.sidebar.statusStorageLabel, tone: 'demo' as const, text: t.sidebar.statusStorageText },
  ];

  return (
    <aside data-testid="sidebar" className="w-72 shrink-0 bg-[#0b1220] text-ink-200 flex flex-col border-r border-white/5">
      <div className="px-5 py-5 flex items-center gap-3 border-b border-white/5">
        <div className="w-10 h-10 rounded-xl bg-brand-600 grid place-items-center font-bold text-white text-[15px] tracking-tight shadow-[0_4px_14px_rgba(37,99,235,0.45)] ring-1 ring-brand-400/40">
          IA
        </div>
        <div className="min-w-0">
          <div className="text-sm font-semibold text-white tracking-tight leading-tight">
            {t.sidebar.brandName}
          </div>
          <div className="text-[11px] text-ink-400 leading-tight mt-0.5">{t.sidebar.brandTagline}</div>
        </div>
      </div>

      <div className="px-5 pt-5 pb-2 text-[10px] uppercase tracking-[0.14em] text-ink-500 font-semibold">
        {t.sidebar.navSection}
      </div>
      <nav className="px-3 flex-1 overflow-y-auto">
        <ul className="space-y-0.5">
          {navItems.map((item) =>
            item.disabled ? (
              <li key={item.to}>
                <span className="flex items-center gap-3 pl-3 pr-3 py-2 rounded-lg text-sm text-ink-500/55 border-l-[3px] border-transparent cursor-not-allowed select-none">
                  <Icon name={item.icon} size={18} className="text-ink-500/55" />
                  <span className="truncate">{item.label}</span>
                </span>
              </li>
            ) : (
              <li key={item.to}>
                <NavLink
                  to={item.to}
                  end={item.end}
                  data-testid={`sidebar-link-${item.to}`}
                  className={({ isActive }) =>
                    clsx(
                      'flex items-center gap-3 pl-3 pr-3 py-2 rounded-lg text-sm border-l-[3px] transition-colors focus-ring',
                      isActive
                        ? 'bg-brand-600/20 text-white border-brand-500 font-medium sidebar-link-active'
                        : 'text-ink-300 hover:bg-white/[0.05] border-transparent',
                    )
                  }
                >
                  {({ isActive }) => (
                    <>
                      <Icon
                        name={item.icon}
                        size={18}
                        className={isActive ? 'text-brand-300' : 'text-ink-400'}
                      />
                      <span className="truncate">{item.label}</span>
                    </>
                  )}
                </NavLink>
              </li>
            ),
          )}
        </ul>
      </nav>

      <div className="px-4 pb-4 pt-4 mt-2">
        <div className="rounded-xl bg-white/[0.03] border border-white/10 p-3.5">
          <div className="flex items-center justify-between mb-2.5">
            <span className="text-[10px] uppercase tracking-[0.14em] text-ink-500 font-semibold">
              {t.sidebar.statusTitle}
            </span>
            <span className="w-1.5 h-1.5 rounded-full bg-good-500 shadow-[0_0_8px_rgba(16,185,129,0.55)]" />
          </div>
          <div className="space-y-1.5">
            {systemStatus.map((s) => {
              const isGood = s.tone === 'good';
              return (
                <div key={s.id} className="flex items-center justify-between text-[12px]">
                  <span className="text-ink-400">{s.label}</span>
                  <span
                    className={clsx(
                      'inline-flex items-center gap-1.5 font-mono text-[11px]',
                      isGood ? 'text-good-400' : 'text-ai-300',
                    )}
                  >
                    <span
                      className={clsx(
                        'w-1.5 h-1.5 rounded-full',
                        isGood
                          ? 'bg-good-500 shadow-[0_0_6px_rgba(16,185,129,0.5)]'
                          : 'bg-ai-400 shadow-[0_0_6px_rgba(129,140,248,0.5)]',
                      )}
                    />
                    {s.text}
                  </span>
                </div>
              );
            })}
          </div>
          <div className="mt-3 pt-3 border-t border-white/10">
            <p className="text-[10px] leading-snug text-ink-500">
              {t.sidebar.footerNote}
            </p>
          </div>
        </div>
      </div>
    </aside>
  );
}
