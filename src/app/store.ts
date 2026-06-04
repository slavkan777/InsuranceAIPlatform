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
import i18nReducer from '@/features/i18n/i18nSlice';
import ragReducer from '@/features/rag/ragSlice';

const sagaMiddleware = createSagaMiddleware();

export const store = configureStore({
  reducer: {
    auth: authReducer,
    i18n: i18nReducer,
    claims: claimsReducer,
    claimWorkspace: claimWorkspaceReducer,
    documents: documentsReducer,
    aiReview: aiReviewReducer,
    approval: approvalReducer,
    demo: demoReducer,
    uiFeedback: uiFeedbackReducer,
    rag: ragReducer,
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
