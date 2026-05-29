import { useParams } from 'react-router-dom';
import { StatusPill } from '@/components/ui/StatusPill';
import { useAppSelector } from '@/app/hooks';
import { selectClaimDetail } from '@/features/claims/claimWorkspaceSelectors';

/**
 * Header card for the claim workspace.
 *
 * Bug PostManualV4 root cause: previously this component used
 * `claimDetail ?? goldenClaim` — for any non-CLM-1006 route the goldenClaim
 * fallback leaked Роберт Джонсон / Toyota Camry 2021 into the header even
 * though the URL said the new id. The visible symptom Slava reported was
 * exactly this header.
 *
 * Fix: only render loaded data when it belongs to the current route claim.
 * Otherwise we show the route id with a "loading…" placeholder — never
 * another claim's customer/vehicle.
 */
export function ClaimHeader() {
  const { claimId } = useParams();
  const claimDetail = useAppSelector(selectClaimDetail);
  const matchesRoute = claimDetail?.id === claimId;
  const c = matchesRoute ? claimDetail! : null;

  return (
    <div
      className="card card-pad flex flex-wrap items-start gap-5 justify-between"
      data-testid="claim-header"
    >
      <div className="min-w-0">
        <div className="flex items-center gap-3 mb-2">
          <h1
            className="text-2xl font-bold text-ink-900 tracking-tight"
            data-testid="claim-header-title"
          >
            {claimId} — {c?.customer ?? '…'}
          </h1>
          {c ? <StatusPill tone="warn">{c.status}</StatusPill> : null}
        </div>
        <p className="text-sm text-ink-500" data-testid="claim-header-meta">
          {c ? (
            <>
              {c.eventType} · {c.vehicle} · {c.policy} · {c.eventDate}
              {c.location ? <>, {c.location}</> : null}
            </>
          ) : (
            <>Завантаження даних кейса {claimId}…</>
          )}
        </p>
      </div>
      {c ? (
        <div className="flex flex-wrap items-center gap-2">
          <StatusPill tone="danger">
            {c.risk} · <span className="font-mono">{c.riskScore}/100</span>
          </StatusPill>
          <StatusPill tone="info">
            Впевненість моделі · <span className="font-mono">{c.confidence}%</span>
          </StatusPill>
          <StatusPill tone="warn">SLA · {c.slaDeadline}</StatusPill>
        </div>
      ) : null}
    </div>
  );
}
