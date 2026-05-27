import type { RootState } from '@/app/store';

export const selectDemoIsPlaying = (s: RootState) => s.demo.active;
export const selectDemoCurrentStep = (s: RootState) => s.demo.currentStep;
export const selectDemoHighlightRoute = (s: RootState) => s.demo.highlightRoute;
export const selectDemoScenario = (s: RootState) => s.demo.scenario;
export const selectDemoScenarioLoading = (s: RootState) => s.demo.scenarioLoading;
export const selectDemoScenarioError = (s: RootState) => s.demo.scenarioError;
export const selectDemoScenarioApiMode = (s: RootState) => s.demo.scenarioApiMode;
