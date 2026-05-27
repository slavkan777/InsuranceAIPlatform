import { createSlice, type PayloadAction } from '@reduxjs/toolkit';
import type { DemoStep } from '@/types';
import type { ClaimsApiMode } from '../claims/claimsSlice';

interface DemoState {
  active: boolean;
  currentStep: number;
  highlightRoute?: string;
  // --- async scenario load ---
  scenario: DemoStep[] | null;
  scenarioLoading: boolean;
  scenarioError: string | null;
  scenarioApiMode: ClaimsApiMode;
}

const initialState: DemoState = {
  active: false,
  currentStep: 1,
  scenario: null,
  scenarioLoading: false,
  scenarioError: null,
  scenarioApiMode: 'mock',
};

const slice = createSlice({
  name: 'demo',
  initialState,
  reducers: {
    startDemo(state) {
      state.active = true;
      state.currentStep = 1;
    },
    stopDemo(state) {
      state.active = false;
      state.currentStep = 1;
      state.highlightRoute = undefined;
    },
    advanceDemo(state) {
      state.currentStep = Math.min(state.currentStep + 1, 7);
    },
    setDemoStep(state, action: PayloadAction<number>) {
      state.currentStep = action.payload;
    },
    setHighlightRoute(state, action: PayloadAction<string | undefined>) {
      state.highlightRoute = action.payload;
    },
    // --- scenario load ---
    loadDemoScenario(state) {
      state.scenarioLoading = true;
      state.scenarioError = null;
    },
    demoScenarioLoaded(
      state,
      action: PayloadAction<{ scenario: DemoStep[]; mode: ClaimsApiMode }>,
    ) {
      state.scenarioLoading = false;
      state.scenario = action.payload.scenario;
      state.scenarioApiMode = action.payload.mode;
      state.scenarioError = null;
    },
    demoScenarioFailed(
      state,
      action: PayloadAction<{ error: string; fallback: DemoStep[] }>,
    ) {
      state.scenarioLoading = false;
      state.scenario = action.payload.fallback;
      state.scenarioError = action.payload.error;
      state.scenarioApiMode = 'mock-fallback';
    },
  },
});

export const {
  startDemo,
  stopDemo,
  advanceDemo,
  setDemoStep,
  setHighlightRoute,
  loadDemoScenario,
  demoScenarioLoaded,
  demoScenarioFailed,
} = slice.actions;
export default slice.reducer;
