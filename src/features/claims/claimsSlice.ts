import { createSlice, type PayloadAction } from '@reduxjs/toolkit';
import { claimRows } from '@/data/mock/claims';
import type { ClaimRow } from '@/types';

interface ClaimsState {
  list: ClaimRow[];
  selectedId: string;
  search: string;
  filters: {
    status: string;
    risk: string;
    eventType: string;
    aiStatus: string;
    date: string;
  };
  segment: 'Усі' | 'ДТП' | 'Високий ризик' | 'Чекає AI' | 'Чекає рішення';
}

const initialState: ClaimsState = {
  list: claimRows,
  selectedId: 'CLM-1006',
  search: '',
  filters: {
    status: 'Усі',
    risk: 'Усі',
    eventType: 'ДТП',
    aiStatus: 'Усі',
    date: '7 днів',
  },
  segment: 'Усі',
};

const claimsSlice = createSlice({
  name: 'claims',
  initialState,
  reducers: {
    setSelected(state, action: PayloadAction<string>) {
      state.selectedId = action.payload;
    },
    setSearch(state, action: PayloadAction<string>) {
      state.search = action.payload;
    },
    setFilter(
      state,
      action: PayloadAction<{ key: keyof ClaimsState['filters']; value: string }>,
    ) {
      state.filters[action.payload.key] = action.payload.value;
    },
    setSegment(state, action: PayloadAction<ClaimsState['segment']>) {
      state.segment = action.payload;
    },
  },
});

export const { setSelected, setSearch, setFilter, setSegment } = claimsSlice.actions;
export default claimsSlice.reducer;
