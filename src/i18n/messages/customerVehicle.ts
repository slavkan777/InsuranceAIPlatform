// Customer & vehicle detail page — client card, vehicle card, prior claims,
// communication history, related policies, customer documents, privacy note.
const en = {
  // Page header
  pageTitle: 'Customer & Vehicle',
  pageSubtitleContext: 'context for',
  activeCustomerPill: 'Active customer',

  // Customer card
  customerSince: 'Customer since 2021',
  customerPhone: 'Phone',
  customerEmail: 'Email',
  customerAddress: 'Address',
  customerRiskProfile: 'Risk profile',
  customerPolicies: 'Policies',
  customerRiskValue: 'Moderate (62/100)',
  customerAddressValue: 'Boryspil, Ukraine',
  customerPoliciesValue: '1 active',

  // Vehicle card
  vehicleSubtitle: 'Sedan',
  vehicleInsured: 'Insured.',
  vehicleMileage: 'Mileage',
  vehicleColor: 'Color',
  vehicleRegistration: 'Registration',
  vehicleInsuredValue: 'Insured value',
  vehicleRiskCategory: 'Risk category',
  vehicleMileageValue: '47,200 km',
  vehicleColorValue: 'Silver',
  vehicleRegistrationValue: '2021',
  vehicleInsuredValueValue: '$24,800',
  vehicleRiskCategoryValue: 'Low',

  // Prior claims section
  priorClaimsTitle: 'Prior claims',
  priorClaimsEmpty: 'No prior claims',

  // Communication history section
  communicationHistoryTitle: 'Communication history',

  // Related policies section
  relatedPoliciesTitle: 'Related policies',
  policyValidThrough: 'valid through 31.12.2026',
  policyDetailsLink: '→ Details',

  // Customer documents section
  customerDocumentsTitle: 'Customer documents',
  customerDocumentsCount: '12 documents',
  customerDocumentsTypes: 'passport, driving licence, registration, claims',
  customerDocumentsLastUpdated: 'Last updated:',
  customerDocumentsDate: '19.05.2026',

  // Privacy / demo note
  privacyLabel: 'Privacy · Demo',
  privacyNote: 'Data is synthetic for demo purposes. No real customer PII.',
  piiMaskedPill: 'PII MASKED',
};

type T = typeof en;

const uk: T = {
  // Page header
  pageTitle: 'Клієнт і транспортний засіб',
  pageSubtitleContext: 'контекст для',
  activeCustomerPill: 'Активний клієнт',

  // Customer card
  customerSince: 'Клієнт з 2021',
  customerPhone: 'Телефон',
  customerEmail: 'Email',
  customerAddress: 'Адреса',
  customerRiskProfile: 'Ризик-профіль',
  customerPolicies: 'Поліси',
  customerRiskValue: 'Середній (62/100)',
  customerAddressValue: 'Бориспіль, Україна',
  customerPoliciesValue: '1 активний',

  // Vehicle card
  vehicleSubtitle: 'Седан',
  vehicleInsured: 'Застрахован.',
  vehicleMileage: 'Пробіг',
  vehicleColor: 'Колір',
  vehicleRegistration: 'Реєстрація',
  vehicleInsuredValue: 'Застрах. вартість',
  vehicleRiskCategory: 'Категорія ризику',
  vehicleMileageValue: '47 200 км',
  vehicleColorValue: 'Сріблястий',
  vehicleRegistrationValue: '2021',
  vehicleInsuredValueValue: '$24 800',
  vehicleRiskCategoryValue: 'Низька',

  // Prior claims section
  priorClaimsTitle: 'Попередні випадки',
  priorClaimsEmpty: 'Попередніх випадків немає',

  // Communication history section
  communicationHistoryTitle: 'Історія комунікації',

  // Related policies section
  relatedPoliciesTitle: "Пов'язані поліси",
  policyValidThrough: 'до 31.12.2026',
  policyDetailsLink: '→ Деталі',

  // Customer documents section
  customerDocumentsTitle: 'Документи клієнта',
  customerDocumentsCount: '12 документів',
  customerDocumentsTypes: 'паспорт, посвідчення, реєстрація, заяви',
  customerDocumentsLastUpdated: 'Останнє оновлення:',
  customerDocumentsDate: '19.05.2026',

  // Privacy / demo note
  privacyLabel: 'Privacy · Demo',
  privacyNote: 'Дані синтетичні для demo. Жодних реальних PII клієнта.',
  piiMaskedPill: 'PII МАСКОВАНІ',
};

export const customerVehicle = { en, uk };
