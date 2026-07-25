// Guided demo ("Usage example") contracts.
export type { DemoStep } from './index';

export interface DemoScenarioState {
  active: boolean;
  currentStep: number;
  totalSteps: number;
  highlightRoute?: string;
}
