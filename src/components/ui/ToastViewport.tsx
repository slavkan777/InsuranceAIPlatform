import { useAppDispatch, useAppSelector } from '@/app/hooks';
import { selectToasts } from '@/features/ui/uiFeedbackSelectors';
import { dismissToast } from '@/features/ui/uiFeedbackSlice';
import { Icon } from './Icon';
import clsx from '@/utils/clsx';

const toneStyles = {
  success: 'border-good-300 bg-good-50 text-good-800',
  warning: 'border-warn-300 bg-warn-50 text-warn-800',
  error: 'border-danger-300 bg-danger-50 text-danger-800',
  info: 'border-brand-300 bg-brand-50 text-brand-800',
} as const;

const toneIcon = {
  success: 'check',
  warning: 'shield',
  error: 'shield',
  info: 'cpu',
} as const;

/** Renders the active feedback toasts in the bottom-right corner. */
export function ToastViewport() {
  const toasts = useAppSelector(selectToasts);
  const dispatch = useAppDispatch();

  if (toasts.length === 0) return null;

  return (
    <div
      className="fixed right-4 bottom-4 z-50 flex flex-col gap-2 max-w-sm pointer-events-none"
      role="status"
      aria-live="polite"
    >
      {toasts.map((t) => (
        <div
          key={t.id}
          className={clsx(
            'pointer-events-auto rounded-xl border px-3.5 py-3 shadow-lg backdrop-blur',
            toneStyles[t.tone],
          )}
        >
          <div className="flex items-start gap-2.5">
            <span className="mt-0.5 shrink-0">
              <Icon name={toneIcon[t.tone]} size={16} />
            </span>
            <div className="flex-1 min-w-0">
              <div className="text-xs font-semibold leading-tight">{t.title}</div>
              {t.detail ? (
                <div className="text-[11px] mt-1 leading-snug opacity-90">
                  {t.detail}
                </div>
              ) : null}
            </div>
            <button
              type="button"
              onClick={() => dispatch(dismissToast(t.id))}
              className="shrink-0 -mr-1 -mt-1 w-6 h-6 rounded-md grid place-items-center text-current opacity-70 hover:opacity-100"
              aria-label="Закрити повідомлення"
            >
              <Icon name="x" size={14} />
            </button>
          </div>
        </div>
      ))}
    </div>
  );
}
