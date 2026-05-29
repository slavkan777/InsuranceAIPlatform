import type { RootState } from '@/app/store';

export const selectToasts = (s: RootState) => s.uiFeedback.toasts;
