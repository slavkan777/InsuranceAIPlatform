import type {
  AuditRow,
  CostLine,
  DamagePhoto,
  DocumentChecklistItem,
  ExtractedEntity,
  RiskFactor,
} from '@/types';

export const claimTimeline = [
  { time: '18.05 14:32', event: 'ДТП — зафіксовано клієнтом', tone: 'info' as const },
  { time: '18.05 15:08', event: 'Поліцейський звіт оформлено', tone: 'info' as const },
  { time: '18.05 16:20', event: 'Заява клієнта подана', tone: 'info' as const },
  { time: '19.05 09:15', event: 'Фото пошкоджень завантажено (5 з 6)', tone: 'warn' as const },
  { time: '19.05 11:40', event: 'Рахунок СТО надано', tone: 'info' as const },
  { time: '19.05 14:05', event: 'AI-аналіз завершено · високий ризик', tone: 'danger' as const },
  { time: 'Зараз', event: 'Очікує людську перевірку', tone: 'warn' as const, current: true },
];

export const damagePhotos: DamagePhoto[] = [
  { id: 'front', label: 'Переднє', confidence: 92 },
  { id: 'side', label: 'Бокове', confidence: 87 },
  { id: 'rear', label: 'Задній бампер', missing: true },
];

export const documentsChecklist: DocumentChecklistItem[] = [
  { id: 'application', label: 'Заява клієнта', detail: '19.05.2026', status: 'ok' },
  { id: 'police', label: 'Поліцейський звіт', detail: 'NoБРС-2026/05/441', status: 'ok' },
  { id: 'photo-front', label: 'Фото — переднє', detail: 'AI conf 92%', status: 'ok' },
  { id: 'photo-side', label: 'Фото — бокове', detail: 'AI conf 87%', status: 'ok' },
  { id: 'invoice', label: 'Рахунок СТО', detail: 'Сума +38%', status: 'warn' },
  { id: 'policy-terms', label: 'Умови полісу', detail: 'Auto Comprehensive', status: 'ok' },
  { id: 'photo-rear', label: 'Фото — задній бампер', detail: 'ВІДСУТНЄ', status: 'missing' },
];

export const stoInvoiceLines = [
  { id: 'bumper', label: 'Заміна переднього бампера', value: 1240 },
  { id: 'headlight', label: 'Ремонт фари ліва', value: 380 },
  { id: 'chassis', label: 'Ремонт ходової', value: 680 },
  { id: 'labor', label: 'Робота', value: 420 },
];

export const keyRisks = [
  'Сума ремонту вище очікуваного діапазону',
  'Розбіжності у поясненнях водіїв',
  'Відсутнє фото заднього бампера',
];

export const keyFindings = [
  { text: 'Рахунок СТО перевищує очікуваний діапазон', detail: '+38% від медіани', tone: 'danger' as const },
  { text: 'У поясненнях водіїв є розбіжності', detail: 'час події ±18 хв', tone: 'warn' as const },
  { text: 'Відсутнє фото пошкодження заднього бампера', detail: 'документ обов\'язковий', tone: 'warn' as const },
  { text: 'Покриття за полісом підтверджено', detail: 'Auto Comprehensive', tone: 'good' as const },
];

export const evidenceTabs = [
  'Поліцейський звіт',
  'Фото пошкоджень',
  'Рахунок СТО',
  'Умови полісу',
  'Лист клієнта',
];

export const modelConfidence = [
  { id: 'extract', label: 'Витягування', value: 95 },
  { id: 'coverage', label: 'Покриття', value: 92 },
  { id: 'damage', label: 'Пошкодження', value: 71 },
  { id: 'recommendation', label: 'Рекомендація', value: 78 },
];

export const extractedEntities: ExtractedEntity[] = [
  { field: 'Дата ДТП', value: '18.05.2026', source: 'Поліцейський звіт', confidence: 99 },
  { field: 'Авто', value: 'Toyota Camry 2021', source: 'Поліс', confidence: 98 },
  { field: 'Сума', value: '$2 720', source: 'Рахунок СТО', confidence: 94 },
  { field: 'Поліс', value: 'POL-2025-AC-4421', source: 'Поліс', confidence: 100 },
  { field: 'Заявник', value: 'Роберт Джонсон', source: 'Заява', confidence: 100 },
  { field: 'Локація', value: 'Бориспіль, Київська 24', source: 'Звіт', confidence: 95 },
];

export const riskFactors: RiskFactor[] = [
  { id: 'amount', label: 'Сума ремонту вище очікуваного діапазону', contribution: 25 },
  { id: 'mismatch', label: 'Розбіжності у поясненнях водіїв', contribution: 18 },
  { id: 'missing-photo', label: 'Відсутнє фото пошкодження', contribution: 22 },
  { id: 'prior', label: 'Попередні claims клієнта', contribution: 8 },
  { id: 'confidence', label: 'Confidence нижче порогу 85%', contribution: 9 },
];

export const auditTrail: AuditRow[] = [
  { time: '14:05:12', actor: 'AI Pipeline', action: 'Запуск аналізу CLM-1006', result: 'OK' },
  { time: '14:05:14', actor: 'Doc Classifier', action: 'Класифікація 6 документів', result: 'OK' },
  { time: '14:05:19', actor: 'Field Extractor', action: 'Витягнуто 47 полів', result: 'OK' },
  { time: '14:05:25', actor: 'Risk Engine', action: 'Ризик 82/100 — Високий', result: 'WARN' },
  { time: '14:05:30', actor: 'Recommender', action: 'Рекомендація: запросити фото', result: 'OK' },
  { time: '14:05:31', actor: 'Governance', action: 'Авто-погодження заблоковано', result: 'BLOCK' },
];

export const costDistribution: CostLine[] = [
  { id: 'extract', label: 'Витягування', value: '$0.0072' },
  { id: 'rag', label: 'RAG / докази', value: '$0.0058' },
  { id: 'risk', label: 'Ризик', value: '$0.0029' },
  { id: 'reco', label: 'Рекомендація', value: '$0.0028' },
];

export const aiPipelineSteps = [
  { id: 'classification', label: 'Класифікація документа', status: 'done' as const, duration: '1.2с' },
  { id: 'extraction', label: 'Витягування даних', status: 'done' as const, duration: '4.8с' },
  { id: 'policy', label: 'Перевірка полісу', status: 'done' as const, duration: '2.1с' },
  { id: 'invoice', label: 'Перевірка рахунку СТО', status: 'warn' as const, duration: '3.4с' },
  { id: 'risk', label: 'Оцінка ризику', status: 'risk' as const, duration: '5.2с' },
  { id: 'draft', label: 'Чернетка відповіді', status: 'done' as const, duration: '0.9с' },
  { id: 'human', label: 'Людська перевірка', status: 'pending' as const, duration: 'очікує' },
];

export const policyCoverageBlocks = [
  { id: 'collision', title: 'Зіткнення', limit: '$50 000', deductible: '$500' },
  { id: 'liability', title: 'Відповідальність', limit: '$100 000', deductible: '$0' },
  { id: 'glass', title: 'Скло', limit: '$1 500', deductible: '$100' },
  { id: 'theft', title: 'Викрадення', limit: 'Ринкова', deductible: '$1 000' },
  { id: 'roadside', title: 'Дорожня допомога', limit: '24/7', deductible: '$0' },
];

export const policyValidation = [
  'Покриття підтверджено',
  'ДТП дата у межах періоду',
  'Lapse не виявлено',
  'Зіткнення входить у покриття',
  'Франшиза $500 застосовується',
  'Виключень не виявлено',
];

export const previousClaims = [
  { id: 'CLM-1006', label: 'ДТП — у роботі', date: '18.05.2026', amount: 'Поточний' },
  { id: 'CLM-0789', label: 'Паркувальне', date: '04.11.2024', amount: '$340 виплачено' },
  { id: 'CLM-0512', label: 'Скло', date: '22.03.2023', amount: '$180 виплачено' },
];

export const communicationHistory = [
  { channel: 'Email', topic: 'Запит фото', when: '19.05 15:22' },
  { channel: 'Чат', topic: 'Рахунок СТО', when: '19.05 11:40' },
  { channel: 'Телефон', topic: 'Перевірка по полісу', when: '18.05 18:15' },
  { channel: 'Web', topic: 'Заявка про ДТП', when: '18.05 16:20' },
];

export const decisionOptions = [
  {
    id: 'approve',
    title: 'Погодити виплату',
    caption: 'Якщо ризики прийнятні',
    tone: 'good' as const,
  },
  {
    id: 'request',
    title: 'Запросити дані',
    caption: 'Рекомендовано AI',
    tone: 'info' as const,
    recommended: true,
  },
  {
    id: 'reject',
    title: 'Відхилити',
    caption: 'З обґрунтуванням',
    tone: 'danger' as const,
  },
  {
    id: 'escalate',
    title: 'Передати старшому',
    caption: 'Ескалація',
    tone: 'warn' as const,
  },
];

export const approvalChecklist = [
  { id: 'coverage', label: 'Покриття перевірено', status: 'ok' as const },
  { id: 'docs-reviewed', label: 'Документи переглянуто', status: 'ok' as const },
  { id: 'risk', label: 'Ризики усвідомлено', status: 'ok' as const },
  { id: 'docs-missing', label: 'Відсутні док-ти', status: 'warn' as const },
  { id: 'amount', label: 'Сума узгоджена', status: 'pending' as const },
  { id: 'expert', label: 'Експерт підтвердив', status: 'pending' as const },
];

export const demoSteps = [
  { step: 1, title: 'Огляд', caption: 'Стан черги ДТП', pdfRef: 'Дошка 01', route: '/' },
  { step: 2, title: 'Обрати CLM-1006', caption: 'Toyota Camry', pdfRef: 'Дошка 03', route: '/claims/CLM-1006' },
  { step: 3, title: 'Документи та фото', caption: '6/7 + відсутнє', pdfRef: 'Дошка 04', route: '/claims/CLM-1006/documents' },
  { step: 4, title: 'AI-докази', caption: '4 знахідки + RAG', pdfRef: 'Дошка 05', route: '/claims/CLM-1006/ai-evidence' },
  { step: 5, title: 'Оцінка ризиків', caption: '82/100 Високий', pdfRef: 'Дошка 06', route: '/claims/CLM-1006/risks' },
  { step: 6, title: 'Людське рішення', caption: 'Експерт обирає', pdfRef: 'Дошка 07', route: '/claims/CLM-1006/approval' },
  { step: 7, title: 'Audit & Cost', caption: 'Trace + governance', pdfRef: 'Дошка 08', route: '/claims/CLM-1006/audit' },
];
