// Shared UI components: modals and DeferredActionButton.
const en = {
  // DeferredActionButton
  deferredHint: 'Available in the next release.',

  // NewClaimModal
  newClaimTitle: 'Create new synthetic claim',
  newClaimDescription:
    'Creates a new row in the claims.Claims table with full audit + outbox. Local sandbox — no real personal data, no real payout, no external notifications.',
  newClaimCancel: 'Cancel',
  newClaimSubmit: 'Create claim',
  newClaimSubmitting: 'Creating…',
  newClaimLabelCustomerName: 'Customer (optional)',
  newClaimLabelCustomerId: 'Customer ID (optional)',
  newClaimLabelVehicle: 'Vehicle *',
  newClaimLabelVin: 'VIN (optional)',
  newClaimLabelEventType: 'Event type *',
  newClaimLabelEventDate: 'Event date *',
  newClaimLabelLocation: 'Location *',
  newClaimLabelDescription: 'Description (optional)',
  newClaimPlaceholderCustomerName: 'E.g. Synthetic Customer 042',
  newClaimPlaceholderLocation: 'Kyiv, Peremohy Ave 50',
  newClaimPlaceholderDescription: 'Brief description of the incident (synthetic).',
  newClaimHelperCustomerName: 'Leave blank to assign the first synthetic customer.',
  newClaimErrorRequired: 'Please fill in "Vehicle" and "Location" (required fields).',
  newClaimSandboxNote:
    'Claim is created in the local sandbox database. No real personal data, no real payout, no external notifications. Action is recorded in audit + outbox.',
  newClaimToastTitle: 'New synthetic claim created',

  // RequestMissingDocumentModal
  reqDocTitle: 'Request missing document',
  reqDocDescription:
    'Creates an internal record in the audit log and outbox. No customer letter is sent — this is an internal note for the adjuster only.',
  reqDocCancel: 'Cancel',
  reqDocSubmit: 'Record request',
  reqDocSubmitting: 'Saving…',
  reqDocLabelTitle: 'Document name',
  reqDocLabelReason: 'Reason (optional)',
  reqDocPlaceholderTitle: 'E.g. Photo of rear bumper damage',
  reqDocPlaceholderReason: 'Describe why this document is required for further review.',
  reqDocErrorRequired: 'Please specify the name of the document to request.',
  reqDocToastTitle: 'Internal request recorded.',
  reqDocSandboxNotePrefix: 'Record is created for claim',
  reqDocSandboxNoteSuffix: 'No external notification is sent to the customer.',

  // DocumentPreviewModal
  docPreviewTitle: 'View original',
  docPreviewDescription:
    'In this demo environment documents are not stored — only reference metadata. Full original preview will be available once binary storage is connected (Azure Blob / S3 / on-prem).',
  docPreviewClose: 'Understood',
  docPreviewDefaultTitle: 'Document',
  docPreviewNotAvailable:
    'Original not available in demo mode. Document metadata (type, date, verification status) is stored in the database.',
  docPreviewBullet1: 'Demo mode does not accept binary uploads.',
  docPreviewBullet2: 'OCR / classification and integrity detection are out of scope for the local demo.',
  docPreviewBullet3: 'The audit log records when and by whom metadata was accessed.',

  // UploadDocumentContentModal
  uploadDocTitle: 'Upload document (synthetic)',
  uploadDocDescription:
    'Saves a synthetic text document to the database (no file, no OCR, no external storage). Content field is nvarchar(max). No real PII is used.',
  uploadDocCancel: 'Cancel',
  uploadDocSubmit: 'Save to DB',
  uploadDocSubmitting: 'Saving…',
  uploadDocLabelKind: 'Record type',
  uploadDocLabelDocType: 'Document type',
  uploadDocLabelTitle: 'Title *',
  uploadDocLabelContent: 'Content (text) *',
  uploadDocUseTemplate: '↻ Use template',
  uploadDocPlaceholderContent: 'Text of the report / statement / note. Stored as nvarchar(max).',
  uploadDocHelperLength: 'chars (sandbox limit: 200,000)',
  uploadDocErrorRequired: 'Please fill in "Title" and "Content" (required).',
  uploadDocToastTitle: 'Document saved to DB.',
  uploadDocSandboxNotePrefix: 'Content is written to the document record for claim',
  uploadDocSandboxNoteSuffix: '(Content field, nvarchar(max)). No file upload, no external service.',

  // UploadDocumentContentModal — kind options
  uploadKindPoliceReport: 'Police report',
  uploadKindCustomerStatement: 'Customer statement',
  uploadKindEstimate: 'Repair estimate',
  uploadKindInternalNote: 'Internal note',
  uploadKindDamageSummary: 'Damage summary',

  // UploadDocumentContentModal — doc type options
  uploadDocTypePlaceholder: '— select type —',
  uploadDocTypePoliceReport: 'Police report',
  uploadDocTypeCustomerStatement: 'Customer statement',
  uploadDocTypeEstimate: 'Repair estimate',
  uploadDocTypeInternalNote: 'Internal note',
  uploadDocTypeOther: 'Other document',

  // ImportDocumentMetadataModal
  importDocTitle: 'Import document (metadata)',
  importDocDescription:
    'Creates a metadata record in the database for the claim. No binary upload is performed — reference fields only. Audit log and outbox are updated.',
  importDocCancel: 'Cancel',
  importDocSubmit: 'Save metadata',
  importDocSubmitting: 'Saving…',
  importDocLabelKind: 'Record type',
  importDocLabelTitle: 'Title',
  importDocLabelDocType: 'Document type (optional)',
  importDocPlaceholderTitle: 'E.g. Police report DTP-2026-1234',
  importDocErrorRequired: 'Please specify the document title.',
  importDocToastTitle: 'Document metadata saved.',
  importDocSandboxNotePrefix: 'Record creates a reference row in the claim document table for',
  importDocSandboxNoteSuffix: 'No files are uploaded.',

  // ImportDocumentMetadataModal — kind options
  importKindDocument: 'Document (PDF / certificate)',
  importKindPhoto: 'Damage photo',
  importKindNote: 'Internal note',

  // ImportDocumentMetadataModal — doc type options
  importDocTypePlaceholder: '— select type —',
  importDocTypePoliceReport: 'Police report',
  importDocTypeDriverLicense: "Driver's licence",
  importDocTypeEstimate: 'Repair estimate',
  importDocTypeDamagePhoto: 'Damage photo',
  importDocTypeOther: 'Other document',

  // PayoutSimulationModal
  payoutSimTitle: 'Payout simulation (DB-only)',
  payoutSimDescription:
    'Creates a record in approval.PayoutSimulations with the SimulationOnly=true flag. No real payout, no customer notification, and no claim status change is performed.',
  payoutSimCancel: 'Cancel',
  payoutSimSubmit: 'Record simulation',
  payoutSimSubmitting: 'Creating…',
  payoutSimLabelAmount: 'Payout amount *',
  payoutSimLabelDeductible: 'Deductible',
  payoutSimLabelCurrency: 'Currency',
  payoutSimLabelDecisionSource: 'Decision source',
  payoutSimLabelNotes: 'Notes (optional)',
  payoutSimNetLabel: 'Net payout:',
  payoutSimPlaceholderNotes: 'Decision context, evidence references, confidence level…',
  payoutSimErrorAmountPositive: 'Payout amount must be positive.',
  payoutSimErrorDeductibleNegative: 'Deductible cannot be negative.',
  payoutSimToastTitle: 'Payout simulation created.',
  payoutSimLinkedRun: 'Linked AI run:',
  payoutSimSandboxNote:
    'SimulationOnly=true is a schema-level guarantee. No real transactions, no customer notifications, no claim status changes. Audit log and outbox event are written for traceability.',

  // PayoutSimulationModal — source options
  payoutSourceHuman: 'Human decision',
  payoutSourceAiAdvisory: 'AI-advisory (confirmed by human)',
  payoutSourceHybrid: 'Hybrid (human + AI)',
};

type T = typeof en;

const uk: T = {
  // DeferredActionButton
  deferredHint: 'З\'явиться у наступному релізі.',

  // NewClaimModal
  newClaimTitle: 'Створити новий синтетичний кейс',
  newClaimDescription:
    'Створює новий рядок у БД claims.Claims з повним audit + outbox. Локальний sandbox — без реальних персональних даних, без реальної виплати, без зовнішніх повідомлень.',
  newClaimCancel: 'Скасувати',
  newClaimSubmit: 'Створити кейс',
  newClaimSubmitting: 'Створення…',
  newClaimLabelCustomerName: 'Клієнт (опціонально)',
  newClaimLabelCustomerId: 'ID клієнта (опціонально)',
  newClaimLabelVehicle: 'Авто *',
  newClaimLabelVin: 'VIN (опціонально)',
  newClaimLabelEventType: 'Тип події *',
  newClaimLabelEventDate: 'Дата події *',
  newClaimLabelLocation: 'Локація *',
  newClaimLabelDescription: 'Опис (опціонально)',
  newClaimPlaceholderCustomerName: 'Напр. Synthetic Customer 042',
  newClaimPlaceholderLocation: 'Київ, проспект Перемоги 50',
  newClaimPlaceholderDescription: 'Короткий опис обставин події (синтетичний).',
  newClaimHelperCustomerName: 'Порожньо → буде обрано першого синтетичного клієнта.',
  newClaimErrorRequired: 'Заповніть «Авто» та «Локація» (обов\'язкові поля).',
  newClaimSandboxNote:
    'Кейс створюється у локальній БД sandbox. Без реальних персональних даних, без реальної виплати, без зовнішніх повідомлень. Дія записується в audit + outbox.',
  newClaimToastTitle: 'Створено новий синтетичний кейс',

  // RequestMissingDocumentModal
  reqDocTitle: 'Запит на відсутній документ',
  reqDocDescription:
    'Створює внутрішній запис у журналі аудиту та outbox. Лист клієнту НЕ надсилається — це лише внутрішня нотатка для адʼюстера.',
  reqDocCancel: 'Скасувати',
  reqDocSubmit: 'Зафіксувати запит',
  reqDocSubmitting: 'Збереження…',
  reqDocLabelTitle: 'Назва документа',
  reqDocLabelReason: 'Причина (опціонально)',
  reqDocPlaceholderTitle: 'Напр. Фото пошкодження заднього бампера',
  reqDocPlaceholderReason: 'Опишіть, чому документ необхідний для подальшого розгляду.',
  reqDocErrorRequired: 'Вкажіть назву документа, який треба запросити.',
  reqDocToastTitle: 'Внутрішній запит зафіксовано.',
  reqDocSandboxNotePrefix: 'Запис створюється для кейсу',
  reqDocSandboxNoteSuffix: 'Зовнішнє повідомлення клієнту не надсилається.',

  // DocumentPreviewModal
  docPreviewTitle: 'Перегляд оригіналу',
  docPreviewDescription:
    'У цьому демо-середовищі файли не зберігаються — лише довідкові метадані. Реальний перегляд оригіналу буде доступний після підключення бінарного сховища (Azure Blob / S3 / on-prem).',
  docPreviewClose: 'Зрозуміло',
  docPreviewDefaultTitle: 'Документ',
  docPreviewNotAvailable:
    'Оригінал не доступний у демо-режимі. Метадані документа (тип, дата, статус перевірки) зберігаються у БД.',
  docPreviewBullet1: 'Демо-режим не приймає бінарні завантаження.',
  docPreviewBullet2: 'OCR/класифікація і виявлення цілісності — поза скоупом локального демо.',
  docPreviewBullet3: 'Журнал аудиту відображає, коли і ким переглядали метадані.',

  // UploadDocumentContentModal
  uploadDocTitle: 'Завантажити документ (синтетичний)',
  uploadDocDescription:
    'Зберігає у БД синтетичний текстовий документ (без файлу, без OCR, без зовнішнього сховища). Поле Content типу nvarchar(max). Реальні PII не використовуються.',
  uploadDocCancel: 'Скасувати',
  uploadDocSubmit: 'Зберегти у БД',
  uploadDocSubmitting: 'Збереження…',
  uploadDocLabelKind: 'Тип запису',
  uploadDocLabelDocType: 'Тип документа',
  uploadDocLabelTitle: 'Назва *',
  uploadDocLabelContent: 'Зміст (текст) *',
  uploadDocUseTemplate: '↻ Підставити шаблон',
  uploadDocPlaceholderContent: 'Текст звіту / заяви / нотатки. Зберігається в nvarchar(max).',
  uploadDocHelperLength: 'симв. (ліміт sandbox: 200 000)',
  uploadDocErrorRequired: 'Заповніть «Назва» та «Зміст» (обов\'язкові).',
  uploadDocToastTitle: 'Документ збережено в БД.',
  uploadDocSandboxNotePrefix: 'Зміст пишеться у документ кейсу',
  uploadDocSandboxNoteSuffix: '(поле Content типу nvarchar(max)). Жодного завантаження файлу, жодного зовнішнього сервісу.',

  // UploadDocumentContentModal — kind options
  uploadKindPoliceReport: 'Поліцейський звіт',
  uploadKindCustomerStatement: 'Заява клієнта',
  uploadKindEstimate: 'Кошторис СТО',
  uploadKindInternalNote: 'Внутрішня нотатка',
  uploadKindDamageSummary: 'Опис пошкоджень',

  // UploadDocumentContentModal — doc type options
  uploadDocTypePlaceholder: '— оберіть тип —',
  uploadDocTypePoliceReport: 'Поліцейський звіт',
  uploadDocTypeCustomerStatement: 'Заява клієнта',
  uploadDocTypeEstimate: 'Кошторис ремонту',
  uploadDocTypeInternalNote: 'Внутрішня нотатка',
  uploadDocTypeOther: 'Інший документ',

  // ImportDocumentMetadataModal
  importDocTitle: 'Імпорт документа (метадані)',
  importDocDescription:
    'Створює запис метаданих у БД для кейсу. Бінарне завантаження не виконується — лише довідкові поля. Аудит-журнал і outbox оновлюються.',
  importDocCancel: 'Скасувати',
  importDocSubmit: 'Зберегти метадані',
  importDocSubmitting: 'Збереження…',
  importDocLabelKind: 'Тип запису',
  importDocLabelTitle: 'Назва',
  importDocLabelDocType: 'Тип документа (опціонально)',
  importDocPlaceholderTitle: 'Напр. Поліцейський звіт DTP-2026-1234',
  importDocErrorRequired: 'Вкажіть назву документа.',
  importDocToastTitle: 'Метадані документа збережено.',
  importDocSandboxNotePrefix: 'Запис створює лише довідковий рядок у таблиці документів кейсу',
  importDocSandboxNoteSuffix: 'Жодних файлів не завантажується.',

  // ImportDocumentMetadataModal — kind options
  importKindDocument: 'Документ (PDF/довідка)',
  importKindPhoto: 'Фото пошкодження',
  importKindNote: 'Внутрішня нотатка',

  // ImportDocumentMetadataModal — doc type options
  importDocTypePlaceholder: '— оберіть тип —',
  importDocTypePoliceReport: 'Поліцейський звіт',
  importDocTypeDriverLicense: 'Посвідчення водія',
  importDocTypeEstimate: 'Кошторис ремонту',
  importDocTypeDamagePhoto: 'Фото пошкоджень',
  importDocTypeOther: 'Інший документ',

  // PayoutSimulationModal
  payoutSimTitle: 'Симуляція виплати (DB-only)',
  payoutSimDescription:
    'Створює запис у approval.PayoutSimulations з прапором SimulationOnly=true. Реальної виплати, листа клієнту або зміни статусу кейсу не виконується.',
  payoutSimCancel: 'Скасувати',
  payoutSimSubmit: 'Зафіксувати симуляцію',
  payoutSimSubmitting: 'Створення…',
  payoutSimLabelAmount: 'Сума виплати *',
  payoutSimLabelDeductible: 'Франшиза',
  payoutSimLabelCurrency: 'Валюта',
  payoutSimLabelDecisionSource: 'Джерело рішення',
  payoutSimLabelNotes: 'Нотатки (опціонально)',
  payoutSimNetLabel: 'Чиста виплата (net):',
  payoutSimPlaceholderNotes: 'Контекст рішення, посилання на докази, рівень впевненості...',
  payoutSimErrorAmountPositive: 'Сума виплати має бути додатня.',
  payoutSimErrorDeductibleNegative: 'Франшиза не може бути від\'ємна.',
  payoutSimToastTitle: 'Симуляція виплати створена.',
  payoutSimLinkedRun: 'Пов\'язаний AI run:',
  payoutSimSandboxNote:
    'SimulationOnly=true гарантовано на рівні схеми. Жодних реальних транзакцій, жодних повідомлень клієнту, жодних змін статусу кейсу. Аудит-журнал і outbox-event записуються для трасування.',

  // PayoutSimulationModal — source options
  payoutSourceHuman: 'Людське рішення',
  payoutSourceAiAdvisory: 'AI-порадницьке (підтверджено людиною)',
  payoutSourceHybrid: 'Гібридне (людина + AI)',
};

export const ui = { en, uk };
