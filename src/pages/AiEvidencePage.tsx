import { useEffect } from 'react';
import { useAppDispatch, useAppSelector } from '@/app/hooks';
import { ProgressBar } from '@/components/ui/ProgressBar';
import { goldenClaim } from '@/data/mock/claims';
import {
  evidenceTabs as mockEvidenceTabs,
  extractedEntities as mockExtractedEntities,
  keyFindings as mockKeyFindings,
  modelConfidence as mockModelConfidence,
} from '@/data/mock/claim-1006';
import {
  loadLatestAiAnalysis,
  runAiAnalysis,
  setConfidenceFilter,
  setSelectedEvidence,
} from '@/features/aiReview/aiReviewSlice';
import {
  selectAiLastRun,
  selectAiLastRunStatus,
  selectAiLastError,
} from '@/features/aiReview/aiReviewSelectors';
import {
  selectClaimDetail,
  selectWorkspaceAiEvidence,
} from '@/features/claims/claimWorkspaceSelectors';
import clsx from '@/utils/clsx';

export default function AiEvidencePage() {
  const dispatch = useAppDispatch();

  // --- store selectors (with mock fallback) ---
  const claimDetailFromStore = useAppSelector(selectClaimDetail);
  const c = claimDetailFromStore ?? goldenClaim;

  const aiEvidenceFromStore = useAppSelector(selectWorkspaceAiEvidence);
  const keyFindings = aiEvidenceFromStore?.findings ?? mockKeyFindings.map((f, i) => ({ id: `f-${i}`, text: f.text, detail: f.detail, tone: f.tone }));
  const evidenceTabs = aiEvidenceFromStore?.evidence ?? mockEvidenceTabs;
  const modelConfidence = aiEvidenceFromStore?.modelConfidence ?? mockModelConfidence;
  const extractedEntities = aiEvidenceFromStore?.extractedEntities ?? mockExtractedEntities;

  const { status, progressPct, selectedEvidence, confidenceFilter } = useAppSelector(
    (s) => s.aiReview,
  );
  const lastRun = useAppSelector(selectAiLastRun);
  const lastRunStatus = useAppSelector(selectAiLastRunStatus);
  const lastError = useAppSelector(selectAiLastError);

  // Load the latest persisted AI analysis run for the current claim on mount / claim change.
  useEffect(() => {
    dispatch(loadLatestAiAnalysis(c.id));
  }, [dispatch, c.id]);

  const filteredEntities = extractedEntities.filter((e) => e.confidence >= confidenceFilter);

  // Derived display values — prefer real BFF run, fall back to claim metadata.
  const providerMode = lastRun?.providerMode ?? '—';
  const modelName = lastRun?.modelName ?? '—';
  const displayTokens = lastRun?.costTrace?.tokens ?? c.tokens;
  const displayCost = lastRun?.costTrace?.estimatedCost ?? c.cost;
  const currencyCode = lastRun?.costTrace?.currencyCode ?? 'USD';

  return (
    <div className="flex flex-col gap-5">
      {/* ---------- Header / run controls ---------- */}
      <section className="card card-pad flex flex-wrap items-center gap-x-6 gap-y-3 justify-between">
        <div>
          <h2 className="text-xl font-bold text-ink-900">AI-аналіз та докази</h2>
          <p className="text-sm text-ink-500 mt-1">
            {c.id} · Trace:{' '}
            <span className="font-mono text-brand-700">{c.traceId}</span> ·{' '}
            <span
              className={clsx(
                'inline-flex items-center px-2 py-0.5 rounded-md text-xs font-semibold uppercase tracking-wide',
                providerMode === 'DeepSeek'
                  ? 'bg-ai-100 text-ai-700'
                  : providerMode === 'Mock'
                    ? 'bg-ink-100 text-ink-600'
                    : 'bg-warn-100 text-warn-700',
              )}
              title="AI-провайдер, який повернув останній прогон"
            >
              {providerMode}
            </span>{' '}
            · <span className="font-mono text-ink-600">{modelName}</span> ·{' '}
            {displayTokens.toLocaleString('uk-UA')} токенів · ${displayCost.toFixed(4)} {currencyCode}
          </p>
        </div>
        <div className="flex flex-wrap items-center gap-3">
          <label className="flex items-center gap-2 text-sm text-ink-600">
            Мін. впевненість
            <input
              type="range"
              min={50}
              max={100}
              step={5}
              value={confidenceFilter}
              onChange={(e) => dispatch(setConfidenceFilter(Number(e.target.value)))}
              className="accent-brand-600"
            />
            <span className="font-mono w-10 text-right">{confidenceFilter}%</span>
          </label>
          <button
            onClick={() => dispatch(runAiAnalysis(c.id))}
            disabled={status === 'running'}
            title="Запустити advisory-only AI-аналіз через BFF (Mock за замовчуванням; DeepSeek тільки за явним opt-in)"
            className="btn-primary"
          >
            {status === 'running' ? `Запускаємо ${progressPct}%` : 'Запустити AI-аналіз'}
          </button>
        </div>
      </section>

      {status === 'running' && (
        <section className="card card-pad">
          <ProgressBar value={progressPct} tone="ai" label="Хід AI-запуску" />
        </section>
      )}

      {lastError && status === 'failed' && (
        <section className="card card-pad border-danger-200 bg-danger-500/5">
          <div className="text-sm font-semibold text-danger-700">AI-аналіз не виконано</div>
          <div className="text-xs text-ink-600 mt-1">{lastError}</div>
        </section>
      )}

      {/* ---------- BFF AI Analysis advisory card ---------- */}
      <section className="card card-pad border-ai-200 bg-gradient-to-br from-ai-50 to-white">
        <div className="flex flex-wrap items-center justify-between gap-3 mb-3">
          <div>
            <div className="metric-label text-ai-700">Advisory-only AI</div>
            <h3 className="text-lg font-semibold text-ink-900 mt-0.5">
              Останній прогон AI-аналізу
            </h3>
          </div>
          <div className="flex flex-wrap items-center gap-2">
            {lastRunStatus === 'loading' && (
              <span className="chip bg-ink-100 text-ink-600">Завантаження…</span>
            )}
            {lastRunStatus === 'succeeded' && lastRun && (
              <>
                <span className="chip bg-good-100 text-good-700">{lastRun.status}</span>
                <span className="chip bg-ink-100 text-ink-600">
                  conf {lastRun.confidenceScore}%
                </span>
                <span
                  className={clsx(
                    'chip',
                    lastRun.riskLevel === 'high'
                      ? 'bg-danger-100 text-danger-700'
                      : lastRun.riskLevel === 'moderate'
                        ? 'bg-warn-100 text-warn-700'
                        : 'bg-good-100 text-good-700',
                  )}
                >
                  risk {lastRun.riskLevel}
                </span>
              </>
            )}
            {lastRunStatus === 'succeeded' && !lastRun && (
              <span className="chip bg-ink-100 text-ink-600">Прогонів ще немає</span>
            )}
            {lastRunStatus === 'failed' && (
              <span className="chip bg-danger-100 text-danger-700">Помилка завантаження</span>
            )}
          </div>
        </div>

        {lastRun ? (
          <div className="grid lg:grid-cols-2 gap-4">
            <div className="space-y-3">
              <div>
                <div className="text-xs font-semibold text-ink-500 uppercase tracking-wide mb-1">
                  Зведення
                </div>
                <p className="text-sm text-ink-800">{lastRun.summaryText}</p>
              </div>
              <div>
                <div className="text-xs font-semibold text-ink-500 uppercase tracking-wide mb-1">
                  Рекомендована дія (порадницька)
                </div>
                <p className="text-sm text-ink-800">{lastRun.recommendedAction.action}</p>
                <p className="text-xs text-ink-500 mt-1">
                  Обґрунтування: {lastRun.recommendedAction.rationale} · conf{' '}
                  {lastRun.recommendedAction.confidenceScore}%
                </p>
              </div>
              <div>
                <div className="text-xs font-semibold text-ink-500 uppercase tracking-wide mb-1">
                  Поліс / покриття
                </div>
                <p className="text-sm text-ink-800">{lastRun.policyCoverageExplanation}</p>
              </div>
            </div>

            <div className="space-y-3">
              <div>
                <div className="text-xs font-semibold text-ink-500 uppercase tracking-wide mb-1">
                  Знахідки ({lastRun.findings.length})
                </div>
                <ul className="space-y-1.5">
                  {lastRun.findings.map((f) => (
                    <li key={f.id} className="text-sm text-ink-800 flex items-start gap-2">
                      <span
                        className={clsx(
                          'w-1.5 h-1.5 rounded-full mt-1.5 shrink-0',
                          f.severity === 'danger'
                            ? 'bg-danger-500'
                            : f.severity === 'warn'
                              ? 'bg-warn-500'
                              : 'bg-good-500',
                        )}
                      />
                      <span>
                        <span className="text-ink-500 text-xs">[{f.category}]</span> {f.text}
                      </span>
                    </li>
                  ))}
                </ul>
              </div>
              <div className="grid grid-cols-2 gap-3 text-xs">
                <div>
                  <div className="text-ink-500 uppercase tracking-wide">Докази</div>
                  <div className="text-ink-800 font-semibold mt-0.5">
                    {lastRun.evidence.length}
                  </div>
                </div>
                <div>
                  <div className="text-ink-500 uppercase tracking-wide">Ризики</div>
                  <div className="text-ink-800 font-semibold mt-0.5">{lastRun.risks.length}</div>
                </div>
                <div>
                  <div className="text-ink-500 uppercase tracking-wide">Токени</div>
                  <div className="text-ink-800 font-semibold mt-0.5 font-mono">
                    {lastRun.costTrace.tokens.toLocaleString('uk-UA')}
                  </div>
                </div>
                <div>
                  <div className="text-ink-500 uppercase tracking-wide">Cost</div>
                  <div className="text-ink-800 font-semibold mt-0.5 font-mono">
                    ${lastRun.costTrace.estimatedCost.toFixed(6)} {lastRun.costTrace.currencyCode}
                  </div>
                </div>
              </div>

              {/* Guardrail authority pills — must all be FALSE; rendered as read-only chips */}
              <div>
                <div className="text-xs font-semibold text-ink-500 uppercase tracking-wide mb-1.5">
                  Guardrails (порадницький режим)
                </div>
                <div className="flex flex-wrap gap-1.5">
                  <GuardrailPill label="advisoryOnly" value={lastRun.guardrails.advisoryOnly} positive />
                  <GuardrailPill
                    label="requiresHumanReview"
                    value={lastRun.guardrails.requiresHumanReview}
                    positive
                  />
                  <GuardrailPill
                    label="canApprovePayout"
                    value={lastRun.guardrails.canApprovePayout}
                  />
                  <GuardrailPill
                    label="canRejectClaim"
                    value={lastRun.guardrails.canRejectClaim}
                  />
                  <GuardrailPill
                    label="canAccuseFraudFinal"
                    value={lastRun.guardrails.canAccuseFraudFinal}
                  />
                  <GuardrailPill
                    label="canSendCustomerMessage"
                    value={lastRun.guardrails.canSendCustomerMessage}
                  />
                  <GuardrailPill
                    label="canChangeClaimStatus"
                    value={lastRun.guardrails.canChangeClaimStatus}
                  />
                </div>
                <p className="text-xs text-ink-500 mt-2 italic">{lastRun.notice}</p>
              </div>
            </div>
          </div>
        ) : lastRunStatus === 'idle' || lastRunStatus === 'loading' ? (
          <div className="text-sm text-ink-500">
            Очікуємо першу відповідь BFF (GET /api/claims/{c.id}/ai-analysis)…
          </div>
        ) : (
          <div className="text-sm text-ink-600">
            Для цього кейсу AI-прогонів ще немає. Натисніть «Запустити AI-аналіз», щоб виконати
            advisory-only прогон.
          </div>
        )}
      </section>

      {/* ---------- Legacy / mock-evidence sections (carried forward; unchanged) ---------- */}
      <div className="grid xl:grid-cols-[1fr_360px] gap-5">
        <div className="flex flex-col gap-5">
          <section className="card card-pad">
            <div className="flex items-center justify-between mb-3">
              <div>
                <div className="section-title">AI-знахідки (mock-візуалізація)</div>
                <p className="text-sm text-ink-500 mt-0.5">
                  {keyFindings.length} висновки після обробки документів
                </p>
              </div>
            </div>
            <ul className="space-y-3">
              {keyFindings.map((f, idx) => (
                <li
                  key={f.id ?? idx}
                  className={clsx(
                    'rounded-lg border p-3 flex items-start gap-3',
                    f.tone === 'danger'
                      ? 'border-danger-200 bg-danger-500/5'
                      : f.tone === 'warn'
                        ? 'border-warn-200 bg-warn-500/5'
                        : 'border-good-200 bg-good-500/5',
                  )}
                >
                  <span
                    className={clsx(
                      'w-2 h-2 rounded-full mt-2 shrink-0',
                      f.tone === 'danger'
                        ? 'bg-danger-500'
                        : f.tone === 'warn'
                          ? 'bg-warn-500'
                          : 'bg-good-500',
                    )}
                  />
                  <div className="flex-1 min-w-0">
                    <p className="font-medium text-ink-900">{f.text}</p>
                    <p className="text-xs text-ink-500 mt-0.5">{f.detail}</p>
                  </div>
                </li>
              ))}
            </ul>
          </section>

          <section className="card overflow-hidden">
            <div className="px-5 py-4 border-b border-ink-100 flex items-center justify-between gap-3">
              <div>
                <div className="section-title">Витягнуті сутності</div>
                <p className="text-sm text-ink-500 mt-0.5">Дані з усіх джерел, нормалізовано</p>
              </div>
              <span className="chip">{filteredEntities.length} полів</span>
            </div>
            <table className="w-full">
              <thead className="bg-ink-50/80">
                <tr>
                  <th className="table-th">Поле</th>
                  <th className="table-th">Значення</th>
                  <th className="table-th">Джерело</th>
                  <th className="table-th text-right">Впевн.</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-ink-100">
                {filteredEntities.map((e) => (
                  <tr key={e.field} className="hover:bg-ink-50">
                    <td className="table-td font-medium text-ink-700">{e.field}</td>
                    <td className="table-td text-ink-900">{e.value}</td>
                    <td className="table-td text-ink-600">{e.source}</td>
                    <td className="table-td text-right">
                      <span
                        className={clsx(
                          'font-mono font-semibold',
                          e.confidence >= 95
                            ? 'text-good-600'
                            : e.confidence >= 80
                              ? 'text-ai-700'
                              : 'text-warn-600',
                        )}
                      >
                        {e.confidence}%
                      </span>
                    </td>
                  </tr>
                ))}
                {filteredEntities.length === 0 && (
                  <tr>
                    <td colSpan={4} className="table-td text-center text-ink-500 py-6">
                      Жодне поле не відповідає поточному фільтру впевненості.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </section>
        </div>

        <aside className="flex flex-col gap-5">
          <section className="card card-pad">
            <div className="section-title mb-3">Докази</div>
            <div className="flex flex-wrap gap-1.5">
              {evidenceTabs.map((tab) => (
                <button
                  key={tab}
                  onClick={() => dispatch(setSelectedEvidence(tab))}
                  className={clsx(
                    'px-2.5 py-1 rounded-md text-xs font-semibold transition-colors',
                    selectedEvidence === tab
                      ? 'bg-ai-600 text-white'
                      : 'bg-ink-100 text-ink-600 hover:bg-ink-200',
                  )}
                >
                  {tab}
                </button>
              ))}
            </div>
            <div className="mt-4 rounded-lg bg-ink-50 border border-ink-100 p-3 text-sm text-ink-700">
              Вибраний доказ: <span className="font-semibold text-ink-900">{selectedEvidence}</span>
            </div>
          </section>

          <section className="card card-pad">
            <div className="section-title mb-3">Впевненість моделі</div>
            <div className="space-y-3">
              {modelConfidence.map((m) => (
                <ProgressBar
                  key={m.id}
                  value={m.value}
                  label={m.label}
                  tone={m.value >= 90 ? 'good' : m.value >= 75 ? 'ai' : 'warn'}
                />
              ))}
            </div>
          </section>
        </aside>
      </div>
    </div>
  );
}

/**
 * Compact pill rendering one boolean guardrail flag.
 * `positive` = green = expected true (advisoryOnly, requiresHumanReview).
 * `!positive` = expected false (every Can-* authority flag).
 */
function GuardrailPill({
  label,
  value,
  positive,
}: {
  label: string;
  value: boolean;
  positive?: boolean;
}) {
  // Expected for safety: positive=true OR (positive=false AND value=false)
  const safe = positive ? value === true : value === false;
  return (
    <span
      className={clsx(
        'inline-flex items-center gap-1 px-2 py-0.5 rounded text-[10px] font-mono font-semibold',
        safe ? 'bg-good-100 text-good-700' : 'bg-danger-100 text-danger-700',
      )}
      title={
        positive
          ? `Очікуємо ${label}=true`
          : `Очікуємо ${label}=false — AI ніколи не отримує цей дозвіл`
      }
    >
      {label}={String(value)}
    </span>
  );
}
