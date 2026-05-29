// Claim workspace overview page — event description, policy check, documents,
// photos, invoice, sandbox notice, AI recommendation, risks, evidence, next action.
const en = {
  // Loading state
  loadingPrefix: 'Loading claim data',
  loadingSuffix: '…',

  // Event description section
  sectionDescription: 'Incident description',
  descriptionFallback: '— no description entered —',
  labelLocation: 'Location',
  labelEventDate: 'Incident date',
  labelEventType: 'Incident type',
  labelCustomer: 'Customer',
  labelVehicle: 'Vehicle',
  labelVin: 'VIN',

  // Policy check (golden claim)
  sectionPolicyCheck: 'Policy verification',
  policyStatusActive: 'Coverage active',
  policyValidity: 'Valid: 01/01/2026 — 31/12/2026 · Incident date within coverage period',
  policyLabelDeductible: 'Deductible',
  policyLabelLimit: 'Limit',

  // Document completeness (golden claim)
  sectionDocuments: 'Document completeness',
  documentsOf: 'of',
  documentsLabelMissing: 'Missing',
  documentsAiNote: 'AI assessed document set as INSUFFICIENT for payout',

  // Damage photos (golden claim)
  sectionDamagePhotos: 'Damage photos',
  photosChip: '3 of 4 photos confirmed',
  photoMissingLabel: 'Photo missing · request',

  // Repair invoice (golden claim)
  sectionInvoice: 'Repair invoice',
  invoiceSubtitle: 'Auto-Expert service centre · issued 19.05.2026',
  invoiceAboveMedian: '+38% above median',
  invoiceTotalLabel: 'Total',

  // Sandbox notice (non-golden claim)
  sectionSandbox: 'Local sandbox claim',
  sandboxBody: 'Newly created local claim. Documents, AI analysis, photos and other decision-support information will appear incrementally — populated from the corresponding tabs (Documents, AI Evidence, Approval).',
  sandboxLabelStatus: 'Status',
  sandboxLabelRisk: 'Risk',
  sandboxLabelPolicy: 'Policy',
  sandboxLabelSla: 'SLA',

  // AI recommendation panel (golden claim)
  aiRecommendationLabel: 'AI RECOMMENDATION',
  aiRecommendationHeading: 'Request additional photo',
  aiRecommendationBody: 'Request a photo of the rear bumper damage from the customer before approving the payout.',
  aiConfidenceLabel: 'Confidence',

  // Key risks (golden claim)
  sectionKeyRisks: 'Key risks',
  keyRiskCoverageConfirmed: 'Policy coverage confirmed',

  // Evidence (golden claim)
  sectionEvidence: 'Evidence',

  // Quick context panel (non-golden claim)
  sectionQuickContext: 'Quick context',
  quickContextBody: 'In-depth context (AI recommendation, key risks, evidence) will appear after the first AI analysis run for this claim.',

  // Next action panel
  nextActionLabel: 'Next action',
  nextActionHeadingGolden: 'Request damage photo',
  nextActionHeadingDefault: 'Collect documents',
  nextActionBody: 'The button below opens the document collection workflow for this claim.',
  btnOpenDocuments: 'Open document collection',

  // Bottom action bar
  btnBackToList: 'Back to list',
  btnSendForReview: 'Send for review',
  btnPrepareDecision: 'Prepare decision',
};

type T = typeof en;

const uk: T = {
  // Loading state
  loadingPrefix: 'Завантаження даних кейса',
  loadingSuffix: '…',

  // Event description section
  sectionDescription: 'Опис події',
  descriptionFallback: '— опис не введено —',
  labelLocation: 'Місце',
  labelEventDate: 'Дата події',
  labelEventType: 'Тип події',
  labelCustomer: 'Клієнт',
  labelVehicle: 'Транспорт',
  labelVin: 'VIN',

  // Policy check (golden claim)
  sectionPolicyCheck: 'Перевірка полісу',
  policyStatusActive: 'Покриття активне',
  policyValidity: 'Дійсний: 01.01.2026 — 31.12.2026 · ДТП дата у періоді',
  policyLabelDeductible: 'Франшиза',
  policyLabelLimit: 'Ліміт',

  // Document completeness (golden claim)
  sectionDocuments: 'Комплектність документів',
  documentsOf: 'із',
  documentsLabelMissing: 'Відсутнє',
  documentsAiNote: 'AI оцінив комплектність як НЕДОСТАТНЮ для виплати',

  // Damage photos (golden claim)
  sectionDamagePhotos: 'Фото пошкоджень',
  photosChip: '3 з 4 фото підтверджено',
  photoMissingLabel: 'Фото відсутнє · запросити',

  // Repair invoice (golden claim)
  sectionInvoice: 'Рахунок СТО',
  invoiceSubtitle: 'СТО «Авто-Експерт» · виставлено 19.05.2026',
  invoiceAboveMedian: '+38% від медіани',
  invoiceTotalLabel: 'Сума',

  // Sandbox notice (non-golden claim)
  sectionSandbox: 'Локальний sandbox кейс',
  sandboxBody: 'Нещодавно створений локальний кейс. Документи, AI-аналіз, фото та інша рішенська інформація з\'являться поетапно — заповнюються з відповідних вкладок (Документи, AI-докази, Погодження).',
  sandboxLabelStatus: 'Статус',
  sandboxLabelRisk: 'Ризик',
  sandboxLabelPolicy: 'Поліс',
  sandboxLabelSla: 'SLA',

  // AI recommendation panel (golden claim)
  aiRecommendationLabel: 'AI-РЕКОМЕНДАЦІЯ',
  aiRecommendationHeading: 'Запросити додаткове фото',
  aiRecommendationBody: 'Запросити у клієнта фото пошкодження заднього бампера перед погодженням виплати.',
  aiConfidenceLabel: 'Впевненість',

  // Key risks (golden claim)
  sectionKeyRisks: 'Ключові ризики',
  keyRiskCoverageConfirmed: 'Покриття за полісом підтверджено',

  // Evidence (golden claim)
  sectionEvidence: 'Докази',

  // Quick context panel (non-golden claim)
  sectionQuickContext: 'Швидкий контекст',
  quickContextBody: 'Поглиблений контекст (AI-рекомендація, ключові ризики, докази) з\'явиться після першого запуску AI-аналізу для цього кейса.',

  // Next action panel
  nextActionLabel: 'Наступна дія',
  nextActionHeadingGolden: 'Запросити фото пошкодження',
  nextActionHeadingDefault: 'Зібрати документи',
  nextActionBody: 'Кнопка нижче відкриває збір документів по цьому випадку.',
  btnOpenDocuments: 'Відкрити збір документів',

  // Bottom action bar
  btnBackToList: 'Повернутись до списку',
  btnSendForReview: 'Передати на перевірку',
  btnPrepareDecision: 'Підготувати рішення',
};

export const claimWorkspace = { en, uk };
