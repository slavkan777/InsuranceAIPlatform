import { useNavigate } from 'react-router-dom';
import { useAppSelector } from '@/app/hooks';
import { StatusPill } from '@/components/ui/StatusPill';
import { goldenClaim } from '@/data/mock/claims';
import { communicationHistory as mockCommunicationHistory, previousClaims as mockPreviousClaims } from '@/data/mock/claim-1006';
import {
  selectClaimDetail,
  selectWorkspaceCustomerVehicle,
} from '@/features/claims/claimWorkspaceSelectors';
import { useI18n } from '@/i18n/useI18n';

export default function CustomerVehiclePage() {
  const navigate = useNavigate();
  const { t } = useI18n();

  // --- store selectors (with mock fallback) ---
  const claimDetailFromStore = useAppSelector(selectClaimDetail);
  const c = claimDetailFromStore ?? goldenClaim;

  const customerVehicleFromStore = useAppSelector(selectWorkspaceCustomerVehicle);
  const previousClaims = customerVehicleFromStore?.previousClaims ?? mockPreviousClaims;
  const communicationHistory = customerVehicleFromStore?.communicationHistory ?? mockCommunicationHistory;

  return (
    <div className="flex flex-col gap-5">
      <section className="card card-pad flex flex-wrap items-center gap-x-6 gap-y-3 justify-between">
        <div>
          <h2 className="text-xl font-bold text-ink-900">{t.customerVehicle.pageTitle}</h2>
          <p className="text-sm text-ink-500 mt-1">
            {c.customer} · {c.vehicle} · {t.customerVehicle.pageSubtitleContext} {c.id}
          </p>
        </div>
        <StatusPill tone="good">{t.customerVehicle.activeCustomerPill}</StatusPill>
      </section>

      <div className="grid xl:grid-cols-2 gap-5">
        <section className="card card-pad">
          <div className="flex items-center gap-4 mb-4">
            <div className="w-16 h-16 rounded-full bg-gradient-to-br from-brand-400 to-brand-700 grid place-items-center text-white text-lg font-semibold">
              РД
            </div>
            <div>
              <h3 className="text-lg font-semibold text-ink-900">{c.customer}</h3>
              <p className="text-xs text-ink-500 mt-0.5">
                {t.customerVehicle.customerSince} · <span className="font-mono">{c.customerId}</span>
              </p>
            </div>
          </div>
          <dl className="grid grid-cols-2 gap-3 text-sm">
            <div>
              <dt className="metric-label">{t.customerVehicle.customerPhone}</dt>
              <dd className="font-mono text-ink-800 mt-1">+1 (555) ***-2147</dd>
            </div>
            <div>
              <dt className="metric-label">{t.customerVehicle.customerEmail}</dt>
              <dd className="font-mono text-ink-800 mt-1">robert.j****@demo.com</dd>
            </div>
            <div>
              <dt className="metric-label">{t.customerVehicle.customerAddress}</dt>
              <dd className="text-ink-800 mt-1">{t.customerVehicle.customerAddressValue}</dd>
            </div>
            <div>
              <dt className="metric-label">{t.customerVehicle.customerRiskProfile}</dt>
              <dd className="text-ink-800 mt-1">{t.customerVehicle.customerRiskValue}</dd>
            </div>
            <div>
              <dt className="metric-label">{t.customerVehicle.customerPolicies}</dt>
              <dd className="text-ink-800 mt-1">{t.customerVehicle.customerPoliciesValue}</dd>
            </div>
          </dl>
        </section>

        <section className="card card-pad">
          <div className="flex items-center gap-4 mb-4">
            <div className="w-16 h-16 rounded-xl bg-ink-100 grid place-items-center text-xl">⌬</div>
            <div>
              <h3 className="text-lg font-semibold text-ink-900">{c.vehicle}</h3>
              <p className="text-xs text-ink-500 mt-0.5">
                {t.customerVehicle.vehicleSubtitle} · <span className="font-mono">{c.vehicleVin}</span> · {t.customerVehicle.vehicleInsured}
              </p>
            </div>
          </div>
          <dl className="grid grid-cols-2 gap-3 text-sm">
            <div>
              <dt className="metric-label">{t.customerVehicle.vehicleMileage}</dt>
              <dd className="font-mono text-ink-800 mt-1">{t.customerVehicle.vehicleMileageValue}</dd>
            </div>
            <div>
              <dt className="metric-label">{t.customerVehicle.vehicleColor}</dt>
              <dd className="text-ink-800 mt-1">{t.customerVehicle.vehicleColorValue}</dd>
            </div>
            <div>
              <dt className="metric-label">{t.customerVehicle.vehicleRegistration}</dt>
              <dd className="text-ink-800 mt-1">{t.customerVehicle.vehicleRegistrationValue}</dd>
            </div>
            <div>
              <dt className="metric-label">{t.customerVehicle.vehicleInsuredValue}</dt>
              <dd className="font-mono text-ink-800 mt-1">{t.customerVehicle.vehicleInsuredValueValue}</dd>
            </div>
            <div>
              <dt className="metric-label">{t.customerVehicle.vehicleRiskCategory}</dt>
              <dd className="text-ink-800 mt-1">{t.customerVehicle.vehicleRiskCategoryValue}</dd>
            </div>
          </dl>
        </section>
      </div>

      <div className="grid xl:grid-cols-3 gap-5">
        <section className="card card-pad">
          <div className="section-title mb-3">{t.customerVehicle.priorClaimsTitle}</div>
          <ul className="space-y-3">
            {previousClaims.length > 0 ? (
              previousClaims.map((p) => (
                <li key={p.id} className="flex items-start justify-between gap-3 text-sm">
                  <div className="min-w-0">
                    <div className="font-mono font-semibold text-brand-700">{p.id}</div>
                    <div className="text-ink-700">{p.label}</div>
                    <div className="text-xs text-ink-500 mt-0.5">{p.date}</div>
                  </div>
                  <div className="text-right text-xs text-ink-600">{p.amount}</div>
                </li>
              ))
            ) : (
              <li className="text-sm text-ink-500">{t.customerVehicle.priorClaimsEmpty}</li>
            )}
          </ul>
        </section>

        <section className="card card-pad">
          <div className="section-title mb-3">{t.customerVehicle.communicationHistoryTitle}</div>
          <ul className="space-y-3">
            {communicationHistory.map((row, idx) => (
              <li key={idx} className="flex items-center justify-between gap-3 text-sm">
                <div>
                  <div className="text-ink-700 font-medium">{row.channel}</div>
                  <div className="text-xs text-ink-500">{row.topic}</div>
                </div>
                <div className="text-xs font-mono text-ink-500">{row.when}</div>
              </li>
            ))}
          </ul>
        </section>

        <section className="card card-pad">
          <div className="section-title mb-3">{t.customerVehicle.relatedPoliciesTitle}</div>
          <div className="rounded-lg border border-ink-100 p-3 bg-ink-50">
            <div className="font-semibold text-ink-900">Auto Comprehensive</div>
            <div className="text-xs text-ink-500 mt-0.5">
              <span className="font-mono">{c.policyId}</span> · {t.customerVehicle.policyValidThrough}
            </div>
            <button
              type="button"
              onClick={() => navigate('/claims/CLM-1006/policy')}
              className="btn-ghost mt-2 text-xs"
            >
              {t.customerVehicle.policyDetailsLink}
            </button>
          </div>
          <div className="mt-4">
            <div className="section-title mb-2">{t.customerVehicle.customerDocumentsTitle}</div>
            <p className="text-sm text-ink-700">
              {t.customerVehicle.customerDocumentsCount} · {t.customerVehicle.customerDocumentsTypes}
            </p>
            <p className="text-xs text-ink-500 mt-1">
              {t.customerVehicle.customerDocumentsLastUpdated} {t.customerVehicle.customerDocumentsDate}
            </p>
          </div>
        </section>
      </div>

      <div className="card card-pad bg-gradient-to-r from-warn-500/5 to-ink-50 border-warn-200">
        <div className="flex flex-wrap items-center gap-3 justify-between">
          <div>
            <div className="metric-label text-warn-700">{t.customerVehicle.privacyLabel}</div>
            <p className="text-sm text-ink-700 mt-1">
              {t.customerVehicle.privacyNote}
            </p>
          </div>
          <StatusPill tone="warn">{t.customerVehicle.piiMaskedPill}</StatusPill>
        </div>
      </div>
    </div>
  );
}
