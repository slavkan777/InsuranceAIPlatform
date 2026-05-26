import { createSlice, type PayloadAction } from '@reduxjs/toolkit';

type ActiveSection =
  | 'overview'
  | 'documents'
  | 'ai-evidence'
  | 'risks'
  | 'approval'
  | 'audit'
  | 'policy'
  | 'customer';

type WorkflowStep =
  | 'registration'
  | 'documents'
  | 'ai-analysis'
  | 'risk-review'
  | 'human-decision'
  | 'completion';

interface ClaimWorkspaceState {
  activeSection: ActiveSection;
  workflowStep: WorkflowStep;
}

const initialState: ClaimWorkspaceState = {
  activeSection: 'overview',
  workflowStep: 'human-decision',
};

const slice = createSlice({
  name: 'claimWorkspace',
  initialState,
  reducers: {
    setSection(state, action: PayloadAction<ActiveSection>) {
      state.activeSection = action.payload;
    },
    setWorkflowStep(state, action: PayloadAction<WorkflowStep>) {
      state.workflowStep = action.payload;
    },
  },
});

export const { setSection, setWorkflowStep } = slice.actions;
export default slice.reducer;
