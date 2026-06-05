import { test, expect } from '@playwright/test';
import { backendConfidenceToFraction } from '../src/utils/ragConfidence';

/**
 * Spec 23 — RAG confidence contract regression (the "1400%" bug).
 *
 * The Evidence Intelligence panel + audit history render confidence as
 * `${(confidence * 100).toFixed(0)}%`, i.e. they expect a 0..1 fraction. The mock
 * already emits a fraction; the real backend emits a 0..100 integer, which used to
 * render as 1400% / 1700% / 4100%. backendConfidenceToFraction() normalizes the
 * backend value so both paths render a sane 0..100%.
 *
 * Pure-logic test (no browser/page) — runs under the existing Playwright runner.
 */

// Mirror the exact UI display formula.
const renderPct = (fraction: number): string => `${(fraction * 100).toFixed(0)}%`;

test.describe('Spec 23 — RAG confidence contract', () => {
  test('backend 0..100 integer confidence renders as sane percent (14 -> 14%, not 1400%)', () => {
    expect(renderPct(backendConfidenceToFraction(14))).toBe('14%');
    expect(renderPct(backendConfidenceToFraction(17))).toBe('17%');
    expect(renderPct(backendConfidenceToFraction(41))).toBe('41%');
    expect(renderPct(backendConfidenceToFraction(0))).toBe('0%');
    expect(renderPct(backendConfidenceToFraction(99))).toBe('99%');
  });

  test('rendered confidence never exceeds 100% for any backend value 0..100', () => {
    for (let raw = 0; raw <= 100; raw++) {
      expect(backendConfidenceToFraction(raw) * 100).toBeLessThanOrEqual(100);
    }
  });

  test('mock 0..1 fraction contract still renders correctly (unchanged path)', () => {
    // The mock path does NOT go through the normalizer; the UI multiplies the fraction by 100.
    expect(renderPct(0.82)).toBe('82%');
    expect(renderPct(0.78)).toBe('78%');
    expect(renderPct(0)).toBe('0%');
  });

  test('normalizer guards bad input (negative / NaN -> 0)', () => {
    expect(backendConfidenceToFraction(-5)).toBe(0);
    expect(backendConfidenceToFraction(Number.NaN)).toBe(0);
    expect(backendConfidenceToFraction(150)).toBe(1); // clamped to 100 -> 1.0 -> 100%
  });
});
