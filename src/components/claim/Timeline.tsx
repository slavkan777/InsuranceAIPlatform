import clsx from '@/utils/clsx';
import { claimTimeline } from '@/data/mock/claim-1006';

const toneDot: Record<string, string> = {
  info: 'bg-brand-500',
  warn: 'bg-warn-500',
  danger: 'bg-danger-500',
  good: 'bg-good-500',
};

export function Timeline() {
  return (
    <div className="card card-pad">
      <div className="section-title mb-4">Хронологія випадку</div>
      <ol className="relative space-y-3 ml-1.5">
        <span className="absolute left-1 top-1 bottom-1 w-px bg-ink-100" />
        {claimTimeline.map((row, idx) => (
          <li key={idx} className="pl-5 relative">
            <span
              className={clsx(
                'absolute left-[-3px] top-1.5 w-2 h-2 rounded-full ring-2 ring-white',
                toneDot[row.tone],
              )}
            />
            <div className="flex flex-wrap items-baseline gap-x-3 gap-y-1">
              <span className="text-[11px] font-mono text-ink-400 uppercase tracking-wider">
                {row.time}
              </span>
              <span
                className={clsx(
                  'text-sm',
                  row.current ? 'font-semibold text-ink-900' : 'text-ink-700',
                )}
              >
                {row.event}
              </span>
            </div>
          </li>
        ))}
      </ol>
    </div>
  );
}
