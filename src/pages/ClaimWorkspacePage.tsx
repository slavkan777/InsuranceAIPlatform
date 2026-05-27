import { useNavigate } from 'react-router-dom';
import { useAppSelector } from '@/app/hooks';
import { ClaimHeader } from '@/components/claim/ClaimHeader';
import { Timeline } from '@/components/claim/Timeline';
import { StatusPill } from '@/components/ui/StatusPill';
import { ProgressBar } from '@/components/ui/ProgressBar';
import { goldenClaim } from '@/data/mock/claims';
import {
  damagePhotos,
  evidenceTabs,
  keyRisks,
  stoInvoiceLines,
} from '@/data/mock/claim-1006';
import { selectClaimDetail } from '@/features/claims/claimWorkspaceSelectors';
import clsx from '@/utils/clsx';

export default function ClaimWorkspacePage() {
  const navigate = useNavigate();
  // Overview header/fields come from the saga-loaded claim detail (backend in
  // backend-mode); fall back to goldenClaim when null so mock mode stays identical
  // and there is no null-flash before the saga resolves.
  const claimDetail = useAppSelector(selectClaimDetail);
  const c = claimDetail ?? goldenClaim;

  return (
    <div className="flex flex-col gap-5">
      <ClaimHeader />

      <div className="grid xl:grid-cols-[1fr_360px] gap-5">
        <div className="flex flex-col gap-5">
          <section className="card card-pad">
            <div className="section-title mb-2">Опис події</div>
            <p className="text-ink-800">{c.description}</p>
            <div className="text-sm text-ink-500 mt-2">
              Місце: {c.location} · Час: 14:32 · Учасники: 2 · Постраждалі: відсутні
            </div>
            <div className="text-sm text-ink-500 mt-1">
              Поліцейський звіт: NoБРС-2026/05/441 · Інспектор: Іваненко О.М.
            </div>
          </section>

          <Timeline />

          <div className="grid md:grid-cols-2 gap-5">
            <section className="card card-pad">
              <div className="flex items-center justify-between mb-3">
                <div className="section-title">Перевірка полісу</div>
                <StatusPill tone="good">Покриття активне</StatusPill>
              </div>
              <div className="text-base font-semibold text-ink-900">Auto Comprehensive</div>
              <div className="text-sm text-ink-500 mt-1">
                Дійсний: 01.01.2026 — 31.12.2026 · ДТП дата у періоді
              </div>
              <dl className="grid grid-cols-2 gap-3 mt-4 text-sm">
                <div>
                  <dt className="metric-label">Франшиза</dt>
                  <dd className="font-semibold text-ink-900 mt-1 font-mono">${c.deductible}</dd>
                </div>
                <div>
                  <dt className="metric-label">Ліміт</dt>
                  <dd className="font-semibold text-ink-900 mt-1 font-mono">$50 000</dd>
                </div>
              </dl>
            </section>

            <section className="card card-pad">
              <div className="flex items-center justify-between mb-3">
                <div className="section-title">Комплектність документів</div>
                <StatusPill tone="warn">{c.documentsReceived} із {c.documentsTotal}</StatusPill>
              </div>
              <ProgressBar value={c.documentsReceived} max={c.documentsTotal} tone="warn" />
              <p className="text-sm text-ink-600 mt-3">
                Відсутнє: <span className="text-ink-900 font-medium">{c.missingDocument}</span>
              </p>
              <p className="text-xs text-danger-600 mt-1">
                AI оцінив комплектність як НЕДОСТАТНЮ для виплати
              </p>
            </section>
          </div>

          <section className="card card-pad">
            <div className="flex items-center justify-between mb-3">
              <div className="section-title">Фото пошкоджень</div>
              <span className="chip">3 з 4 фото підтверджено</span>
            </div>
            <div className="grid grid-cols-3 gap-3">
              {damagePhotos.map((p) => (
                <div
                  key={p.id}
                  className={clsx(
                    'rounded-xl border aspect-[4/3] flex flex-col items-center justify-center text-center px-3 py-3',
                    p.missing
                      ? 'border-dashed border-danger-300 bg-danger-500/5'
                      : 'border-ink-100 bg-ink-50',
                  )}
                >
                  <div
                    className={clsx(
                      'w-9 h-9 rounded-full grid place-items-center mb-2 text-base',
                      p.missing ? 'bg-danger-500 text-white' : 'bg-ink-200 text-ink-700',
                    )}
                  >
                    {p.missing ? '!' : '✓'}
                  </div>
                  <div className="text-sm font-semibold text-ink-900">{p.label}</div>
                  <div
                    className={clsx(
                      'text-[11px] mt-1',
                      p.missing ? 'text-danger-600 font-semibold' : 'text-ink-500',
                    )}
                  >
                    {p.missing ? 'Фото відсутнє · запросити' : `AI conf ${p.confidence}%`}
                  </div>
                </div>
              ))}
            </div>
          </section>

          <section className="card card-pad">
            <div className="flex flex-wrap items-baseline justify-between mb-3 gap-2">
              <div>
                <div className="section-title">Рахунок СТО</div>
                <p className="text-sm text-ink-500 mt-0.5">
                  СТО «Авто-Експерт» · виставлено 19.05.2026
                </p>
              </div>
              <StatusPill tone="warn">+38% від медіани</StatusPill>
            </div>
            <table className="w-full text-sm">
              <tbody className="divide-y divide-ink-100">
                {stoInvoiceLines.map((line) => (
                  <tr key={line.id}>
                    <td className="py-2 text-ink-700">{line.label}</td>
                    <td className="py-2 text-right font-mono font-semibold text-ink-900">
                      ${line.value.toLocaleString('uk-UA')}
                    </td>
                  </tr>
                ))}
                <tr className="bg-ink-50">
                  <td className="py-2.5 px-2 font-semibold text-ink-900">Сума</td>
                  <td className="py-2.5 px-2 text-right font-mono font-bold text-ink-900">
                    ${c.estimate.toLocaleString('uk-UA')}
                  </td>
                </tr>
              </tbody>
            </table>
          </section>
        </div>

        <aside className="flex flex-col gap-5">
          <section className="card card-pad">
            <div className="metric-label mb-1 text-ai-600">AI-РЕКОМЕНДАЦІЯ</div>
            <h4 className="text-base font-semibold text-ink-900">Запросити додаткове фото</h4>
            <p className="text-sm text-ink-600 mt-2 leading-snug">
              Запросити у клієнта фото пошкодження заднього бампера перед погодженням виплати.
            </p>
            <div className="mt-4">
              <ProgressBar value={c.confidence} tone="ai" label="Впевненість" />
            </div>
          </section>

          <section className="card card-pad">
            <div className="section-title mb-2">Ключові ризики</div>
            <ul className="space-y-2 text-sm">
              {keyRisks.map((r) => (
                <li key={r} className="flex gap-2 items-start">
                  <span className="mt-1.5 w-1.5 h-1.5 rounded-full bg-danger-500 shrink-0" />
                  <span className="text-ink-700 leading-snug">{r}</span>
                </li>
              ))}
              <li className="flex gap-2 items-start">
                <span className="mt-1.5 w-1.5 h-1.5 rounded-full bg-good-500 shrink-0" />
                <span className="text-ink-700 leading-snug">Покриття за полісом підтверджено</span>
              </li>
            </ul>
          </section>

          <section className="card card-pad">
            <div className="section-title mb-2">Докази</div>
            <div className="flex flex-wrap gap-1.5">
              {evidenceTabs.map((e) => (
                <span key={e} className="chip">
                  {e}
                </span>
              ))}
            </div>
          </section>

          <section className="card card-pad bg-gradient-to-br from-brand-50 to-white border-brand-200">
            <div className="metric-label text-brand-700">Наступна дія</div>
            <h4 className="text-base font-semibold text-ink-900 mt-1">Запросити фото пошкодження</h4>
            <p className="text-sm text-ink-600 mt-2 leading-snug">
              Кнопка нижче надсилає запит клієнту через SMS + email.
            </p>
            <button
              onClick={() => navigate('/claims/CLM-1006/documents')}
              className="btn-primary w-full mt-4"
            >
              Запросити дані
            </button>
          </section>
        </aside>
      </div>

      <div className="card card-pad flex flex-wrap gap-2 justify-end">
        <button onClick={() => navigate('/claims')} className="btn-ghost">
          Повернутись до списку
        </button>
        <button onClick={() => navigate('/claims/CLM-1006/ai-evidence')} className="btn-secondary">
          Передати на перевірку
        </button>
        <button onClick={() => navigate('/claims/CLM-1006/approval')} className="btn-primary">
          Підготувати рішення
        </button>
      </div>
    </div>
  );
}
