import clsx from '@/utils/clsx';

interface ProgressBarProps {
  value: number;
  max?: number;
  tone?: 'brand' | 'ai' | 'good' | 'warn' | 'danger';
  label?: string;
}

const toneFill: Record<NonNullable<ProgressBarProps['tone']>, string> = {
  brand: 'bg-brand-500',
  ai: 'bg-ai-500',
  good: 'bg-good-500',
  warn: 'bg-warn-500',
  danger: 'bg-danger-500',
};

export function ProgressBar({ value, max = 100, tone = 'brand', label }: ProgressBarProps) {
  const pct = Math.min(100, Math.max(0, (value / max) * 100));
  return (
    <div className="w-full">
      {label && (
        <div className="flex items-center justify-between mb-1">
          <span className="text-xs font-medium text-ink-600">{label}</span>
          <span className="text-xs font-semibold text-ink-700">{Math.round(pct)}%</span>
        </div>
      )}
      <div className="h-1.5 w-full rounded-full bg-ink-100 overflow-hidden">
        <div
          className={clsx('h-full rounded-full transition-all', toneFill[tone])}
          style={{ width: `${pct}%` }}
        />
      </div>
    </div>
  );
}
