import { useNavigate } from 'react-router-dom';
import { useAppSelector } from '@/app/hooks';
import { StatusPill } from '@/components/ui/StatusPill';
import { goldenClaim } from '@/data/mock/claims';
import { communicationHistory as mockCommunicationHistory, previousClaims as mockPreviousClaims } from '@/data/mock/claim-1006';
import {
  selectClaimDetail,
  selectWorkspaceCustomerVehicle,
} from '@/features/claims/claimWorkspaceSelectors';

export default function CustomerVehiclePage() {
  const navigate = useNavigate();

  // --- store selectors (with mock fallback) ---
  const claimDetailFromStore = useAppSelector(selectClaimDetail);
  const c = claimDetailFromStore ?? goldenClaim;

  const customerVehicleFromStore = useAppSelector(selectWorkspaceCustomerVehicle);
  const previousClaims = customerVehicleFromStore?.previousClaims ?? mockPreviousClaims;
  const communicationHistory = customerVehicleFromStore?.communicationHistory ?? mockCommunicationHistory;

  return (
    <div className="flex flex-col gap-5">
      <section className="card card-pad flex flex-wrap items-center gap-x-6 gap-y-3 justify-between">
        <div>
          <h2 className="text-xl font-bold text-ink-900">Клієнт і транспортний засіб</h2>
          <p className="text-sm text-ink-500 mt-1">
            {c.customer} · {c.vehicle} · контекст для {c.id}
          </p>
        </div>
        <StatusPill tone="good">Активний клієнт</StatusPill>
      </section>

      <div className="grid xl:grid-cols-2 gap-5">
        <section className="card card-pad">
          <div className="flex items-center gap-4 mb-4">
            <div className="w-16 h-16 rounded-full bg-gradient-to-br from-brand-400 to-brand-700 grid place-items-center text-white text-lg font-semibold">
              РД
            </div>
            <div>
              <h3 className="text-lg font-semibold text-ink-900">{c.customer}</h3>
              <p className="text-xs text-ink-500 mt-0.5">
                Клієнт з 2021 · <span className="font-mono">{c.customerId}</span>
              </p>
            </div>
          </div>
          <dl className="grid grid-cols-2 gap-3 text-sm">
            <div>
              <dt className="metric-label">Телефон</dt>
              <dd className="font-mono text-ink-800 mt-1">+1 (555) ***-2147</dd>
            </div>
            <div>
              <dt className="metric-label">Email</dt>
              <dd className="font-mono text-ink-800 mt-1">robert.j****@demo.com</dd>
            </div>
            <div>
              <dt className="metric-label">Адреса</dt>
              <dd className="text-ink-800 mt-1">Бориспіль, Україна</dd>
            </div>
            <div>
              <dt className="metric-label">Ризик-профіль</dt>
              <dd className="text-ink-800 mt-1">Середній (62/100)</dd>
            </div>
            <div>
              <dt className="metric-label">Поліси</dt>
              <dd className="text-ink-800 mt-1">1 активний</dd>
            </div>
          </dl>
        </section>

        <section className="card card-pad">
          <div className="flex items-center gap-4 mb-4">
            <div className="w-16 h-16 rounded-xl bg-ink-100 grid place-items-center text-xl">⌬</div>
            <div>
              <h3 className="text-lg font-semibold text-ink-900">{c.vehicle}</h3>
              <p className="text-xs text-ink-500 mt-0.5">
                Седан · <span className="font-mono">{c.vehicleVin}</span> · Застрахован.
              </p>
            </div>
          </div>
          <dl className="grid grid-cols-2 gap-3 text-sm">
            <div>
              <dt className="metric-label">Пробіг</dt>
              <dd className="font-mono text-ink-800 mt-1">47 200 км</dd>
            </div>
            <div>
              <dt className="metric-label">Колір</dt>
              <dd className="text-ink-800 mt-1">Сріблястий</dd>
            </div>
            <div>
              <dt className="metric-label">Реєстрація</dt>
              <dd className="text-ink-800 mt-1">2021</dd>
            </div>
            <div>
              <dt className="metric-label">Застрах. вартість</dt>
              <dd className="font-mono text-ink-800 mt-1">$24 800</dd>
            </div>
            <div>
              <dt className="metric-label">Категорія ризику</dt>
              <dd className="text-ink-800 mt-1">Низька</dd>
            </div>
          </dl>
        </section>
      </div>

      <div className="grid xl:grid-cols-3 gap-5">
        <section className="card card-pad">
          <div className="section-title mb-3">Попередні випадки</div>
          <ul className="space-y-3">
            {previousClaims.length > 0 ? (
              previousClaims.map((p) => (
                <li key={p.id} className="flex items-start justify-between gap-3 text-sm">
                  <div className="min-w-0">
                    <div className="font-mono font-semibold text-brand-700">{p.id}</div>
                    <div className="text-ink-700">{p.label}</div>
                    <div className="text-xs text-ink-500 mt-0.5">{p.date}</div>
                  </div>
                  <div className="text-right text-xs text-ink-600">{p.amount}</div>
                </li>
              ))
            ) : (
              <li className="text-sm text-ink-500">Попередніх випадків немає</li>
            )}
          </ul>
        </section>

        <section className="card card-pad">
          <div className="section-title mb-3">Історія комунікації</div>
          <ul className="space-y-3">
            {communicationHistory.map((row, idx) => (
              <li key={idx} className="flex items-center justify-between gap-3 text-sm">
                <div>
                  <div className="text-ink-700 font-medium">{row.channel}</div>
                  <div className="text-xs text-ink-500">{row.topic}</div>
                </div>
                <div className="text-xs font-mono text-ink-500">{row.when}</div>
              </li>
            ))}
          </ul>
        </section>

        <section className="card card-pad">
          <div className="section-title mb-3">Пов'язані поліси</div>
          <div className="rounded-lg border border-ink-100 p-3 bg-ink-50">
            <div className="font-semibold text-ink-900">Auto Comprehensive</div>
            <div className="text-xs text-ink-500 mt-0.5">
              <span className="font-mono">{c.policyId}</span> · до 31.12.2026
            </div>
            <button
              type="button"
              onClick={() => navigate('/claims/CLM-1006/policy')}
              className="btn-ghost mt-2 text-xs"
            >
              → Деталі
            </button>
          </div>
          <div className="mt-4">
            <div className="section-title mb-2">Документи клієнта</div>
            <p className="text-sm text-ink-700">
              12 документів · паспорт, посвідчення, реєстрація, заяви
            </p>
            <p className="text-xs text-ink-500 mt-1">Останнє оновлення: 19.05.2026</p>
          </div>
        </section>
      </div>

      <div className="card card-pad bg-gradient-to-r from-warn-500/5 to-ink-50 border-warn-200">
        <div className="flex flex-wrap items-center gap-3 justify-between">
          <div>
            <div className="metric-label text-warn-700">Privacy · Demo</div>
            <p className="text-sm text-ink-700 mt-1">
              Дані синтетичні для demo. Жодних реальних PII клієнта.
            </p>
          </div>
          <StatusPill tone="warn">PII МАСКОВАНІ</StatusPill>
        </div>
      </div>
    </div>
  );
}
