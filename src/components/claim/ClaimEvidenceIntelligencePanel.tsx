import { useNavigate } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '@/app/hooks';
import {
  askRag,
  fetchSimilarClaims,
  ragReset,
  setActiveUseCase,
  setCustomQuestion,
} from '@/features/rag/ragSlice';
import {
  selectRagActiveUseCase,
  selectRagAskError,
  selectRagAskStatus,
  selectRagCustomQuestion,
  selectRagLastAnswer,
  selectSimilarClaims,
  selectSimilarClaimsError,
  selectSimilarClaimsStatus,
} from '@/features/rag/ragSelectors';
import { useI18n } from '@/i18n/useI18n';
import clsx from '@/utils/clsx';

// The six use-case buttons defined by the product spec.
const USE_CASE_BUTTONS: Array<{ useCase: string; labelKey: keyof RagMessages }> = [
  { useCase: 'coverage', labelKey: 'btnCoverage' },
  { useCase: 'missing_docs', labelKey: 'btnMissingDocs' },
  { useCase: 'risk', labelKey: 'btnRisk' },
  { useCase: 'similar', labelKey: 'btnSimilar' },
  { useCase: 'summary', labelKey: 'btnSummary' },
  { useCase: 'custom', labelKey: 'btnCustom' },
];

// Narrowed type for the rag namespace keys so we can use them as index.
type RagMessages = typeof import('@/i18n/messages/rag').rag.en;

interface Props {
  claimId: string;
}

/**
 * Claim Evidence Intelligence panel.
 *
 * Integrates into AiEvidencePage — mounts as a section after existing content.
 * Dispatches ragSlice actions → ragSaga → POST /api/claims/{id}/rag/ask.
 * Renders loading, error, empty, and answer card states.
 * Advisory-only banner is always visible in the answer area.
 */
export function ClaimEvidenceIntelligencePanel({ claimId }: Props) {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { t } = useI18n();
  const tr = t.rag;

  const askStatus = useAppSelector(selectRagAskStatus);
  const lastAnswer = useAppSelector(selectRagLastAnswer);
  const askError = useAppSelector(selectRagAskError);
  const activeUseCase = useAppSelector(selectRagActiveUseCase);
  const customQuestion = useAppSelector(selectRagCustomQuestion);

  const similarStatus = useAppSelector(selectSimilarClaimsStatus);
  const similarClaims = useAppSelector(selectSimilarClaims);
  const similarError = useAppSelector(selectSimilarClaimsError);

  // The panel is "busy" if either saga is in-flight.
  const isLoading = askStatus === 'loading' || similarStatus === 'loading';

  function handleUseCaseClick(useCase: string) {
    if (useCase === 'similar') {
      // Wire "Find similar claims" to the dedicated cross-claim endpoint.
      dispatch(setActiveUseCase('similar'));
      dispatch(fetchSimilarClaims(claimId));
    } else if (useCase !== 'custom') {
      dispatch(setActiveUseCase(useCase));
      const question = getCanonicalQuestion(useCase);
      dispatch(askRag(claimId, question, useCase));
    } else {
      // 'custom' just activates the input row; user presses "Ask" to send.
      dispatch(setActiveUseCase('custom'));
    }
  }

  function handleCustomSubmit() {
    const q = customQuestion.trim();
    if (!q) return;
    dispatch(askRag(claimId, q, 'custom'));
  }

  function handleReset() {
    dispatch(ragReset());
  }

  return (
    <section
      className="card card-pad border-ai-200 bg-gradient-to-br from-ai-50 to-white"
      data-testid="rag-panel"
    >
      {/* Panel header */}
      <div className="flex flex-wrap items-center justify-between gap-2 mb-4">
        <div>
          <div className="metric-label text-ai-700">{tr.panelTitle}</div>
          <p className="text-sm text-ink-500 mt-0.5">{tr.panelSubtitle}</p>
        </div>
        {(askStatus === 'succeeded' || askStatus === 'failed' ||
          similarStatus === 'succeeded' || similarStatus === 'failed') && (
          <button
            type="button"
            onClick={handleReset}
            className="btn-ghost text-xs"
            data-testid="rag-reset"
          >
            ↩ Reset
          </button>
        )}
      </div>

      {/* Use-case button grid */}
      <div className="flex flex-wrap gap-2 mb-4" data-testid="rag-use-case-buttons">
        {USE_CASE_BUTTONS.map(({ useCase, labelKey }) => (
          <button
            key={useCase}
            type="button"
            disabled={isLoading}
            data-testid={`rag-btn-${useCase}`}
            onClick={() => handleUseCaseClick(useCase)}
            className={clsx(
              'px-3 py-1.5 rounded-lg text-sm font-semibold border transition-colors disabled:opacity-50 disabled:cursor-not-allowed',
              activeUseCase === useCase
                ? 'bg-ai-600 text-white border-ai-600'
                : 'bg-white text-ai-700 border-ai-300 hover:bg-ai-50',
            )}
          >
            {tr[labelKey]}
          </button>
        ))}
      </div>

      {/* Custom question input — shown only when 'custom' is active */}
      {activeUseCase === 'custom' && (
        <div className="flex gap-2 mb-4" data-testid="rag-custom-input-row">
          <input
            type="text"
            value={customQuestion}
            onChange={(e) => dispatch(setCustomQuestion(e.target.value))}
            onKeyDown={(e) => {
              if (e.key === 'Enter') handleCustomSubmit();
            }}
            placeholder={tr.customPlaceholder}
            disabled={isLoading}
            className="flex-1 rounded-lg border border-ink-200 px-3 py-1.5 text-sm text-ink-900 focus:outline-none focus:ring-2 focus:ring-ai-400 disabled:opacity-50"
            data-testid="rag-custom-question-input"
          />
          <button
            type="button"
            onClick={handleCustomSubmit}
            disabled={isLoading || !customQuestion.trim()}
            className="btn-primary disabled:opacity-50 disabled:cursor-not-allowed"
            data-testid="rag-custom-ask-button"
          >
            {tr.btnAsk}
          </button>
        </div>
      )}

      {/* State: idle — shown only when neither saga has started */}
      {askStatus === 'idle' && similarStatus === 'idle' && (
        <div
          className="rounded-lg border border-ink-100 bg-ink-50 px-4 py-3 text-sm text-ink-500"
          data-testid="rag-state-idle"
        >
          {tr.stateIdle}
        </div>
      )}

      {/* State: loading */}
      {isLoading && (
        <div
          className="rounded-lg border border-ai-200 bg-ai-50 px-4 py-3 text-sm text-ai-700 animate-pulse"
          data-testid="rag-state-loading"
        >
          {activeUseCase === 'similar' ? tr.similarClaimsLoadingState : tr.stateLoading}
        </div>
      )}

      {/* State: ask error */}
      {askStatus === 'failed' && askError && (
        <div
          className="rounded-lg border border-danger-200 bg-danger-50 px-4 py-3 text-sm text-danger-700"
          data-testid="rag-state-error"
        >
          <span className="font-semibold">{tr.stateError}</span> {askError}
        </div>
      )}

      {/* State: similar-claims error */}
      {similarStatus === 'failed' && similarError && (
        <div
          className="rounded-lg border border-danger-200 bg-danger-50 px-4 py-3 text-sm text-danger-700"
          data-testid="rag-similar-error"
        >
          <span className="font-semibold">{tr.similarClaimsErrorState}</span> {similarError}
        </div>
      )}

      {/* State: similar-claims result — CLAIM-LEVEL CARDS ONLY, no evidence text */}
      {similarStatus === 'succeeded' && similarClaims && (
        <div className="space-y-3" data-testid="rag-similar-claims">
          <div className="text-xs font-semibold text-ink-500 uppercase tracking-wide">
            {tr.similarClaimsHeading} ({similarClaims.similarClaims.length})
          </div>
          {similarClaims.similarClaims.length === 0 ? (
            <p className="text-sm text-ink-500" data-testid="rag-similar-empty">
              {tr.similarClaimsEmptyState}
            </p>
          ) : (
            <div className="space-y-2">
              {similarClaims.similarClaims.map((sc) => (
                <div
                  key={sc.claimId}
                  className="rounded-lg border border-ink-100 bg-white px-4 py-3 space-y-2"
                  data-testid={`rag-similar-card-${sc.claimId}`}
                >
                  {/* Card header: claim id + score + open button */}
                  <div className="flex flex-wrap items-center justify-between gap-2">
                    <span className="font-mono font-semibold text-sm text-ink-900">
                      {sc.claimId}
                    </span>
                    <div className="flex items-center gap-3">
                      <span className="text-xs text-ink-500">
                        {tr.similarClaimsScoreLabel}:{' '}
                        <span className="font-semibold text-ai-700">
                          {(sc.score * 100).toFixed(0)}%
                        </span>
                      </span>
                      <button
                        type="button"
                        onClick={() => navigate(`/claims/${sc.claimId}`)}
                        className="px-2 py-0.5 rounded text-xs font-semibold border border-ai-300 text-ai-700 bg-white hover:bg-ai-50 transition-colors"
                        data-testid={`rag-similar-open-${sc.claimId}`}
                      >
                        {tr.similarClaimsOpenBtn}
                      </button>
                    </div>
                  </div>

                  {/* Reason */}
                  <p className="text-sm text-ink-700">
                    <span className="font-semibold text-xs text-ink-400 uppercase tracking-wide mr-1">
                      {tr.similarClaimsReasonLabel}:
                    </span>
                    {sc.reason}
                  </p>

                  {/* Matching category chips */}
                  {sc.matchingCategories.length > 0 && (
                    <div className="flex flex-wrap gap-1 items-center">
                      <span className="text-xs text-ink-400 uppercase tracking-wide mr-1">
                        {tr.similarClaimsCategoriesLabel}:
                      </span>
                      {sc.matchingCategories.map((cat) => (
                        <span
                          key={cat}
                          className="px-2 py-0.5 rounded-full text-[10px] font-semibold bg-ai-100 text-ai-700 border border-ai-200"
                        >
                          {cat}
                        </span>
                      ))}
                    </div>
                  )}
                </div>
              ))}
            </div>
          )}
        </div>
      )}

      {/* State: answer */}
      {askStatus === 'succeeded' && lastAnswer && (
        <div className="space-y-4" data-testid="rag-answer-card">
          {/* Persistent advisory banner */}
          <div
            className="flex items-center gap-2 rounded-lg border border-warn-200 bg-warn-50 px-4 py-2 text-sm font-semibold text-warn-800"
            data-testid="rag-advisory-banner"
          >
            <span className="text-warn-600 text-base">⚠</span>
            {tr.advisoryBanner}
          </div>

          {/* Answer text */}
          <div>
            <div className="text-xs font-semibold text-ink-500 uppercase tracking-wide mb-1">
              {tr.answerHeading}
            </div>
            <p
              className="text-sm text-ink-800 leading-relaxed"
              data-testid="rag-answer-text"
            >
              {lastAnswer.answer}
            </p>
          </div>

          {/* Metadata grid */}
          <dl className="grid grid-cols-2 sm:grid-cols-3 gap-x-4 gap-y-2 text-xs">
            <MetaItem label={tr.labelConfidence} value={`${(lastAnswer.confidence * 100).toFixed(0)}%`} testId="rag-meta-confidence" />
            <MetaItem label={tr.labelProviderMode} value={lastAnswer.providerMode} testId="rag-meta-provider" />
            <MetaItem label={tr.labelUseCase} value={lastAnswer.useCase} testId="rag-meta-usecase" />
            <MetaItem
              label={tr.labelTokens}
              value={`${lastAnswer.promptTokens} + ${lastAnswer.completionTokens}`}
              testId="rag-meta-tokens"
            />
            <MetaItem label={tr.labelCost} value={String(lastAnswer.costMicros)} testId="rag-meta-cost" />
            <MetaItem label={tr.labelRetrievalMs} value={`${lastAnswer.retrievalMs} ms`} testId="rag-meta-retrieval" />
            <MetaItem
              label={tr.labelTraceId}
              value={lastAnswer.traceId}
              mono
              testId="rag-meta-trace"
            />
          </dl>

          {/* Retrieved chunk ids */}
          {lastAnswer.retrievedChunkIds.length > 0 && (
            <div data-testid="rag-retrieved-chunks">
              <div className="text-xs font-semibold text-ink-500 uppercase tracking-wide mb-1">
                {tr.labelRetrievedChunks} ({lastAnswer.retrievedChunkIds.length})
              </div>
              <div className="flex flex-wrap gap-1">
                {lastAnswer.retrievedChunkIds.map((id) => (
                  <span
                    key={id}
                    className="px-1.5 py-0.5 rounded text-[10px] font-mono bg-ink-100 text-ink-700"
                  >
                    {id}
                  </span>
                ))}
              </div>
            </div>
          )}

          {/* Citations table */}
          <div data-testid="rag-citations">
            <div className="text-xs font-semibold text-ink-500 uppercase tracking-wide mb-1.5">
              {tr.citationsHeading}{' '}
              {lastAnswer.citations.length > 0 ? `(${lastAnswer.citations.length})` : ''}
            </div>
            {lastAnswer.citations.length === 0 ? (
              <p className="text-xs text-ink-500">{tr.noCitations}</p>
            ) : (
              <div className="overflow-x-auto rounded-lg border border-ink-100">
                <table className="w-full text-xs" data-testid="rag-citations-table">
                  <thead className="bg-ink-50">
                    <tr>
                      <th className="table-th">{tr.colKind}</th>
                      <th className="table-th">{tr.colDocId}</th>
                      <th className="table-th">{tr.colChunkId}</th>
                      <th className="table-th text-right">{tr.colScore}</th>
                      <th className="table-th">{tr.colSnippet}</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-ink-100">
                    {lastAnswer.citations.map((c) => (
                      <tr key={c.chunkId} className="hover:bg-ink-50">
                        <td className="table-td">
                          <span className="px-1.5 py-0.5 rounded text-[10px] font-mono bg-ai-100 text-ai-700">
                            {c.kind}
                          </span>
                        </td>
                        <td className="table-td font-mono text-ink-700 text-[10px]">
                          {c.documentId}
                        </td>
                        <td className="table-td font-mono text-ink-500 text-[10px]">
                          {c.chunkId}
                        </td>
                        <td className="table-td text-right font-mono font-semibold text-ai-700">
                          {(c.score * 100).toFixed(0)}%
                        </td>
                        <td className="table-td text-ink-700 max-w-xs truncate" title={c.snippet}>
                          {c.snippet}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        </div>
      )}
    </section>
  );
}

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

/** Canonical question text for non-custom use cases. */
function getCanonicalQuestion(useCase: string): string {
  switch (useCase) {
    case 'coverage':
      return 'Check policy coverage';
    case 'missing_docs':
      return 'Find missing documents';
    case 'risk':
      return 'Explain risk';
    case 'similar':
      return 'Find similar claims';
    case 'summary':
      return 'Prepare approval summary';
    default:
      return useCase;
  }
}

function MetaItem({
  label,
  value,
  mono = false,
  testId,
}: {
  label: string;
  value: string;
  mono?: boolean;
  testId?: string;
}) {
  return (
    <div>
      <dt className="text-ink-400 uppercase tracking-wide text-[10px]">{label}</dt>
      <dd
        className={clsx('text-ink-800 font-semibold mt-0.5', mono && 'font-mono text-[10px] break-all')}
        data-testid={testId}
      >
        {value}
      </dd>
    </div>
  );
}
