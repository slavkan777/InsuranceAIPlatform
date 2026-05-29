// Claim shell breadcrumb + sub-page tab navigation.
const en = {
  backToList: '← Back to claims list',
  tabOverview: 'Workspace',
  tabDocuments: 'Documents & photos',
  tabAiEvidence: 'AI evidence',
  tabRisks: 'Risks',
  tabApproval: 'Approval',
  tabAudit: 'Audit & Cost',
  tabPolicy: 'Policy',
  tabCustomerVehicle: 'Customer & vehicle',
};
type T = typeof en;
const uk: T = {
  backToList: '← Повернутись до списку',
  tabOverview: 'Робоче місце',
  tabDocuments: 'Документи та фото',
  tabAiEvidence: 'AI-докази',
  tabRisks: 'Ризики',
  tabApproval: 'Погодження',
  tabAudit: 'Аудит і витрати',
  tabPolicy: 'Поліс',
  tabCustomerVehicle: 'Клієнт і ТЗ',
};
export const claimShell = { en, uk };
