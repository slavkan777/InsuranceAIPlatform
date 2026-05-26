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
  { id: 'new', label: 'НОВІ ДТП', value: '8', delta: '+2 сьогодні', tone: 'info', icon: 'car' },
  { id: 'wait-doc', label: 'ОЧІКУЮТЬ ДОКУМЕНТІВ', value: '12', delta: '+3 сьогодні', tone: 'warn', icon: 'file' },
  { id: 'ai-today', label: 'AI-ОБРОБЛЕНО СЬОГОДНІ', value: '48', delta: '+18%', tone: 'ai', icon: 'cpu' },
  { id: 'high-risk', label: 'ВИСОКИЙ РИЗИК', value: '7', delta: '+2 нових', tone: 'danger', icon: 'shield' },
  { id: 'avg-time', label: 'СЕРЕДНІЙ ЧАС РОЗГЛЯДУ', value: '18 хв', delta: '-12%', tone: 'good', icon: 'clock' },
];

interface Phase {
  id: string;
  label: string;
  count: number;
  icon: IconName;
}

export const lifecyclePhases: Phase[] = [
  { id: 'reg', label: 'Реєстрація ДТП', count: 8, icon: 'clipboard' },
  { id: 'docs', label: 'Збір документів', count: 12, icon: 'folder' },
  { id: 'ai', label: 'AI-аналіз', count: 14, icon: 'cpu' },
  { id: 'risk', label: 'Перевірка ризиків', count: 7, icon: 'gauge' },
  { id: 'human', label: 'Людське рішення', count: 5, icon: 'userCheck' },
  { id: 'done', label: 'Завершення', count: 21, icon: 'checkCircle' },
];

export const claimsListMetrics: Metric[] = [
  { id: 'today', label: 'СЬОГОДНІ В РОБОТІ', value: '24', delta: '+5 з ранку', tone: 'info' },
  { id: 'sla', label: 'ПРОСТРОЧЕНІ SLA', value: '2', delta: 'критично', tone: 'danger' },
  { id: 'high-risk', label: 'ВИСОКИЙ РИЗИК', value: '7', delta: '+2 нових', tone: 'warn' },
  { id: 'human', label: 'ОЧІКУЮТЬ ЛЮДИНУ', value: '5', delta: 'середній SLA', tone: 'good' },
];

// Dashboard-level aggregate telemetry (synthetic demo totals for "today")
export const auditToday = [
  { id: 'cases', label: 'Оброблено випадків', value: '48', delta: '+18%' },
  { id: 'tokens', label: 'Витрачено токенів', value: '128K', delta: '+12%' },
  { id: 'cost', label: 'Вартість', value: '$6.24', delta: '+9%' },
  { id: 'latency', label: 'Середня затримка', value: '2.1с', delta: '-8%' },
];

export const recentEvents = [
  { id: 'e1', time: '22:45', text: 'AI-аналіз завершено для CLM-1006', tone: 'ai' as const },
  { id: 'e2', time: '22:42', text: 'Нові документи завантажено для CLM-1007', tone: 'info' as const },
  { id: 'e3', time: '22:38', text: 'Ризик підвищено для CLM-1006', tone: 'danger' as const },
  { id: 'e4', time: '22:30', text: 'CLM-1008 готовий до погодження', tone: 'good' as const },
];

export const caseTypeBreakdown = [
  { label: 'ДТП', value: 28, pct: '53%', color: '#2563eb' },
  { label: 'Паркування', value: 12, pct: '23%', color: '#6366f1' },
  { label: 'Зіткнення', value: 8, pct: '15%', color: '#f59e0b' },
  { label: 'Пошкодження', value: 5, pct: '9%', color: '#bcc4d6' },
];

export const confidenceDistribution = [
  { label: '0–40%', value: 3, color: '#ef4444' },
  { label: '40–60%', value: 8, color: '#f59e0b' },
  { label: '60–80%', value: 18, color: '#6366f1' },
  { label: '80–100%', value: 19, color: '#10b981' },
];

export const processingTrend = {
  labels: ['18 Тра', '19 Тра', '20 Тра', '21 Тра', '22 Тра', '23 Тра', '24 Тра'],
  series: [
    { name: 'Оброблено', color: '#2563eb', points: [22, 28, 26, 34, 30, 33, 38] },
    { name: 'Нові', color: '#10b981', points: [12, 14, 11, 16, 13, 15, 12] },
  ],
};
