import { useEffect, type ReactNode } from 'react';
import { Icon } from './Icon';
import clsx from '@/utils/clsx';

interface ModalProps {
  open: boolean;
  onClose: () => void;
  title: string;
  description?: string;
  children: ReactNode;
  footer?: ReactNode;
  /** Optional max-width class. Defaults to max-w-md. */
  size?: 'sm' | 'md' | 'lg';
}

const sizeMap = {
  sm: 'max-w-sm',
  md: 'max-w-md',
  lg: 'max-w-xl',
};

/**
 * Lightweight accessible modal. No focus trap library — keeps bundle small.
 * ESC closes; backdrop click closes; content area is scrollable for long bodies.
 */
export function Modal({
  open,
  onClose,
  title,
  description,
  children,
  footer,
  size = 'md',
}: ModalProps) {
  useEffect(() => {
    if (!open) return;
    function onKey(e: KeyboardEvent) {
      if (e.key === 'Escape') onClose();
    }
    window.addEventListener('keydown', onKey);
    return () => window.removeEventListener('keydown', onKey);
  }, [open, onClose]);

  if (!open) return null;

  return (
    <div
      role="dialog"
      aria-modal="true"
      aria-label={title}
      className="fixed inset-0 z-50 flex items-center justify-center px-4"
    >
      <button
        type="button"
        onClick={onClose}
        aria-label="Закрити вікно"
        className="absolute inset-0 bg-ink-900/50 backdrop-blur-sm"
      />
      <div
        className={clsx(
          'relative w-full bg-white rounded-2xl shadow-xl border border-ink-200 overflow-hidden max-h-[90vh] flex flex-col',
          sizeMap[size],
        )}
      >
        <div className="px-5 py-4 border-b border-ink-100 flex items-start gap-3">
          <div className="flex-1 min-w-0">
            <h3 className="text-base font-semibold text-ink-900">{title}</h3>
            {description ? (
              <p className="text-xs text-ink-500 mt-1 leading-snug">
                {description}
              </p>
            ) : null}
          </div>
          <button
            type="button"
            onClick={onClose}
            aria-label="Закрити"
            className="shrink-0 w-8 h-8 rounded-lg grid place-items-center text-ink-500 hover:bg-ink-100"
          >
            <Icon name="x" size={16} />
          </button>
        </div>
        <div className="flex-1 overflow-y-auto px-5 py-4">{children}</div>
        {footer ? (
          <div className="px-5 py-3 border-t border-ink-100 flex items-center justify-end gap-2 bg-ink-50/40">
            {footer}
          </div>
        ) : null}
      </div>
    </div>
  );
}
