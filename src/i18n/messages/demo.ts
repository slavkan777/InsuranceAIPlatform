// Guided walkthrough page. Replaces the former portfolio/architecture/"walking
// skeleton" copy with product-positioned capability and environment messaging.
const en = {
  title: 'Guided product walkthrough',
  subtitle: 'A 7-step tour of the claims workflow · ~6 minutes',
  playingStep: 'Playing step',
  startWalkthrough: '▶ Start walkthrough',
  stopWalkthrough: '■ Stop',
  nowPlaying: '⏵ now',
  stepLabel: 'Step',
  openStep: 'Open →',
  capabilitiesTitle: 'Platform capabilities',
  capabilitiesSubtitle: 'How the claims workbench supports your team',
  cap1Title: 'Claim review workspace',
  cap1Body:
    'Organize incoming claims, track status, and focus reviewers on the cases that need attention first.',
  cap2Title: 'AI evidence assistance',
  cap2Body:
    'Classify documents, extract key fields, and surface risk signals before human review.',
  cap3Title: 'Audit & governance',
  cap3Body:
    'Keep decisions explainable with evidence-first outputs, human review checkpoints, and a full audit trail.',
  cap4Title: 'Cloud operations',
  cap4Body:
    'Runs as a live cloud application — a React frontend and a .NET API with health checks and cost-aware scaling.',
  valueTitle: 'What this platform does',
  valueBody:
    'A deterministic claims-processing system with AI evidence, human review, and audit / cost governance.',
  techNote: '.NET 9 API · React + TypeScript · live cloud deployment',
  environmentTitle: 'Demo environment status',
  environmentBullets: [
    'Seeded claim data and a mock AI provider demonstrate the workflow safely.',
    'Live API integration is enabled (health-checked .NET API).',
    'Persistent SQL storage and real AI provider keys are intentionally disabled in this demo.',
  ],
};
type T = typeof en;
const uk: T = {
  title: 'Огляд можливостей платформи',
  subtitle: '7 кроків · ~6 хвилин — тур по процесу обробки випадків',
  playingStep: 'Крок',
  startWalkthrough: '▶ Запустити огляд',
  stopWalkthrough: '■ Зупинити',
  nowPlaying: '⏵ зараз',
  stepLabel: 'Крок',
  openStep: 'Перейти →',
  capabilitiesTitle: 'Можливості платформи',
  capabilitiesSubtitle: 'Як робоче місце допомагає вашій команді',
  cap1Title: 'Робоче місце перевірки випадків',
  cap1Body:
    'Організуйте вхідні випадки, відстежуйте статуси та фокусуйте ревʼюерів на справах, які потребують уваги першими.',
  cap2Title: 'AI-допомога з доказами',
  cap2Body:
    'Класифікуйте документи, витягуйте ключові поля та підсвічуйте ризикові сигнали до людської перевірки.',
  cap3Title: 'Аудит і governance',
  cap3Body:
    'Зберігайте рішення пояснюваними завдяки evidence-first виходам, контрольним точкам людської перевірки та повному audit trail.',
  cap4Title: 'Cloud operations',
  cap4Body:
    'Працює як живий cloud-додаток — React frontend і .NET API з health-перевірками та економним масштабуванням.',
  valueTitle: 'Що робить ця платформа',
  valueBody:
    'Детермінована система обробки випадків з AI-доказами, людською перевіркою та audit / cost governance.',
  techNote: '.NET 9 API · React + TypeScript · живий cloud-деплой',
  environmentTitle: 'Статус демо-середовища',
  environmentBullets: [
    'Seeded дані випадків і mock AI-провайдер безпечно демонструють процес.',
    'Жива інтеграція API увімкнена (health-checked .NET API).',
    'Persistent SQL storage і реальні AI provider keys навмисно вимкнені в цьому демо.',
  ],
};
export const demo = { en, uk };
