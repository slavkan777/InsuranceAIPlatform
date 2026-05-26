import type { RootState } from '@/app/store';

export const selectDemoIsPlaying = (s: RootState) => s.demo.active;
export const selectDemoCurrentStep = (s: RootState) => s.demo.currentStep;
export const selectDemoHighlightRoute = (s: RootState) => s.demo.highlightRoute;
