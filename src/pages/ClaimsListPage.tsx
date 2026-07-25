import { useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '@/app/hooks';
import { MetricCard } from '@/components/ui/MetricCard';
import { StatusPill } from '@/components/ui/StatusPill';
import { SectionHeader } from '@/components/ui/SectionHeader';
import { Icon } from '@/components/ui/Icon';
import { NewClaimModal } from '@/components/claim/NewClaimModal';
import { ImportDocumentMetadataModal } from '@/components/claim/ImportDocumentMetadataModal';
import { claimRows } from '@/data/mock/claims';
import { claimsListMetrics } from '@/data/mock/dashboard';
import {
  setSearch,
  setSegment,
  setFilter,
  setSelected,
  loadClaimsQueue,
} from '@/features/claims/claimsSlice';
import {
  selectClaimsState,
  selectClaimsQueue,
  selectClaimsApiMode,
  selectClaimsError,
} from '@/features/claims/claimsSelectors';
import { pushToast } from '@/features/ui/uiFeedbackSlice';
import { buildCsv, downloadBlob, localDateStamp } from '@/utils/csv';
import clsx from '@/utils/clsx';
import type { ClaimRow } from '@/types';
import {
  AI_STATUS_LABELS,
  CLAIM_STATUS_LABELS,
  EVENT_TYPE_LABELS,
  FILTER_ALL,
  RISK_LEVEL_LABELS,
  aiStatusLabel,
  aiStatusTone,
  claimStatusLabel,
  claimStatusTone,
  eventTypeLabel,
  isSlaOverdue,
  riskLevelLabel,
  riskLevelTone,
  toAiStatusCode,
  toClaimStatusCode,
  toEventTypeCode,
  toRiskLevelCode,
  type ClaimSegmentCode,
} from '@/utils/claimContract';
import { useI18n } from '@/i18n/useI18n';

export default function ClaimsListPage() {
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const { t } = useI18n();
  const { search, segment, filters } = useAppSelector(selectClaimsState);
  // Source rows from the saga-loaded store (backend data in backend-mode); fall back to
  // the static mock when the store list is empty so MOCK MODE stays byte-identical
  // to the accepted baseline.
  const storeRows = useAppSelector(selectClaimsQueue);
  const sourceRows = storeRows && storeRows.length > 0 ? storeRows : claimRows;
  const apiMode = useAppSelector(selectClaimsApiMode);
  const claimsError = useAppSelector(selectClaimsError);
  const [newClaimOpen, setNewClaimOpen] = useState(false);
  const [importOpen, setImportOpen] = useState(false);

  // Refresh the queue from the backend on every mount and every time a refresh
  // is signalled (e.g. after a successful create). Without this, mock rows
  // persist forever in backend mode and Slava bug 4 ("created claim not in
  // search") reproduces.
  useEffect(() => {
    dispatch(loadClaimsQueue());
  }, [dispatch]);

  // Apply UI search/filter/segment to the source rows. Previously the table
  // rendered `rows.map(...)` unfiltered — the search box and dropdowns did
  // literally nothing. This is the actual fix for Slava bug 4.
  const rows = useMemo(() => filterClaimRows(sourceRows, search, segment, filters),
    [sourceRows, search, segment, filters]);

  function openClaim(id: string) {
    dispatch(setSelected(id));
    navigate(`/claims/${id}`);
  }

  function handleExportCsv() {
    const csv = buildCsv<ClaimRow>(rows, [
      { header: 'ClaimId', accessor: (r) => r.id },
      { header: 'Customer', accessor: (r) => r.customer },
      { header: 'Vehicle', accessor: (r) => r.vehicle },
      { header: 'EventType', accessor: (r) => eventTypeLabel(r.eventType) },
      { header: 'Status', accessor: (r) => claimStatusLabel(r.status) },
      { header: 'Documents', accessor: (r) => r.documentsCount },
      { header: 'AiStatus', accessor: (r) => aiStatusLabel(r.aiStatus) },
      { header: 'Risk', accessor: (r) => riskLevelLabel(r.risk) },
      { header: 'Sla', accessor: (r) => r.sla },
      { header: 'NextAction', accessor: (r) => r.nextAction },
      { header: 'Updated', accessor: (r) => r.updated },
    ]);
    const filename = `claims-${localDateStamp()}.csv`;
    downloadBlob(csv, filename);
    dispatch(
      pushToast({
        tone: 'success',
        title: `${t.claimsList.toastExportTitle} ${rows.length} ${t.claimsList.toastExportTitleSuffix}`,
        detail: `${filename} — ${t.claimsList.toastExportDetail}`,
      }),
    );
  }

  return (
    <div className="flex flex-col gap-6">
      <SectionHeader
        title={t.claimsList.pageTitle}
        subtitle={t.claimsList.pageSubtitle}
        actions={
          <>
            <button
              type="button"
              onClick={handleExportCsv}
              className="btn-secondary inline-flex items-center gap-1.5"
              title={t.claimsList.btnExportCsvTitle}
            >
              <Icon name="download" size={14} />
              {t.claimsList.btnExportCsv}
            </button>
            <button
              type="button"
              onClick={() => setImportOpen(true)}
              className="btn-secondary inline-flex items-center gap-1.5"
              title={t.claimsList.btnImportDocTitle}
            >
              <Icon name="upload" size={14} />
              {t.claimsList.btnImportDoc}
            </button>
            <button
              type="button"
              data-testid="new-claim-open"
              onClick={() => setNewClaimOpen(true)}
              className="btn-primary inline-flex items-center gap-1.5"
              title={t.claimsList.btnNewClaimTitle}
            >
              <Icon name="plus" size={14} />
              {t.claimsList.btnNewClaim}
            </button>
          </>
        }
      />

      <NewClaimModal
        open={newClaimOpen}
        onClose={() => {
          setNewClaimOpen(false);
          // After a successful (or cancelled) close, refresh the queue. Cheap
          // and idempotent — the saga is `takeLatest` so duplicate dispatches
          // get cancelled cleanly.
          dispatch(loadClaimsQueue());
        }}
      />
      <ImportDocumentMetadataModal
        open={importOpen}
        onClose={() => setImportOpen(false)}
      />
      <div data-testid="claims-list-page" />

      <section className="card card-pad grid md:grid-cols-6 gap-3">
        <label className="flex flex-col gap-1 md:col-span-2">
          <span className="metric-label">{t.claimsList.filterSearchLabel}</span>
          <input
            type="search"
            data-testid="claims-search"
            value={search}
            onChange={(e) => dispatch(setSearch(e.target.value))}
            placeholder={t.claimsList.filterSearchPlaceholder}
            className="rounded-lg border border-ink-200 bg-white px-3 py-2 text-sm focus-ring"
          />
        </label>
        {(
          [
            ['status', t.claimsList.filterStatusLabel, [
              { value: FILTER_ALL, label: 'All' },
              { value: 'InProgress', label: CLAIM_STATUS_LABELS.InProgress },
              { value: 'CollectingDocuments', label: CLAIM_STATUS_LABELS.CollectingDocuments },
              { value: 'Ready', label: CLAIM_STATUS_LABELS.Ready },
              { value: 'Completed', label: CLAIM_STATUS_LABELS.Completed },
            ]],
            ['risk', t.claimsList.filterRiskLabel, [
              { value: FILTER_ALL, label: 'All' },
              { value: 'Low', label: RISK_LEVEL_LABELS.Low },
              { value: 'Medium', label: RISK_LEVEL_LABELS.Medium },
              { value: 'High', label: RISK_LEVEL_LABELS.High },
            ]],
            ['eventType', t.claimsList.filterEventTypeLabel, [
              { value: FILTER_ALL, label: 'All' },
              { value: 'RoadAccident', label: EVENT_TYPE_LABELS.RoadAccident },
              { value: 'Parking', label: EVENT_TYPE_LABELS.Parking },
              { value: 'Collision', label: EVENT_TYPE_LABELS.Collision },
              { value: 'Damage', label: EVENT_TYPE_LABELS.Damage },
            ]],
            // Date filter is a documented no-op (see filterClaimRows).
            ['date', t.claimsList.filterDateLabel, [
              { value: 'Today', label: 'Today' },
              { value: '7 days', label: '7 days' },
              { value: '30 days', label: '30 days' },
            ]],
            ['aiStatus', t.claimsList.filterAiStatusLabel, [
              { value: FILTER_ALL, label: 'All' },
              { value: 'AiVerified', label: AI_STATUS_LABELS.AiVerified },
              { value: 'Processing', label: AI_STATUS_LABELS.Processing },
            ]],
          ] as [keyof typeof filters, string, readonly { value: string; label: string }[]][]
        ).map(([key, label, options]) => (
          <label key={key} className="flex flex-col gap-1">
            <span className="metric-label">{label}</span>
            <select
              value={filters[key]}
              onChange={(e) => dispatch(setFilter({ key, value: e.target.value }))}
              className="rounded-lg border border-ink-200 bg-white px-3 py-2 text-sm focus-ring"
            >
              {options.map((o) => (
                <option key={o.value} value={o.value}>
                  {o.label}
                </option>
              ))}
            </select>
          </label>
        ))}
      </section>

      <div className="grid xl:grid-cols-[1fr_320px] gap-5">
        <div className="flex flex-col gap-5 min-w-0">
          <section className="card overflow-hidden">
            <div className="px-5 py-4 flex flex-wrap items-center justify-between gap-3 border-b border-ink-100">
              <div>
                <h3 className="text-base font-semibold text-ink-900">{t.claimsList.queueTitle}</h3>
                <p className="text-xs text-ink-500 mt-0.5">
                  {t.claimsList.queueSortedBySla}
                  {apiMode === 'mock-fallback' && (
                    <span className="ml-2 text-warn-600" title={claimsError ?? undefined}>
                      {t.claimsList.queueDemoFallback}
                    </span>
                  )}
                </p>
              </div>
              <div className="flex flex-wrap gap-1.5">
                {([
                  ['All', t.claimsList.segAll, '53'],
                  ['Accident', t.claimsList.segAccident, '32'],
                  ['HighRisk', t.claimsList.segHighRisk, '7'],
                  ['AwaitingAi', t.claimsList.segAwaitingAi, '4'],
                  ['AwaitingDecision', t.claimsList.segAwaitingDecision, '5'],
                ] as [ClaimSegmentCode, string, string][]).map(([segKey, label, count]) => (
                  <button
                    key={segKey}
                    onClick={() => dispatch(setSegment(segKey))}
                    className={clsx(
                      'px-2.5 py-1 rounded-md text-xs font-semibold transition-colors',
                      segment === segKey
                        ? 'bg-ink-900 text-white'
                        : 'bg-ink-100 text-ink-600 hover:bg-ink-200',
                    )}
                  >
                    {label} <span className="opacity-70">({count})</span>
                  </button>
                ))}
              </div>
            </div>
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead className="bg-ink-50/80">
                  <tr>
                    <th className="table-th">{t.claimsList.thClaimId}</th>
                    <th className="table-th">{t.claimsList.thCustomerVehicle}</th>
                    <th className="table-th">{t.claimsList.thType}</th>
                    <th className="table-th">{t.claimsList.thStatus}</th>
                    <th className="table-th">{t.claimsList.thDocs}</th>
                    <th className="table-th">{t.claimsList.thAiStatus}</th>
                    <th className="table-th">{t.claimsList.thRisk}</th>
                    <th className="table-th">{t.claimsList.thSla}</th>
                    <th className="table-th">{t.claimsList.thNextAction}</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-ink-100" data-testid="claims-table-body">
                  {rows.length === 0 && (
                    <tr>
                      <td colSpan={9} className="table-td text-center text-ink-500 py-8" data-testid="claims-empty">
                        {t.claimsList.emptyState}
                      </td>
                    </tr>
                  )}
                  {rows.map((row) => (
                    <tr
                      key={row.id}
                      data-testid={`claim-row-${row.id}`}
                      onClick={() => openClaim(row.id)}
                      className="cursor-pointer hover:bg-ink-50 transition-colors"
                    >
                      <td className="table-td font-mono font-semibold text-brand-700">{row.id}</td>
                      <td className="table-td">
                        <div className="font-medium text-ink-900">{row.customer}</div>
                        <div className="text-xs text-ink-500">{row.vehicle}</div>
                      </td>
                      <td className="table-td text-ink-600">{eventTypeLabel(row.eventType)}</td>
                      <td className="table-td">
                        <StatusPill tone={claimStatusTone(row.status)}>
                          {claimStatusLabel(row.status)}
                        </StatusPill>
                      </td>
                      <td className="table-td">
                        <span className="chip">{row.documentsCount}</span>
                      </td>
                      <td className="table-td">
                        <StatusPill tone={aiStatusTone(row.aiStatus)}>
                          {aiStatusLabel(row.aiStatus)}
                        </StatusPill>
                      </td>
                      <td className="table-td">
                        <StatusPill tone={riskLevelTone(row.risk)}>
                          {riskLevelLabel(row.risk)}
                        </StatusPill>
                      </td>
                      <td className="table-td">
                        <span
                          className={clsx(
                            'text-sm font-medium',
                            isSlaOverdue(row.sla) ? 'text-danger-600' : 'text-ink-700',
                          )}
                        >
                          {row.sla}
                        </span>
                      </td>
                      <td className="table-td text-ink-600">{row.nextAction}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </section>
        </div>

        <aside className="flex flex-col gap-3">
          {claimsListMetrics.map((m) => (
            <MetricCard key={m.id} {...m} />
          ))}
        </aside>
      </div>
    </div>
  );
}

/**
 * Applies the visible UI controls (search input, filter dropdowns, segment
 * chips) to the source row set. Previously the table rendered `sourceRows`
 * directly — the search box and dropdowns had no effect on what was shown.
 *
 * Status / risk / AI-status are compared as CONTRACT CODES: each row value is
 * normalized first (`toClaimStatusCode` etc.), so rows carrying legacy Ukrainian
 * values from persisted data or the mock layer filter identically to code-shaped
 * rows from the backend. Display labels are never used in this logic.
 *
 * Filter semantics:
 *   - search: case-insensitive substring over id / customer / vehicle
 *   - status: status code match unless FILTER_ALL
 *   - risk:   risk code match unless FILTER_ALL
 *   - eventType: event type code match unless FILTER_ALL
 *   - aiStatus:  AI status code match unless FILTER_ALL
 *   - date:   no-op for now (string-formatted relative time is not filterable
 *             without the original ISO timestamp; tracked as Phase-2 polish)
 *
 * Segment semantics:
 *   - 'All'              : no extra filter
 *   - 'Accident'         : event type code === 'RoadAccident'
 *   - 'HighRisk'         : risk code === 'High'
 *   - 'AwaitingAi'       : AI status code === 'Processing'
 *   - 'AwaitingDecision' : status code === 'InProgress'
 */
export function filterClaimRows(
  sourceRows: ClaimRow[],
  search: string,
  segment: ClaimSegmentCode,
  filters: { status: string; risk: string; eventType: string; aiStatus: string; date: string },
): ClaimRow[] {
  const q = (search ?? '').trim().toLowerCase();
  return sourceRows.filter((r) => {
    if (q.length > 0) {
      const hay = `${r.id} ${r.customer} ${r.vehicle}`.toLowerCase();
      if (!hay.includes(q)) return false;
    }
    const statusCode = toClaimStatusCode(r.status);
    const riskCode = toRiskLevelCode(r.risk);
    const aiCode = toAiStatusCode(r.aiStatus);
    const eventCode = toEventTypeCode(r.eventType);

    if (filters.status && filters.status !== FILTER_ALL && statusCode !== filters.status) return false;
    if (filters.risk && filters.risk !== FILTER_ALL && riskCode !== filters.risk) return false;
    if (filters.eventType && filters.eventType !== FILTER_ALL && eventCode !== filters.eventType) return false;
    if (filters.aiStatus && filters.aiStatus !== FILTER_ALL && aiCode !== filters.aiStatus) return false;
    if (segment === 'Accident' && eventCode !== 'RoadAccident') return false;
    if (segment === 'HighRisk' && riskCode !== 'High') return false;
    if (segment === 'AwaitingAi' && aiCode !== 'Processing') return false;
    if (segment === 'AwaitingDecision' && statusCode !== 'InProgress') return false;
    return true;
  });
}
