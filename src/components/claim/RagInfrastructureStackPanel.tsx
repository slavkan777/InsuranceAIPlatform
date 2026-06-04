import { useEffect } from 'react';
import { useAppDispatch, useAppSelector } from '@/app/hooks';
import { fetchInfrastructure, reindexInfrastructure } from '@/features/rag/ragSlice';
import {
  selectRagInfrastructure,
  selectRagInfraStatus,
} from '@/features/rag/ragSelectors';
import { useI18n } from '@/i18n/useI18n';
import clsx from '@/utils/clsx';

interface Props {
  claimId: string;
}

/** Map a status string to a badge colour token. */
function statusColor(status: string): 'green' | 'amber' | 'red' | 'neutral' {
  switch (status.toLowerCase()) {
    case 'healthy':
    case 'live_local':
      return 'green';
    case 'degraded':
    case 'skipped_not_available':
      return 'amber';
    case 'disabled':
      return 'neutral';
    default:
      return 'red'; // empty | unavailable | unknown
  }
}

function StatusBadge({
  status,
  testId,
}: {
  status: string;
  testId: string;
}) {
  const color = statusColor(status);
  return (
    <span
      data-testid={testId}
      className={clsx(
        'inline-flex items-center px-2 py-0.5 rounded text-[10px] font-mono font-semibold uppercase tracking-wide',
        color === 'green' && 'bg-good-100 text-good-700 border border-good-200',
        color === 'amber' && 'bg-warn-100 text-warn-700 border border-warn-200',
        color === 'red' && 'bg-danger-100 text-danger-700 border border-danger-200',
        color === 'neutral' && 'bg-ink-100 text-ink-500 border border-ink-200',
      )}
    >
      {status}
    </span>
  );
}

/**
 * RAG Infrastructure Stack panel.
 *
 * Dispatches fetchInfrastructure on mount to show the health of the three
 * local RAG pipeline layers: SQL Source of Truth, Evidence Memory Index, and
 * Local Reasoning Runtime. The runtime row is always advisory-framed as
 * disabled/mock — it never implies a live paid model is running.
 *
 * Testids:
 *   container:      rag-infra-stack
 *   layer rows:     rag-infra-sql, rag-infra-index, rag-infra-runtime
 *   status badges:  rag-infra-sql-status, rag-infra-index-status, rag-infra-runtime-status
 *   loading:        rag-infra-loading
 *   buttons:        rag-infra-check-btn, rag-infra-reindex-btn
 */
export function RagInfrastructureStackPanel({ claimId }: Props) {
  const dispatch = useAppDispatch();
  const { t } = useI18n();
  const tr = t.rag;

  const infraStatus = useAppSelector(selectRagInfraStatus);
  const infra = useAppSelector(selectRagInfrastructure);

  // Fetch on mount so the panel populates without any user interaction.
  useEffect(() => {
    dispatch(fetchInfrastructure(claimId));
  }, [dispatch, claimId]);

  const isLoading = infraStatus === 'loading';

  return (
    <section
      className="card card-pad border-ink-200"
      data-testid="rag-infra-stack"
    >
      {/* Panel header + action buttons */}
      <div className="flex flex-wrap items-start justify-between gap-3 mb-4">
        <div>
          <div className="metric-label text-ink-700">{tr.infraStackTitle}</div>
          <p className="text-sm text-ink-500 mt-0.5">{tr.infraStackSubtitle}</p>
        </div>
        <div className="flex gap-2">
          <button
            type="button"
            disabled={isLoading}
            data-testid="rag-infra-check-btn"
            onClick={() => dispatch(fetchInfrastructure(claimId))}
            className="px-3 py-1.5 rounded-lg text-sm font-semibold border border-ink-300 bg-white text-ink-700 hover:bg-ink-50 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {tr.infraBtnCheck}
          </button>
          <button
            type="button"
            disabled={isLoading}
            data-testid="rag-infra-reindex-btn"
            onClick={() => dispatch(reindexInfrastructure(claimId))}
            className="px-3 py-1.5 rounded-lg text-sm font-semibold border border-ai-300 bg-white text-ai-700 hover:bg-ai-50 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {tr.infraBtnReindex}
          </button>
        </div>
      </div>

      {/* Loading state */}
      {isLoading && (
        <div
          className="rounded-lg border border-ink-100 bg-ink-50 px-4 py-3 text-sm text-ink-500 animate-pulse"
          data-testid="rag-infra-loading"
        >
          {tr.infraLoading}
        </div>
      )}

      {/* Error state */}
      {infraStatus === 'error' && (
        <div className="rounded-lg border border-danger-200 bg-danger-50 px-4 py-3 text-sm text-danger-700">
          <span className="font-semibold">{tr.infraError}</span>
        </div>
      )}

      {/* Ready state — 3-layer stack */}
      {infraStatus === 'ready' && infra && (
        <div className="space-y-3">
          {/* --- Pipeline strip --- */}
          <div className="flex flex-wrap items-center gap-1 text-[11px] text-ink-500 mb-1">
            <span className="font-semibold text-ink-600 mr-1">{tr.infraPipelineLabel}:</span>
            {[
              tr.infraPipelineSql,
              tr.infraPipelineIndex,
              tr.infraPipelineRetrieval,
              tr.infraPipelineReasoning,
              tr.infraPipelineAudit,
              tr.infraPipelineHuman,
            ].map((step, i, arr) => (
              <span key={step} className="flex items-center gap-1">
                <span className="px-1.5 py-0.5 rounded bg-ink-100 text-ink-600 font-medium">
                  {step}
                </span>
                {i < arr.length - 1 && <span className="text-ink-300">→</span>}
              </span>
            ))}
          </div>

          {/* Layer 1: SQL Source of Truth */}
          <div
            className="rounded-lg border border-ink-100 bg-white px-4 py-3 space-y-2"
            data-testid="rag-infra-sql"
          >
            <div className="flex flex-wrap items-center justify-between gap-2">
              <span className="text-sm font-semibold text-ink-800">{tr.infraLayerSql}</span>
              <StatusBadge
                status={infra.sqlSourceOfTruth.status}
                testId="rag-infra-sql-status"
              />
            </div>
            <dl className="grid grid-cols-2 sm:grid-cols-4 gap-x-4 gap-y-1 text-xs">
              <InfraField label={tr.infraFieldPolicyClauses} value={infra.sqlSourceOfTruth.policyClauses} />
              <InfraField label={tr.infraFieldEvidenceChunks} value={infra.sqlSourceOfTruth.evidenceChunks} />
              <InfraField label={tr.infraFieldEvalQuestions} value={infra.sqlSourceOfTruth.evaluationQuestions} />
              <InfraField label={tr.infraFieldAuditTraces} value={infra.sqlSourceOfTruth.auditTraces} />
            </dl>
          </div>

          {/* Layer 2: Evidence Memory Index */}
          <div
            className="rounded-lg border border-ink-100 bg-white px-4 py-3 space-y-2"
            data-testid="rag-infra-index"
          >
            <div className="flex flex-wrap items-center justify-between gap-2">
              <span className="text-sm font-semibold text-ink-800">{tr.infraLayerIndex}</span>
              <StatusBadge
                status={infra.evidenceMemoryIndex.status}
                testId="rag-infra-index-status"
              />
            </div>
            <dl className="grid grid-cols-2 sm:grid-cols-4 gap-x-4 gap-y-1 text-xs">
              <InfraField
                label={tr.infraFieldEmbeddedChunks}
                value={`${infra.evidenceMemoryIndex.embeddedChunks} / ${infra.evidenceMemoryIndex.totalChunks}`}
              />
              <InfraField label={tr.infraFieldEmbeddingModel} value={infra.evidenceMemoryIndex.embeddingModel} mono />
              <InfraField label={tr.infraFieldDimensions} value={infra.evidenceMemoryIndex.dimensions} />
            </dl>
          </div>

          {/* Layer 3: Vector Runtime (Qdrant) — disabled/fallback-safe; in-process index serves when absent */}
          <div
            className="rounded-lg border border-ink-100 bg-white px-4 py-3 space-y-2"
            data-testid="rag-infra-vector"
          >
            <div className="flex flex-wrap items-center justify-between gap-2">
              <span className="text-sm font-semibold text-ink-800">{tr.infraLayerVector}</span>
              <StatusBadge
                status={infra.vectorRuntime.status}
                testId="rag-infra-vector-status"
              />
            </div>
            <dl className="grid grid-cols-2 sm:grid-cols-4 gap-x-4 gap-y-1 text-xs">
              <InfraField label={tr.infraFieldEnabled} value={String(infra.vectorRuntime.enabled)} />
              <InfraField label={tr.infraFieldBackend} value={infra.vectorRuntime.backend} mono />
              <InfraField label={tr.infraFieldReachable} value={String(infra.vectorRuntime.reachable)} />
              <InfraField label={tr.infraFieldEndpoint} value={String(infra.vectorRuntime.endpointConfigured)} />
            </dl>
            <p className="text-[11px] text-ink-500 italic leading-snug mt-1">
              {tr.infraVectorDisabledNote}
            </p>
          </div>

          {/* Layer 4: Local Reasoning Runtime */}
          <div
            className="rounded-lg border border-ink-100 bg-white px-4 py-3 space-y-2"
            data-testid="rag-infra-runtime"
          >
            <div className="flex flex-wrap items-center justify-between gap-2">
              <span className="text-sm font-semibold text-ink-800">{tr.infraLayerRuntime}</span>
              <StatusBadge
                status={infra.localReasoningRuntime.status}
                testId="rag-infra-runtime-status"
              />
            </div>
            <dl className="grid grid-cols-2 sm:grid-cols-4 gap-x-4 gap-y-1 text-xs">
              <InfraField label={tr.infraFieldEnabled} value={String(infra.localReasoningRuntime.enabled)} />
              <InfraField label={tr.infraFieldModel} value={infra.localReasoningRuntime.model} mono />
              <InfraField label={tr.infraFieldReachable} value={String(infra.localReasoningRuntime.reachable)} />
              <InfraField label={tr.infraFieldEndpoint} value={String(infra.localReasoningRuntime.endpointConfigured)} />
            </dl>
            {/* Advisory note — always visible for the runtime row */}
            <p className="text-[11px] text-ink-500 italic leading-snug mt-1">
              {tr.infraRuntimeDisabledNote}
            </p>
          </div>

          {/* Footer: generated timestamp + correlation id */}
          <div className="flex flex-wrap gap-4 text-[11px] text-ink-400 pt-1">
            <span>
              <span className="font-medium text-ink-500">{tr.infraGeneratedAt}:</span>{' '}
              <span className="font-mono">
                {new Date(infra.generatedAtUtc).toLocaleString('en-GB', {
                  dateStyle: 'short',
                  timeStyle: 'short',
                })}
              </span>
            </span>
            <span>
              <span className="font-medium text-ink-500">{tr.infraCorrelationId}:</span>{' '}
              <span className="font-mono">{infra.correlationId}</span>
            </span>
          </div>
        </div>
      )}
    </section>
  );
}

// ---------------------------------------------------------------------------
// Helper
// ---------------------------------------------------------------------------

function InfraField({
  label,
  value,
  mono = false,
}: {
  label: string;
  value: string | number;
  mono?: boolean;
}) {
  return (
    <div>
      <dt className="text-ink-400 uppercase tracking-wide text-[10px]">{label}</dt>
      <dd
        className={clsx(
          'text-ink-800 font-semibold mt-0.5',
          mono && 'font-mono text-[10px] break-all',
        )}
      >
        {value}
      </dd>
    </div>
  );
}
