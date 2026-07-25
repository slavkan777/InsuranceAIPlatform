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
export const demo = { en };
