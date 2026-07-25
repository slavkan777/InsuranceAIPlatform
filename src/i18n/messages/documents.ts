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

export const documents = { en };
