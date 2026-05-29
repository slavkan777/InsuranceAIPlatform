import { useEffect, useState, type FormEvent } from 'react';
import { useAppDispatch } from '@/app/hooks';
import { Modal } from '@/components/ui/Modal';
import { Icon } from '@/components/ui/Icon';
import { pushToast } from '@/features/ui/uiFeedbackSlice';
import { insuranceApi } from '@/api/insuranceApi';
import type {
  CreatePayoutSimulationBody,
  PayoutSimulationResultDto,
} from '@/api/insuranceApi.types';
import { useI18n } from '@/i18n/useI18n';

interface PayoutSimulationModalProps {
  open: boolean;
  onClose: () => void;
  claimId?: string;
  defaultAmount?: number;
  defaultDeductible?: number;
  defaultSourceAiRunId?: string | null;
  /** Pre-fill the decision source — typically derived from the operator's UI state. */
  defaultDecisionSource?: 'Human' | 'AI-advisory' | 'Hybrid';
  onCreated?: (result: PayoutSimulationResultDto) => void;
}

/**
 * Creates a DB-only payout/settlement simulation row. NO real money transfer.
 * SimulationOnly=true is a schema-level guarantee. The UI title/copy makes the
 * simulation status explicit so a reviewer cannot confuse it with a real payout.
 */
export function PayoutSimulationModal({
  open,
  onClose,
  claimId = 'CLM-1006',
  defaultAmount = 1800,
  defaultDeductible = 500,
  defaultSourceAiRunId = null,
  defaultDecisionSource = 'Human',
  onCreated,
}: PayoutSimulationModalProps) {
  const { t } = useI18n();
  const dispatch = useAppDispatch();

  const SOURCE_OPTIONS = [
    { value: 'Human', label: t.ui.payoutSourceHuman },
    { value: 'AI-advisory', label: t.ui.payoutSourceAiAdvisory },
    { value: 'Hybrid', label: t.ui.payoutSourceHybrid },
  ];

  const [amount, setAmount] = useState(String(defaultAmount));
  const [deductible, setDeductible] = useState(String(defaultDeductible));
  const [currency, setCurrency] = useState('USD');
  const [decisionSource, setDecisionSource] = useState(defaultDecisionSource);
  const [notes, setNotes] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (open) {
      setAmount(String(defaultAmount));
      setDeductible(String(defaultDeductible));
      setCurrency('USD');
      setDecisionSource(defaultDecisionSource);
      setNotes('');
      setError(null);
      setSubmitting(false);
    }
  }, [open, defaultAmount, defaultDeductible, defaultDecisionSource]);

  const amountNum = Number(amount) || 0;
  const deductibleNum = Number(deductible) || 0;
  const net = Math.max(0, amountNum - deductibleNum);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    if (amountNum <= 0) {
      setError(t.ui.payoutSimErrorAmountPositive);
      return;
    }
    if (deductibleNum < 0) {
      setError(t.ui.payoutSimErrorDeductibleNegative);
      return;
    }
    setSubmitting(true);
    setError(null);
    try {
      const body: CreatePayoutSimulationBody = {
        amount: amountNum,
        deductible: deductibleNum,
        currency,
        decisionSource,
        sourceAiRunId: defaultSourceAiRunId,
        notes: notes.trim() || null,
      };
      const idempKey = `payout-sim-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`;
      const result = await insuranceApi.createPayoutSimulation(claimId, body, idempKey);
      dispatch(
        pushToast({
          tone: 'success',
          title: `${t.ui.payoutSimToastTitle} #${result.simulationId}`,
          detail:
            `${result.amount} ${result.currency} (net ${result.netPayoutAmount}). ` +
            'SimulationOnly=true; реальної виплати не виконано.',
        }),
      );
      onCreated?.(result);
      onClose();
    } catch (err) {
      const msg = err instanceof Error ? err.message : 'Невідома помилка.';
      setError(msg);
      setSubmitting(false);
    }
  }

  return (
    <Modal
      open={open}
      onClose={() => !submitting && onClose()}
      size="lg"
      title={t.ui.payoutSimTitle}
      description={t.ui.payoutSimDescription}
      footer={
        <>
          <button
            type="button"
            data-testid="payout-sim-cancel"
            onClick={() => !submitting && onClose()}
            disabled={submitting}
            className="btn-ghost px-3 py-1.5 text-sm disabled:opacity-50"
          >
            {t.ui.payoutSimCancel}
          </button>
          <button
            type="submit"
            form="payout-sim-form"
            data-testid="payout-sim-submit"
            disabled={submitting}
            className="btn-primary inline-flex items-center gap-1.5 px-3 py-1.5 text-sm disabled:opacity-50"
          >
            <Icon name="check" size={14} />
            {submitting ? t.ui.payoutSimSubmitting : t.ui.payoutSimSubmit}
          </button>
        </>
      }
    >
      <form id="payout-sim-form" onSubmit={handleSubmit} className="space-y-3 text-sm">
        <div className="grid sm:grid-cols-3 gap-3">
          <div>
            <label className="block text-xs font-semibold text-ink-700 uppercase tracking-wide mb-1.5">
              {t.ui.payoutSimLabelAmount}
            </label>
            <input
              type="number"
              min={0.01}
              step={0.01}
              data-testid="payout-sim-amount"
              value={amount}
              onChange={(e) => setAmount(e.target.value)}
              disabled={submitting}
              required
              className="w-full px-3 py-2 rounded-lg border border-ink-200 bg-white text-sm focus-ring font-mono"
            />
          </div>
          <div>
            <label className="block text-xs font-semibold text-ink-700 uppercase tracking-wide mb-1.5">
              {t.ui.payoutSimLabelDeductible}
            </label>
            <input
              type="number"
              min={0}
              step={0.01}
              value={deductible}
              onChange={(e) => setDeductible(e.target.value)}
              disabled={submitting}
              className="w-full px-3 py-2 rounded-lg border border-ink-200 bg-white text-sm focus-ring font-mono"
            />
          </div>
          <div>
            <label className="block text-xs font-semibold text-ink-700 uppercase tracking-wide mb-1.5">
              {t.ui.payoutSimLabelCurrency}
            </label>
            <select
              value={currency}
              onChange={(e) => setCurrency(e.target.value)}
              disabled={submitting}
              className="w-full px-3 py-2 rounded-lg border border-ink-200 bg-white text-sm focus-ring"
            >
              <option value="USD">USD</option>
              <option value="EUR">EUR</option>
              <option value="UAH">UAH</option>
            </select>
          </div>
        </div>
        <div className="rounded-lg border border-brand-200 bg-brand-50 px-3 py-2 flex items-center justify-between text-sm">
          <span className="text-brand-800 font-semibold">{t.ui.payoutSimNetLabel}</span>
          <span className="font-mono text-lg font-bold text-brand-700">
            {net.toLocaleString('uk-UA', { maximumFractionDigits: 2 })} {currency}
          </span>
        </div>
        <div>
          <label className="block text-xs font-semibold text-ink-700 uppercase tracking-wide mb-1.5">
            {t.ui.payoutSimLabelDecisionSource}
          </label>
          <select
            value={decisionSource}
            onChange={(e) => setDecisionSource(e.target.value as 'Human' | 'AI-advisory' | 'Hybrid')}
            disabled={submitting}
            className="w-full px-3 py-2 rounded-lg border border-ink-200 bg-white text-sm focus-ring"
          >
            {SOURCE_OPTIONS.map((o) => (
              <option key={o.value} value={o.value}>{o.label}</option>
            ))}
          </select>
          {defaultSourceAiRunId ? (
            <p className="text-[10px] text-ink-500 mt-1">
              {t.ui.payoutSimLinkedRun} <span className="font-mono">{defaultSourceAiRunId}</span>
            </p>
          ) : null}
        </div>
        <div>
          <label className="block text-xs font-semibold text-ink-700 uppercase tracking-wide mb-1.5">
            {t.ui.payoutSimLabelNotes}
          </label>
          <textarea
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
            disabled={submitting}
            rows={2}
            placeholder={t.ui.payoutSimPlaceholderNotes}
            className="w-full px-3 py-2 rounded-lg border border-ink-200 bg-white text-sm focus-ring resize-none"
          />
        </div>
        {error ? (
          <div role="alert" className="rounded-lg border border-danger-200 bg-danger-50 text-danger-700 text-xs px-3 py-2">
            {error}
          </div>
        ) : null}
        <div className="rounded-lg border border-warn-200 bg-warn-50 px-3 py-2.5 text-[11px] text-warn-800 leading-snug flex items-start gap-2">
          <span className="mt-0.5 shrink-0"><Icon name="shield" size={12} /></span>
          <span>{t.ui.payoutSimSandboxNote}</span>
        </div>
      </form>
    </Modal>
  );
}
