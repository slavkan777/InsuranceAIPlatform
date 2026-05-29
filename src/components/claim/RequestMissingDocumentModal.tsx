import { useEffect, useState, type FormEvent } from 'react';
import { useAppDispatch } from '@/app/hooks';
import { Modal } from '@/components/ui/Modal';
import { Icon } from '@/components/ui/Icon';
import { pushToast } from '@/features/ui/uiFeedbackSlice';
import { insuranceApi } from '@/api/insuranceApi';
import type { RequestMissingDocumentBody } from '@/api/insuranceApi.types';
import { useI18n } from '@/i18n/useI18n';

interface RequestMissingDocumentModalProps {
  open: boolean;
  onClose: () => void;
  claimId?: string;
  /** Pre-fill the document-title field, e.g. when triggered from a specific missing item. */
  defaultTitle?: string;
  /** Optional default reason; rarely used in current UX. */
  defaultReason?: string;
}

/**
 * Records an internal missing-document request for a claim. Calls
 * POST /api/claims/{claimId}/missing-document-requests. NO external customer
 * message is sent — this is an internal audit + outbox record only.
 */
export function RequestMissingDocumentModal({
  open,
  onClose,
  claimId = 'CLM-1006',
  defaultTitle = '',
  defaultReason = '',
}: RequestMissingDocumentModalProps) {
  const { t } = useI18n();
  const dispatch = useAppDispatch();
  const [title, setTitle] = useState(defaultTitle);
  const [reason, setReason] = useState(defaultReason);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (open) {
      setTitle(defaultTitle);
      setReason(defaultReason);
      setError(null);
      setSubmitting(false);
    }
  }, [open, defaultTitle, defaultReason]);

  function handleClose() {
    if (submitting) return;
    onClose();
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    if (!title.trim()) {
      setError(t.ui.reqDocErrorRequired);
      return;
    }
    setSubmitting(true);
    setError(null);
    try {
      const body: RequestMissingDocumentBody = {
        documentTitle: title.trim(),
        reason: reason.trim() || null,
      };
      const idempKey = `req-doc-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`;
      const result = await insuranceApi.requestMissingDocument(
        claimId,
        body,
        idempKey,
      );
      dispatch(
        pushToast({
          tone: 'success',
          title: t.ui.reqDocToastTitle,
          detail: `${result.message} cmd=${result.commandId.slice(0, 14)}…`,
        }),
      );
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
      title={t.ui.reqDocTitle}
      description={t.ui.reqDocDescription}
      footer={
        <>
          <button
            type="button"
            onClick={handleClose}
            disabled={submitting}
            className="btn-ghost px-3 py-1.5 text-sm disabled:opacity-50"
          >
            {t.ui.reqDocCancel}
          </button>
          <button
            type="submit"
            form="request-doc-form"
            disabled={submitting}
            className="btn-primary inline-flex items-center gap-1.5 px-3 py-1.5 text-sm disabled:opacity-50"
          >
            <Icon name="check" size={14} />
            {submitting ? t.ui.reqDocSubmitting : t.ui.reqDocSubmit}
          </button>
        </>
      }
    >
      <form
        id="request-doc-form"
        onSubmit={handleSubmit}
        className="space-y-3 text-sm"
      >
        <div>
          <label className="block text-xs font-semibold text-ink-700 uppercase tracking-wide mb-1.5">
            {t.ui.reqDocLabelTitle}
          </label>
          <input
            type="text"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            disabled={submitting}
            placeholder={t.ui.reqDocPlaceholderTitle}
            className="w-full px-3 py-2 rounded-lg border border-ink-200 bg-white text-sm focus-ring disabled:bg-ink-50"
          />
        </div>
        <div>
          <label className="block text-xs font-semibold text-ink-700 uppercase tracking-wide mb-1.5">
            {t.ui.reqDocLabelReason}
          </label>
          <textarea
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            disabled={submitting}
            rows={2}
            placeholder={t.ui.reqDocPlaceholderReason}
            className="w-full px-3 py-2 rounded-lg border border-ink-200 bg-white text-sm focus-ring disabled:bg-ink-50 resize-none"
          />
        </div>
        {error ? (
          <div
            role="alert"
            className="rounded-lg border border-danger-200 bg-danger-50 text-danger-700 text-xs px-3 py-2"
          >
            {error}
          </div>
        ) : null}
        <div className="rounded-lg border border-ink-200 bg-ink-50 px-3 py-2 text-[11px] text-ink-600 leading-snug flex items-start gap-2">
          <span className="mt-0.5 shrink-0">
            <Icon name="shield" size={12} />
          </span>
          <span>
            {t.ui.reqDocSandboxNotePrefix}{' '}
            <span className="font-mono text-ink-800">{claimId}</span>.{' '}
            {t.ui.reqDocSandboxNoteSuffix}
          </span>
        </div>
      </form>
    </Modal>
  );
}
