// Landing / sign-in screen — product hero + form + demo access note.
const en = {
  heroEyebrow: 'Insurance claims operations',
  heroTitle: 'AI-Assisted Insurance Claims Workbench',
  heroSubtitle:
    'Help claim managers review evidence, prioritize cases, detect risk signals, and prepare auditable claim decisions faster.',
  valueBullets: [
    'Centralized claim intake and review workflow',
    'AI-assisted document classification and field extraction',
    'Risk scoring and recommendation support',
    'Human-in-the-loop decisions with audit-ready evidence',
    'Live operational dashboard for claim teams',
  ],
  formTitle: 'Sign in',
  formSubtitle: 'Access the claims operations workbench.',
  emailLabel: 'Email',
  passwordLabel: 'Password',
  signInCta: 'Sign in',
  demoHintTitle: 'Demo access',
  demoLoginLabel: 'Email',
  demoPasswordLabel: 'Password',
  demoNote:
    'Demo environment: seeded claim data and a mock AI provider, with live API integration. No external identity provider and no real personal data — your session is stored in your browser.',
  errorInvalid: 'Incorrect email or password. Use the demo access hint below.',
};
type T = typeof en;
const uk: T = {
  heroEyebrow: 'Операції зі страховими випадками',
  heroTitle: 'AI-помічник для обробки страхових випадків',
  heroSubtitle:
    'Допомагає менеджерам зі страхування швидше перевіряти докази, пріоритезувати випадки, виявляти ризики та готувати прозорі, аудитовані рішення за випадком.',
  valueBullets: [
    'Централізований процес приймання та перевірки випадків',
    'AI-допомога з класифікації документів і витягування полів',
    'Оцінка ризику та підтримка рекомендацій',
    'Рішення з людиною в контурі та аудитованими доказами',
    'Операційний dashboard для страхових команд',
  ],
  formTitle: 'Вхід',
  formSubtitle: 'Доступ до робочого місця операцій зі страхування.',
  emailLabel: 'Email',
  passwordLabel: 'Пароль',
  signInCta: 'Увійти',
  demoHintTitle: 'Демо-доступ',
  demoLoginLabel: 'Email',
  demoPasswordLabel: 'Пароль',
  demoNote:
    'Демо-середовище: seeded дані випадків і mock AI-провайдер, із живою інтеграцією API. Без зовнішнього провайдера ідентичності та без реальних персональних даних — сесія зберігається у вашому браузері.',
  errorInvalid: 'Невірний email або пароль. Скористайтесь підказкою демо-доступу нижче.',
};
export const login = { en, uk };
