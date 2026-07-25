import type { IconName } from '@/components/ui/Icon';

type Tone = 'info' | 'good' | 'warn' | 'danger' | 'ai';

interface Metric {
  id: string;
  label: string;
  value: string;
  delta: string;
  tone: Tone;
  icon?: IconName;
}

export const overviewMetrics: Metric[] = [
  { id: 'new', label: 'NEW ROAD ACCIDENTS', value: '8', delta: '+2 today', tone: 'info', icon: 'car' },
  { id: 'wait-doc', label: 'AWAITING DOCUMENTS', value: '12', delta: '+3 today', tone: 'warn', icon: 'file' },
  { id: 'ai-today', label: 'AI PROCESSED TODAY', value: '48', delta: '+18%', tone: 'ai', icon: 'cpu' },
  { id: 'high-risk', label: 'HIGH RISK', value: '7', delta: '+2 new', tone: 'danger', icon: 'shield' },
  { id: 'avg-time', label: 'AVERAGE HANDLING TIME', value: '18m', delta: '-12%', tone: 'good', icon: 'clock' },
];

interface Phase {
  id: string;
  label: string;
  count: number;
  icon: IconName;
}

export const lifecyclePhases: Phase[] = [
  { id: 'reg', label: 'Accident registration', count: 8, icon: 'clipboard' },
  { id: 'docs', label: 'Document collection', count: 12, icon: 'folder' },
  { id: 'ai', label: 'AI analysis', count: 14, icon: 'cpu' },
  { id: 'risk', label: 'Risk assessment', count: 7, icon: 'gauge' },
  { id: 'human', label: 'Human decision', count: 5, icon: 'userCheck' },
  { id: 'done', label: 'Completion', count: 21, icon: 'checkCircle' },
];

export const claimsListMetrics: Metric[] = [
  { id: 'today', label: 'IN PROGRESS TODAY', value: '24', delta: '+5 since morning', tone: 'info' },
  { id: 'sla', label: 'BREACHED SLA', value: '2', delta: 'critical', tone: 'danger' },
  { id: 'high-risk', label: 'HIGH RISK', value: '7', delta: '+2 new', tone: 'warn' },
  { id: 'human', label: 'AWAITING HUMAN', value: '5', delta: 'average SLA', tone: 'good' },
];

// Dashboard-level aggregate telemetry (synthetic demo totals for "today")
export const auditToday = [
  { id: 'cases', label: 'Claims processed', value: '48', delta: '+18%' },
  { id: 'tokens', label: 'Tokens spent', value: '128K', delta: '+12%' },
  { id: 'cost', label: 'Cost', value: '$6.24', delta: '+9%' },
  { id: 'latency', label: 'Average latency', value: '2.1s', delta: '-8%' },
];

export const recentEvents = [
  { id: 'e1', time: '22:45', text: 'AI analysis completed for CLM-1006', tone: 'ai' as const },
  { id: 'e2', time: '22:42', text: 'New documents uploaded for CLM-1007', tone: 'info' as const },
  { id: 'e3', time: '22:38', text: 'Risk raised for CLM-1006', tone: 'danger' as const },
  { id: 'e4', time: '22:30', text: 'CLM-1008 ready for approval', tone: 'good' as const },
];

export const caseTypeBreakdown = [
  { label: 'Road accident', value: 28, pct: '53%', color: '#2563eb' },
  { label: 'Parking', value: 12, pct: '23%', color: '#6366f1' },
  { label: 'Collision', value: 8, pct: '15%', color: '#f59e0b' },
  { label: 'Damage', value: 5, pct: '9%', color: '#bcc4d6' },
];

export const confidenceDistribution = [
  { label: '0–40%', value: 3, color: '#ef4444' },
  { label: '40–60%', value: 8, color: '#f59e0b' },
  { label: '60–80%', value: 18, color: '#6366f1' },
  { label: '80–100%', value: 19, color: '#10b981' },
];

export const processingTrend = {
  labels: ['18 May', '19 May', '20 May', '21 May', '22 May', '23 May', '24 May'],
  series: [
    { name: 'Processed', color: '#2563eb', points: [22, 28, 26, 34, 30, 33, 38] },
    { name: 'New', color: '#10b981', points: [12, 14, 11, 16, 13, 15, 12] },
  ],
};
