import { useNavigate } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '@/app/hooks';
import { SectionHeader } from '@/components/ui/SectionHeader';
import { demoSteps } from '@/data/mock/claim-1006';
import {
  setDemoStep,
  startDemo,
  stopDemo,
} from '@/features/demo/demoSlice';
import clsx from '@/utils/clsx';

export default function DemoScenarioPage() {
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const { active, currentStep } = useAppSelector((s) => s.demo);

  function goStep(step: number, route: string) {
    dispatch(setDemoStep(step));
    navigate(route);
  }

  return (
    <div className="flex flex-col gap-6">
      <SectionHeader
        title="Demo Scenario — Auto Insurance Claim AI Workbench"
        subtitle="7 кроків · ~6 хвилин · готово для портфоліо/interview"
        actions={
          <>
            <button
              onClick={() => dispatch(startDemo())}
              disabled={active}
              className="btn-primary"
            >
              {active ? `Грає крок ${currentStep}/7` : '▶ Приклад використання'}
            </button>
            <button
              onClick={() => dispatch(stopDemo())}
              disabled={!active}
              className="btn-secondary"
            >
              ■ Зупинити
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
                  ⏵ зараз
                </span>
              )}
              <div className="flex items-center gap-3 mb-3">
                <div className="w-9 h-9 rounded-full bg-brand-600 text-white grid place-items-center font-semibold">
                  {step.step}
                </div>
                <div>
                  <div className="text-[10px] uppercase tracking-wider text-ink-400">
                    Крок {step.step} · {step.pdfRef}
                  </div>
                  <div className="text-sm font-semibold text-ink-900">{step.title}</div>
                </div>
              </div>
              <p className="text-xs text-ink-600 leading-snug">{step.caption}</p>
              <div className="mt-3 text-xs font-medium text-brand-700">Перейти →</div>
            </button>
          );
        })}
      </section>

      <section className="card card-pad">
        <div className="section-title mb-4">Архітектура системи</div>
        <p className="text-sm text-ink-500 mb-4">Три шари — core + AI копілот + Azure</p>
        <div className="grid md:grid-cols-3 gap-4">
          {[
            {
              title: 'Core .NET',
              subtitle: 'Insurance Operations Platform',
              tone: 'brand',
              items: ['CQRS / MediatR', 'Clean Architecture', 'Polly + OpenTelemetry'],
            },
            {
              title: 'AI Document Intelligence',
              subtitle: 'RAG · scoring · evidence',
              tone: 'teal',
              items: ['Document Classifier', 'Field Extractor', 'Risk + Recommender'],
            },
            {
              title: 'Azure AI / Infrastructure',
              subtitle: 'OpenAI · App Insights · Storage',
              tone: 'warn',
              items: ['Azure OpenAI', 'Application Insights', 'Blob Storage + Search'],
            },
          ].map((layer) => (
            <div
              key={layer.title}
              className={clsx(
                'rounded-xl border p-4',
                layer.tone === 'brand'
                  ? 'bg-brand-50/60 border-brand-200'
                  : layer.tone === 'teal'
                    ? 'bg-teal-500/10 border-teal-500/20'
                    : 'bg-warn-500/10 border-warn-500/20',
              )}
            >
              <div className="text-sm font-semibold text-ink-900">{layer.title}</div>
              <p className="text-xs text-ink-600 mt-1">{layer.subtitle}</p>
              <ul className="mt-3 space-y-1 text-xs text-ink-700">
                {layer.items.map((i) => (
                  <li key={i}>· {i}</li>
                ))}
              </ul>
            </div>
          ))}
        </div>
      </section>

      <section className="card card-pad bg-gradient-to-br from-ink-900 to-ink-700 text-white">
        <div className="text-[11px] uppercase tracking-wider text-brand-300 font-semibold">
          Portfolio message
        </div>
        <p className="text-lg font-semibold mt-2 leading-snug">
          Детермінована система обробки claims з AI evidence,
          <br />
          human review та audit / cost governance.
        </p>
        <p className="text-xs text-ink-300 mt-3 font-mono">
          Stack: .NET 9 · ASP.NET Core · Azure OpenAI · Polly · OpenTelemetry · React+TS (planned)
        </p>
      </section>

      <section className="card card-pad">
        <div className="section-title mb-2">Plan: stack note</div>
        <ul className="text-sm text-ink-700 space-y-1">
          <li>· Цей walking skeleton — лише frontend з mock-даними.</li>
          <li>· Наступна фаза — .NET backend (CQRS + Polly + OpenTelemetry).</li>
          <li>· Після того — Azure deployment + реальні AI provider keys.</li>
        </ul>
      </section>
    </div>
  );
}
