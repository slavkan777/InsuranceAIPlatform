// Documents & Photos page — missing-document banner, damage photos panel,
// document checklist, police-report preview, action buttons, toasts.
const en = {
  // Missing document banner
  missingDocLabel: 'Missing document',
  missingDocHeading: 'Rear bumper damage photo — missing',
  missingDocBody: 'AI is blocking automatic approval until the document is received.',
  uploadDocBtn: 'Upload document',
  uploadDocTooltip: 'Upload a synthetic document to the database (text content, no file)',
  requestDocBtn: 'Log request',
  requestDocTooltip: 'Record an internal request for this document (no client letter)',

  // Damage photos section
  photosTitle: 'Damage photos',
  photosSubtitle: '2 of 3 photos confirmed',
  photosPillNeedsRear: 'rear bumper required',
  photoMissingStatus: 'Request required',
  photoAiConf: 'AI conf',

  // Document checklist
  checklistTitle: 'Document checklist',
  checklistReviewed: 'Reviewed',
  checklistPending: 'Pending review',

  // Police report preview
  policeReportLabel: 'Preview · Police report',
  policeReportDate: 'Date: 18.05.2026 · 14:32',
  policeReportLocation: 'Location: Boryspil, Kyivska 24',
  policeReportInspector: 'Inspector: Ivanenko O.M.',
  policeReportParticipants: 'Participants: 2 · Injured: 0 · At fault: Party B',
  extractedTitle: 'Extracted',
  extractedAccidentDate: '· Accident date',
  extractedLocation: '· Location',
  extractedAtFault: '· At-fault party',
  extractedInspector: '· Inspector',
  progressConfidence: 'Confidence',

  // Action buttons
  requestPhotoBtn: 'Log photo request',
  requestPhotoTooltip: 'Internal request for additional photos (no client letter)',
  viewDetailsBtn: 'View details',
  viewDetailsTooltip: 'View details of the selected document',
  confirmDocBtn: 'Confirm document',
  confirmDocSaving: 'Saving…',
  confirmDocTooltip: 'Record document confirmation in the database and audit log',

  // Toast messages
  toastSelectDocTitle: 'Select a document from the checklist to confirm it.',
  toastConfirmSuccessTitle: 'Document confirmed.',
  toastConfirmErrorTitle: 'Failed to confirm document.',
  toastUnknownError: 'Unknown error.',

  // Misc / default values
  defaultDocumentTitle: 'Document',
  confirmDocApiPrefix: 'Confirmed:',

  // Request prefill — bumper
  requestBumperTitle: 'Additional rear bumper damage photo',
  requestBumperReason:
    'AI is blocking automatic approval until the document is received. Internal adjuster request.',

  // Request prefill — photo
  requestPhotoDefaultTitle: 'Additional damage photo',
  requestPhotoPrefix: 'Photo —',
  requestPhotoReason: 'Full photo package required for AI analysis.',
};

type T = typeof en;

const uk: T = {
  // Missing document banner
  missingDocLabel: 'Відсутній документ',
  missingDocHeading: 'Додаткове фото пошкодження заднього бампера — відсутнє',
  missingDocBody: 'AI блокує автоматичне погодження до отримання документа.',
  uploadDocBtn: 'Завантажити документ',
  uploadDocTooltip: 'Завантажити синтетичний документ у БД (текстовий зміст, без файлу)',
  requestDocBtn: 'Запит у журналі',
  requestDocTooltip: 'Зафіксувати внутрішній запит на цей документ (без листа клієнту)',

  // Damage photos section
  photosTitle: 'Фото пошкоджень',
  photosSubtitle: '2 з 3 фото підтверджено',
  photosPillNeedsRear: 'потрібен задній бампер',
  photoMissingStatus: 'Потрібен запит',
  photoAiConf: 'AI conf',

  // Document checklist
  checklistTitle: 'Контрольний список документів',
  checklistReviewed: 'Переглянуто',
  checklistPending: 'До перевірки',

  // Police report preview
  policeReportLabel: 'Прев\'ю · Поліцейський звіт',
  policeReportDate: 'Дата: 18.05.2026 · 14:32',
  policeReportLocation: 'Локація: Бориспіль, Київська 24',
  policeReportInspector: 'Інспектор: Іваненко О.М.',
  policeReportParticipants: 'Учасники: 2 · Постраждалі: 0 · Винуватець: Сторона Б',
  extractedTitle: 'Витягнуто',
  extractedAccidentDate: '· Дата ДТП',
  extractedLocation: '· Локація',
  extractedAtFault: '· Винуватець',
  extractedInspector: '· Інспектор',
  progressConfidence: 'Впевненість',

  // Action buttons
  requestPhotoBtn: 'Запросити фото у журналі',
  requestPhotoTooltip: 'Внутрішній запит на додаткові фото (без листа клієнту)',
  viewDetailsBtn: 'Переглянути деталі',
  viewDetailsTooltip: 'Перегляд деталей вибраного документа',
  confirmDocBtn: 'Підтвердити документ',
  confirmDocSaving: 'Збереження…',
  confirmDocTooltip: 'Зафіксувати підтвердження документа у БД + аудиті',

  // Toast messages
  toastSelectDocTitle: 'Оберіть документ у списку, щоб підтвердити його.',
  toastConfirmSuccessTitle: 'Документ підтверджено.',
  toastConfirmErrorTitle: 'Не вдалося підтвердити документ.',
  toastUnknownError: 'Невідома помилка.',

  // Misc / default values
  defaultDocumentTitle: 'Документ',
  confirmDocApiPrefix: 'Підтверджено:',

  // Request prefill — bumper
  requestBumperTitle: 'Додаткове фото пошкодження заднього бампера',
  requestBumperReason:
    'AI блокує автоматичне погодження до отримання документа. Внутрішній запит для адʼюстера.',

  // Request prefill — photo
  requestPhotoDefaultTitle: 'Додаткове фото пошкодження',
  requestPhotoPrefix: 'Фото —',
  requestPhotoReason: 'Потрібен повний фото-пакет для AI-аналізу.',
};

export const documents = { en, uk };
