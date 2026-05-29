// Auto-insurance claims overview dashboard namespace.
// EN = default product language; UK = Ukrainian locale.
// `const uk: T` enforces compile-time key parity with `en`.
const en = {
  // Page header
  overviewTitle: 'Auto Insurance Claims Overview',
  overviewSubtitle: 'Operations Dashboard · As of 24 May 2026, 22:48',

  // Period / export toolbar
  periodToday: 'Today',
  periodTodayHint: 'Period filter will be available in the next release',
  period7Days: '7 days',
  period7DaysHint: 'Period filter will be available in the next release',
  exportCsvLabel: 'Export CSV',
  exportCsvTitle: 'Export queue overview to CSV (local)',

  // Metric cards — store-derived labels (used when summaryFromStore is present)
  metricNewClaims: 'NEW INCIDENTS',
  metricNewClaimsDelta: 'AI runs',
  metricAwaitingDecision: 'AWAITING DECISION',
  metricAwaitingDecisionDelta: 'under review',
  metricAiProcessedToday: 'AI-PROCESSED TODAY',
  metricAiProcessedTodayDelta: 'now',
  metricHighRisk: 'HIGH RISK',
  metricHighRiskDelta: 'active',
  metricAvgSla: 'AVERAGE SLA TIME',
  metricAvgSlaDeltaSuffix: 'h',
  metricAvgSlaDelta: 'remaining',

  // Lifecycle chart section
  lifecycleTitle: 'Auto Insurance Claim Lifecycle',
  lifecycleSubtitle: 'Distribution of active claims by phase',
  lifecycleActiveChip: 'active',

  // Claims queue section
  claimsQueueTitle: 'Auto Insurance Claims Queue',
  claimsQueueSubtitleActive: 'active',
  claimsQueueSubtitleSuffix: ' · updated every minute',
  claimsQueueActiveDefault: '53 active',
  newClaimButton: 'Create Claim',
  newClaimButtonTitle: 'Create a new synthetic case (local sandbox)',

  // Queue filter tabs
  filterAll: 'All',
  filterRta: 'RTA',
  filterHighRisk: 'High risk',
  filterAwaitAi: 'Awaiting AI',
  filterAwaitDecision: 'Awaiting decision',
  filterTabsTitle: 'Filters are active in the "Auto Insurance Claims" section',

  // Table headers
  thClaimNo: 'Claim No.',
  thCustomerVehicle: 'Customer · Vehicle',
  thEventType: 'Event type',
  thDocuments: 'Documents',
  thAiStatus: 'AI status',
  thRisk: 'Risk',
  thNextAction: 'Next action',
  thUpdated: 'Updated',

  // View-all link
  viewAllClaims: 'View all claims',

  // AI recommendation card
  aiRecTitle: 'AI recommendation for',
  aiRecPayoutLabel: 'Estimated payout',
  aiRecConfidenceLabel: 'Confidence',
  aiRecPill: 'Recommendation',
  aiRecAdvisory: 'human review required',
  aiRecBody: 'Request additional photo of rear bumper damage before approving payout.',
  aiRecKeyFactors: 'Key factors',
  aiRecViewButton: 'View AI analysis',

  // Audit & cost card
  auditTitle: 'Audit & Cost (today)',
  auditViewDetails: 'View details',

  // Recent events card
  recentEventsTitle: 'Recent Events',
  recentEventsViewAudit: 'View audit log',

  // Chart sections
  chartCaseTypeTitle: 'Claims by event type',
  chartCaseTypeSubtitle: 'last 7 days',
  chartConfidenceTitle: 'AI confidence (distribution)',
  chartConfidenceSubtitle: 'today',
  chartConfidenceSubtitleSuffix: '= 78%',
  chartTrendTitle: 'Processing trend',
  chartTrendSubtitle: 'last 7 days',

  // Toast (export)
  toastExportTitle: 'Exported 5 rows.',
  toastExportDetailPrefix: 'File ',
  toastExportDetailSuffix: ' saved to browser downloads.',
};

type T = typeof en;

const uk: T = {
  // Page header
  overviewTitle: 'Огляд автострахових випадків',
  overviewSubtitle: 'Операційна панель · Станом на 24 травня 2026, 22:48',

  // Period / export toolbar
  periodToday: 'Сьогодні',
  periodTodayHint: 'Перемикач періоду з\'явиться у наступному релізі',
  period7Days: '7 днів',
  period7DaysHint: 'Перемикач періоду з\'явиться у наступному релізі',
  exportCsvLabel: 'Експорт CSV',
  exportCsvTitle: 'Експортувати огляд черги у CSV (локально)',

  // Metric cards — store-derived labels
  metricNewClaims: 'НОВІ ДТП',
  metricNewClaimsDelta: 'AI runs',
  metricAwaitingDecision: 'ОЧІКУЮТЬ РІШЕННЯ',
  metricAwaitingDecisionDelta: 'на розгляді',
  metricAiProcessedToday: 'AI-ОБРОБЛЕНО СЬОГОДНІ',
  metricAiProcessedTodayDelta: 'зараз',
  metricHighRisk: 'ВИСОКИЙ РИЗИК',
  metricHighRiskDelta: 'поточні',
  metricAvgSla: 'СЕРЕДНІЙ ЧАС SLA',
  metricAvgSlaDeltaSuffix: ' год',
  metricAvgSlaDelta: 'залишилось',

  // Lifecycle chart section
  lifecycleTitle: 'Життєвий цикл автострахового випадку',
  lifecycleSubtitle: 'Розподіл активних випадків за фазами',
  lifecycleActiveChip: 'активних',

  // Claims queue section
  claimsQueueTitle: 'Черга автострахових випадків',
  claimsQueueSubtitleActive: 'активних',
  claimsQueueSubtitleSuffix: ' · оновлено щохвилини',
  claimsQueueActiveDefault: '53 активних',
  newClaimButton: 'Створити випадок',
  newClaimButtonTitle: 'Створення нового синтетичного кейсу (локальний sandbox)',

  // Queue filter tabs
  filterAll: 'Усі',
  filterRta: 'ДТП',
  filterHighRisk: 'Високий ризик',
  filterAwaitAi: 'Чекає AI',
  filterAwaitDecision: 'Чекає рішення',
  filterTabsTitle: 'Фільтри активні у розділі «Автострахові випадки»',

  // Table headers
  thClaimNo: 'Номер',
  thCustomerVehicle: 'Клієнт · Авто',
  thEventType: 'Тип події',
  thDocuments: 'Документи',
  thAiStatus: 'AI-статус',
  thRisk: 'Ризик',
  thNextAction: 'Наступна дія',
  thUpdated: 'Оновлено',

  // View-all link
  viewAllClaims: 'Переглянути всі випадки',

  // AI recommendation card
  aiRecTitle: 'AI-рекомендація для',
  aiRecPayoutLabel: 'Ймовірна виплата',
  aiRecConfidenceLabel: 'Впевненість',
  aiRecPill: 'Рекомендація',
  aiRecAdvisory: 'людська перевірка обов\'язкова',
  aiRecBody: 'Запросити додаткове фото пошкодження заднього бампера перед погодженням виплати.',
  aiRecKeyFactors: 'Ключові фактори',
  aiRecViewButton: 'Переглянути AI-аналіз',

  // Audit & cost card
  auditTitle: 'Аудит і витрати (сьогодні)',
  auditViewDetails: 'Переглянути деталі',

  // Recent events card
  recentEventsTitle: 'Останні події',
  recentEventsViewAudit: 'Переглянути журнал аудиту',

  // Chart sections
  chartCaseTypeTitle: 'Випадки за типом події',
  chartCaseTypeSubtitle: 'за 7 днів',
  chartConfidenceTitle: 'AI-впевненість (розподіл)',
  chartConfidenceSubtitle: 'сьогодні',
  chartConfidenceSubtitleSuffix: '= 78%',
  chartTrendTitle: 'Тренд обробки',
  chartTrendSubtitle: 'за 7 днів',

  // Toast (export)
  toastExportTitle: 'Експортовано 5 рядків.',
  toastExportDetailPrefix: 'Файл ',
  toastExportDetailSuffix: ' збережено у завантаженнях браузера.',
};

export const dashboard = { en, uk };
