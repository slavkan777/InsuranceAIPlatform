// Audit & Cost page — AI run audit trail, metrics, governance panel.
const en = {
  pageHeading: 'AI Run Audit & Cost',
  pageSubheadingTrail: 'full execution trace · governance evidence',
  runSuccess: 'Run successful',

  metricRunId: 'Run ID',
  metricTraceId: 'Trace ID',
  metricModel: 'Model',
  metricTokens: 'Tokens',
  metricCost: 'Cost',
  metricDuration: 'Duration',

  sectionPipeline: 'AI Run Progress',
  sectionAuditTrail: 'Audit trail',
  auditTrailSubtitle: 'All actions with full trace',
  thTime: 'Time',
  thActor: 'Actor',
  thAction: 'Action',
  thResult: 'Result',

  sectionCostBreakdown: 'Cost breakdown',

  governanceLabel: 'Governance',
  governanceHeading: 'AI operates under governance controls',
  govAutoApproveLabel: 'Auto-approve:',
  govAutoApproveValue: 'NOT ALLOWED',
  govHumanReviewLabel: 'Human review:',
  govHumanReviewValue: 'REQUIRED',
  govDecisionLogsLabel: 'Decision logs:',
  govDecisionLogsValue: 'YES',
  govReplayLabel: 'Replay:',
  govReplayValue: 'YES',
};

type T = typeof en;

const uk: T = {
  pageHeading: 'Аудит і витрати AI-запуску',
  pageSubheadingTrail: 'повний слід виконання · governance evidence',
  runSuccess: 'Запуск успішний',

  metricRunId: 'Run ID',
  metricTraceId: 'Trace ID',
  metricModel: 'Модель',
  metricTokens: 'Токени',
  metricCost: 'Вартість',
  metricDuration: 'Час',

  sectionPipeline: 'Хід AI-запуску',
  sectionAuditTrail: 'Audit trail',
  auditTrailSubtitle: 'Усі дії з повним слідом',
  thTime: 'Час',
  thActor: 'Актор',
  thAction: 'Дія',
  thResult: 'Рез.',

  sectionCostBreakdown: 'Розподіл витрат',

  governanceLabel: 'Governance',
  governanceHeading: 'AI підпорядковується процедурі',
  govAutoApproveLabel: 'Авто-погодження:',
  govAutoApproveValue: 'НЕ ДОЗВОЛЕНО',
  govHumanReviewLabel: 'Людська перевірка:',
  govHumanReviewValue: "ОБОВ'ЯЗКОВА",
  govDecisionLogsLabel: 'Логи рішень:',
  govDecisionLogsValue: 'ТАК',
  govReplayLabel: 'Replay:',
  govReplayValue: 'ТАК',
};

export const audit = { en, uk };
