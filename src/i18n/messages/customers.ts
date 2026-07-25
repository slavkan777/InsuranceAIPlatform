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

export const customers = { en };
