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
        'SMS+email-запит надіслано клієнту. Очікуємо завантаження.',
      ),
    );
  } catch (error) {
    yield call(() => undefined);
    yield put(requestMissingPhotoFailed('Не вдалось надіслати запит. Спробуйте ще раз.'));
  }
}

export function* documentsSaga() {
  yield takeLatest(requestMissingPhoto.type, requestMissingPhotoWorker);
}
