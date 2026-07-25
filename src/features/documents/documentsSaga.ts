import { call, delay, put, takeLatest } from 'redux-saga/effects';
import {
  requestMissingPhoto,
  requestMissingPhotoFailed,
  requestMissingPhotoSucceeded,
} from './documentsSlice';

function* requestMissingPhotoWorker() {
  try {
    yield delay(900);
    yield put(
      requestMissingPhotoSucceeded(
        'SMS + email request sent to the customer. Awaiting upload.',
      ),
    );
  } catch (error) {
    yield call(() => undefined);
    yield put(requestMissingPhotoFailed('Could not send the request. Please try again.'));
  }
}

export function* documentsSaga() {
  yield takeLatest(requestMissingPhoto.type, requestMissingPhotoWorker);
}
