// Claims list page — page header, filters, segment tabs, table, empty state,
// toast messages, and the demo-fallback note.
// en/uk key parity is enforced at compile time by `const uk: T`.
const en = {
  // Page header
  pageTitle: 'Auto insurance claims',
  pageSubtitle: '53 active · 8 high-risk · 5 awaiting human decision',

  // Action buttons
  btnExportCsv: 'Export CSV',
  btnExportCsvTitle: 'Export the current list to CSV (locally, in your browser)',
  btnImportDoc: 'Import document',
  btnImportDocTitle: 'Import document metadata (no binary upload)',
  btnNewClaim: 'New claim',
  btnNewClaimTitle: 'Create a new synthetic case (local sandbox)',

  // Filter section
  filterSearchLabel: 'Search',
  filterSearchPlaceholder: 'Toyota Camry, Robert Johnson…',
  filterStatusLabel: 'Status',
  filterRiskLabel: 'Risk',
  filterEventTypeLabel: 'Event type',
  filterDateLabel: 'Date',
  filterAiStatusLabel: 'AI status',

  // Table section header
  queueTitle: 'Auto insurance claims queue',
  queueSortedBySla: 'Sorted by SLA',
  queueDemoFallback: '· demo data (backend unavailable)',

  // Segment tabs (display labels only — dispatch values stay as Ukrainian keys)
  segAll: 'All',
  segAccident: 'Accident',
  segHighRisk: 'High risk',
  segAwaitingAi: 'Awaiting AI',
  segAwaitingDecision: 'Awaiting decision',

  // Table headers
  thClaimId: 'Claim no.',
  thCustomerVehicle: 'Customer · Vehicle',
  thType: 'Type',
  thStatus: 'Status',
  thDocs: 'Docs',
  thAiStatus: 'AI status',
  thRisk: 'Risk',
  thSla: 'SLA',
  thNextAction: 'Next action',

  // Empty state
  emptyState: 'No claims match the current filters.',

  // Toast (export)
  toastExportTitle: 'Exported',
  toastExportTitleSuffix: 'rows.',
  toastExportDetail: 'File saved to browser downloads.',
};

type T = typeof en;

const uk: T = {
  // Page header
  pageTitle: 'Автострахові випадки',
  pageSubtitle: '53 активних · 8 з високим ризиком · 5 чекають людського рішення',

  // Action buttons
  btnExportCsv: 'Експорт CSV',
  btnExportCsvTitle: 'Експортувати поточний список у CSV (локально, у вашому браузері)',
  btnImportDoc: 'Імпорт документа',
  btnImportDocTitle: 'Імпорт метаданих документа (без бінарного завантаження)',
  btnNewClaim: 'Новий випадок',
  btnNewClaimTitle: 'Створення нового синтетичного кейсу (локальний sandbox)',

  // Filter section
  filterSearchLabel: 'Пошук',
  filterSearchPlaceholder: 'Toyota Camry, Роберт Джонсон…',
  filterStatusLabel: 'Статус',
  filterRiskLabel: 'Ризик',
  filterEventTypeLabel: 'Тип події',
  filterDateLabel: 'Дата',
  filterAiStatusLabel: 'AI-статус',

  // Table section header
  queueTitle: 'Черга автострахових випадків',
  queueSortedBySla: 'Сортовано за SLA',
  queueDemoFallback: '· демо-дані (бекенд недоступний)',

  // Segment tabs
  segAll: 'Усі',
  segAccident: 'ДТП',
  segHighRisk: 'Високий ризик',
  segAwaitingAi: 'Чекає AI',
  segAwaitingDecision: 'Чекає рішення',

  // Table headers
  thClaimId: 'Номер',
  thCustomerVehicle: 'Клієнт · Авто',
  thType: 'Тип',
  thStatus: 'Статус',
  thDocs: 'Док.',
  thAiStatus: 'AI-статус',
  thRisk: 'Ризик',
  thSla: 'SLA',
  thNextAction: 'Наступна дія',

  // Empty state
  emptyState: 'Жодного кейсу не знайдено за поточними фільтрами.',

  // Toast (export)
  toastExportTitle: 'Експортовано',
  toastExportTitleSuffix: 'рядків.',
  toastExportDetail: 'Файл збережено у завантаженнях браузера.',
};

export const claimsList = { en, uk };
