import { useState } from 'react';
import { useParams } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '@/app/hooks';
import { ProgressBar } from '@/components/ui/ProgressBar';
import { StatusPill } from '@/components/ui/StatusPill';
import { Icon } from '@/components/ui/Icon';
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
import { pushToast } from '@/features/ui/uiFeedbackSlice';
import { RequestMissingDocumentModal } from '@/components/claim/RequestMissingDocumentModal';
import { PayoutSimulationModal } from '@/components/claim/PayoutSimulationModal';
import { insuranceApi } from '@/api/insuranceApi';
import clsx from '@/utils/clsx';

/** Map the UI decision tile id (lowercase) to the backend decision enum (PascalCase, allow-listed). */
function uiDecisionToBackend(id: string | null): string | null {
  switch (id) {
    case 'approve':
      return 'ApproveForReview';
    case 'request':
      return 'RequestDocuments';
    case 'reject':
      return 'RejectForReview';
    case 'escalate':
      return 'NeedsMoreInformation';
    default:
      return null;
  }
}

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
  const { claimId: routeClaimId } = useParams<{ claimId: string }>();

  // --- store selectors (with mock fallback) ---
  const claimDetailFromStore = useAppSelector(selectClaimDetail);
  const c = claimDetailFromStore ?? goldenClaim;
  const claimId = routeClaimId ?? c.id;

  const approvalReadFromStore = useAppSelector(selectWorkspaceApprovalRead);

  // --- local action state ---
  const [savingDraft, setSavingDraft] = useState(false);
  const [submittingDecision, setSubmittingDecision] = useState(false);
  const [requestDocOpen, setRequestDocOpen] = useState(false);
  const [payoutSimOpen, setPayoutSimOpen] = useState(false);

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

  async function handleSaveDraft() {
    setSavingDraft(true);
    try {
      const idempKey = `save-draft-${claimId}-${Date.now()}`;
      const backendDecision = uiDecisionToBackend(selectedDecision);
      const result = await insuranceApi.saveApprovalDraftCommand(
        claimId,
        {
          currentDecision: backendDecision,
          notes: reviewerNotes.trim() || null,
        },
        idempKey,
      );
      dispatch(
        pushToast({
          tone: 'success',
          title: 'Чернетку рішення збережено.',
          detail: `${result.message} cmd=${result.commandId.slice(0, 14)}…`,
        }),
      );
    } catch (err) {
      const msg = err instanceof Error ? err.message : 'Невідома помилка.';
      dispatch(
        pushToast({
          tone: 'error',
          title: 'Не вдалося зберегти чернетку.',
          detail: msg,
        }),
      );
    } finally {
      setSavingDraft(false);
    }
  }

  async function handleApproveAfterReview() {
    if (selectedDecision !== 'approve') {
      dispatch(
        pushToast({
          tone: 'warning',
          title: 'Оберіть «Погодити виплату» у варіантах рішення.',
          detail: 'Без явного вибору не можна підтверджувати погодження.',
        }),
      );
      return;
    }
    setSubmittingDecision(true);
    try {
      const idempKey = `submit-decision-${claimId}-approve-${Date.now()}`;
      const result = await insuranceApi.submitHumanDecision(
        claimId,
        {
          decision: 'ApproveForReview',
          notes: reviewerNotes.trim() || null,
        },
        idempKey,
      );
      dispatch(
        pushToast({
          tone: 'success',
          title: 'Погодження після перевірки зафіксовано.',
          detail:
            `${result.message} ` +
            'Реальна виплата не виконувалась — це локальний sandbox-запис.',
        }),
      );
    } catch (err) {
      const msg = err instanceof Error ? err.message : 'Невідома помилка.';
      dispatch(
        pushToast({
          tone: 'error',
          title: 'Не вдалося зафіксувати рішення.',
          detail: msg,
        }),
      );
    } finally {
      setSubmittingDecision(false);
    }
  }

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
            <button
              type="button"
              data-testid="save-draft"
              onClick={handleSaveDraft}
              disabled={savingDraft}
              title="Зберегти чернетку поточного рішення + нотатки у БД (audit + outbox)"
              className="btn-secondary inline-flex items-center justify-center gap-1.5 disabled:opacity-50"
            >
              <Icon name="edit" size={14} />
              {savingDraft ? 'Збереження…' : 'Зберегти чернетку'}
            </button>
            <button
              type="button"
              data-testid="request-missing-doc-open-approval"
              onClick={() => setRequestDocOpen(true)}
              title="Зафіксувати внутрішній запит на додаткові дані (без листа клієнту)"
              className="btn-primary inline-flex items-center justify-center gap-1.5"
            >
              <Icon name="check" size={14} />
              Зафіксувати запит у журналі
            </button>
            <button
              type="button"
              data-testid="approve-after-review"
              onClick={handleApproveAfterReview}
              disabled={submittingDecision || selectedDecision !== 'approve'}
              title="Зафіксувати погодження після перевірки. Без виплати, без листа клієнту, без зміни статусу кейсу."
              className="btn-secondary inline-flex items-center justify-center gap-1.5 text-good-700 border-good-300 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              <Icon name="check" size={14} />
              {submittingDecision ? 'Збереження…' : 'Погодити після перевірки'}
            </button>
            <button
              type="button"
              data-testid="payout-sim-open"
              onClick={() => setPayoutSimOpen(true)}
              title="Створити DB-only симуляцію виплати (SimulationOnly=true; без реальної транзакції)"
              className="btn-secondary inline-flex items-center justify-center gap-1.5 text-brand-700 border-brand-300"
            >
              <Icon name="receipt" size={14} />
              Симуляція виплати (sandbox)
            </button>
            <p className="text-[11px] text-center text-ink-400 mt-1 leading-snug">
              Локальний sandbox. Реальна виплата та повідомлення клієнту не виконуються.
            </p>
          </div>
        </aside>
      </div>

      <RequestMissingDocumentModal
        open={requestDocOpen}
        onClose={() => setRequestDocOpen(false)}
        claimId={claimId}
        defaultTitle="Уточнення / додаткові документи від клієнта"
        defaultReason="Потрібно для остаточного людського рішення."
      />
      <PayoutSimulationModal
        open={payoutSimOpen}
        onClose={() => setPayoutSimOpen(false)}
        claimId={claimId}
        defaultAmount={Number(c.recommendedPayout)}
        defaultDeductible={Number(c.deductible)}
        defaultDecisionSource="Human"
      />
    </div>
  );
}
