import { createSlice, type PayloadAction } from '@reduxjs/toolkit';

interface DemoState {
  active: boolean;
  currentStep: number;
  highlightRoute?: string;
}

const initialState: DemoState = {
  active: false,
  currentStep: 1,
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
  },
});

export const { startDemo, stopDemo, advanceDemo, setDemoStep, setHighlightRoute } =
  slice.actions;
export default slice.reducer;
