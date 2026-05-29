import { configureStore } from '@reduxjs/toolkit';
import createSagaMiddleware from 'redux-saga';
import { rootSaga } from './rootSaga';
import claimsReducer from '@/features/claims/claimsSlice';
import claimWorkspaceReducer from '@/features/claims/claimWorkspaceSlice';
import documentsReducer from '@/features/documents/documentsSlice';
import aiReviewReducer from '@/features/aiReview/aiReviewSlice';
import approvalReducer from '@/features/approval/approvalSlice';
import demoReducer from '@/features/demo/demoSlice';
import authReducer from '@/features/auth/authSlice';
import uiFeedbackReducer from '@/features/ui/uiFeedbackSlice';

const sagaMiddleware = createSagaMiddleware();

export const store = configureStore({
  reducer: {
    auth: authReducer,
    claims: claimsReducer,
    claimWorkspace: claimWorkspaceReducer,
    documents: documentsReducer,
    aiReview: aiReviewReducer,
    approval: approvalReducer,
    demo: demoReducer,
    uiFeedback: uiFeedbackReducer,
  },
  middleware: (getDefault) =>
    getDefault({
      thunk: false,
      serializableCheck: {
        ignoredActions: [],
      },
    }).concat(sagaMiddleware),
  devTools: import.meta.env.DEV,
});

sagaMiddleware.run(rootSaga);

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
