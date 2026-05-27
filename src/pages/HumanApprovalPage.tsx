import { useAppDispatch, useAppSelector } from '@/app/hooks';
import { ProgressBar } from '@/components/ui/ProgressBar';
import { StatusPill } from '@/components/ui/StatusPill';
import { goldenClaim } from '@/data/mock/claims';
import { approvalChecklist, decisionOptions as mockDecisionOptions } from '@/data/mock/claim-1006';
import {
  setDecision,
  setNotes,
  toggleChecklistItem,
} from '@/features/approval/approvalSlice';
import {
  selectClaimDetail,
  selectWorkspaceApprovalRead,
} from '@/features/claims/claimWorkspaceSelectors';
import { DeferredActionButton } from '@/components/ui/DeferredActionButton';
import clsx from '@/utils/clsx';

const toneRing: Record<string, string> = {
  good: 'border-good-300 hover:border-good-500 data-[selected=true]:bg-good-500/10 data-[selected=true]:border-good-500',
  info: 'border-brand-300 hover:border-brand-500 data-[selected=true]:bg-brand-500/10 data-[selected=true]:border-brand-500',
  danger:
    'border-danger-300 hover:border-danger-500 data-[selected=true]:bg-danger-500/10 data-[selected=true]:border-danger-500',
  warn: 'border-warn-300 hover:border-warn-500 data-[selected=true]:bg-warn-500/10 data-[selected=true]:border-warn-500',
};

/** Map option value to a display tone. */
function optionTone(value: string): string {
  switch (value) {
    case 'approve': return 'good';
    case 'request': return 'info';
    case 'reject': return 'danger';
    case 'escalate': return 'warn';
    default: return 'info';
  }
}

export default function HumanApprovalPage() {
  const dispatch = useAppDispatch();

  // --- store selectors (with mock fallback) ---
  const claimDetailFromStore = useAppSelector(selectClaimDetail);
  const c = claimDetailFromStore ?? goldenClaim;

  const approvalReadFromStore = useAppSelector(selectWorkspaceApprovalRead);

  // Build decision options from the approval read model (or fall back to static mock)
  const decisionOptions = approvalReadFromStore
    ? approvalReadFromStore.availableOptions.map((o) => ({
        id: o.value,
        title: o.label,
        caption: o.description ?? '',
        tone: optionTone(o.value) as 'good' | 'info' | 'danger' | 'warn',
        recommended: o.recommended,
      }))
    : mockDecisionOptions;

  const recommendedPayout = approvalReadFromStore?.recommendedPayout ?? c.recommendedPayout;
  const aiRecommendation = approvalReadFromStore?.aiRecommendation ?? 'Запросити додаткове фото перед погодженням виплати';

  const { selectedDecision, reviewerNotes, checklist } = useAppSelector((s) => s.approval);

  const reductionAmount = 420;
  const draftPayout = recommendedPayout;

  return (
    <div className="flex flex-col gap-5">
      <section className="card card-pad flex flex-wrap items-center gap-x-6 gap-y-3 justify-between">
        <div>
          <h2 className="text-xl font-bold text-ink-900">Людське погодження</h2>
          <p className="text-sm text-ink-500 mt-1">
            {c.id} · фінальне рішення приймає відповідальний експерт
          </p>
        </div>
        <StatusPill tone="info">Trace · {c.traceId}</StatusPill>
      </section>

      <section className="card card-pad bg-gradient-to-br from-ai-50 to-white border-ai-200">
        <div className="flex flex-wrap items-start gap-3 justify-between">
          <div>
            <div className="metric-label text-ai-700">AI-РЕКОМЕНДАЦІЯ</div>
            <h3 className="text-lg font-semibold text-ink-900 mt-1">
              {aiRecommendation}
            </h3>
            <p className="text-sm text-ink-600 mt-2 max-w-2xl">
              Документи неповні + перевищення вартості ремонту на 38%. Confidence {c.confidence}%.
            </p>
          </div>
          <div className="w-44">
            <ProgressBar value={c.confidence} tone="ai" label="Впевненість" />
          </div>
        </div>
      </section>

      <div className="grid xl:grid-cols-[1fr_360px] gap-5">
        <div className="flex flex-col gap-5">
          <section className="card card-pad">
            <div className="section-title mb-3">Варіанти рішення</div>
            <div className="grid md:grid-cols-2 gap-3">
              {decisionOptions.map((opt) => {
                const selected = selectedDecision === opt.id;
                return (
                  <button
                    key={opt.id}
                    onClick={() => dispatch(setDecision(opt.id as never))}
                    data-selected={selected}
                    className={clsx(
                      'text-left rounded-xl border-2 px-4 py-3 transition-colors bg-white',
                      toneRing[opt.tone],
                    )}
                  >
                    <div className="flex items-center justify-between gap-2">
                      <span className="font-semibold text-ink-900">{opt.title}</span>
                      {opt.recommended && <StatusPill tone="ai">AI</StatusPill>}
                    </div>
                    <p className="text-xs text-ink-500 mt-1">{opt.caption}</p>
                  </button>
                );
              })}
            </div>
          </section>

          <section className="card card-pad">
            <div className="section-title mb-3">Чернетка виплати</div>
            <dl className="divide-y divide-ink-100">
              {[
                { label: 'Рахунок СТО', value: `$${c.estimate.toLocaleString('uk-UA')}`, tone: 'ink-900' },
                {
                  label: 'Очікувана сума',
                  value: `$${c.expectedBenchmark.toLocaleString('uk-UA')}`,
                  tone: 'ink-700',
                },
                {
                  label: 'Відхилення',
                  value: `+$${(c.estimate - c.expectedBenchmark).toLocaleString('uk-UA')}`,
                  tone: 'danger',
                },
                { label: 'Франшиза', value: `–$${c.deductible}`, tone: 'ink-700' },
                { label: 'Можлива редукція', value: `–$${reductionAmount}`, tone: 'ink-700' },
              ].map((row) => (
                <div key={row.label} className="flex items-center justify-between py-2 text-sm">
                  <span className="text-ink-600">{row.label}</span>
                  <span
                    className={clsx(
                      'font-mono font-semibold',
                      row.tone === 'danger' ? 'text-danger-600' : 'text-ink-900',
                    )}
                  >
                    {row.value}
                  </span>
                </div>
              ))}
              <div className="flex items-center justify-between py-3 text-sm bg-brand-50 -mx-5 px-5">
                <span className="font-semibold text-ink-900">Рекомендована виплата</span>
                <span className="font-mono text-lg font-bold text-brand-700">
                  ${draftPayout.toLocaleString('uk-UA')}
                </span>
              </div>
            </dl>
          </section>

          <section className="card card-pad">
            <div className="section-title mb-3">Нотатки рецензента</div>
            <textarea
              value={reviewerNotes}
              onChange={(e) => dispatch(setNotes(e.target.value))}
              rows={4}
              className="w-full rounded-lg border border-ink-200 bg-white px-3 py-2 text-sm focus-ring resize-y"
              placeholder="Аргументи на користь рішення, посилання на докази…"
            />
          </section>
        </div>

        <aside className="flex flex-col gap-5">
          <section className="card card-pad">
            <div className="section-title mb-3">Чеклист перевірки</div>
            <ul className="space-y-2 text-sm">
              {approvalChecklist.map((item) => (
                <li key={item.id}>
                  <label className="flex items-center gap-2 cursor-pointer">
                    <input
                      type="checkbox"
                      checked={!!checklist[item.id]}
                      onChange={() => dispatch(toggleChecklistItem(item.id))}
                      className="accent-brand-600 w-4 h-4 rounded border-ink-300"
                    />
                    <span
                      className={clsx(
                        'flex-1',
                        item.status === 'warn'
                          ? 'text-warn-700 font-medium'
                          : 'text-ink-700',
                      )}
                    >
                      {item.label}
                    </span>
                  </label>
                </li>
              ))}
            </ul>
          </section>

          <section className="card card-pad bg-gradient-to-br from-warn-500/5 to-white border-warn-200">
            <div className="metric-label text-warn-600">Відповідальність</div>
            <h4 className="text-sm font-semibold text-ink-900 mt-1">
              Остаточне рішення приймає відповідальний спеціаліст
            </h4>
            <p className="text-xs text-ink-600 mt-2">
              AI-рекомендація допоміжна. Логи в audit trail ({c.traceId}).
            </p>
          </section>

          <div className="grid gap-2">
            <DeferredActionButton
              hint="Збереження чернетки рішення — потрібен backend write-гейт"
              className="btn-secondary"
            >
              Зберегти чернетку
            </DeferredActionButton>
            <DeferredActionButton
              hint="Надсилання запиту клієнту — потрібен backend write-гейт"
              className="btn-primary"
            >
              Надіслати запит клієнту
            </DeferredActionButton>
            <DeferredActionButton
              hint="Погодження виплати — потрібен backend write-гейт + людський підпис"
              className="btn-secondary text-good-600 border-good-300"
              badge="demo"
            >
              Погодити після перевірки
            </DeferredActionButton>
            <p className="text-[11px] text-center text-ink-400 mt-1 leading-snug">
              Дії рішення доступні лише для перегляду в demo · запис вмикається після backend write-гейту
            </p>
          </div>
        </aside>
      </div>
    </div>
  );
}
