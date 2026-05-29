// Top header bar — search, status badges, walkthrough/help/notifications, profile, sign-out.
const en = {
  searchPlaceholder: 'Search by claim, vehicle, or customer… (Enter)',
  searchTitle: 'Press Enter to search within the claims list',
  systemReady: 'System ready',
  envBadge: 'Demo environment',
  runWalkthrough: 'Guided walkthrough',
  stopWalkthrough: 'Stop walkthrough',
  helpDisabledTitle: 'Help arrives in a future release',
  helpAria: 'Help — not yet available',
  notifDisabledTitle: 'Notification center arrives in a future release',
  notifAria: 'Notifications — not yet available',
  logoutTitle: 'Sign out of the demo session',
  logoutAria: 'Sign out of the demo session',
  logout: 'Sign out',
  toastLogoutTitle: 'Demo session ended.',
  toastLogoutDetail: 'Local token cleared.',
  languageAria: 'Select language',
};
type T = typeof en;
const uk: T = {
  searchPlaceholder: 'Пошук за номером, авто або клієнтом… (Enter)',
  searchTitle: 'Натисніть Enter — пошук виконається у списку випадків',
  systemReady: 'Система готова',
  envBadge: 'Демо-середовище',
  runWalkthrough: 'Огляд можливостей',
  stopWalkthrough: 'Зупинити огляд',
  helpDisabledTitle: 'Довідка з’явиться в наступному релізі',
  helpAria: 'Довідка — поки що недоступна',
  notifDisabledTitle: 'Центр сповіщень з’явиться в наступному релізі',
  notifAria: 'Сповіщення — поки що недоступні',
  logoutTitle: 'Вийти з демо-сесії',
  logoutAria: 'Вийти з демо-сесії',
  logout: 'Вихід',
  toastLogoutTitle: 'Сесію демо-доступу завершено.',
  toastLogoutDetail: 'Локальний токен очищено.',
  languageAria: 'Вибір мови',
};
export const topbar = { en, uk };
