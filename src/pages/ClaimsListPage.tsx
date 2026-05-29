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
      { header: 'EventType', accessor: (r) => r.eventType },
      { header: 'Status', accessor: (r) => r.status },
      { header: 'Documents', accessor: (r) => r.documentsCount },
      { header: 'AiStatus', accessor: (r) => r.aiStatus },
      { header: 'Risk', accessor: (r) => r.risk },
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
            ['status', t.claimsList.filterStatusLabel, ['Усі', 'В роботі', 'Збір документів', 'Готова', 'Завершено']],
            ['risk', t.claimsList.filterRiskLabel, ['Усі', 'Низький', 'Середній', 'Високий']],
            ['eventType', t.claimsList.filterEventTypeLabel, ['Усі', 'ДТП', 'Паркування', 'Зіткнення', 'Пошкодження']],
            ['date', t.claimsList.filterDateLabel, ['Сьогодні', '7 днів', '30 днів']],
            ['aiStatus', t.claimsList.filterAiStatusLabel, ['Усі', 'AI-перевірено', 'Обробляється']],
          ] as [keyof typeof filters, string, readonly string[]][]
        ).map(([key, label, options]) => (
          <label key={key} className="flex flex-col gap-1">
            <span className="metric-label">{label}</span>
            <select
              value={filters[key]}
              onChange={(e) => dispatch(setFilter({ key, value: e.target.value }))}
              className="rounded-lg border border-ink-200 bg-white px-3 py-2 text-sm focus-ring"
            >
              {options.map((o) => (
                <option key={o} value={o}>
                  {o}
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
                  ['Усі', t.claimsList.segAll, '53'],
                  ['ДТП', t.claimsList.segAccident, '32'],
                  ['Високий ризик', t.claimsList.segHighRisk, '7'],
                  ['Чекає AI', t.claimsList.segAwaitingAi, '4'],
                  ['Чекає рішення', t.claimsList.segAwaitingDecision, '5'],
                ] as [typeof segment, string, string][]).map(([segKey, label, count]) => (
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
                      <td className="table-td text-ink-600">{row.eventType}</td>
                      <td className="table-td">
                        <StatusPill
                          tone={
                            row.status === 'Високий ризик'
                              ? 'danger'
                              : row.status === 'Готова'
                                ? 'good'
                                : row.status === 'AI-обробка'
                                  ? 'info'
                                  : row.status === 'Збір документів'
                                    ? 'warn'
                                    : 'muted'
                          }
                        >
                          {row.status}
                        </StatusPill>
                      </td>
                      <td className="table-td">
                        <span className="chip">{row.documentsCount}</span>
                      </td>
                      <td className="table-td">
                        <StatusPill
                          tone={
                            row.aiStatus === 'AI-перевірено'
                              ? 'good'
                              : row.aiStatus === 'Потрібна перевірка'
                                ? 'warn'
                                : row.aiStatus === 'Обробляється'
                                  ? 'info'
                                  : 'muted'
                          }
                        >
                          {row.aiStatus}
                        </StatusPill>
                      </td>
                      <td className="table-td">
                        <StatusPill
                          tone={
                            row.risk === 'Високий'
                              ? 'danger'
                              : row.risk === 'Середній'
                                ? 'warn'
                                : 'good'
                          }
                        >
                          {row.risk}
                        </StatusPill>
                      </td>
                      <td className="table-td">
                        <span
                          className={clsx(
                            'text-sm font-medium',
                            row.sla === 'Прострочено' ? 'text-danger-600' : 'text-ink-700',
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
 * Filter semantics:
 *   - search: case-insensitive substring over id / customer / vehicle
 *   - status: exact match unless 'Усі'
 *   - risk:   exact match unless 'Усі'
 *   - eventType: exact match unless 'Усі'
 *   - aiStatus:  exact match unless 'Усі'
 *   - date:   no-op for now (string-formatted relative time is not filterable
 *             without the original ISO timestamp; tracked as Phase-2 polish)
 *
 * Segment semantics:
 *   - 'Усі'           : no extra filter
 *   - 'ДТП'           : eventType === 'ДТП'
 *   - 'Високий ризик'  : risk === 'Високий'
 *   - 'Чекає AI'       : aiStatus === 'Обробляється'
 *   - 'Чекає рішення'  : status === 'В роботі'
 */
export function filterClaimRows(
  sourceRows: ClaimRow[],
  search: string,
  segment: 'Усі' | 'ДТП' | 'Високий ризик' | 'Чекає AI' | 'Чекає рішення',
  filters: { status: string; risk: string; eventType: string; aiStatus: string; date: string },
): ClaimRow[] {
  const q = (search ?? '').trim().toLowerCase();
  return sourceRows.filter((r) => {
    if (q.length > 0) {
      const hay = `${r.id} ${r.customer} ${r.vehicle}`.toLowerCase();
      if (!hay.includes(q)) return false;
    }
    if (filters.status && filters.status !== 'Усі' && r.status !== filters.status) return false;
    if (filters.risk && filters.risk !== 'Усі' && r.risk !== filters.risk) return false;
    if (filters.eventType && filters.eventType !== 'Усі' && r.eventType !== filters.eventType) return false;
    if (filters.aiStatus && filters.aiStatus !== 'Усі' && r.aiStatus !== filters.aiStatus) return false;
    if (segment === 'ДТП' && r.eventType !== 'ДТП') return false;
    if (segment === 'Високий ризик' && r.risk !== 'Високий') return false;
    if (segment === 'Чекає AI' && r.aiStatus !== 'Обробляється') return false;
    if (segment === 'Чекає рішення' && r.status !== 'В роботі') return false;
    return true;
  });
}
