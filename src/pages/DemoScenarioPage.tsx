import { useNavigate } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '@/app/hooks';
import { SectionHeader } from '@/components/ui/SectionHeader';
import { Icon, type IconName } from '@/components/ui/Icon';
import { demoSteps as mockDemoSteps } from '@/data/mock/claim-1006';
import {
  setDemoStep,
  startDemo,
  stopDemo,
} from '@/features/demo/demoSlice';
import { selectDemoScenario } from '@/features/demo/demoSelectors';
import { useI18n } from '@/i18n/useI18n';
import clsx from '@/utils/clsx';

export default function DemoScenarioPage() {
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const { t } = useI18n();
  const { active, currentStep } = useAppSelector((s) => s.demo);

  // --- store selector (with mock fallback) ---
  const scenarioFromStore = useAppSelector(selectDemoScenario);
  const demoSteps = scenarioFromStore ?? mockDemoSteps;

  function goStep(step: number, route: string) {
    dispatch(setDemoStep(step));
    navigate(route);
  }

  const capabilities: { icon: IconName; tone: 'brand' | 'teal' | 'warn'; title: string; body: string }[] = [
    { icon: 'layers', tone: 'brand', title: t.demo.cap1Title, body: t.demo.cap1Body },
    { icon: 'cpu', tone: 'teal', title: t.demo.cap2Title, body: t.demo.cap2Body },
    { icon: 'receipt', tone: 'warn', title: t.demo.cap3Title, body: t.demo.cap3Body },
    { icon: 'grid', tone: 'brand', title: t.demo.cap4Title, body: t.demo.cap4Body },
  ];

  return (
    <div className="flex flex-col gap-6">
      <SectionHeader
        title={t.demo.title}
        subtitle={t.demo.subtitle}
        actions={
          <>
            <button
              onClick={() => dispatch(startDemo())}
              disabled={active}
              className="btn-primary"
            >
              {active ? `${t.demo.playingStep} ${currentStep}/7` : t.demo.startWalkthrough}
            </button>
            <button
              onClick={() => dispatch(stopDemo())}
              disabled={!active}
              className="btn-secondary"
            >
              {t.demo.stopWalkthrough}
            </button>
          </>
        }
      />

      <section className="grid md:grid-cols-2 xl:grid-cols-4 gap-3">
        {demoSteps.map((step) => {
          const isActive = active && currentStep === step.step;
          return (
            <button
              key={step.step}
              onClick={() => goStep(step.step, step.route)}
              className={clsx(
                'card card-pad text-left transition-all relative overflow-hidden',
                isActive && 'ring-2 ring-brand-500 bg-brand-50',
              )}
            >
              {isActive && (
                <span className="absolute top-2 right-3 text-[10px] font-bold text-brand-700 uppercase">
                  {t.demo.nowPlaying}
                </span>
              )}
              <div className="flex items-center gap-3 mb-3">
                <div className="w-9 h-9 rounded-full bg-brand-600 text-white grid place-items-center font-semibold">
                  {step.step}
                </div>
                <div>
                  <div className="text-[10px] uppercase tracking-wider text-ink-400">
                    {t.demo.stepLabel} {step.step} · {step.pdfRef}
                  </div>
                  <div className="text-sm font-semibold text-ink-900">{step.title}</div>
                </div>
              </div>
              <p className="text-xs text-ink-600 leading-snug">{step.caption}</p>
              <div className="mt-3 text-xs font-medium text-brand-700">{t.demo.openStep}</div>
            </button>
          );
        })}
      </section>

      <section className="card card-pad">
        <div className="section-title mb-1">{t.demo.capabilitiesTitle}</div>
        <p className="text-sm text-ink-500 mb-4">{t.demo.capabilitiesSubtitle}</p>
        <div className="grid md:grid-cols-2 xl:grid-cols-4 gap-4">
          {capabilities.map((cap) => (
            <div
              key={cap.title}
              className={clsx(
                'rounded-xl border p-4',
                cap.tone === 'brand'
                  ? 'bg-brand-50/60 border-brand-200'
                  : cap.tone === 'teal'
                    ? 'bg-teal-500/10 border-teal-500/20'
                    : 'bg-warn-500/10 border-warn-500/20',
              )}
            >
              <span
                className={clsx(
                  'inline-flex w-9 h-9 rounded-lg items-center justify-center mb-3',
                  cap.tone === 'brand'
                    ? 'bg-brand-100 text-brand-700'
                    : cap.tone === 'teal'
                      ? 'bg-teal-500/15 text-teal-700'
                      : 'bg-warn-500/15 text-warn-600',
                )}
              >
                <Icon name={cap.icon} size={18} />
              </span>
              <div className="text-sm font-semibold text-ink-900">{cap.title}</div>
              <p className="text-xs text-ink-600 mt-1.5 leading-snug">{cap.body}</p>
            </div>
          ))}
        </div>
      </section>

      <section className="card card-pad bg-gradient-to-br from-ink-900 to-ink-700 text-white">
        <div className="text-[11px] uppercase tracking-wider text-brand-300 font-semibold">
          {t.demo.valueTitle}
        </div>
        <p className="text-lg font-semibold mt-2 leading-snug">{t.demo.valueBody}</p>
        <p className="text-xs text-ink-300 mt-3 font-mono">{t.demo.techNote}</p>
      </section>

      <section className="card card-pad">
        <div className="section-title mb-2">{t.demo.environmentTitle}</div>
        <ul className="text-sm text-ink-700 space-y-1">
          {t.demo.environmentBullets.map((line) => (
            <li key={line}>· {line}</li>
          ))}
        </ul>
      </section>
    </div>
  );
}
