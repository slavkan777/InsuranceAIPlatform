// Auto-insurance claims overview dashboard namespace.
// EN = the product language. The product is English-only.
// `const uk: T` enforces compile-time key parity with `en`.
const en = {
  // Page header
  overviewTitle: 'Auto Insurance Claims Overview',
  overviewSubtitle: 'Operations Dashboard · As of 24 May 2026, 22:48',

  // Period / export toolbar
  periodToday: 'Today',
  periodTodayHint: 'Period filter will be available in the next release',
  period7Days: '7 days',
  period7DaysHint: 'Period filter will be available in the next release',
  exportCsvLabel: 'Export CSV',
  exportCsvTitle: 'Export queue overview to CSV (local)',

  // Metric cards — store-derived labels (used when summaryFromStore is present)
  metricNewClaims: 'NEW INCIDENTS',
  metricNewClaimsDelta: 'AI runs',
  metricAwaitingDecision: 'AWAITING DECISION',
  metricAwaitingDecisionDelta: 'under review',
  metricAiProcessedToday: 'AI-PROCESSED TODAY',
  metricAiProcessedTodayDelta: 'now',
  metricHighRisk: 'HIGH RISK',
  metricHighRiskDelta: 'active',
  metricAvgSla: 'AVERAGE SLA TIME',
  metricAvgSlaDeltaSuffix: 'h',
  metricAvgSlaDelta: 'remaining',

  // Lifecycle chart section
  lifecycleTitle: 'Auto Insurance Claim Lifecycle',
  lifecycleSubtitle: 'Distribution of active claims by phase',
  lifecycleActiveChip: 'active',

  // Claims queue section
  claimsQueueTitle: 'Auto Insurance Claims Queue',
  claimsQueueSubtitleActive: 'active',
  claimsQueueSubtitleSuffix: ' · updated every minute',
  claimsQueueActiveDefault: '53 active',
  newClaimButton: 'Create Claim',
  newClaimButtonTitle: 'Create a new synthetic case (local sandbox)',

  // Queue filter tabs
  filterAll: 'All',
  filterRta: 'RTA',
  filterHighRisk: 'High risk',
  filterAwaitAi: 'Awaiting AI',
  filterAwaitDecision: 'Awaiting decision',
  filterTabsTitle: 'Filters are active in the "Auto Insurance Claims" section',

  // Table headers
  thClaimNo: 'Claim No.',
  thCustomerVehicle: 'Customer · Vehicle',
  thEventType: 'Event type',
  thDocuments: 'Documents',
  thAiStatus: 'AI status',
  thRisk: 'Risk',
  thNextAction: 'Next action',
  thUpdated: 'Updated',

  // View-all link
  viewAllClaims: 'View all claims',

  // AI recommendation card
  aiRecTitle: 'AI recommendation for',
  aiRecPayoutLabel: 'Estimated payout',
  aiRecConfidenceLabel: 'Confidence',
  aiRecPill: 'Recommendation',
  aiRecAdvisory: 'human review required',
  aiRecBody: 'Request additional photo of rear bumper damage before approving payout.',
  aiRecKeyFactors: 'Key factors',
  aiRecViewButton: 'View AI analysis',

  // Audit & cost card
  auditTitle: 'Audit & Cost (today)',
  auditViewDetails: 'View details',

  // Recent events card
  recentEventsTitle: 'Recent Events',
  recentEventsViewAudit: 'View audit log',

  // Chart sections
  chartCaseTypeTitle: 'Claims by event type',
  chartCaseTypeSubtitle: 'last 7 days',
  chartConfidenceTitle: 'AI confidence (distribution)',
  chartConfidenceSubtitle: 'today',
  chartConfidenceSubtitleSuffix: '= 78%',
  chartTrendTitle: 'Processing trend',
  chartTrendSubtitle: 'last 7 days',

  // Toast (export)
  toastExportTitle: 'Exported 5 rows.',
  toastExportDetailPrefix: 'File ',
  toastExportDetailSuffix: ' saved to browser downloads.',
};

export const dashboard = { en };
