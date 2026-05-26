import { StatusPill } from '@/components/ui/StatusPill';
import { goldenClaim } from '@/data/mock/claims';
import { aiPipelineSteps, auditTrail, costDistribution } from '@/data/mock/claim-1006';
import clsx from '@/utils/clsx';

const resultTone = {
  OK: 'good',
  WARN: 'warn',
  BLOCK: 'danger',
} as const;

const stepDotTone: Record<string, string> = {
  done: 'bg-good-500',
  warn: 'bg-warn-500',
  risk: 'bg-danger-500',
  pending: 'bg-ink-300',
};

export default function AuditCostPage() {
  const c = goldenClaim;
  return (
    <div className="flex flex-col gap-5">
      <section className="card card-pad flex flex-wrap items-center gap-x-6 gap-y-3 justify-between">
        <div>
          <h2 className="text-xl font-bold text-ink-900">Аудит і витрати AI-запуску</h2>
          <p className="text-sm text-ink-500 mt-1">
            {c.id} · повний слід виконання · governance evidence
          </p>
        </div>
        <StatusPill tone="good">Запуск успішний</StatusPill>
      </section>

      <section className="grid grid-cols-2 lg:grid-cols-6 gap-3">
        {[
          { label: 'Run ID', value: c.runId, mono: true },
          { label: 'Trace ID', value: c.traceId, mono: true },
          { label: 'Модель', value: 'Azure OpenAI' },
          { label: 'Токени', value: c.tokens.toLocaleString('uk-UA'), mono: true },
          { label: 'Вартість', value: `$${c.cost.toFixed(4)}`, mono: true },
          { label: 'Час', value: `${c.durationSec} с`, mono: true },
        ].map((item) => (
          <div key={item.label} className="card card-pad">
            <div className="metric-label">{item.label}</div>
            <div
              className={clsx(
                'text-base font-semibold text-ink-900 mt-2 break-all',
                item.mono && 'font-mono',
              )}
            >
              {item.value}
            </div>
          </div>
        ))}
      </section>

      <section className="card card-pad">
        <div className="section-title mb-4">Хід AI-запуску</div>
        <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-7 gap-2">
          {aiPipelineSteps.map((step) => (
            <div key={step.id} className="rounded-xl border border-ink-100 bg-ink-50 p-3 text-center">
              <span
                className={clsx('inline-block w-2 h-2 rounded-full mb-2', stepDotTone[step.status])}
              />
              <div className="text-xs font-medium text-ink-800 truncate">
                {step.label}
              </div>
              <div className="text-[11px] text-ink-500 mt-1 font-mono">{step.duration}</div>
            </div>
          ))}
        </div>
      </section>

      <div className="grid xl:grid-cols-3 gap-5">
        <section className="card overflow-hidden xl:col-span-2">
          <div className="px-5 py-4 border-b border-ink-100">
            <div className="section-title">Audit trail</div>
            <p className="text-sm text-ink-500 mt-0.5">Усі дії з повним слідом</p>
          </div>
          <table className="w-full">
            <thead className="bg-ink-50/80">
              <tr>
                <th className="table-th">Час</th>
                <th className="table-th">Актор</th>
                <th className="table-th">Дія</th>
                <th className="table-th text-right">Рез.</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-ink-100">
              {auditTrail.map((row, idx) => (
                <tr key={idx} className="hover:bg-ink-50">
                  <td className="table-td font-mono text-xs text-ink-600">{row.time}</td>
                  <td className="table-td">
                    <span className="font-medium text-ink-900">{row.actor}</span>
                  </td>
                  <td className="table-td text-ink-700">{row.action}</td>
                  <td className="table-td text-right">
                    <StatusPill tone={resultTone[row.result]}>{row.result}</StatusPill>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </section>

        <aside className="flex flex-col gap-5">
          <section className="card card-pad">
            <div className="section-title mb-3">Розподіл витрат</div>
            <ul className="space-y-2">
              {costDistribution.map((row) => {
                const value = parseFloat(row.value.replace('$', ''));
                const pct = (value / c.cost) * 100;
                return (
                  <li key={row.id}>
                    <div className="flex items-center justify-between text-sm">
                      <span className="text-ink-700">{row.label}</span>
                      <span className="font-mono text-ink-900 font-semibold">{row.value}</span>
                    </div>
                    <div className="h-1.5 w-full rounded-full bg-ink-100 overflow-hidden mt-1">
                      <div
                        className="h-full rounded-full bg-brand-500"
                        style={{ width: `${pct}%` }}
                      />
                    </div>
                  </li>
                );
              })}
            </ul>
          </section>

          <section className="card card-pad bg-gradient-to-br from-good-500/5 to-white border-good-200">
            <div className="metric-label text-good-600">Governance</div>
            <h4 className="text-sm font-semibold text-ink-900 mt-1">
              AI підпорядковується процедурі
            </h4>
            <ul className="text-sm text-ink-700 mt-2 space-y-1">
              <li>
                · Авто-погодження:{' '}
                <span className="font-semibold text-danger-600">НЕ ДОЗВОЛЕНО</span>
              </li>
              <li>
                · Людська перевірка:{' '}
                <span className="font-semibold text-good-600">ОБОВ'ЯЗКОВА</span>
              </li>
              <li>
                · Логи рішень: <span className="font-semibold text-good-600">ТАК</span>
              </li>
              <li>
                · Replay: <span className="font-semibold text-good-600">ТАК</span>
              </li>
            </ul>
          </section>
        </aside>
      </div>
    </div>
  );
}
