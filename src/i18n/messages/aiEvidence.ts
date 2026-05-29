// AI analysis & evidence page — EN is the product default; UK is the switched locale.
// The `const uk: T` annotation enforces identical keys at compile time.
const en = {
  // Page header
  pageTitle: 'AI Analysis & Evidence',
  pageSubtitleTrace: 'Trace:',
  pageSubtitleProviderTooltip: 'AI provider that returned the last run',
  pageSubtitleTokens: 'tokens',

  // Run controls
  minConfidenceLabel: 'Min. confidence',
  runButtonIdle: 'Run AI analysis',
  runButtonRunning: 'Running',
  runButtonTooltip:
    'Launch advisory-only AI analysis via BFF (Mock by default; DeepSeek by explicit opt-in only)',

  // Progress / error banners
  progressLabel: 'AI run in progress',
  errorTitle: 'AI analysis failed',

  // BFF advisory card
  advisoryBadge: 'Advisory-only AI',
  lastRunHeading: 'Latest AI analysis run',
  chipLoading: 'Loading…',
  chipNoRuns: 'No runs yet',
  chipLoadError: 'Load error',
  chipConfPrefix: 'conf',
  chipRiskPrefix: 'risk',

  // Advisory card sections
  sectionSummary: 'Summary',
  sectionRecommendedAction: 'Recommended action (advisory)',
  sectionPolicy: 'Policy / coverage',
  sectionFindings: 'Findings',
  rationalePrefix: 'Rationale:',
  confSuffix: 'conf',

  // Advisory card counters
  counterEvidence: 'Evidence',
  counterRisks: 'Risks',
  counterTokens: 'Tokens',
  counterCost: 'Cost',

  // Guardrails
  guardrailsHeading: 'Guardrails (advisory mode)',
  guardrailExpectTrue: 'Expected',
  guardrailExpectFalseNote: 'AI never receives this permission',

  // Advisory card empty / waiting states
  waitingBff: 'Waiting for first BFF response (GET /api/claims/',
  waitingBffSuffix: '/ai-analysis)…',
  noRunsYet:
    'No AI runs for this claim yet. Click "Run AI analysis" to execute an advisory-only run.',

  // AI Decision logging block
  decisionBadge: 'AI decision log (sandbox)',
  decisionHeading: 'Record AI recommendation as an audited decision',
  decisionDescription:
    'Creates an entry in the audit log with source ',
  decisionDescriptionSuffix:
    ' and a corresponding outbox event. No payout is triggered, no customer message is sent, and the claim status is not changed.',
  decisionButtonSaving: 'Saving…',
  decisionButtonIdle: 'Record AI decision',
  decisionButtonTooltipReady:
    'Record the AI recommendation from the latest run in the log (no payout / no notifications)',
  decisionButtonTooltipNoRun:
    'Run AI analysis first — an AI decision cannot be created without a run',
  decisionSavedBanner: 'Saved · advisory-only ·',
  decisionLabelCmd: 'cmd',
  decisionLabelAudit: 'audit',
  decisionLabelOutbox: 'outbox',
  decisionLabelRunId: 'runId',
  decisionLabelProvider: 'provider',
  decisionLabelSource: 'source',
  decisionNoRunHint:
    'Run "Run AI analysis" first — nothing to record without a run.',

  // Toast notifications
  toastDecisionSuccessTitle: 'AI decision recorded in the log.',
  toastDecisionSuccessDetail: 'Source: AI · cmd=',
  toastDecisionSuccessRun: ' · run=',
  toastDecisionErrorTitle: 'Failed to record AI decision.',

  // Findings section (mock visualisation)
  findingsTitle: 'AI findings (visualisation)',
  findingsSubtitle: 'findings after document processing',

  // Extracted entities table
  entitiesTitle: 'Extracted entities',
  entitiesSubtitle: 'Data from all sources, normalised',
  entitiesChipSuffix: 'fields',
  entitiesColField: 'Field',
  entitiesColValue: 'Value',
  entitiesColSource: 'Source',
  entitiesColConfidence: 'Conf.',
  entitiesEmptyState: 'No fields match the current confidence filter.',

  // Evidence panel
  evidenceTitle: 'Evidence',
  evidenceSelectedPrefix: 'Selected evidence:',

  // Model confidence panel
  modelConfidenceTitle: 'Model confidence',
};

type T = typeof en;

const uk: T = {
  // Page header
  pageTitle: 'AI-аналіз та докази',
  pageSubtitleTrace: 'Trace:',
  pageSubtitleProviderTooltip: 'AI-провайдер, який повернув останній прогон',
  pageSubtitleTokens: 'токенів',

  // Run controls
  minConfidenceLabel: 'Мін. впевненість',
  runButtonIdle: 'Запустити AI-аналіз',
  runButtonRunning: 'Запускаємо',
  runButtonTooltip:
    'Запустити advisory-only AI-аналіз через BFF (Mock за замовчуванням; DeepSeek тільки за явним opt-in)',

  // Progress / error banners
  progressLabel: 'Хід AI-запуску',
  errorTitle: 'AI-аналіз не виконано',

  // BFF advisory card
  advisoryBadge: 'Advisory-only AI',
  lastRunHeading: 'Останній прогон AI-аналізу',
  chipLoading: 'Завантаження…',
  chipNoRuns: 'Прогонів ще немає',
  chipLoadError: 'Помилка завантаження',
  chipConfPrefix: 'conf',
  chipRiskPrefix: 'risk',

  // Advisory card sections
  sectionSummary: 'Зведення',
  sectionRecommendedAction: 'Рекомендована дія (порадницька)',
  sectionPolicy: 'Поліс / покриття',
  sectionFindings: 'Знахідки',
  rationalePrefix: 'Обґрунтування:',
  confSuffix: 'conf',

  // Advisory card counters
  counterEvidence: 'Докази',
  counterRisks: 'Ризики',
  counterTokens: 'Токени',
  counterCost: 'Cost',

  // Guardrails
  guardrailsHeading: 'Guardrails (порадницький режим)',
  guardrailExpectTrue: 'Очікуємо',
  guardrailExpectFalseNote: 'AI ніколи не отримує цей дозвіл',

  // Advisory card empty / waiting states
  waitingBff: 'Очікуємо першу відповідь BFF (GET /api/claims/',
  waitingBffSuffix: '/ai-analysis)…',
  noRunsYet:
    'Для цього кейсу AI-прогонів ще немає. Натисніть «Запустити AI-аналіз», щоб виконати advisory-only прогон.',

  // AI Decision logging block
  decisionBadge: 'AI-рішення у журналі (sandbox)',
  decisionHeading: 'Зафіксувати AI-рекомендацію як аудитоване рішення',
  decisionDescription: 'Створює запис у журналі аудиту з джерелом ',
  decisionDescriptionSuffix:
    ' та відповідний outbox-event. Виплата не виконується, лист клієнту не надсилається, статус кейсу не змінюється.',
  decisionButtonSaving: 'Збереження…',
  decisionButtonIdle: 'Зафіксувати AI-рішення',
  decisionButtonTooltipReady:
    'Зафіксувати AI-рекомендацію останнього прогону у журналі (без виплати/повідомлень)',
  decisionButtonTooltipNoRun:
    'Спершу виконайте AI-аналіз — без прогону неможливо створити AI-рішення',
  decisionSavedBanner: 'Збережено · advisory-only ·',
  decisionLabelCmd: 'cmd',
  decisionLabelAudit: 'audit',
  decisionLabelOutbox: 'outbox',
  decisionLabelRunId: 'runId',
  decisionLabelProvider: 'provider',
  decisionLabelSource: 'source',
  decisionNoRunHint:
    'Спершу виконайте «Запустити AI-аналіз» — без прогону немає що фіксувати.',

  // Toast notifications
  toastDecisionSuccessTitle: 'AI-рішення зафіксовано у журналі.',
  toastDecisionSuccessDetail: 'Джерело: AI · cmd=',
  toastDecisionSuccessRun: ' · run=',
  toastDecisionErrorTitle: 'Не вдалося зафіксувати AI-рішення.',

  // Findings section (mock visualisation)
  findingsTitle: 'AI-знахідки (mock-візуалізація)',
  findingsSubtitle: 'висновки після обробки документів',

  // Extracted entities table
  entitiesTitle: 'Витягнуті сутності',
  entitiesSubtitle: 'Дані з усіх джерел, нормалізовано',
  entitiesChipSuffix: 'полів',
  entitiesColField: 'Поле',
  entitiesColValue: 'Значення',
  entitiesColSource: 'Джерело',
  entitiesColConfidence: 'Впевн.',
  entitiesEmptyState: 'Жодне поле не відповідає поточному фільтру впевненості.',

  // Evidence panel
  evidenceTitle: 'Докази',
  evidenceSelectedPrefix: 'Вибраний доказ:',

  // Model confidence panel
  modelConfidenceTitle: 'Впевненість моделі',
};

export const aiEvidence = { en, uk };
