import { useState, type FormEvent } from 'react';
import { useAppDispatch } from '@/app/hooks';
import { Modal } from '@/components/ui/Modal';
import { Icon } from '@/components/ui/Icon';
import { pushToast } from '@/features/ui/uiFeedbackSlice';
import { insuranceApi } from '@/api/insuranceApi';
import type { CreateDocumentMetadataBody } from '@/api/insuranceApi.types';

interface ImportDocumentMetadataModalProps {
  open: boolean;
  onClose: () => void;
  /** Claim to attach the document metadata to. Defaults to CLM-1006 (demo claim). */
  claimId?: string;
}

const DOC_KIND_OPTIONS = [
  { value: 'document', label: 'Документ (PDF/довідка)' },
  { value: 'photo', label: 'Фото пошкодження' },
  { value: 'note', label: 'Внутрішня нотатка' },
];

const DOC_TYPE_OPTIONS = [
  { value: '', label: '— оберіть тип —' },
  { value: 'PoliceReport', label: 'Поліцейський звіт' },
  { value: 'DriverLicense', label: 'Посвідчення водія' },
  { value: 'Estimate', label: 'Кошторис ремонту' },
  { value: 'DamagePhoto', label: 'Фото пошкоджень' },
  { value: 'OtherDocument', label: 'Інший документ' },
];

/**
 * Document metadata-only import. NO binary upload, NO blob storage, NO OCR.
 * Calls POST /api/claims/{claimId}/document-metadata which writes a metadata
 * row + audit + outbox locally. Safe for portfolio demo.
 */
export function ImportDocumentMetadataModal({
  open,
  onClose,
  claimId = 'CLM-1006',
}: ImportDocumentMetadataModalProps) {
  const dispatch = useAppDispatch();
  const [kind, setKind] = useState('document');
  const [title, setTitle] = useState('');
  const [docType, setDocType] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  function reset() {
    setKind('document');
    setTitle('');
    setDocType('');
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
    if (!title.trim()) {
      setError('Вкажіть назву документа.');
      return;
    }
    setSubmitting(true);
    setError(null);
    try {
      const body: CreateDocumentMetadataBody = {
        kind,
        title: title.trim(),
        docType: docType || null,
      };
      const idempKey = `import-doc-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`;
      const result = await insuranceApi.createDocumentMetadata(
        claimId,
        body,
        idempKey,
      );
      dispatch(
        pushToast({
          tone: 'success',
          title: 'Метадані документа збережено.',
          detail: `${result.message} cmd=${result.commandId.slice(0, 14)}…`,
        }),
      );
      reset();
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
      onClose={handleClose}
      title="Імпорт документа (метадані)"
      description="Створює запис метаданих у БД для кейсу. Бінарне завантаження не виконується — лише довідкові поля. Аудит-журнал і outbox оновлюються."
      footer={
        <>
          <button
            type="button"
            onClick={handleClose}
            disabled={submitting}
            className="btn-ghost px-3 py-1.5 text-sm disabled:opacity-50"
          >
            Скасувати
          </button>
          <button
            type="submit"
            form="import-doc-form"
            disabled={submitting}
            className="btn-primary inline-flex items-center gap-1.5 px-3 py-1.5 text-sm disabled:opacity-50"
          >
            <Icon name="upload" size={14} />
            {submitting ? 'Збереження…' : 'Зберегти метадані'}
          </button>
        </>
      }
    >
      <form id="import-doc-form" onSubmit={handleSubmit} className="space-y-3 text-sm">
        <div>
          <label className="block text-xs font-semibold text-ink-700 uppercase tracking-wide mb-1.5">
            Тип запису
          </label>
          <select
            value={kind}
            onChange={(e) => setKind(e.target.value)}
            disabled={submitting}
            className="w-full px-3 py-2 rounded-lg border border-ink-200 bg-white text-sm focus-ring disabled:bg-ink-50"
          >
            {DOC_KIND_OPTIONS.map((o) => (
              <option key={o.value} value={o.value}>
                {o.label}
              </option>
            ))}
          </select>
        </div>
        <div>
          <label className="block text-xs font-semibold text-ink-700 uppercase tracking-wide mb-1.5">
            Назва
          </label>
          <input
            type="text"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            disabled={submitting}
            placeholder="Напр. Поліцейський звіт DTP-2026-1234"
            className="w-full px-3 py-2 rounded-lg border border-ink-200 bg-white text-sm focus-ring disabled:bg-ink-50"
          />
        </div>
        <div>
          <label className="block text-xs font-semibold text-ink-700 uppercase tracking-wide mb-1.5">
            Тип документа (опціонально)
          </label>
          <select
            value={docType}
            onChange={(e) => setDocType(e.target.value)}
            disabled={submitting}
            className="w-full px-3 py-2 rounded-lg border border-ink-200 bg-white text-sm focus-ring disabled:bg-ink-50"
          >
            {DOC_TYPE_OPTIONS.map((o) => (
              <option key={o.value} value={o.value}>
                {o.label}
              </option>
            ))}
          </select>
        </div>
        {error ? (
          <div
            role="alert"
            className="rounded-lg border border-danger-200 bg-danger-50 text-danger-700 text-xs px-3 py-2"
          >
            {error}
          </div>
        ) : null}
        <div className="rounded-lg border border-ink-200 bg-ink-50 px-3 py-2 text-[11px] text-ink-600 leading-snug">
          Запис створює лише довідковий рядок у таблиці документів кейсу
          {' '}<span className="font-mono text-ink-800">{claimId}</span>.
          Жодних файлів не завантажується.
        </div>
      </form>
    </Modal>
  );
}
