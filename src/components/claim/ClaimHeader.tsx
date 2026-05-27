import { goldenClaim } from '@/data/mock/claims';
import { StatusPill } from '@/components/ui/StatusPill';
import { useAppSelector } from '@/app/hooks';
import { selectClaimDetail } from '@/features/claims/claimWorkspaceSelectors';

export function ClaimHeader() {
  // Header reflects the saga-loaded claim detail (backend data in backend-mode);
  // fall back to goldenClaim when null so mock mode stays byte-identical.
  const claimDetail = useAppSelector(selectClaimDetail);
  const c = claimDetail ?? goldenClaim;
  return (
    <div className="card card-pad flex flex-wrap items-start gap-5 justify-between">
      <div className="min-w-0">
        <div className="flex items-center gap-3 mb-2">
          <h1 className="text-2xl font-bold text-ink-900 tracking-tight">
            {c.id} — {c.customer}
          </h1>
          <StatusPill tone="warn">{c.status}</StatusPill>
        </div>
        <p className="text-sm text-ink-500">
          {c.eventType} · {c.vehicle} · {c.policy} · ДТП {c.eventDate}, м. Бориспіль
        </p>
      </div>
      <div className="flex flex-wrap items-center gap-2">
        <StatusPill tone="danger">
          Високий ризик · <span className="font-mono">{c.riskScore}/100</span>
        </StatusPill>
        <StatusPill tone="info">
          Впевненість моделі · <span className="font-mono">{c.confidence}%</span>
        </StatusPill>
        <StatusPill tone="warn">SLA · {c.slaDeadline}</StatusPill>
      </div>
    </div>
  );
}
