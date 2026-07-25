import type {
  AuditRow,
  CostLine,
  DamagePhoto,
  DocumentChecklistItem,
  ExtractedEntity,
  RiskFactor,
} from '@/types';

export const claimTimeline = [
  { time: '18.05 14:32', event: 'Road accident — reported by customer', tone: 'info' as const },
  { time: '18.05 15:08', event: 'Police report filed', tone: 'info' as const },
  { time: '18.05 16:20', event: 'Customer statement submitted', tone: 'info' as const },
  { time: '19.05 09:15', event: 'Damage photos uploaded (5 of 6)', tone: 'warn' as const },
  { time: '19.05 11:40', event: 'Repair invoice provided', tone: 'info' as const },
  { time: '19.05 14:05', event: 'AI analysis completed · high risk', tone: 'danger' as const },
  { time: 'Now', event: 'Awaiting human review', tone: 'warn' as const, current: true },
];

export const damagePhotos: DamagePhoto[] = [
  { id: 'front', label: 'Front', confidence: 92 },
  { id: 'side', label: 'Side', confidence: 87 },
  { id: 'rear', label: 'Rear bumper', missing: true },
];

export const documentsChecklist: DocumentChecklistItem[] = [
  { id: 'application', label: 'Customer statement', detail: '19.05.2026', status: 'ok' },
  { id: 'police', label: 'Police report', detail: 'No. PR-2026/05/441', status: 'ok' },
  { id: 'photo-front', label: 'Photo — front', detail: 'AI conf 92%', status: 'ok' },
  { id: 'photo-side', label: 'Photo — side', detail: 'AI conf 87%', status: 'ok' },
  { id: 'invoice', label: 'Repair invoice', detail: 'Amount +38%', status: 'warn' },
  { id: 'policy-terms', label: 'Policy terms', detail: 'Auto Comprehensive', status: 'ok' },
  { id: 'photo-rear', label: 'Photo — rear bumper', detail: 'MISSING', status: 'missing' },
];

export const stoInvoiceLines = [
  { id: 'bumper', label: 'Front bumper replacement', value: 1240 },
  { id: 'headlight', label: 'Left headlight repair', value: 380 },
  { id: 'chassis', label: 'Suspension repair', value: 680 },
  { id: 'labor', label: 'Labour', value: 420 },
];

export const keyRisks = [
  'Repair amount above the expected range',
  'Discrepancies between the driver statements',
  'Rear bumper photo missing',
];

export const keyFindings = [
  { text: 'Repair invoice exceeds the expected range', detail: '+38% vs median', tone: 'danger' as const },
  { text: 'Driver statements contain discrepancies', detail: 'event time ±18 min', tone: 'warn' as const },
  { text: 'Rear bumper damage photo is missing', detail: 'document required', tone: 'warn' as const },
  { text: 'Policy coverage confirmed', detail: 'Auto Comprehensive', tone: 'good' as const },
];

export const evidenceTabs = [
  'Police report',
  'Damage photos',
  'Repair invoice',
  'Policy terms',
  'Customer letter',
];

export const modelConfidence = [
  { id: 'extract', label: 'Extraction', value: 95 },
  { id: 'coverage', label: 'Coverage', value: 92 },
  { id: 'damage', label: 'Damage', value: 71 },
  { id: 'recommendation', label: 'Recommendation', value: 78 },
];

export const extractedEntities: ExtractedEntity[] = [
  { field: 'Accident date', value: '18.05.2026', source: 'Police report', confidence: 99 },
  { field: 'Vehicle', value: 'Toyota Camry 2021', source: 'Policy', confidence: 98 },
  { field: 'Amount', value: '$2,720', source: 'Repair invoice', confidence: 94 },
  { field: 'Policy', value: 'POL-2025-AC-4421', source: 'Policy', confidence: 100 },
  { field: 'Claimant', value: 'Robert Johnson', source: 'Statement', confidence: 100 },
  { field: 'Location', value: 'Springfield, Main Street 24', source: 'Report', confidence: 95 },
];

export const riskFactors: RiskFactor[] = [
  { id: 'amount', label: 'Repair amount above the expected range', contribution: 25 },
  { id: 'mismatch', label: 'Discrepancies between driver statements', contribution: 18 },
  { id: 'missing-photo', label: 'Damage photo missing', contribution: 22 },
  { id: 'prior', label: 'Customer prior claims', contribution: 8 },
  { id: 'confidence', label: 'Confidence below the 85% threshold', contribution: 9 },
];

export const auditTrail: AuditRow[] = [
  { time: '14:05:12', actor: 'AI Pipeline', action: 'Analysis started for CLM-1006', result: 'OK' },
  { time: '14:05:14', actor: 'Doc Classifier', action: 'Classified 6 documents', result: 'OK' },
  { time: '14:05:19', actor: 'Field Extractor', action: 'Extracted 47 fields', result: 'OK' },
  { time: '14:05:25', actor: 'Risk Engine', action: 'Risk 82/100 — High', result: 'WARN' },
  { time: '14:05:30', actor: 'Recommender', action: 'Recommendation: request photo', result: 'OK' },
  { time: '14:05:31', actor: 'Governance', action: 'Auto-approval blocked', result: 'BLOCK' },
];

export const costDistribution: CostLine[] = [
  { id: 'extract', label: 'Extraction', value: '$0.0072' },
  { id: 'rag', label: 'RAG / evidence', value: '$0.0058' },
  { id: 'risk', label: 'Risk', value: '$0.0029' },
  { id: 'reco', label: 'Recommendation', value: '$0.0028' },
];

export const aiPipelineSteps = [
  { id: 'classification', label: 'Document classification', status: 'done' as const, duration: '1.2s' },
  { id: 'extraction', label: 'Data extraction', status: 'done' as const, duration: '4.8s' },
  { id: 'policy', label: 'Policy verification', status: 'done' as const, duration: '2.1s' },
  { id: 'invoice', label: 'Repair invoice check', status: 'warn' as const, duration: '3.4s' },
  { id: 'risk', label: 'Risk assessment', status: 'risk' as const, duration: '5.2s' },
  { id: 'draft', label: 'Draft response', status: 'done' as const, duration: '0.9s' },
  { id: 'human', label: 'Human review', status: 'pending' as const, duration: 'pending' },
];

export const policyCoverageBlocks = [
  { id: 'collision', title: 'Collision', limit: '$50,000', deductible: '$500' },
  { id: 'liability', title: 'Liability', limit: '$100,000', deductible: '$0' },
  { id: 'glass', title: 'Glass', limit: '$1,500', deductible: '$100' },
  { id: 'theft', title: 'Theft', limit: 'Market value', deductible: '$1,000' },
  { id: 'roadside', title: 'Roadside assistance', limit: '24/7', deductible: '$0' },
];

export const policyValidation = [
  'Coverage confirmed',
  'Accident date within the policy period',
  'No lapse detected',
  'Collision is covered',
  '$500 deductible applies',
  'No exclusions found',
];

export const previousClaims = [
  { id: 'CLM-1006', label: 'Road accident — in progress', date: '18.05.2026', amount: 'Current' },
  { id: 'CLM-0789', label: 'Parking', date: '04.11.2024', amount: '$340 paid' },
  { id: 'CLM-0512', label: 'Glass', date: '22.03.2023', amount: '$180 paid' },
];

export const communicationHistory = [
  { channel: 'Email', topic: 'Photo request', when: '19.05 15:22' },
  { channel: 'Chat', topic: 'Repair invoice', when: '19.05 11:40' },
  { channel: 'Phone', topic: 'Policy verification', when: '18.05 18:15' },
  { channel: 'Web', topic: 'Accident notification', when: '18.05 16:20' },
];

export const decisionOptions = [
  {
    id: 'approve',
    title: 'Approve payout',
    caption: 'If the risks are acceptable',
    tone: 'good' as const,
  },
  {
    id: 'request',
    title: 'Request information',
    caption: 'AI recommended',
    tone: 'info' as const,
    recommended: true,
  },
  {
    id: 'reject',
    title: 'Reject',
    caption: 'With written justification',
    tone: 'danger' as const,
  },
  {
    id: 'escalate',
    title: 'Escalate to senior adjuster',
    caption: 'Escalation',
    tone: 'warn' as const,
  },
];

export const approvalChecklist = [
  { id: 'coverage', label: 'Coverage verified', status: 'ok' as const },
  { id: 'docs-reviewed', label: 'Documents reviewed', status: 'ok' as const },
  { id: 'risk', label: 'Risks acknowledged', status: 'ok' as const },
  { id: 'docs-missing', label: 'Missing documents', status: 'warn' as const },
  { id: 'amount', label: 'Amount agreed', status: 'pending' as const },
  { id: 'expert', label: 'Expert confirmed', status: 'pending' as const },
];

export const demoSteps = [
  { step: 1, title: 'Overview', caption: 'Claims queue status', pdfRef: 'Board 01', route: '/' },
  { step: 2, title: 'Select CLM-1006', caption: 'Toyota Camry', pdfRef: 'Board 03', route: '/claims/CLM-1006' },
  { step: 3, title: 'Documents and photos', caption: '6/7 + one missing', pdfRef: 'Board 04', route: '/claims/CLM-1006/documents' },
  { step: 4, title: 'AI evidence', caption: '4 findings + RAG', pdfRef: 'Board 05', route: '/claims/CLM-1006/ai-evidence' },
  { step: 5, title: 'Risk assessment', caption: '82/100 High', pdfRef: 'Board 06', route: '/claims/CLM-1006/risks' },
  { step: 6, title: 'Human decision', caption: 'Expert decides', pdfRef: 'Board 07', route: '/claims/CLM-1006/approval' },
  { step: 7, title: 'Audit & Cost', caption: 'Trace + governance', pdfRef: 'Board 08', route: '/claims/CLM-1006/audit' },
];
