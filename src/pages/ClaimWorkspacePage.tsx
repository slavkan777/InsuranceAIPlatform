import { useNavigate, useParams } from 'react-router-dom';
import { useAppSelector } from '@/app/hooks';
import { ClaimHeader } from '@/components/claim/ClaimHeader';
import { Timeline } from '@/components/claim/Timeline';
import { StatusPill } from '@/components/ui/StatusPill';
import { ProgressBar } from '@/components/ui/ProgressBar';
import {
  damagePhotos,
  evidenceTabs,
  keyRisks,
  stoInvoiceLines,
} from '@/data/mock/claim-1006';
import { selectClaimDetail } from '@/features/claims/claimWorkspaceSelectors';
import { useI18n } from '@/i18n/useI18n';
import clsx from '@/utils/clsx';

const GOLDEN_CLAIM_ID = 'CLM-1006';

/**
 * Claim workspace overview page.
 *
 * Two render branches:
 *   1. Golden CLM-1006 — keeps the rich demo fixtures (timeline, photos, STO
 *      invoice, key-risks, evidence tabs) — that's the demo claim.
 *   2. DB-created claims (CLM-1011+) — renders the SUBMITTED sandbox fields
 *      honestly (customer, vehicle, VIN, event type/date/location, description,
 *      status, risk). No CLM-1006 fixtures leak in.
 *
 * Bug PostManualV4 root cause:
 *   - The page used `claimDetail ?? goldenClaim` which silently substituted
 *     CLM-1006 data into any non-CLM-1006 detail page that had not yet loaded.
 *   - Combined with rootSaga.ts boot-time `loadClaimDetail('CLM-1006')` and
 *     no per-route re-dispatch, the Redux state stayed locked on CLM-1006
 *     forever, and the page rendered it on every route.
 *   - Bottom-rail "Передати на перевірку" / "Підготувати рішення" buttons
 *     navigated to `/claims/CLM-1006/...` (hardcoded), so even if the user
 *     was looking at CLM-1032, those buttons jumped them to CLM-1006.
 *
 * Fix: only render loaded detail when its id matches the current route, and
 * route every navigation back through useParams().claimId.
 */
export default function ClaimWorkspacePage() {
  const navigate = useNavigate();
  const { claimId = '' } = useParams();
  const claimDetail = useAppSelector(selectClaimDetail);
  const { t } = useI18n();

  const isLoadedForThisRoute = claimDetail?.id === claimId;
  const isGoldenClaim = isLoadedForThisRoute && claimId === GOLDEN_CLAIM_ID;
  const c = isLoadedForThisRoute ? claimDetail! : null;

  if (!c) {
    return (
      <div className="flex flex-col gap-5">
        <ClaimHeader />
        <div
          className="card card-pad text-sm text-ink-600"
          data-testid="claim-detail-loading"
        >
          {t.claimWorkspace.loadingPrefix}{' '}
          <span className="font-mono">{claimId}</span>
          {t.claimWorkspace.loadingSuffix}
        </div>
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-5">
      <ClaimHeader />

      <div className="grid xl:grid-cols-[1fr_360px] gap-5">
        <div className="flex flex-col gap-5">
          <section className="card card-pad" data-testid="claim-detail-description">
            <div className="section-title mb-2">{t.claimWorkspace.sectionDescription}</div>
            <p
              className="text-ink-800"
              data-testid="claim-detail-description-text"
            >
              {c.description || t.claimWorkspace.descriptionFallback}
            </p>
            <div className="text-sm text-ink-500 mt-2">
              {t.claimWorkspace.labelLocation}:{' '}
              <span data-testid="claim-detail-location">{c.location || '—'}</span> ·{' '}
              {t.claimWorkspace.labelEventDate}:{' '}
              <span data-testid="claim-detail-event-date">{c.eventDate}</span> ·{' '}
              {t.claimWorkspace.labelEventType}:{' '}
              <span data-testid="claim-detail-event-type">{c.eventType}</span>
            </div>
            <div className="text-sm text-ink-500 mt-1">
              {t.claimWorkspace.labelCustomer}:{' '}
              <span data-testid="claim-detail-customer">{c.customer}</span>
              {c.customerId ? (
                <span className="text-ink-400"> ({c.customerId})</span>
              ) : null}
              {' · '}
              {t.claimWorkspace.labelVehicle}:{' '}
              <span data-testid="claim-detail-vehicle">{c.vehicle}</span>
              {c.vehicleVin ? (
                <span className="text-ink-400">
                  {' '}
                  · {t.claimWorkspace.labelVin}{' '}
                  <span data-testid="claim-detail-vin">{c.vehicleVin}</span>
                </span>
              ) : null}
            </div>
          </section>

          {isGoldenClaim ? (
            <>
              <Timeline />

              <div className="grid md:grid-cols-2 gap-5">
                <section className="card card-pad">
                  <div className="flex items-center justify-between mb-3">
                    <div className="section-title">{t.claimWorkspace.sectionPolicyCheck}</div>
                    <StatusPill tone="good">{t.claimWorkspace.policyStatusActive}</StatusPill>
                  </div>
                  <div className="text-base font-semibold text-ink-900">
                    Auto Comprehensive
                  </div>
                  <div className="text-sm text-ink-500 mt-1">
                    {t.claimWorkspace.policyValidity}
                  </div>
                  <dl className="grid grid-cols-2 gap-3 mt-4 text-sm">
                    <div>
                      <dt className="metric-label">{t.claimWorkspace.policyLabelDeductible}</dt>
                      <dd className="font-semibold text-ink-900 mt-1 font-mono">
                        ${c.deductible}
                      </dd>
                    </div>
                    <div>
                      <dt className="metric-label">{t.claimWorkspace.policyLabelLimit}</dt>
                      <dd className="font-semibold text-ink-900 mt-1 font-mono">
                        $50 000
                      </dd>
                    </div>
                  </dl>
                </section>

                <section className="card card-pad">
                  <div className="flex items-center justify-between mb-3">
                    <div className="section-title">{t.claimWorkspace.sectionDocuments}</div>
                    <StatusPill tone="warn">
                      {c.documentsReceived} {t.claimWorkspace.documentsOf} {c.documentsTotal}
                    </StatusPill>
                  </div>
                  <ProgressBar
                    value={c.documentsReceived}
                    max={c.documentsTotal}
                    tone="warn"
                  />
                  <p className="text-sm text-ink-600 mt-3">
                    {t.claimWorkspace.documentsLabelMissing}:{' '}
                    <span className="text-ink-900 font-medium">
                      {c.missingDocument}
                    </span>
                  </p>
                  <p className="text-xs text-danger-600 mt-1">
                    {t.claimWorkspace.documentsAiNote}
                  </p>
                </section>
              </div>

              <section className="card card-pad">
                <div className="flex items-center justify-between mb-3">
                  <div className="section-title">{t.claimWorkspace.sectionDamagePhotos}</div>
                  <span className="chip">{t.claimWorkspace.photosChip}</span>
                </div>
                <div className="grid grid-cols-3 gap-3">
                  {damagePhotos.map((p) => (
                    <div
                      key={p.id}
                      className={clsx(
                        'rounded-xl border aspect-[4/3] flex flex-col items-center justify-center text-center px-3 py-3',
                        p.missing
                          ? 'border-dashed border-danger-300 bg-danger-500/5'
                          : 'border-ink-100 bg-ink-50',
                      )}
                    >
                      <div
                        className={clsx(
                          'w-9 h-9 rounded-full grid place-items-center mb-2 text-base',
                          p.missing
                            ? 'bg-danger-500 text-white'
                            : 'bg-ink-200 text-ink-700',
                        )}
                      >
                        {p.missing ? '!' : '✓'}
                      </div>
                      <div className="text-sm font-semibold text-ink-900">
                        {p.label}
                      </div>
                      <div
                        className={clsx(
                          'text-[11px] mt-1',
                          p.missing
                            ? 'text-danger-600 font-semibold'
                            : 'text-ink-500',
                        )}
                      >
                        {p.missing
                          ? t.claimWorkspace.photoMissingLabel
                          : `AI conf ${p.confidence}%`}
                      </div>
                    </div>
                  ))}
                </div>
              </section>

              <section className="card card-pad">
                <div className="flex flex-wrap items-baseline justify-between mb-3 gap-2">
                  <div>
                    <div className="section-title">{t.claimWorkspace.sectionInvoice}</div>
                    <p className="text-sm text-ink-500 mt-0.5">
                      {t.claimWorkspace.invoiceSubtitle}
                    </p>
                  </div>
                  <StatusPill tone="warn">{t.claimWorkspace.invoiceAboveMedian}</StatusPill>
                </div>
                <table className="w-full text-sm">
                  <tbody className="divide-y divide-ink-100">
                    {stoInvoiceLines.map((line) => (
                      <tr key={line.id}>
                        <td className="py-2 text-ink-700">{line.label}</td>
                        <td className="py-2 text-right font-mono font-semibold text-ink-900">
                          ${line.value.toLocaleString('uk-UA')}
                        </td>
                      </tr>
                    ))}
                    <tr className="bg-ink-50">
                      <td className="py-2.5 px-2 font-semibold text-ink-900">
                        {t.claimWorkspace.invoiceTotalLabel}
                      </td>
                      <td className="py-2.5 px-2 text-right font-mono font-bold text-ink-900">
                        ${c.estimate.toLocaleString('uk-UA')}
                      </td>
                    </tr>
                  </tbody>
                </table>
              </section>
            </>
          ) : (
            // DB-created claim: honest sandbox card, no CLM-1006 fixtures.
            <section
              className="card card-pad"
              data-testid="claim-detail-sandbox-notice"
            >
              <div className="section-title mb-2">{t.claimWorkspace.sectionSandbox}</div>
              <p className="text-sm text-ink-600">
                {t.claimWorkspace.sandboxBody}
              </p>
              <dl className="grid grid-cols-2 gap-3 mt-4 text-sm">
                <div>
                  <dt className="metric-label">{t.claimWorkspace.sandboxLabelStatus}</dt>
                  <dd
                    className="font-semibold text-ink-900 mt-1"
                    data-testid="claim-detail-status"
                  >
                    {c.status}
                  </dd>
                </div>
                <div>
                  <dt className="metric-label">{t.claimWorkspace.sandboxLabelRisk}</dt>
                  <dd
                    className="font-semibold text-ink-900 mt-1"
                    data-testid="claim-detail-risk"
                  >
                    {c.risk} · {c.riskScore}/100
                  </dd>
                </div>
                <div>
                  <dt className="metric-label">{t.claimWorkspace.sandboxLabelPolicy}</dt>
                  <dd className="font-semibold text-ink-900 mt-1">
                    {c.policy}
                    {c.policyId ? (
                      <span className="text-ink-400 font-mono">
                        {' '}
                        · {c.policyId}
                      </span>
                    ) : null}
                  </dd>
                </div>
                <div>
                  <dt className="metric-label">{t.claimWorkspace.sandboxLabelSla}</dt>
                  <dd className="font-semibold text-ink-900 mt-1">
                    {c.slaDeadline}
                  </dd>
                </div>
              </dl>
            </section>
          )}
        </div>

        <aside className="flex flex-col gap-5">
          {isGoldenClaim ? (
            <>
              <section className="card card-pad">
                <div className="metric-label mb-1 text-ai-600">
                  {t.claimWorkspace.aiRecommendationLabel}
                </div>
                <h4 className="text-base font-semibold text-ink-900">
                  {t.claimWorkspace.aiRecommendationHeading}
                </h4>
                <p className="text-sm text-ink-600 mt-2 leading-snug">
                  {t.claimWorkspace.aiRecommendationBody}
                </p>
                <div className="mt-4">
                  <ProgressBar
                    value={c.confidence}
                    tone="ai"
                    label={t.claimWorkspace.aiConfidenceLabel}
                  />
                </div>
              </section>

              <section className="card card-pad">
                <div className="section-title mb-2">{t.claimWorkspace.sectionKeyRisks}</div>
                <ul className="space-y-2 text-sm">
                  {keyRisks.map((r) => (
                    <li key={r} className="flex gap-2 items-start">
                      <span className="mt-1.5 w-1.5 h-1.5 rounded-full bg-danger-500 shrink-0" />
                      <span className="text-ink-700 leading-snug">{r}</span>
                    </li>
                  ))}
                  <li className="flex gap-2 items-start">
                    <span className="mt-1.5 w-1.5 h-1.5 rounded-full bg-good-500 shrink-0" />
                    <span className="text-ink-700 leading-snug">
                      {t.claimWorkspace.keyRiskCoverageConfirmed}
                    </span>
                  </li>
                </ul>
              </section>

              <section className="card card-pad">
                <div className="section-title mb-2">{t.claimWorkspace.sectionEvidence}</div>
                <div className="flex flex-wrap gap-1.5">
                  {evidenceTabs.map((e) => (
                    <span key={e} className="chip">
                      {e}
                    </span>
                  ))}
                </div>
              </section>
            </>
          ) : (
            <section className="card card-pad" data-testid="claim-detail-context">
              <div className="section-title mb-2">{t.claimWorkspace.sectionQuickContext}</div>
              <p className="text-sm text-ink-600">
                {t.claimWorkspace.quickContextBody}
              </p>
            </section>
          )}

          <section className="card card-pad bg-gradient-to-br from-brand-50 to-white border-brand-200">
            <div className="metric-label text-brand-700">
              {t.claimWorkspace.nextActionLabel}
            </div>
            <h4 className="text-base font-semibold text-ink-900 mt-1">
              {isGoldenClaim
                ? t.claimWorkspace.nextActionHeadingGolden
                : t.claimWorkspace.nextActionHeadingDefault}
            </h4>
            <p className="text-sm text-ink-600 mt-2 leading-snug">
              {t.claimWorkspace.nextActionBody}
            </p>
            <button
              onClick={() => navigate(`/claims/${claimId}/documents`)}
              className="btn-primary w-full mt-4"
              data-testid="open-documents-collection"
            >
              {t.claimWorkspace.btnOpenDocuments}
            </button>
          </section>
        </aside>
      </div>

      <div className="card card-pad flex flex-wrap gap-2 justify-end">
        <button
          onClick={() => navigate('/claims')}
          className="btn-ghost"
          data-testid="back-to-list"
        >
          {t.claimWorkspace.btnBackToList}
        </button>
        <button
          onClick={() => navigate(`/claims/${claimId}/ai-evidence`)}
          className="btn-secondary"
          data-testid="open-ai-evidence"
        >
          {t.claimWorkspace.btnSendForReview}
        </button>
        <button
          onClick={() => navigate(`/claims/${claimId}/approval`)}
          className="btn-primary"
          data-testid="open-approval"
        >
          {t.claimWorkspace.btnPrepareDecision}
        </button>
      </div>
    </div>
  );
}
