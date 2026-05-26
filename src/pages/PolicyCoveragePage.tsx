import { StatusPill } from '@/components/ui/StatusPill';
import { goldenClaim } from '@/data/mock/claims';
import { policyCoverageBlocks, policyValidation } from '@/data/mock/claim-1006';

export default function PolicyCoveragePage() {
  const c = goldenClaim;
  return (
    <div className="flex flex-col gap-5">
      <section className="card card-pad flex flex-wrap items-center gap-x-6 gap-y-3 justify-between">
        <div>
          <h2 className="text-xl font-bold text-ink-900">Поліс і покриття</h2>
          <p className="text-sm text-ink-500 mt-1">
            <span className="font-mono">{c.policyId}</span> · {c.policy} · {c.customer}
          </p>
        </div>
        <StatusPill tone="good">Активний · до закінчення 220 днів</StatusPill>
      </section>

      <section className="card card-pad">
        <div className="flex flex-wrap items-baseline justify-between gap-2 mb-3">
          <div>
            <div className="metric-label">Поліс</div>
            <h3 className="text-lg font-semibold text-ink-900 mt-0.5">{c.policy}</h3>
            <p className="text-xs text-ink-500 mt-1">
              <span className="font-mono">{c.policyId}</span> · дійсний 01.01.2026 — 31.12.2026
            </p>
          </div>
          <div className="text-right">
            <div className="metric-label">До закінчення</div>
            <div className="text-base font-bold text-good-600">220 днів</div>
          </div>
        </div>
      </section>

      <div className="grid md:grid-cols-2 xl:grid-cols-3 gap-4">
        {policyCoverageBlocks.map((block) => (
          <div key={block.id} className="card card-pad">
            <div className="text-sm font-semibold text-ink-900">{block.title}</div>
            <dl className="grid grid-cols-2 gap-3 mt-3 text-sm">
              <div>
                <dt className="metric-label">Ліміт</dt>
                <dd className="font-semibold text-ink-900 mt-1 font-mono">{block.limit}</dd>
              </div>
              <div>
                <dt className="metric-label">Франшиза</dt>
                <dd className="font-semibold text-ink-900 mt-1 font-mono">{block.deductible}</dd>
              </div>
            </dl>
          </div>
        ))}
      </div>

      <div className="grid xl:grid-cols-3 gap-5">
        <section className="card card-pad">
          <div className="section-title mb-3">Ліміти та франшиза</div>
          <dl className="space-y-2 text-sm">
            {[
              ['Загальний ліміт', '$100 000'],
              ['Ліміт на ДТП', '$50 000'],
              ['Базова франшиза', '$500'],
              ['Бонус-малус', '–10%'],
            ].map(([k, v]) => (
              <div key={k} className="flex items-center justify-between">
                <dt className="text-ink-600">{k}</dt>
                <dd className="font-mono font-semibold text-ink-900">{v}</dd>
              </div>
            ))}
          </dl>
        </section>

        <section className="card card-pad">
          <div className="section-title mb-3">Виключення</div>
          <ul className="space-y-2 text-sm text-ink-700">
            <li>· Стан сп'яніння</li>
            <li>· Гонки</li>
            <li>· Військові дії</li>
          </ul>
        </section>

        <section className="card card-pad">
          <div className="section-title mb-3">Валідація полісу</div>
          <ul className="space-y-1.5 text-sm">
            {policyValidation.map((v) => (
              <li key={v} className="flex items-start gap-2">
                <span className="mt-1 w-4 h-4 rounded-full bg-good-500 text-white grid place-items-center text-[10px] font-bold shrink-0">
                  ✓
                </span>
                <span className="text-ink-700">{v}</span>
              </li>
            ))}
          </ul>
        </section>
      </div>

      <div className="grid md:grid-cols-2 gap-5">
        <section className="card card-pad flex items-center gap-4">
          <div className="w-14 h-14 rounded-full bg-gradient-to-br from-brand-400 to-brand-700 grid place-items-center text-white text-base font-semibold">
            РД
          </div>
          <div>
            <div className="metric-label">Власник</div>
            <div className="text-base font-semibold text-ink-900 mt-0.5">{c.customer}</div>
            <div className="text-xs text-ink-500 mt-0.5">Клієнт з 2021</div>
          </div>
        </section>
        <section className="card card-pad flex items-center gap-4">
          <div className="w-14 h-14 rounded-lg bg-ink-100 grid place-items-center text-base font-bold text-ink-700">
            ⌬
          </div>
          <div>
            <div className="metric-label">Транспорт</div>
            <div className="text-base font-semibold text-ink-900 mt-0.5">{c.vehicle}</div>
            <div className="text-xs text-ink-500 mt-0.5">
              {c.vehicleVin} · застрах. $24 800
            </div>
          </div>
        </section>
      </div>
    </div>
  );
}
