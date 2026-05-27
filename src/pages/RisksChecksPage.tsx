import { useNavigate } from 'react-router-dom';
import { useAppSelector } from '@/app/hooks';
import { StatusPill } from '@/components/ui/StatusPill';
import { ProgressBar } from '@/components/ui/ProgressBar';
import { goldenClaim } from '@/data/mock/claims';
import { riskFactors as mockRiskFactors } from '@/data/mock/claim-1006';
import {
  selectClaimDetail,
  selectWorkspaceRisks,
} from '@/features/claims/claimWorkspaceSelectors';

export default function RisksChecksPage() {
  const navigate = useNavigate();

  // --- store selectors (with mock fallback) ---
  const claimDetailFromStore = useAppSelector(selectClaimDetail);
  const c = claimDetailFromStore ?? goldenClaim;

  const risksFromStore = useAppSelector(selectWorkspaceRisks);
  const riskFactors = risksFromStore?.factors ?? mockRiskFactors;
  const riskScore = risksFromStore?.score ?? c.riskScore;
  const threshold = risksFromStore?.threshold ?? 60;

  const deviationAbs = c.estimate - c.expectedBenchmark;
  const deviationPct = ((deviationAbs / c.expectedBenchmark) * 100).toFixed(0);

  return (
    <div className="flex flex-col gap-5">
      <section className="card card-pad flex flex-wrap items-center gap-x-6 gap-y-3 justify-between">
        <div>
          <h2 className="text-xl font-bold text-ink-900">Ризики та перевірки</h2>
          <p className="text-sm text-ink-500 mt-1">
            {c.id} · автоматичне погодження заблоковано · потрібна людська перевірка
          </p>
        </div>
        <StatusPill tone="danger">Високий ризик</StatusPill>
      </section>

      <div className="grid xl:grid-cols-3 gap-5">
        <section className="card card-pad xl:col-span-1 flex flex-col items-center text-center">
          <div className="metric-label">Ризиковий бал</div>
          <div className="relative w-40 h-40 mt-3">
            <svg viewBox="0 0 120 120" className="w-full h-full -rotate-90">
              <circle cx="60" cy="60" r="50" fill="none" stroke="#eef0f5" strokeWidth="10" />
              <circle
                cx="60"
                cy="60"
                r="50"
                fill="none"
                stroke="#e5484d"
                strokeWidth="10"
                strokeDasharray={`${(riskScore / 100) * 314.16} 314.16`}
                strokeLinecap="round"
              />
            </svg>
            <div className="absolute inset-0 flex flex-col items-center justify-center">
              <span className="text-4xl font-bold text-ink-900 leading-none font-mono">
                {riskScore}
              </span>
              <span className="text-xs text-ink-500 mt-1">/ 100</span>
            </div>
          </div>
          <p className="text-sm text-ink-600 mt-4">
            Поріг автопогодження: {threshold}. Перевищено на {riskScore - threshold}.
          </p>
        </section>

        <section className="card card-pad xl:col-span-2">
          <div className="section-title mb-3">Фактори ризику</div>
          <ul className="space-y-3">
            {riskFactors.map((f) => (
              <li key={f.id}>
                <div className="flex items-center justify-between text-sm mb-1">
                  <span className="text-ink-700">{f.label}</span>
                  <span className="font-mono font-semibold text-danger-600">+{f.contribution}</span>
                </div>
                <div className="h-1.5 w-full rounded-full bg-ink-100 overflow-hidden">
                  <div
                    className="h-full rounded-full bg-danger-500"
                    style={{ width: `${(f.contribution / 30) * 100}%` }}
                  />
                </div>
              </li>
            ))}
          </ul>
        </section>
      </div>

      <div className="grid xl:grid-cols-3 gap-5">
        <section className="card card-pad">
          <div className="flex items-center justify-between mb-3">
            <div className="section-title">Перевірка полісу</div>
            <StatusPill tone="good">Покриття активне</StatusPill>
          </div>
          <ul className="space-y-2 text-sm text-ink-700">
            <li>· ДТП дата у періоді</li>
            <li>· Франшиза $500</li>
            <li>· Ліміт $50 000</li>
            <li>· Виключень немає</li>
          </ul>
        </section>

        <section className="card card-pad">
          <div className="section-title mb-3">Бенчмарк вартості</div>
          <dl className="grid grid-cols-2 gap-3 text-sm">
            <div>
              <dt className="metric-label">Очікувано</dt>
              <dd className="text-xl font-bold text-ink-900 mt-1 font-mono">
                ${c.expectedBenchmark.toLocaleString('uk-UA')}
              </dd>
            </div>
            <div>
              <dt className="metric-label">Подано</dt>
              <dd className="text-xl font-bold text-ink-900 mt-1 font-mono">
                ${c.estimate.toLocaleString('uk-UA')}
              </dd>
            </div>
          </dl>
          <div className="mt-3">
            <ProgressBar
              value={(c.estimate / (c.expectedBenchmark * 1.5)) * 100}
              tone="warn"
              label={`Відхилення: +$${deviationAbs} (+${deviationPct}%)`}
            />
          </div>
        </section>

        <section className="card card-pad bg-gradient-to-br from-danger-500/5 to-white border-danger-200">
          <div className="metric-label text-danger-600">Обмеження автоматизації</div>
          <h4 className="text-base font-semibold text-ink-900 mt-1">
            Автоматичне погодження ЗАБЛОКОВАНО
          </h4>
          <div className="grid grid-cols-3 gap-2 mt-4 text-xs">
            <div className="rounded-lg border border-ink-100 bg-white p-2 text-center">
              <div className="text-[10px] text-ink-400 uppercase">Авто</div>
              <div className="font-bold text-danger-600 mt-1">НІ</div>
            </div>
            <div className="rounded-lg border border-ink-100 bg-white p-2 text-center">
              <div className="text-[10px] text-ink-400 uppercase">Людина</div>
              <div className="font-bold text-good-600 mt-1">ОБОВ'ЯЗК.</div>
            </div>
            <div className="rounded-lg border border-ink-100 bg-white p-2 text-center">
              <div className="text-[10px] text-ink-400 uppercase">Ескалація</div>
              <div className="font-bold text-warn-600 mt-1">РЕКОМ.</div>
            </div>
          </div>
        </section>
      </div>

      <div className="card card-pad flex flex-wrap gap-2 justify-end">
        <button onClick={() => navigate('/claims/CLM-1006/ai-evidence')} className="btn-secondary">
          Відкрити докази
        </button>
        <button onClick={() => navigate('/claims/CLM-1006/documents')} className="btn-secondary">
          Запросити дані
        </button>
        <button onClick={() => navigate('/claims/CLM-1006/approval')} className="btn-primary">
          Передати на погодження
        </button>
      </div>
    </div>
  );
}
