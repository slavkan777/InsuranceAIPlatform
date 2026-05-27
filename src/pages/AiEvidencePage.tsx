import { useAppDispatch, useAppSelector } from '@/app/hooks';
import { StatusPill } from '@/components/ui/StatusPill';
import { ProgressBar } from '@/components/ui/ProgressBar';
import { goldenClaim } from '@/data/mock/claims';
import {
  evidenceTabs as mockEvidenceTabs,
  extractedEntities as mockExtractedEntities,
  keyFindings as mockKeyFindings,
  modelConfidence as mockModelConfidence,
} from '@/data/mock/claim-1006';
import {
  runAiAnalysis,
  setConfidenceFilter,
  setSelectedEvidence,
} from '@/features/aiReview/aiReviewSlice';
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

  const filteredEntities = extractedEntities.filter((e) => e.confidence >= confidenceFilter);

  return (
    <div className="flex flex-col gap-5">
      <section className="card card-pad flex flex-wrap items-center gap-x-6 gap-y-3 justify-between">
        <div>
          <h2 className="text-xl font-bold text-ink-900">AI-аналіз та докази</h2>
          <p className="text-sm text-ink-500 mt-1">
            {c.id} · Trace:{' '}
            <span className="font-mono text-brand-700">{c.traceId}</span> · Azure OpenAI ·{' '}
            {c.tokens.toLocaleString('uk-UA')} токенів · ${c.cost.toFixed(4)}
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
            onClick={() => dispatch(runAiAnalysis())}
            disabled={status === 'running'}
            title="Локальний демо-прогон mock-аналізу · реальний AI-провайдер не підключений"
            className="btn-primary"
          >
            {status === 'running' ? `Запускаємо ${progressPct}%` : 'Перезапустити mock-аналіз'}
          </button>
        </div>
      </section>

      {status === 'running' && (
        <section className="card card-pad">
          <ProgressBar value={progressPct} tone="ai" label="Хід mock-AI-запуску" />
        </section>
      )}

      <div className="grid xl:grid-cols-[1fr_360px] gap-5">
        <div className="flex flex-col gap-5">
          <section className="card card-pad">
            <div className="flex items-center justify-between mb-3">
              <div>
                <div className="section-title">AI-знахідки</div>
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

          <section className="card card-pad bg-gradient-to-br from-ai-50 to-white border-ai-200">
            <div className="metric-label text-ai-700">Guardrail</div>
            <h4 className="text-base font-semibold text-ink-900 mt-1">
              AI надає аналіз, але не приймає фінальне рішення
            </h4>
            <p className="text-sm text-ink-600 mt-2">
              Усі рекомендації перевіряються експертом і фіксуються в audit trail.
            </p>
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
