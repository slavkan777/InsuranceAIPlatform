import { useState, type FormEvent } from 'react';
import { useAppDispatch } from '@/app/hooks';
import { Modal } from '@/components/ui/Modal';
import { Icon } from '@/components/ui/Icon';
import { pushToast } from '@/features/ui/uiFeedbackSlice';
import { insuranceApi } from '@/api/insuranceApi';
import type { CreateCustomerBody, CreateCustomerResult } from '@/api/insuranceApi.types';
import { useI18n } from '@/i18n/useI18n';

interface CreateCustomerModalProps {
  open: boolean;
  onClose: () => void;
  onCreated?: (created: CreateCustomerResult) => void;
}

/**
 * Creates a new synthetic customer row in the customers_policies domain via
 * POST /api/customers. Sandbox only — no real PII. Server allocates the next
 * CUST-T0XXX id. After successful creation the modal pushes a toast and
 * calls `onCreated` so the parent can refresh its list / pre-select the new
 * customer in a follow-up flow.
 */
export function CreateCustomerModal({ open, onClose, onCreated }: CreateCustomerModalProps) {
  const dispatch = useAppDispatch();
  const { t } = useI18n();

  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [phone, setPhone] = useState('');
  const [addressLine, setAddressLine] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  function reset() {
    setFullName('');
    setEmail('');
    setPhone('');
    setAddressLine('');
    setError(null);
    setSubmitting(false);
  }

  function handleClose() {
    if (submitting) return;
    reset();
    onClose();
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    const trimmed = fullName.trim();
    if (trimmed.length === 0) {
      setError(t.customers.errorNameRequired);
      return;
    }
    if (trimmed.length > 200) {
      setError(t.customers.errorNameTooLong);
      return;
    }
    setSubmitting(true);
    setError(null);
    try {
      const body: CreateCustomerBody = {
        fullName: trimmed,
        email: email.trim() || null,
        phone: phone.trim() || null,
        addressLine: addressLine.trim() || null,
        customerSince: null,
      };
      const idempKey = `new-customer-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`;
      const result = await insuranceApi.createCustomer(body, idempKey);
      dispatch(
        pushToast({
          tone: 'success',
          title: `${t.customers.toastCreatedPrefix} ${result.customerId}${t.customers.toastCreatedSuffix}`,
          detail: result.message,
        }),
      );
      reset();
      onClose();
      onCreated?.(result);
    } catch (err) {
      const msg = err instanceof Error ? err.message : t.customers.errorUnknown;
      setError(msg);
      setSubmitting(false);
    }
  }

  return (
    <Modal
      open={open}
      onClose={handleClose}
      size="md"
      title={t.customers.modalTitle}
      description={t.customers.modalDescription}
      footer={
        <>
          <button
            type="button"
            data-testid="create-customer-cancel"
            onClick={handleClose}
            disabled={submitting}
            className="btn-ghost px-3 py-1.5 text-sm disabled:opacity-50"
          >
            {t.customers.cancelButton}
          </button>
          <button
            type="submit"
            form="create-customer-form"
            data-testid="create-customer-submit"
            disabled={submitting}
            className="btn-primary inline-flex items-center gap-1.5 px-3 py-1.5 text-sm disabled:opacity-50"
          >
            <Icon name="plus" size={14} />
            {submitting ? t.customers.submitButtonBusy : t.customers.submitButtonIdle}
          </button>
        </>
      }
    >
      <form id="create-customer-form" onSubmit={handleSubmit} className="space-y-3 text-sm">
        <div>
          <label className="block text-xs font-semibold text-ink-700 uppercase tracking-wide mb-1.5">
            {t.customers.labelFullName}
          </label>
          <input
            type="text"
            data-testid="create-customer-fullName"
            value={fullName}
            onChange={(e) => setFullName(e.target.value)}
            disabled={submitting}
            required
            placeholder={t.customers.placeholderFullName}
            className="w-full px-3 py-2 rounded-lg border border-ink-200 bg-white text-sm focus-ring"
          />
        </div>
        <div className="grid sm:grid-cols-2 gap-3">
          <div>
            <label className="block text-xs font-semibold text-ink-700 uppercase tracking-wide mb-1.5">
              {t.customers.labelEmail}
            </label>
            <input
              type="email"
              data-testid="create-customer-email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              disabled={submitting}
              placeholder={t.customers.placeholderEmail}
              className="w-full px-3 py-2 rounded-lg border border-ink-200 bg-white text-sm focus-ring font-mono"
            />
          </div>
          <div>
            <label className="block text-xs font-semibold text-ink-700 uppercase tracking-wide mb-1.5">
              {t.customers.labelPhone}
            </label>
            <input
              type="text"
              data-testid="create-customer-phone"
              value={phone}
              onChange={(e) => setPhone(e.target.value)}
              disabled={submitting}
              placeholder={t.customers.placeholderPhone}
              className="w-full px-3 py-2 rounded-lg border border-ink-200 bg-white text-sm focus-ring font-mono"
            />
          </div>
        </div>
        <div>
          <label className="block text-xs font-semibold text-ink-700 uppercase tracking-wide mb-1.5">
            {t.customers.labelAddress}
          </label>
          <input
            type="text"
            data-testid="create-customer-address"
            value={addressLine}
            onChange={(e) => setAddressLine(e.target.value)}
            disabled={submitting}
            placeholder={t.customers.placeholderAddress}
            className="w-full px-3 py-2 rounded-lg border border-ink-200 bg-white text-sm focus-ring"
          />
        </div>
        {error ? (
          <div role="alert" data-testid="create-customer-error" className="rounded-lg border border-danger-200 bg-danger-50 text-danger-700 text-xs px-3 py-2">
            {error}
          </div>
        ) : null}
        <div className="rounded-lg border border-ink-200 bg-ink-50 px-3 py-2 text-[11px] text-ink-600 leading-snug flex items-start gap-2">
          <span className="mt-0.5 shrink-0"><Icon name="shield" size={12} /></span>
          <span>{t.customers.idHintText}</span>
        </div>
      </form>
    </Modal>
  );
}
