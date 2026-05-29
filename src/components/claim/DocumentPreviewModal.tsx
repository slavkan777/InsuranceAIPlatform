import { Modal } from '@/components/ui/Modal';
import { Icon } from '@/components/ui/Icon';

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
  documentTitle = 'Документ',
}: DocumentPreviewModalProps) {
  return (
    <Modal
      open={open}
      onClose={onClose}
      title="Перегляд оригіналу"
      description="У цьому демо ми не зберігаємо файли — лише довідкові метадані. Реальний перегляд оригіналу буде доступний після підключення бінарного сховища (Azure Blob / S3 / on-prem)."
      footer={
        <button
          type="button"
          onClick={onClose}
          className="btn-primary inline-flex items-center gap-1.5 px-3 py-1.5 text-sm"
        >
          <Icon name="check" size={14} />
          Зрозуміло
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
              {documentTitle}
            </div>
            <div className="text-[11px] text-ink-500 mt-1 leading-snug">
              Оригінал не доступний у демо-режимі. Метадані документа
              (тип, дата, статус перевірки) зберігаються у БД.
            </div>
          </div>
        </div>
        <ul className="text-xs text-ink-600 space-y-1.5 list-disc pl-5">
          <li>Демо-режим не приймає бінарні завантаження.</li>
          <li>OCR/класифікація і виявлення цілісності — поза скоупом локального демо.</li>
          <li>Журнал аудиту відображає, коли і ким переглядали метадані.</li>
        </ul>
      </div>
    </Modal>
  );
}
