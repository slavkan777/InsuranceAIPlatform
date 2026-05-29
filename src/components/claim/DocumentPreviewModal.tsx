import { Modal } from '@/components/ui/Modal';
import { Icon } from '@/components/ui/Icon';
import { useI18n } from '@/i18n/useI18n';

interface DocumentPreviewModalProps {
  open: boolean;
  onClose: () => void;
  documentTitle?: string;
}

/**
 * Honest "view original" modal. Document originals are not stored in this
 * demo — only metadata. We explain that without faking a preview image.
 */
export function DocumentPreviewModal({
  open,
  onClose,
  documentTitle,
}: DocumentPreviewModalProps) {
  const { t } = useI18n();
  const resolvedTitle = documentTitle ?? t.ui.docPreviewDefaultTitle;

  return (
    <Modal
      open={open}
      onClose={onClose}
      title={t.ui.docPreviewTitle}
      description={t.ui.docPreviewDescription}
      footer={
        <button
          type="button"
          onClick={onClose}
          className="btn-primary inline-flex items-center gap-1.5 px-3 py-1.5 text-sm"
        >
          <Icon name="check" size={14} />
          {t.ui.docPreviewClose}
        </button>
      }
    >
      <div className="space-y-3 text-sm">
        <div className="rounded-xl border border-ink-200 bg-ink-50 p-4 flex flex-col items-center gap-2 text-center">
          <span className="w-10 h-10 rounded-lg bg-white text-ink-400 grid place-items-center border border-ink-200">
            <Icon name="file" size={20} />
          </span>
          <div>
            <div className="text-sm font-semibold text-ink-800">
              {resolvedTitle}
            </div>
            <div className="text-[11px] text-ink-500 mt-1 leading-snug">
              {t.ui.docPreviewNotAvailable}
            </div>
          </div>
        </div>
        <ul className="text-xs text-ink-600 space-y-1.5 list-disc pl-5">
          <li>{t.ui.docPreviewBullet1}</li>
          <li>{t.ui.docPreviewBullet2}</li>
          <li>{t.ui.docPreviewBullet3}</li>
        </ul>
      </div>
    </Modal>
  );
}
