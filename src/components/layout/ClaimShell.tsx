import { NavLink, Outlet, useParams } from 'react-router-dom';
import { useEffect } from 'react';
import clsx from '@/utils/clsx';
import { useAppDispatch, useAppSelector } from '@/app/hooks';
import { loadClaimDetail } from '@/features/claims/claimWorkspaceSlice';
import { selectClaimDetail } from '@/features/claims/claimWorkspaceSelectors';
import { useI18n } from '@/i18n/useI18n';

export function ClaimShell() {
  const { t } = useI18n();
  const { claimId } = useParams();
  const dispatch = useAppDispatch();
  const claimDetail = useAppSelector(selectClaimDetail);

  // Module-level tabs array moved inside the component so it can reference `t`.
  const tabs = [
    { to: '', label: t.claimShell.tabOverview, end: true },
    { to: 'documents', label: t.claimShell.tabDocuments },
    { to: 'ai-evidence', label: t.claimShell.tabAiEvidence },
    { to: 'risks', label: t.claimShell.tabRisks },
    { to: 'approval', label: t.claimShell.tabApproval },
    { to: 'audit', label: t.claimShell.tabAudit },
    { to: 'policy', label: t.claimShell.tabPolicy },
    { to: 'customer-vehicle', label: t.claimShell.tabCustomerVehicle },
  ];

  // Fire the saga whenever the route claimId changes — this is the single
  // source of truth for "which claim's data lives in Redux right now".
  // Previously rootSaga.ts dispatched loadClaimDetail('CLM-1006') ONCE at boot
  // and no route handler ever re-dispatched, so every /claims/:claimId route
  // rendered CLM-1006 data regardless of URL (PostManualV4 bug).
  useEffect(() => {
    if (claimId) dispatch(loadClaimDetail(claimId));
  }, [claimId, dispatch]);

  // Breadcrumb labels use the loaded detail ONLY when it actually belongs to
  // the current route claim. Until the saga resolves we show "…" — never the
  // stale-claim customer/vehicle (that was the visible symptom of the bug).
  const headerMatchesRoute = claimDetail?.id === claimId;
  const customerLabel = headerMatchesRoute ? claimDetail!.customer : '…';
  const vehicleLabel = headerMatchesRoute ? claimDetail!.vehicle : '…';

  return (
    <div className="flex flex-col gap-5 min-w-0">
      <div className="flex flex-wrap items-center gap-3 text-sm">
        <NavLink to="/claims" className="text-ink-500 hover:text-brand-700">
          {t.claimShell.backToList}
        </NavLink>
        <span className="text-ink-300">/</span>
        <span className="font-semibold text-ink-900" data-testid="claim-shell-id">
          {claimId}
        </span>
        <span className="text-ink-400">·</span>
        <span className="text-ink-600" data-testid="claim-shell-customer">
          {customerLabel}
        </span>
        <span className="text-ink-400">·</span>
        <span className="text-ink-600" data-testid="claim-shell-vehicle">
          {vehicleLabel}
        </span>
      </div>

      <nav className="bg-white border border-ink-100 rounded-xl px-1.5 py-1 shadow-card overflow-x-auto">
        <ul className="flex gap-1 min-w-max">
          {tabs.map((tab) => (
            <li key={tab.to || 'root'}>
              <NavLink
                to={tab.to}
                end={tab.end}
                className={({ isActive }) =>
                  clsx(
                    'inline-flex items-center px-3 py-2 rounded-lg text-sm font-medium transition-colors',
                    isActive
                      ? 'bg-brand-600 text-white shadow-card'
                      : 'text-ink-600 hover:bg-ink-100',
                  )
                }
              >
                {tab.label}
              </NavLink>
            </li>
          ))}
        </ul>
      </nav>

      <Outlet />
    </div>
  );
}
