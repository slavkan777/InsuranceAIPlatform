import { useEffect, useState, type FormEvent } from 'react';
import { useAppDispatch } from '@/app/hooks';
import { Modal } from '@/components/ui/Modal';
import { Icon } from '@/components/ui/Icon';
import { pushToast } from '@/features/ui/uiFeedbackSlice';
import { insuranceApi } from '@/api/insuranceApi';
import type { UploadDocumentContentBody } from '@/api/insuranceApi.types';
import { useI18n } from '@/i18n/useI18n';

interface UploadDocumentContentModalProps {
  open: boolean;
  onClose: () => void;
  claimId?: string;
  /** Optional callback after successful upload (e.g. to refresh a doc list). */
  onUploaded?: (commandId: string) => void;
}

const SAMPLE_TEMPLATES: Record<string, string> = {
  'police-report':
    'ДТП на перехресті Київська 24, 18.05.2026 14:32.\n' +
    'Учасники: 2. Постраждалі: 0. Винуватець: Сторона Б (Toyota Camry 2021).\n' +
    'Видимі пошкодження: задній бампер, задні двері.\n' +
    'Поліцейський: Іваненко О.М.',
  'customer-statement':
    'Я, [синтетичне імʼя], підтверджую факт ДТП 18.05.2026 о 14:32.\n' +
    'Автомобіль: Toyota Camry 2021. Пошкодження: задній бампер.\n' +
    'Свідомі помилки протоколу відсутні.',
  estimate:
    'Кошторис на ремонт Toyota Camry 2021:\n' +
    '— заміна заднього бампера: 1 200 USD\n' +
    '— ремонт задніх дверей: 800 USD\n' +
    '— покраска: 720 USD\n' +
    'Сума: 2 720 USD',
};

/**
 * Persists a synthetic document with real text content to the DB via
 * POST /api/claims/{id}/documents/upload. NO binary, NO file system, NO blob.
 * Plain nvarchar(max) column. A reviewer can verify via direct SQL SELECT.
 */
export function UploadDocumentContentModal({
  open,
  onClose,
  claimId = 'CLM-1006',
  onUploaded,
}: UploadDocumentContentModalProps) {
  const { t } = useI18n();
  const dispatch = useAppDispatch();

  const KIND_OPTIONS = [
    { value: 'police-report', label: t.ui.uploadKindPoliceReport },
    { value: 'customer-statement', label: t.ui.uploadKindCustomerStatement },
    { value: 'estimate', label: t.ui.uploadKindEstimate },
    { value: 'note', label: t.ui.uploadKindInternalNote },
    { value: 'damage-summary', label: t.ui.uploadKindDamageSummary },
  ];

  const DOC_TYPE_OPTIONS = [
    { value: '', label: t.ui.uploadDocTypePlaceholder },
    { value: 'PoliceReport', label: t.ui.uploadDocTypePoliceReport },
    { value: 'CustomerStatement', label: t.ui.uploadDocTypeCustomerStatement },
    { value: 'Estimate', label: t.ui.uploadDocTypeEstimate },
    { value: 'InternalNote', label: t.ui.uploadDocTypeInternalNote },
    { value: 'OtherDocument', label: t.ui.uploadDocTypeOther },
  ];

  const [kind, setKind] = useState('police-report');
  const [title, setTitle] = useState('');
  const [docType, setDocType] = useState('');
  const [content, setContent] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (open) {
      setKind('police-report');
      setTitle('Поліцейський звіт NoБРС-2026/' + Math.floor(Math.random() * 9000 + 1000));
      setDocType('PoliceReport');
      setContent(SAMPLE_TEMPLATES['police-report']);
      setSubmitting(false);
      setError(null);
    }
  }, [open]);

  function handleKindChange(next: string) {
    setKind(next);
    if (SAMPLE_TEMPLATES[next] && !content) setContent(SAMPLE_TEMPLATES[next]);
  }

  function handleUseTemplate() {
    if (SAMPLE_TEMPLATES[kind]) setContent(SAMPLE_TEMPLATES[kind]);
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    if (!title.trim() || !content.trim()) {
      setError(t.ui.uploadDocErrorRequired);
      return;
    }
    setSubmitting(true);
    setError(null);
    try {
      const body: UploadDocumentContentBody = {
        kind,
        title: title.trim(),
        docType: docType || null,
        content,
      };
      const idempKey = `upload-content-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`;
      const result = await insuranceApi.uploadDocumentContent(claimId, body, idempKey);
      dispatch(
        pushToast({
          tone: 'success',
          title: t.ui.uploadDocToastTitle,
          detail: `${result.message} cmd=${result.commandId.slice(0, 14)}…`,
        }),
      );
      onUploaded?.(result.commandId);
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
      title={t.ui.uploadDocTitle}
      description={t.ui.uploadDocDescription}
      footer={
        <>
          <button
            type="button"
            data-testid="upload-doc-cancel"
            onClick={() => !submitting && onClose()}
            disabled={submitting}
            className="btn-ghost px-3 py-1.5 text-sm disabled:opacity-50"
          >
            {t.ui.uploadDocCancel}
          </button>
          <button
            type="submit"
            form="upload-content-form"
            data-testid="upload-doc-submit"
            disabled={submitting}
            className="btn-primary inline-flex items-center gap-1.5 px-3 py-1.5 text-sm disabled:opacity-50"
          >
            <Icon name="upload" size={14} />
            {submitting ? t.ui.uploadDocSubmitting : t.ui.uploadDocSubmit}
          </button>
        </>
      }
    >
      <form id="upload-content-form" onSubmit={handleSubmit} className="space-y-3 text-sm">
        <div className="grid sm:grid-cols-2 gap-3">
          <div>
            <label className="block text-xs font-semibold text-ink-700 uppercase tracking-wide mb-1.5">
              {t.ui.uploadDocLabelKind}
            </label>
            <select
              value={kind}
              onChange={(e) => handleKindChange(e.target.value)}
              disabled={submitting}
              className="w-full px-3 py-2 rounded-lg border border-ink-200 bg-white text-sm focus-ring"
            >
              {KIND_OPTIONS.map((o) => (
                <option key={o.value} value={o.value}>{o.label}</option>
              ))}
            </select>
          </div>
          <div>
            <label className="block text-xs font-semibold text-ink-700 uppercase tracking-wide mb-1.5">
              {t.ui.uploadDocLabelDocType}
            </label>
            <select
              value={docType}
              onChange={(e) => setDocType(e.target.value)}
              disabled={submitting}
              className="w-full px-3 py-2 rounded-lg border border-ink-200 bg-white text-sm focus-ring"
            >
              {DOC_TYPE_OPTIONS.map((o) => (
                <option key={o.value} value={o.value}>{o.label}</option>
              ))}
            </select>
          </div>
        </div>
        <div>
          <label className="block text-xs font-semibold text-ink-700 uppercase tracking-wide mb-1.5">
            {t.ui.uploadDocLabelTitle}
          </label>
          <input
            type="text"
            data-testid="upload-doc-title"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            disabled={submitting}
            required
            className="w-full px-3 py-2 rounded-lg border border-ink-200 bg-white text-sm focus-ring"
          />
        </div>
        <div>
          <div className="flex items-center justify-between mb-1.5">
            <label className="text-xs font-semibold text-ink-700 uppercase tracking-wide">
              {t.ui.uploadDocLabelContent}
            </label>
            <button
              type="button"
              onClick={handleUseTemplate}
              className="text-[10px] text-brand-700 hover:text-brand-900 font-semibold uppercase tracking-wide"
            >
              {t.ui.uploadDocUseTemplate}
            </button>
          </div>
          <textarea
            value={content}
            data-testid="upload-doc-content"
            onChange={(e) => setContent(e.target.value)}
            disabled={submitting}
            rows={9}
            placeholder={t.ui.uploadDocPlaceholderContent}
            className="w-full px-3 py-2 rounded-lg border border-ink-200 bg-white text-xs focus-ring font-mono resize-y"
          />
          <p className="text-[10px] text-ink-500 mt-1">
            {content.length} {t.ui.uploadDocHelperLength}
          </p>
        </div>
        {error ? (
          <div role="alert" className="rounded-lg border border-danger-200 bg-danger-50 text-danger-700 text-xs px-3 py-2">
            {error}
          </div>
        ) : null}
        <div className="rounded-lg border border-ink-200 bg-ink-50 px-3 py-2 text-[11px] text-ink-600 leading-snug flex items-start gap-2">
          <span className="mt-0.5 shrink-0"><Icon name="shield" size={12} /></span>
          <span>
            {t.ui.uploadDocSandboxNotePrefix}{' '}
            <span className="font-mono text-ink-800">{claimId}</span>{' '}
            {t.ui.uploadDocSandboxNoteSuffix}
          </span>
        </div>
      </form>
    </Modal>
  );
}
