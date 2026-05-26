import { useNavigate } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '@/app/hooks';
import { MetricCard } from '@/components/ui/MetricCard';
import { StatusPill } from '@/components/ui/StatusPill';
import { SectionHeader } from '@/components/ui/SectionHeader';
import { claimRows } from '@/data/mock/claims';
import { claimsListMetrics } from '@/data/mock/dashboard';
import {
  setSearch,
  setSegment,
  setFilter,
  setSelected,
} from '@/features/claims/claimsSlice';
import { selectClaimsState } from '@/features/claims/claimsSelectors';
import clsx from '@/utils/clsx';

export default function ClaimsListPage() {
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const { search, segment, filters } = useAppSelector(selectClaimsState);

  function openClaim(id: string) {
    dispatch(setSelected(id));
    navigate(`/claims/${id}`);
  }

  return (
    <div className="flex flex-col gap-6">
      <SectionHeader
        title="Автострахові випадки"
        subtitle="53 активних · 8 з високим ризиком · 5 чекають людського рішення"
        actions={
          <>
            <button className="btn-secondary">Експорт CSV</button>
            <button className="btn-secondary">Імпорт документів</button>
            <button className="btn-primary">+ Новий випадок</button>
          </>
        }
      />

      <section className="card card-pad grid md:grid-cols-6 gap-3">
        <label className="flex flex-col gap-1 md:col-span-2">
          <span className="metric-label">Пошук</span>
          <input
            type="search"
            value={search}
            onChange={(e) => dispatch(setSearch(e.target.value))}
            placeholder="Toyota Camry, Роберт Джонсон..."
            className="rounded-lg border border-ink-200 bg-white px-3 py-2 text-sm focus-ring"
          />
        </label>
        {(
          [
            ['status', 'Статус', ['Усі', 'В роботі', 'Збір документів', 'Готова', 'Завершено']],
            ['risk', 'Ризик', ['Усі', 'Низький', 'Середній', 'Високий']],
            ['eventType', 'Тип події', ['Усі', 'ДТП', 'Паркування', 'Зіткнення', 'Пошкодження']],
            ['date', 'Дата', ['Сьогодні', '7 днів', '30 днів']],
            ['aiStatus', 'AI-статус', ['Усі', 'AI-перевірено', 'Обробляється']],
          ] as const
        ).map(([key, label, options]) => (
          <label key={key} className="flex flex-col gap-1">
            <span className="metric-label">{label}</span>
            <select
              value={filters[key]}
              onChange={(e) => dispatch(setFilter({ key, value: e.target.value }))}
              className="rounded-lg border border-ink-200 bg-white px-3 py-2 text-sm focus-ring"
            >
              {options.map((o) => (
                <option key={o} value={o}>
                  {o}
                </option>
              ))}
            </select>
          </label>
        ))}
      </section>

      <div className="grid xl:grid-cols-[1fr_320px] gap-5">
        <div className="flex flex-col gap-5 min-w-0">
          <section className="card overflow-hidden">
            <div className="px-5 py-4 flex flex-wrap items-center justify-between gap-3 border-b border-ink-100">
              <div>
                <h3 className="text-base font-semibold text-ink-900">Черга автострахових випадків</h3>
                <p className="text-xs text-ink-500 mt-0.5">Сортовано за SLA</p>
              </div>
              <div className="flex flex-wrap gap-1.5">
                {([
                  ['Усі', '53'],
                  ['ДТП', '32'],
                  ['Високий ризик', '7'],
                  ['Чекає AI', '4'],
                  ['Чекає рішення', '5'],
                ] as const).map(([label, count]) => (
                  <button
                    key={label}
                    onClick={() => dispatch(setSegment(label as never))}
                    className={clsx(
                      'px-2.5 py-1 rounded-md text-xs font-semibold transition-colors',
                      segment === label
                        ? 'bg-ink-900 text-white'
                        : 'bg-ink-100 text-ink-600 hover:bg-ink-200',
                    )}
                  >
                    {label} <span className="opacity-70">({count})</span>
                  </button>
                ))}
              </div>
            </div>
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead className="bg-ink-50/80">
                  <tr>
                    <th className="table-th">Номер</th>
                    <th className="table-th">Клієнт · Авто</th>
                    <th className="table-th">Тип</th>
                    <th className="table-th">Статус</th>
                    <th className="table-th">Док.</th>
                    <th className="table-th">AI-статус</th>
                    <th className="table-th">Ризик</th>
                    <th className="table-th">SLA</th>
                    <th className="table-th">Наступна дія</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-ink-100">
                  {claimRows.map((row) => (
                    <tr
                      key={row.id}
                      onClick={() => openClaim(row.id)}
                      className="cursor-pointer hover:bg-ink-50 transition-colors"
                    >
                      <td className="table-td font-mono font-semibold text-brand-700">{row.id}</td>
                      <td className="table-td">
                        <div className="font-medium text-ink-900">{row.customer}</div>
                        <div className="text-xs text-ink-500">{row.vehicle}</div>
                      </td>
                      <td className="table-td text-ink-600">{row.eventType}</td>
                      <td className="table-td">
                        <StatusPill
                          tone={
                            row.status === 'Високий ризик'
                              ? 'danger'
                              : row.status === 'Готова'
                                ? 'good'
                                : row.status === 'AI-обробка'
                                  ? 'info'
                                  : row.status === 'Збір документів'
                                    ? 'warn'
                                    : 'muted'
                          }
                        >
                          {row.status}
                        </StatusPill>
                      </td>
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
                            row.risk === 'Високий'
                              ? 'danger'
                              : row.risk === 'Середній'
                                ? 'warn'
                                : 'good'
                          }
                        >
                          {row.risk}
                        </StatusPill>
                      </td>
                      <td className="table-td">
                        <span
                          className={clsx(
                            'text-sm font-medium',
                            row.sla === 'Прострочено' ? 'text-danger-600' : 'text-ink-700',
                          )}
                        >
                          {row.sla}
                        </span>
                      </td>
                      <td className="table-td text-ink-600">{row.nextAction}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </section>
        </div>

        <aside className="flex flex-col gap-3">
          {claimsListMetrics.map((m) => (
            <MetricCard key={m.id} {...m} />
          ))}
        </aside>
      </div>
    </div>
  );
}
