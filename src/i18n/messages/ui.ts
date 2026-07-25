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

export const ui = { en };
