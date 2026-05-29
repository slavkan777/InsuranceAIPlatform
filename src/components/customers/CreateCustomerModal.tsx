import { useState, type FormEvent } from 'react';
import { useAppDispatch } from '@/app/hooks';
import { Modal } from '@/components/ui/Modal';
import { Icon } from '@/components/ui/Icon';
import { pushToast } from '@/features/ui/uiFeedbackSlice';
import { insuranceApi } from '@/api/insuranceApi';
import type { CreateCustomerBody, CreateCustomerResult } from '@/api/insuranceApi.types';

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
      setError('Заповніть повне ім’я (синтетичне, без реальних даних).');
      return;
    }
    if (trimmed.length > 200) {
      setError('Повне ім’я має бути не довше 200 символів.');
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
          title: `Створено клієнта ${result.customerId}.`,
          detail: result.message,
        }),
      );
      reset();
      onClose();
      onCreated?.(result);
    } catch (err) {
      const msg = err instanceof Error ? err.message : 'Невідома помилка.';
      setError(msg);
      setSubmitting(false);
    }
  }

  return (
    <Modal
      open={open}
      onClose={handleClose}
      size="md"
      title="Створити нового синтетичного клієнта"
      description="Створює новий рядок у customers_policies.SyntheticCustomers з IsSynthetic=true. Локальний sandbox — без реальних персональних даних."
      footer={
        <>
          <button
            type="button"
            data-testid="create-customer-cancel"
            onClick={handleClose}
            disabled={submitting}
            className="btn-ghost px-3 py-1.5 text-sm disabled:opacity-50"
          >
            Скасувати
          </button>
          <button
            type="submit"
            form="create-customer-form"
            data-testid="create-customer-submit"
            disabled={submitting}
            className="btn-primary inline-flex items-center gap-1.5 px-3 py-1.5 text-sm disabled:opacity-50"
          >
            <Icon name="plus" size={14} />
            {submitting ? 'Створення…' : 'Створити клієнта'}
          </button>
        </>
      }
    >
      <form id="create-customer-form" onSubmit={handleSubmit} className="space-y-3 text-sm">
        <div>
          <label className="block text-xs font-semibold text-ink-700 uppercase tracking-wide mb-1.5">
            Повне імʼя *
          </label>
          <input
            type="text"
            data-testid="create-customer-fullName"
            value={fullName}
            onChange={(e) => setFullName(e.target.value)}
            disabled={submitting}
            required
            placeholder="Synthetic Customer Smith"
            className="w-full px-3 py-2 rounded-lg border border-ink-200 bg-white text-sm focus-ring"
          />
        </div>
        <div className="grid sm:grid-cols-2 gap-3">
          <div>
            <label className="block text-xs font-semibold text-ink-700 uppercase tracking-wide mb-1.5">
              Email (опц.)
            </label>
            <input
              type="email"
              data-testid="create-customer-email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              disabled={submitting}
              placeholder="testuser@synthetic.invalid"
              className="w-full px-3 py-2 rounded-lg border border-ink-200 bg-white text-sm focus-ring font-mono"
            />
          </div>
          <div>
            <label className="block text-xs font-semibold text-ink-700 uppercase tracking-wide mb-1.5">
              Телефон (опц.)
            </label>
            <input
              type="text"
              data-testid="create-customer-phone"
              value={phone}
              onChange={(e) => setPhone(e.target.value)}
              disabled={submitting}
              placeholder="+380501234567"
              className="w-full px-3 py-2 rounded-lg border border-ink-200 bg-white text-sm focus-ring font-mono"
            />
          </div>
        </div>
        <div>
          <label className="block text-xs font-semibold text-ink-700 uppercase tracking-wide mb-1.5">
            Адреса (опц.)
          </label>
          <input
            type="text"
            data-testid="create-customer-address"
            value={addressLine}
            onChange={(e) => setAddressLine(e.target.value)}
            disabled={submitting}
            placeholder="Київ, вул. Грушевського 5"
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
          <span>
            ID присвоюється сервером (CUST-T0XXX, наступний після сидованого діапазону).
            Запис позначається IsSynthetic=true; UI ніколи не пише реальні PII.
          </span>
        </div>
      </form>
    </Modal>
  );
}
