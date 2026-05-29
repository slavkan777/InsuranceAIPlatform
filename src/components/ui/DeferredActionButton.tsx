import type { ReactNode } from 'react';
import clsx from '@/utils/clsx';
import { useI18n } from '@/i18n/useI18n';

interface DeferredActionButtonProps {
  children: ReactNode;
  /** Tooltip explaining why the action is deferred. */
  hint?: string;
  /** Pass-through so existing button styling (e.g. `btn-primary`) is preserved. */
  className?: string;
  /** Optional small inline badge, e.g. `demo`. */
  badge?: string;
}

/**
 * Visually-present but intentionally non-functional action button. Used for actions
 * that are deferred until a backend write gate exists. Read-only-demo safe: it is
 * `disabled`, never dispatches, and never performs a mutation. Mirrors the disabled-nav
 * treatment in Sidebar so the UI stays honest about what is wired vs. deferred.
 */
export function DeferredActionButton({
  children,
  hint,
  className,
  badge,
}: DeferredActionButtonProps) {
  const { t } = useI18n();
  const resolvedHint = hint ?? t.ui.deferredHint;

  return (
    <button
      type="button"
      disabled
      aria-disabled="true"
      title={resolvedHint}
      className={clsx('cursor-not-allowed opacity-60', className)}
    >
      {children}
      {badge ? (
        <span className="ml-1.5 inline-flex items-center rounded px-1 py-0.5 align-middle text-[9px] font-semibold uppercase tracking-wider bg-ink-200/70 text-ink-500">
          {badge}
        </span>
      ) : null}
    </button>
  );
}
