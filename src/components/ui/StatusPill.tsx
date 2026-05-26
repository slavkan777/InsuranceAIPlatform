import clsx from '@/utils/clsx';
import type { ReactNode } from 'react';

type Tone = 'good' | 'warn' | 'danger' | 'info' | 'ai' | 'muted';

const palette: Record<Tone, string> = {
  good: 'bg-good-500/10 text-good-700 border border-good-500/20',
  warn: 'bg-warn-500/15 text-warn-700 border border-warn-500/25',
  danger: 'bg-danger-500/10 text-danger-700 border border-danger-500/20',
  info: 'bg-brand-50 text-brand-700 border border-brand-500/15',
  ai: 'bg-ai-500/10 text-ai-700 border border-ai-500/20',
  muted: 'bg-ink-100 text-ink-600 border border-ink-200',
};

export function StatusPill({
  tone = 'info',
  children,
  className,
}: {
  tone?: Tone;
  children: ReactNode;
  className?: string;
}) {
  return (
    <span
      className={clsx(
        'inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-[11px] font-semibold uppercase tracking-wider',
        palette[tone],
        className,
      )}
    >
      {children}
    </span>
  );
}
