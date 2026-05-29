// Policy & coverage page strings.
const en = {
  pageTitle: 'Policy & Coverage',
  statusActive: 'Active · expires in 220 days',

  // Policy detail card
  sectionPolicyLabel: 'Policy',
  sectionPolicyValidity: 'valid 01.01.2026 — 31.12.2026',
  sectionExpiryLabel: 'Expires in',
  sectionExpiryValue: '220 days',

  // Coverage cards
  coverageLimitLabel: 'Limit',
  coverageDeductibleLabel: 'Deductible',

  // Limits & deductible section
  limitsTitle: 'Limits & Deductible',
  limitTotal: 'Total limit',
  limitPerIncident: 'Per-incident limit',
  limitBaseDeductible: 'Base deductible',
  limitBonusMalus: 'Bonus-malus',

  // Exclusions section
  exclusionsTitle: 'Exclusions',
  exclusion1: 'Driving under the influence',
  exclusion2: 'Racing',
  exclusion3: 'Acts of war',

  // Policy validation section
  validationTitle: 'Policy Validation',

  // Owner card
  ownerLabel: 'Policy Holder',
  ownerSince: 'Customer since 2021',

  // Vehicle card
  vehicleLabel: 'Vehicle',
  vehicleInsuredLabel: 'insured',
};

type T = typeof en;

const uk: T = {
  pageTitle: 'Поліс і покриття',
  statusActive: 'Активний · до закінчення 220 днів',

  // Policy detail card
  sectionPolicyLabel: 'Поліс',
  sectionPolicyValidity: 'дійсний 01.01.2026 — 31.12.2026',
  sectionExpiryLabel: 'До закінчення',
  sectionExpiryValue: '220 днів',

  // Coverage cards
  coverageLimitLabel: 'Ліміт',
  coverageDeductibleLabel: 'Франшиза',

  // Limits & deductible section
  limitsTitle: 'Ліміти та франшиза',
  limitTotal: 'Загальний ліміт',
  limitPerIncident: 'Ліміт на ДТП',
  limitBaseDeductible: 'Базова франшиза',
  limitBonusMalus: 'Бонус-малус',

  // Exclusions section
  exclusionsTitle: 'Виключення',
  exclusion1: 'Стан сп\'яніння',
  exclusion2: 'Гонки',
  exclusion3: 'Військові дії',

  // Policy validation section
  validationTitle: 'Валідація полісу',

  // Owner card
  ownerLabel: 'Власник',
  ownerSince: 'Клієнт з 2021',

  // Vehicle card
  vehicleLabel: 'Транспорт',
  vehicleInsuredLabel: 'застрах.',
};

export const policy = { en, uk };
