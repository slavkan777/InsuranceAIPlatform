// Customer directory page and create-customer modal strings.
const en = {
  // Page header
  pageTitle: 'Customer Directory',
  pageSubtitle: 'Local catalogue of synthetic customers · IsSynthetic=true · no real personal data',

  // Create button
  createButtonLabel: 'Create customer',
  createButtonTitle: 'Create a new synthetic customer (local sandbox)',

  // Search / meta bar
  searchPlaceholder: "Search by name, email, or ID (CUST-T0042)…",
  metaLoading: 'Loading…',
  metaReady: 'Ready to search',

  // Table headers
  colId: 'ID',
  colFullName: 'Full Name',
  colEmail: 'Email',
  colPhone: 'Phone',
  colCustomerSince: 'Customer Since',
  colPriorCases: 'Prior Cases',

  // Empty state
  emptyState: 'No customers found. Try a different search.',

  // Pagination
  paginationBack: '← Back',
  paginationNext: 'Next →',
  paginationPageOf: 'of',
  paginationPageLabel: 'Page',

  // Result count: composed in JSX as `{total} found · page {page}/{totalPages}`
  metaFoundPrefix: 'found · page',
  metaFoundSeparator: '/',

  // Synthetic-data note
  syntheticNoteText:
    'This directory is a local synthetic dataset (rows with IsSynthetic=true). No real personal data is stored. Records may be used when creating a new synthetic claim (via the Create Claim form).',

  // Modal — title / description
  modalTitle: 'Create New Synthetic Customer',
  modalDescription:
    'Creates a new row in customers_policies.SyntheticCustomers with IsSynthetic=true. Local sandbox — no real personal data.',

  // Modal — form labels
  labelFullName: 'Full Name *',
  labelEmail: 'Email (optional)',
  labelPhone: 'Phone (optional)',
  labelAddress: 'Address (optional)',

  // Modal — placeholders (kept as static keys; no template literals)
  placeholderFullName: 'Synthetic Customer Smith',
  placeholderEmail: 'testuser@synthetic.invalid',
  placeholderPhone: '+380501234567',
  placeholderAddress: '123 Demo St, Springfield',

  // Modal — validation errors
  errorNameRequired: 'Full name is required (use synthetic data, no real PII).',
  errorNameTooLong: 'Full name must not exceed 200 characters.',
  errorUnknown: 'Unknown error.',

  // Modal — footer buttons
  cancelButton: 'Cancel',
  submitButtonIdle: 'Create Customer',
  submitButtonBusy: 'Creating…',

  // Modal — ID hint note
  idHintText:
    'ID is assigned by the server (CUST-T0XXX, next after the seeded range). The record is flagged IsSynthetic=true; the UI never writes real PII.',

  // Toast (title only; detail comes from the server)
  toastCreatedPrefix: 'Customer',
  toastCreatedSuffix: 'created.',
};

type T = typeof en;

const uk: T = {
  // Page header
  pageTitle: 'Каталог клієнтів',
  pageSubtitle: 'Локальний каталог синтетичних клієнтів · IsSynthetic=true · без реальних персональних даних',

  // Create button
  createButtonLabel: 'Створити клієнта',
  createButtonTitle: 'Створити нового синтетичного клієнта (локальний sandbox)',

  // Search / meta bar
  searchPlaceholder: "Пошук за ім'ям, email або ID (CUST-T0042)…",
  metaLoading: 'Завантаження…',
  metaReady: 'Готово до пошуку',

  // Table headers
  colId: 'ID',
  colFullName: 'Повне імʼя',
  colEmail: 'Email',
  colPhone: 'Телефон',
  colCustomerSince: 'Клієнт з',
  colPriorCases: 'Попередніх кейсів',

  // Empty state
  emptyState: 'Жодного клієнта не знайдено. Спробуйте інший пошук.',

  // Pagination
  paginationBack: '← Назад',
  paginationNext: 'Далі →',
  paginationPageOf: 'з',
  paginationPageLabel: 'Сторінка',

  // Result count
  metaFoundPrefix: 'знайдено · сторінка',
  metaFoundSeparator: '/',

  // Synthetic-data note
  syntheticNoteText:
    'Цей каталог — локальна синтетична база (rows with IsSynthetic=true). Реальні персональні дані не зберігаються. Записи можна використовувати при створенні нового синтетичного кейсу (через форму «Створити кейс»).',

  // Modal — title / description
  modalTitle: 'Створити нового синтетичного клієнта',
  modalDescription:
    'Створює новий рядок у customers_policies.SyntheticCustomers з IsSynthetic=true. Локальний sandbox — без реальних персональних даних.',

  // Modal — form labels
  labelFullName: 'Повне імʼя *',
  labelEmail: 'Email (опц.)',
  labelPhone: 'Телефон (опц.)',
  labelAddress: 'Адреса (опц.)',

  // Modal — placeholders
  placeholderFullName: 'Synthetic Customer Smith',
  placeholderEmail: 'testuser@synthetic.invalid',
  placeholderPhone: '+380501234567',
  placeholderAddress: 'Київ, вул. Грушевського 5',

  // Modal — validation errors
  errorNameRequired: 'Заповніть повне імʼя (синтетичне, без реальних даних).',
  errorNameTooLong: 'Повне імʼя має бути не довше 200 символів.',
  errorUnknown: 'Невідома помилка.',

  // Modal — footer buttons
  cancelButton: 'Скасувати',
  submitButtonIdle: 'Створити клієнта',
  submitButtonBusy: 'Створення…',

  // Modal — ID hint note
  idHintText:
    'ID присвоюється сервером (CUST-T0XXX, наступний після сидованого діапазону). Запис позначається IsSynthetic=true; UI ніколи не пише реальні PII.',

  // Toast
  toastCreatedPrefix: 'Створено клієнта',
  toastCreatedSuffix: '.',
};

export const customers = { en, uk };
