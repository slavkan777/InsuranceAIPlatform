import { useEffect } from 'react';
import { useAppDispatch, useAppSelector } from '@/app/hooks';
import { fetchAuditHistory } from '@/features/rag/ragSlice';
import {
  selectRagAuditHistory,
  selectRagAuditStatus,
} from '@/features/rag/ragSelectors';
import { useI18n } from '@/i18n/useI18n';

interface Props {
  claimId: string;
}

/**
 * RAG Audit History panel.
 *
 * Dispatches fetchAuditHistory on mount so persisted audit rows re-hydrate
 * from the backend (or deterministic mock) on every page load, including after
 * a full browser reload. This closes the persistence gap identified in spec 22.
 *
 * Testids: rag-audit-history (container), rag-audit-row (each row),
 *          rag-audit-empty (empty state), rag-audit-loading (loading state),
 *          rag-audit-error (error state).
 */
export function RagAuditHistoryPanel({ claimId }: Props) {
  const dispatch = useAppDispatch();
  const { t } = useI18n();
  const tr = t.rag;

  const auditStatus = useAppSelector(selectRagAuditStatus);
  const auditHistory = useAppSelector(selectRagAuditHistory);

  // Re-hydrate from backend / mock on every mount (including after page.reload()).
  useEffect(() => {
    dispatch(fetchAuditHistory(claimId));
  }, [dispatch, claimId]);

  return (
    <section
      className="card card-pad border-ink-200"
      data-testid="rag-audit-history"
    >
      {/* Panel header */}
      <div className="metric-label text-ink-700 mb-3">{tr.auditHistoryTitle}</div>

      {/* Loading state */}
      {auditStatus === 'loading' && (
        <div
          className="rounded-lg border border-ink-100 bg-ink-50 px-4 py-3 text-sm text-ink-500 animate-pulse"
          data-testid="rag-audit-loading"
        >
          {tr.auditHistoryLoading}
        </div>
      )}

      {/* Error state */}
      {auditStatus === 'error' && (
        <div
          className="rounded-lg border border-danger-200 bg-danger-50 px-4 py-3 text-sm text-danger-700"
          data-testid="rag-audit-error"
        >
          <span className="font-semibold">{tr.auditHistoryError}</span>
        </div>
      )}

      {/* Empty state */}
      {auditStatus === 'ready' && auditHistory.length === 0 && (
        <div
          className="rounded-lg border border-ink-100 bg-ink-50 px-4 py-3 text-sm text-ink-500"
          data-testid="rag-audit-empty"
        >
          {tr.auditHistoryEmpty}
        </div>
      )}

      {/* Row list */}
      {auditStatus === 'ready' && auditHistory.length > 0 && (
        <div className="space-y-2">
          {auditHistory.map((row) => (
            <div
              key={row.traceId}
              className="rounded-lg border border-ink-100 bg-white px-4 py-3 space-y-1"
              data-testid="rag-audit-row"
            >
              {/* Header row: useCase + confidence + date */}
              <div className="flex flex-wrap items-center justify-between gap-2">
                <span className="px-2 py-0.5 rounded text-[10px] font-mono font-semibold bg-ai-100 text-ai-700 border border-ai-200">
                  {row.useCase}
                </span>
                <div className="flex items-center gap-3 text-xs text-ink-500">
                  <span>
                    {tr.auditColConfidence}:{' '}
                    <span className="font-semibold text-ai-700">
                      {(row.confidence * 100).toFixed(0)}%
                    </span>
                  </span>
                  <span className="font-mono text-ink-400">
                    {new Date(row.createdAtUtc).toLocaleString('en-GB', {
                      dateStyle: 'short',
                      timeStyle: 'short',
                    })}
                  </span>
                </div>
              </div>

              {/* Query snippet */}
              <p className="text-xs text-ink-500 truncate" title={row.query}>
                <span className="font-semibold text-ink-600">{tr.auditColQuery}: </span>
                {row.query}
              </p>

              {/* Answer snippet (first 120 chars) */}
              <p className="text-xs text-ink-700 line-clamp-2" title={row.answer}>
                <span className="font-semibold text-ink-600">{tr.auditColAnswer}: </span>
                {row.answer.length > 120 ? `${row.answer.slice(0, 120)}…` : row.answer}
              </p>
            </div>
          ))}
        </div>
      )}
    </section>
  );
}
