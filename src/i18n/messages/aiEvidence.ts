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

export const aiEvidence = { en };
