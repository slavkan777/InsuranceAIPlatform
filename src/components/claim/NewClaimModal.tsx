import { useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAppDispatch } from '@/app/hooks';
import { Modal } from '@/components/ui/Modal';
import { Icon } from '@/components/ui/Icon';
import { pushToast } from '@/features/ui/uiFeedbackSlice';
import { insuranceApi } from '@/api/insuranceApi';
import type { CreateClaimBody } from '@/api/insuranceApi.types';

interface NewClaimModalProps {
  open: boolean;
  onClose: () => void;
}

const EVENT_TYPE_OPTIONS = [
  'ДТП',
  'Паркування',
  'Зіткнення',
  'Пошкодження',
  'Скло',
  'Угон',
];

/**
 * Creates a new synthetic claim row in the DB via POST /api/claims. All fields
 * are synthetic / sandbox-only — no real PII. After successful creation the modal
 * navigates to the newly-created claim detail.
 */
export function NewClaimModal({ open, onClose }: NewClaimModalProps) {
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const today = new Date().toISOString().slice(0, 10);

  const [customerName, setCustomerName] = useState('');
  const [customerId, setCustomerId] = useState('');
  const [vehicle, setVehicle] = useState('');
  const [vehicleVin, setVehicleVin] = useState('VIN ****0000');
  const [eventType, setEventType] = useState(EVENT_TYPE_OPTIONS[0]);
  const [eventDate, setEventDate] = useState(today);
  const [location, setLocation] = useState('');
  const [description, setDescription] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  function reset() {
    setCustomerName('');
    setCustomerId('');
    setVehicle('');
    setVehicleVin('VIN ****0000');
    setEventType(EVENT_TYPE_OPTIONS[0]);
    setEventDate(today);
    setLocation('');
    setDescription('');
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
    if (!vehicle.trim() || !location.trim()) {
      setError('Заповніть «Авто» та «Локація» (обов’язкові поля).');
      return;
    }
    setSubmitting(true);
    setError(null);
    try {
      const body: CreateClaimBody = {
        customerId: customerId.trim() || null,
        customerName: customerName.trim() || null,
        vehicle: vehicle.trim(),
        vehicleVin: vehicleVin.trim() || null,
        eventType,
        eventDate,
        location: location.trim(),
        description: description.trim() || null,
      };
      const idempKey = `new-claim-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`;
      const result = await insuranceApi.createClaim(body, idempKey);
      dispatch(
        pushToast({
          tone: 'success',
          title: `Створено новий синтетичний кейс ${result.claimId}.`,
          detail: `${result.message} cmd=${result.commandId.slice(0, 14)}…`,
        }),
      );
      reset();
      onClose();
      navigate(`/claims/${result.claimId}`);
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
      size="lg"
      title="Створити новий синтетичний кейс"
      description="Створює новий рядок у БД claims.Claims з повним audit + outbox. Локальний sandbox — без реальних персональних даних, без реальної виплати, без зовнішніх повідомлень."
      footer={
        <>
          <button
            type="button"
            data-testid="new-claim-cancel"
            onClick={handleClose}
            disabled={submitting}
            className="btn-ghost px-3 py-1.5 text-sm disabled:opacity-50"
          >
            Скасувати
          </button>
          <button
            type="submit"
            form="new-claim-form"
            data-testid="new-claim-submit"
            disabled={submitting}
            className="btn-primary inline-flex items-center gap-1.5 px-3 py-1.5 text-sm disabled:opacity-50"
          >
            <Icon name="plus" size={14} />
            {submitting ? 'Створення…' : 'Створити кейс'}
          </button>
        </>
      }
    >
      <form id="new-claim-form" onSubmit={handleSubmit} className="space-y-3 text-sm">
        <div className="grid sm:grid-cols-2 gap-3">
          <div>
            <label className="block text-xs font-semibold text-ink-700 uppercase tracking-wide mb-1.5">
              Клієнт (опціонально)
            </label>
            <input
              type="text"
              data-testid="new-claim-customerName"
              value={customerName}
              onChange={(e) => setCustomerName(e.target.value)}
              disabled={submitting}
              placeholder="Напр. Synthetic Customer 042"
              className="w-full px-3 py-2 rounded-lg border border-ink-200 bg-white text-sm focus-ring"
            />
            <p className="text-[10px] text-ink-500 mt-1">Порожньо → буде обрано першого синтетичного клієнта.</p>
          </div>
          <div>
            <label className="block text-xs font-semibold text-ink-700 uppercase tracking-wide mb-1.5">
              ID клієнта (опціонально)
            </label>
            <input
              type="text"
              data-testid="new-claim-customerId"
              value={customerId}
              onChange={(e) => setCustomerId(e.target.value)}
              disabled={submitting}
              placeholder="CUST-T0042"
              className="w-full px-3 py-2 rounded-lg border border-ink-200 bg-white text-sm focus-ring font-mono"
            />
          </div>
        </div>
        <div className="grid sm:grid-cols-2 gap-3">
          <div>
            <label className="block text-xs font-semibold text-ink-700 uppercase tracking-wide mb-1.5">
              Авто *
            </label>
            <input
              type="text"
              data-testid="new-claim-vehicle"
              value={vehicle}
              onChange={(e) => setVehicle(e.target.value)}
              disabled={submitting}
              required
              placeholder="Honda Civic 2022"
              className="w-full px-3 py-2 rounded-lg border border-ink-200 bg-white text-sm focus-ring"
            />
          </div>
          <div>
            <label className="block text-xs font-semibold text-ink-700 uppercase tracking-wide mb-1.5">
              VIN (опціонально)
            </label>
            <input
              type="text"
              data-testid="new-claim-vehicleVin"
              value={vehicleVin}
              onChange={(e) => setVehicleVin(e.target.value)}
              disabled={submitting}
              className="w-full px-3 py-2 rounded-lg border border-ink-200 bg-white text-sm focus-ring font-mono"
            />
          </div>
        </div>
        <div className="grid sm:grid-cols-2 gap-3">
          <div>
            <label className="block text-xs font-semibold text-ink-700 uppercase tracking-wide mb-1.5">
              Тип події *
            </label>
            <select
              value={eventType}
              onChange={(e) => setEventType(e.target.value)}
              disabled={submitting}
              className="w-full px-3 py-2 rounded-lg border border-ink-200 bg-white text-sm focus-ring"
            >
              {EVENT_TYPE_OPTIONS.map((o) => (
                <option key={o} value={o}>{o}</option>
              ))}
            </select>
          </div>
          <div>
            <label className="block text-xs font-semibold text-ink-700 uppercase tracking-wide mb-1.5">
              Дата події *
            </label>
            <input
              type="date"
              value={eventDate}
              onChange={(e) => setEventDate(e.target.value)}
              disabled={submitting}
              required
              className="w-full px-3 py-2 rounded-lg border border-ink-200 bg-white text-sm focus-ring"
            />
          </div>
        </div>
        <div>
          <label className="block text-xs font-semibold text-ink-700 uppercase tracking-wide mb-1.5">
            Локація *
          </label>
          <input
            type="text"
            data-testid="new-claim-location"
            value={location}
            onChange={(e) => setLocation(e.target.value)}
            disabled={submitting}
            required
            placeholder="Київ, проспект Перемоги 50"
            className="w-full px-3 py-2 rounded-lg border border-ink-200 bg-white text-sm focus-ring"
          />
        </div>
        <div>
          <label className="block text-xs font-semibold text-ink-700 uppercase tracking-wide mb-1.5">
            Опис (опціонально)
          </label>
          <textarea
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            disabled={submitting}
            rows={2}
            placeholder="Короткий опис обставин події (синтетичний)."
            className="w-full px-3 py-2 rounded-lg border border-ink-200 bg-white text-sm focus-ring resize-none"
          />
        </div>
        {error ? (
          <div role="alert" className="rounded-lg border border-danger-200 bg-danger-50 text-danger-700 text-xs px-3 py-2">
            {error}
          </div>
        ) : null}
        <div className="rounded-lg border border-ink-200 bg-ink-50 px-3 py-2 text-[11px] text-ink-600 leading-snug flex items-start gap-2">
          <span className="mt-0.5 shrink-0"><Icon name="shield" size={12} /></span>
          <span>
            Кейс створюється у локальній БД sandbox. Без реальних персональних даних,
            без реальної виплати, без зовнішніх повідомлень. Дія записується в audit + outbox.
          </span>
        </div>
      </form>
    </Modal>
  );
}
