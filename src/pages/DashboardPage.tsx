import { useNavigate } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '@/app/hooks';
import { MetricCard } from '@/components/ui/MetricCard';
import { StatusPill } from '@/components/ui/StatusPill';
import { ProgressBar } from '@/components/ui/ProgressBar';
import { SectionHeader } from '@/components/ui/SectionHeader';
import { Icon } from '@/components/ui/Icon';
import { DeferredActionButton } from '@/components/ui/DeferredActionButton';
import { DonutChart } from '@/components/charts/DonutChart';
import { BarList } from '@/components/charts/BarList';
import { LineChart } from '@/components/charts/LineChart';
import {
  overviewMetrics,
  lifecyclePhases,
  auditToday,
  recentEvents,
  caseTypeBreakdown,
  confidenceDistribution,
  processingTrend,
} from '@/data/mock/dashboard';
import { claimRows, goldenClaim } from '@/data/mock/claims';
import { keyFindings } from '@/data/mock/claim-1006';
import { setSelected } from '@/features/claims/claimsSlice';
import { selectSelectedClaimId, selectClaimsQueue, selectClaimsSummary } from '@/features/claims/claimsSelectors';
import { selectClaimDetail } from '@/features/claims/claimWorkspaceSelectors';
import clsx from '@/utils/clsx';

const eventDot: Record<string, string> = {
  ai: 'bg-ai-500',
  danger: 'bg-danger-500',
  good: 'bg-good-500',
  info: 'bg-brand-500',
};

export default function DashboardPage() {
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const selectedId = useAppSelector(selectSelectedClaimId);

  // --- store selectors (with mock fallback) ---
  const queueFromStore = useAppSelector(selectClaimsQueue);
  const claimRows_ = queueFromStore.length > 0 ? queueFromStore : claimRows;

  const summaryFromStore = useAppSelector(selectClaimsSummary);
  // Build overviewMetrics from summary when available; fall back to static mock
  const resolvedMetrics = summaryFromStore
    ? [
        { id: 'new', label: 'НОВІ ДТП', value: String(summaryFromStore.totalActive), delta: `${summaryFromStore.aiAnalysisRunning} AI runs`, tone: 'info' as const, icon: 'car' as const },
        { id: 'wait-doc', label: 'ОЧІКУЮТЬ РІШЕННЯ', value: String(summaryFromStore.pendingReview), delta: 'на розгляді', tone: 'warn' as const, icon: 'file' as const },
        { id: 'ai-today', label: 'AI-ОБРОБЛЕНО СЬОГОДНІ', value: String(summaryFromStore.processedToday), delta: `+${summaryFromStore.aiAnalysisRunning} зараз`, tone: 'ai' as const, icon: 'cpu' as const },
        { id: 'high-risk', label: 'ВИСОКИЙ РИЗИК', value: String(summaryFromStore.highRisk), delta: 'поточні', tone: 'danger' as const, icon: 'shield' as const },
        { id: 'avg-time', label: 'СЕРЕДНІЙ ЧАС SLA', value: `${summaryFromStore.avgSlaRemainingHours} год`, delta: 'залишилось', tone: 'good' as const, icon: 'clock' as const },
      ]
    : overviewMetrics;

  const claimDetailFromStore = useAppSelector(selectClaimDetail);
  const c = claimDetailFromStore ?? goldenClaim;

  function openClaim(id: string) {
    dispatch(setSelected(id));
    navigate(`/claims/${id}`);
  }

  return (
    <div className="flex flex-col gap-6">
      <SectionHeader
        title="Огляд автострахових випадків"
        subtitle="Операційна панель · Станом на 24 травня 2026, 22:48"
        actions={
          <>
            <DeferredActionButton className="btn-secondary" hint="Перемикач періоду — read-only demo">
              Сьогодні
            </DeferredActionButton>
            <DeferredActionButton className="btn-ghost" hint="Перемикач періоду — read-only demo">
              7 днів
            </DeferredActionButton>
            <DeferredActionButton
              className="btn-secondary"
              hint="Експорт — доступний після backend-гейту"
              badge="demo"
            >
              Експорт
            </DeferredActionButton>
          </>
        }
      />

      <div className="grid grid-cols-2 md:grid-cols-3 xl:grid-cols-5 gap-4">
        {resolvedMetrics.map((m) => (
          <MetricCard key={m.id} {...m} />
        ))}
      </div>

      <section className="card card-pad">
        <div className="flex flex-wrap items-end justify-between mb-4 gap-2">
          <div>
            <div className="section-title mb-1">Життєвий цикл автострахового випадку</div>
            <div className="text-sm text-ink-500">Розподіл активних випадків за фазами</div>
          </div>
          <span className="chip">{summaryFromStore ? `${summaryFromStore.totalActive} активних` : '53 активних'}</span>
        </div>
        <div className="flex items-center gap-1 overflow-x-auto pb-1">
          {lifecyclePhases.map((p, i) => {
            const active = p.id === 'ai';
            return (
              <div key={p.id} className="flex items-center gap-1 shrink-0">
                <div
                  className={clsx(
                    'min-w-[124px] rounded-xl border px-4 py-3 flex flex-col items-center text-center',
                    active ? 'bg-ai-50 border-ai-200' : 'bg-ink-50 border-ink-100',
                  )}
                >
                  <span
                    className={clsx(
                      'w-8 h-8 rounded-lg grid place-items-center mb-2',
                      active ? 'bg-ai-500/15 text-ai-600' : 'bg-white text-ink-400 border border-ink-100',
                    )}
                  >
                    <Icon name={p.icon} size={17} />
                  </span>
                  <div
                    className={clsx(
                      'text-2xl font-bold leading-none tabular-nums',
                      active ? 'text-ai-700' : 'text-ink-900',
                    )}
                  >
                    {p.count}
                  </div>
                  <div className="text-[11px] text-ink-500 mt-1.5 leading-tight">{p.label}</div>
                </div>
                {i < lifecyclePhases.length - 1 && (
                  <span className="text-ink-300 px-0.5">
                    <Icon name="arrowRight" size={16} />
                  </span>
                )}
              </div>
            );
          })}
        </div>
      </section>

      <div className="grid xl:grid-cols-3 gap-5">
        <section className="card xl:col-span-2 overflow-hidden self-start">
          <div className="px-5 py-4 flex flex-wrap items-center justify-between gap-2 border-b border-ink-100">
            <div>
              <h3 className="text-base font-semibold text-ink-900">Черга автострахових випадків</h3>
              <p className="text-xs text-ink-500 mt-0.5">{summaryFromStore ? `${summaryFromStore.totalActive} активних` : '53 активних'} · оновлено щохвилини</p>
            </div>
            <DeferredActionButton
              className="btn-primary"
              hint="Створення кейсу — потрібен backend write-гейт"
              badge="demo"
            >
              + Створити випадок
            </DeferredActionButton>
          </div>
          <div className="flex flex-wrap gap-1.5 px-5 py-3 border-b border-ink-100 text-xs text-ink-600">
            {['Усі', 'ДТП', 'Високий ризик', 'Чекає AI', 'Чекає рішення'].map((seg, idx) => (
              <button
                key={seg}
                type="button"
                disabled
                aria-disabled="true"
                title="Фільтри активні у розділі «Автострахові випадки»"
                className={clsx(
                  'px-2.5 py-1 rounded-md font-medium cursor-not-allowed',
                  idx === 0 ? 'bg-ink-900 text-white' : 'bg-ink-100 text-ink-500',
                )}
              >
                {seg}
              </button>
            ))}
          </div>
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead className="bg-ink-50/80">
                <tr>
                  <th className="table-th">Номер</th>
                  <th className="table-th">Клієнт · Авто</th>
                  <th className="table-th">Тип події</th>
                  <th className="table-th">Документи</th>
                  <th className="table-th">AI-статус</th>
                  <th className="table-th">Ризик</th>
                  <th className="table-th">Наступна дія</th>
                  <th className="table-th">Оновлено</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-ink-100">
                {claimRows_.slice(0, 5).map((row) => (
                  <tr
                    key={row.id}
                    role="button"
                    onClick={() => openClaim(row.id)}
                    className={clsx(
                      'cursor-pointer transition-colors',
                      row.id === selectedId
                        ? 'bg-brand-50 shadow-[inset_3px_0_0_0_#2563eb]'
                        : 'hover:bg-ink-50',
                    )}
                  >
                    <td className="table-td font-mono font-semibold text-brand-700">{row.id}</td>
                    <td className="table-td">
                      <div className="font-medium text-ink-900">{row.customer}</div>
                      <div className="text-xs text-ink-500">{row.vehicle}</div>
                    </td>
                    <td className="table-td text-ink-600">{row.eventType}</td>
                    <td className="table-td">
                      <span className="chip">{row.documentsCount}</span>
                    </td>
                    <td className="table-td">
                      <StatusPill
                        tone={
                          row.aiStatus === 'AI-перевірено'
                            ? 'good'
                            : row.aiStatus === 'Потрібна перевірка'
                              ? 'warn'
                              : row.aiStatus === 'Обробляється'
                                ? 'info'
                                : 'muted'
                        }
                      >
                        {row.aiStatus}
                      </StatusPill>
                    </td>
                    <td className="table-td">
                      <StatusPill
                        tone={
                          row.risk === 'Високий' ? 'danger' : row.risk === 'Середній' ? 'warn' : 'good'
                        }
                      >
                        {row.risk}
                      </StatusPill>
                    </td>
                    <td className="table-td text-ink-600">{row.nextAction}</td>
                    <td className="table-td text-xs text-ink-500">{row.updated}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <div className="px-5 py-3 border-t border-ink-100">
            <button
              onClick={() => navigate('/claims')}
              className="text-sm font-semibold text-brand-700 hover:text-brand-800 inline-flex items-center gap-1"
            >
              Переглянути всі випадки <Icon name="arrowRight" size={14} />
            </button>
          </div>
        </section>

        <div className="flex flex-col gap-5">
          <section className="card card-pad border-t-[3px] border-t-ai-500">
            <div className="flex items-center gap-2 mb-4">
              <span className="w-7 h-7 rounded-lg bg-ai-500/10 text-ai-600 grid place-items-center">
                <Icon name="cpu" size={16} />
              </span>
              <h3 className="text-sm font-semibold text-ink-900">
                AI-рекомендація для {c.id}
              </h3>
            </div>
            <div className="flex items-end justify-between gap-3 mb-2">
              <div>
                <div className="metric-label">Ймовірна виплата</div>
                <div className="text-2xl font-bold text-ink-900 font-mono mt-0.5">
                  ${c.recommendedPayout.toLocaleString('uk-UA')}
                </div>
              </div>
              <div className="text-right">
                <div className="metric-label">Впевненість</div>
                <div className="text-2xl font-bold text-ai-700 mt-0.5">{c.confidence}%</div>
              </div>
            </div>
            <ProgressBar value={c.confidence} tone="ai" />
            <div className="mt-3 flex items-center gap-2">
              <span className="pill-ai">Рекомендація</span>
              <span className="text-xs text-ink-500">людська перевірка обов'язкова</span>
            </div>
            <p className="text-sm text-ink-700 mt-3 leading-snug">
              Запросити додаткове фото пошкодження заднього бампера перед погодженням виплати.
            </p>
            <div className="section-title mt-4 mb-2">Ключові фактори</div>
            <ul className="space-y-1.5 text-sm">
              {keyFindings.slice(0, 3).map((f, idx) => (
                <li key={idx} className="flex items-start gap-2">
                  <span
                    className={clsx(
                      'mt-1.5 w-1.5 h-1.5 rounded-full shrink-0',
                      f.tone === 'danger'
                        ? 'bg-danger-500'
                        : f.tone === 'warn'
                          ? 'bg-warn-500'
                          : 'bg-good-500',
                    )}
                  />
                  <span className="text-ink-700 leading-snug">{f.text}</span>
                </li>
              ))}
            </ul>
            <button
              onClick={() => navigate('/claims/CLM-1006/ai-evidence')}
              className="mt-4 w-full inline-flex items-center justify-center gap-2 px-3.5 py-2 rounded-lg border border-ai-200 text-ai-700 hover:bg-ai-50 text-sm font-semibold transition-colors"
            >
              Переглянути AI-аналіз <Icon name="arrowRight" size={15} />
            </button>
          </section>

          <section className="card card-pad border-t-[3px] border-t-ink-600">
            <div className="section-title mb-3">Аудит і витрати (сьогодні)</div>
            <dl className="space-y-2.5">
              {auditToday.map((r) => (
                <div key={r.id} className="flex items-center justify-between text-sm">
                  <dt className="text-ink-600">{r.label}</dt>
                  <dd className="flex items-center gap-2">
                    <span className="font-mono font-semibold text-ink-900">{r.value}</span>
                    <span className="text-xs font-medium text-good-600">{r.delta}</span>
                  </dd>
                </div>
              ))}
            </dl>
            <button
              onClick={() => navigate('/claims/CLM-1006/audit')}
              className="mt-3 text-sm font-semibold text-brand-700 hover:text-brand-800 inline-flex items-center gap-1"
            >
              Переглянути деталі <Icon name="arrowRight" size={14} />
            </button>
          </section>

          <section className="card card-pad">
            <div className="section-title mb-3">Останні події</div>
            <ol className="space-y-3">
              {recentEvents.map((e) => (
                <li key={e.id} className="flex items-start gap-3 text-sm">
                  <span className="font-mono text-xs text-ink-400 w-10 shrink-0 mt-0.5">{e.time}</span>
                  <span className={clsx('mt-1.5 w-1.5 h-1.5 rounded-full shrink-0', eventDot[e.tone])} />
                  <span className="text-ink-700 leading-snug">{e.text}</span>
                </li>
              ))}
            </ol>
            <button
              onClick={() => navigate('/claims/CLM-1006/audit')}
              className="mt-3 text-sm font-semibold text-brand-700 hover:text-brand-800 inline-flex items-center gap-1"
            >
              Переглянути журнал аудиту <Icon name="arrowRight" size={14} />
            </button>
          </section>
        </div>
      </div>

      <div className="grid lg:grid-cols-3 gap-5">
        <section className="card card-pad">
          <div className="section-title mb-1">Випадки за типом події</div>
          <div className="text-xs text-ink-400 mb-4">за 7 днів</div>
          <div className="flex items-center gap-5">
            <DonutChart data={caseTypeBreakdown} />
            <ul className="space-y-2 text-sm flex-1">
              {caseTypeBreakdown.map((d) => (
                <li key={d.label} className="flex items-center justify-between gap-2">
                  <span className="flex items-center gap-2">
                    <span className="w-2.5 h-2.5 rounded-sm" style={{ backgroundColor: d.color }} />
                    <span className="text-ink-700">{d.label}</span>
                  </span>
                  <span className="text-ink-500 tabular-nums">
                    {d.value} ({d.pct})
                  </span>
                </li>
              ))}
            </ul>
          </div>
        </section>

        <section className="card card-pad">
          <div className="section-title mb-1">AI-впевненість (розподіл)</div>
          <div className="text-xs text-ink-400 mb-4">сьогодні · {c.id} = 78%</div>
          <BarList data={confidenceDistribution} />
        </section>

        <section className="card card-pad">
          <div className="section-title mb-1">Тренд обробки</div>
          <div className="text-xs text-ink-400 mb-3">за 7 днів</div>
          <LineChart labels={processingTrend.labels} series={processingTrend.series} />
          <div className="flex items-center gap-4 mt-2 text-xs">
            {processingTrend.series.map((s) => (
              <span key={s.name} className="flex items-center gap-1.5">
                <span className="w-2.5 h-2.5 rounded-full" style={{ backgroundColor: s.color }} />
                <span className="text-ink-600">{s.name}</span>
              </span>
            ))}
          </div>
        </section>
      </div>
    </div>
  );
}
