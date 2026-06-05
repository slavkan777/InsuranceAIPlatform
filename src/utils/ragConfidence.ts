/**
 * RAG confidence contract normalization.
 *
 * The local RAG backend emits answer/audit confidence as a 0..100 INTEGER band
 * (see MockGroundedAnswerGenerator.Confidence — cosine mapped to a 0..99 int).
 * The frontend RAG contract — matching the mock fixtures and the panel display,
 * which renders `${(confidence * 100).toFixed(0)}%` — treats confidence as a
 * 0..1 FRACTION. Passing the raw backend integer straight through therefore
 * rendered nonsense like 1400% (14 * 100).
 *
 * This converts a backend 0..100 integer to the 0..1 fraction the UI expects, so
 * the real-backend path and the mock path render identically (and never > 100%).
 * It is applied ONLY on the backend path (the mock already emits a 0..1 fraction).
 */
export function backendConfidenceToFraction(raw: number): number {
  if (!Number.isFinite(raw) || raw <= 0) return 0;
  // Clamp defensively to the documented 0..100 band, then scale to 0..1.
  return Math.min(raw, 100) / 100;
}
