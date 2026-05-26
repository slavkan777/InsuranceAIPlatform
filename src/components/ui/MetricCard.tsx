import clsx from '@/utils/clsx';
import { Icon, type IconName } from './Icon';

type Tone = 'info' | 'good' | 'warn' | 'danger' | 'ai' | 'muted';

interface MetricCardProps {
  label: string;
  value: string;
  delta?: string;
  tone?: Tone;
  icon?: IconName;
}

const tile: Record<Tone, string> = {
  info: 'bg-brand-500/10 text-brand-600',
  good: 'bg-good-500/10 text-good-600',
  warn: 'bg-warn-500/10 text-warn-600',
  danger: 'bg-danger-500/10 text-danger-600',
  ai: 'bg-ai-500/10 text-ai-600',
  muted: 'bg-ink-100 text-ink-500',
};

const deltaColor: Record<Tone, string> = {
  info: 'text-brand-700',
  good: 'text-good-600',
  warn: 'text-warn-600',
  danger: 'text-danger-600',
  ai: 'text-ai-600',
  muted: 'text-ink-500',
};

const accentTop: Record<Tone, string> = {
  info: 'border-t-brand-500',
  good: 'border-t-good-500',
  warn: 'border-t-warn-500',
  danger: 'border-t-danger-500',
  ai: 'border-t-ai-500',
  muted: 'border-t-ink-300',
};

export function MetricCard({ label, value, delta, tone = 'info', icon }: MetricCardProps) {
  return (
    <div className={clsx('card card-pad border-t-[3px]', accentTop[tone])}>
      <div className="flex items-center gap-2.5 mb-3">
        {icon && (
          <span className={clsx('w-9 h-9 rounded-lg grid place-items-center shrink-0', tile[tone])}>
            <Icon name={icon} size={18} />
          </span>
        )}
        <span className="metric-label leading-tight">{label}</span>
      </div>
      <div className="text-3xl font-bold text-ink-900 tracking-tight tabular-nums">{value}</div>
      {delta && <div className={clsx('text-xs font-medium mt-1', deltaColor[tone])}>{delta}</div>}
    </div>
  );
}
