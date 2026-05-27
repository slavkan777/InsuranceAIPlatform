import { useAppDispatch, useAppSelector } from '@/app/hooks';
import { StatusPill } from '@/components/ui/StatusPill';
import { ProgressBar } from '@/components/ui/ProgressBar';
import { goldenClaim } from '@/data/mock/claims';
import { damagePhotos as mockDamagePhotos, documentsChecklist as mockDocumentsChecklist } from '@/data/mock/claim-1006';
import {
  requestMissingPhoto,
  selectDocument,
  toggleReviewed,
} from '@/features/documents/documentsSlice';
import {
  selectWorkspaceDocuments,
  selectWorkspacePhotos,
  selectClaimDetail,
} from '@/features/claims/claimWorkspaceSelectors';
import clsx from '@/utils/clsx';

const statusIcon = {
  ok: { ch: '✓', color: 'bg-good-500 text-white' },
  warn: { ch: '!', color: 'bg-warn-500 text-white' },
  missing: { ch: '×', color: 'bg-danger-500 text-white' },
} as const;

export default function DocumentsPhotosPage() {
  const dispatch = useAppDispatch();

  // --- store selectors (with mock fallback) ---
  const claimDetailFromStore = useAppSelector(selectClaimDetail);
  const c = claimDetailFromStore ?? goldenClaim;

  const documentsFromStore = useAppSelector(selectWorkspaceDocuments);
  const documentsChecklist = documentsFromStore ?? mockDocumentsChecklist;

  const photosFromStore = useAppSelector(selectWorkspacePhotos);
  const damagePhotos = photosFromStore ?? mockDamagePhotos;

  const { selectedDocumentId, reviewedIds, reviewStatus, reviewMessage } = useAppSelector(
    (s) => s.documents,
  );

  return (
    <div className="flex flex-col gap-5">
      <div className="card card-pad bg-gradient-to-r from-danger-500/5 to-warn-500/5 border-l-4 border-l-danger-500">
        <div className="flex flex-wrap items-start gap-3 justify-between">
          <div>
            <div className="metric-label text-danger-600">Відсутній документ</div>
            <h3 className="text-base font-semibold text-ink-900 mt-1">
              Додаткове фото пошкодження заднього бампера — відсутнє
            </h3>
            <p className="text-sm text-ink-600 mt-2">
              AI блокує автоматичне погодження до отримання документа.
            </p>
            {reviewMessage && (
              <p
                className={clsx(
                  'text-sm mt-2 font-medium',
                  reviewStatus === 'sent' ? 'text-good-600' : 'text-danger-600',
                )}
              >
                {reviewMessage}
              </p>
            )}
          </div>
          <button
            onClick={() => dispatch(requestMissingPhoto())}
            disabled={reviewStatus === 'requesting'}
            className="btn-primary"
          >
            {reviewStatus === 'requesting' ? 'Надсилаємо…' : 'Запросити у клієнта'}
          </button>
        </div>
      </div>

      <div className="grid xl:grid-cols-[1fr_360px] gap-5">
        <div className="flex flex-col gap-5">
          <section className="card card-pad">
            <div className="flex items-center justify-between mb-3">
              <div>
                <div className="section-title">Фото пошкоджень</div>
                <p className="text-sm text-ink-500 mt-0.5">2 з 3 фото підтверджено</p>
              </div>
              <StatusPill tone="warn">потрібен задній бампер</StatusPill>
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
                    {p.missing ? 'Запросити у клієнта' : `AI conf ${p.confidence}%`}
                  </div>
                </div>
              ))}
            </div>
          </section>

          <section className="card card-pad">
            <div className="flex items-center justify-between mb-3">
              <div className="section-title">Контрольний список документів</div>
              <span className="chip">
                {c.documentsReceived}/{c.documentsTotal}
              </span>
            </div>
            <ul className="divide-y divide-ink-100">
              {documentsChecklist.map((d) => {
                const reviewed = !!reviewedIds[d.id];
                const icon = statusIcon[d.status];
                return (
                  <li
                    key={d.id}
                    onClick={() => dispatch(selectDocument(d.id))}
                    className={clsx(
                      'flex items-center justify-between gap-3 py-3 px-2 rounded-lg cursor-pointer transition-colors',
                      selectedDocumentId === d.id ? 'bg-brand-50' : 'hover:bg-ink-50',
                    )}
                  >
                    <div className="flex items-center gap-3 min-w-0">
                      <span
                        className={clsx(
                          'w-7 h-7 rounded-full grid place-items-center text-xs font-bold shrink-0',
                          icon.color,
                        )}
                      >
                        {icon.ch}
                      </span>
                      <div className="min-w-0">
                        <div className="font-medium text-ink-900 truncate">{d.label}</div>
                        <div className="text-xs text-ink-500">{d.detail}</div>
                      </div>
                    </div>
                    <label
                      onClick={(e) => {
                        e.stopPropagation();
                        dispatch(toggleReviewed(d.id));
                      }}
                      className="inline-flex items-center gap-2 text-xs text-ink-500 cursor-pointer"
                    >
                      <input
                        type="checkbox"
                        checked={reviewed}
                        readOnly
                        className="rounded border-ink-300 accent-brand-600"
                      />
                      {reviewed ? 'Переглянуто' : 'До перевірки'}
                    </label>
                  </li>
                );
              })}
            </ul>
          </section>
        </div>

        <aside className="flex flex-col gap-5">
          <section className="card card-pad">
            <div className="metric-label mb-2">Прев'ю · Поліцейський звіт</div>
            <div className="rounded-xl bg-ink-950 text-ink-200 p-4 text-xs font-mono leading-relaxed">
              <div className="text-brand-300">NoБРС-2026/05/441</div>
              <div>Дата: 18.05.2026 · 14:32</div>
              <div>Локація: Бориспіль, Київська 24</div>
              <div>Інспектор: Іваненко О.М.</div>
              <div className="mt-2 text-ink-300">
                Учасники: 2 · Постраждалі: 0 · Винуватець: Сторона Б
              </div>
            </div>
            <div className="mt-3">
              <div className="section-title mb-2">Витягнуто</div>
              <ul className="text-sm text-ink-700 space-y-1">
                <li>· Дата ДТП</li>
                <li>· Локація</li>
                <li>· Винуватець</li>
                <li>· Інспектор</li>
              </ul>
            </div>
            <ProgressBar value={95} tone="good" label="Впевненість" />
          </section>

          <div className="grid gap-2">
            <button
              onClick={() => dispatch(requestMissingPhoto())}
              disabled={reviewStatus === 'requesting'}
              className="btn-primary"
            >
              Запросити фото
            </button>
            <button className="btn-secondary">Переглянути оригінал</button>
            <button className="btn-secondary">Підтвердити документ</button>
          </div>
        </aside>
      </div>
    </div>
  );
}
