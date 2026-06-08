import { useState } from 'react';
import { Icon } from '@/components/ui/Icon';

/**
 * Optional "Advanced AI Review" — calls the .NET endpoint POST /api/claims/{id}/advanced-ai-review,
 * which (when the feature flag is on) proxies the claim + its claim-scoped evidence to the Python
 * LangChain analytics sidecar and returns a structured advisory manager review. When the flag is off
 * or the sidecar is unreachable the endpoint returns a safe fallback (providerMode Disabled/Unavailable),
 * which this panel renders honestly. Advisory only — never a final payout/fraud/legal decision.
 *
 * Self-contained (own fetch) so it adds zero coupling to the existing api facade / mock contract.
 */
interface AdvancedReviewCitation {
  chunkId: string;
  kind: string;
}
interface AdvancedReviewDto {
  claimId: string;
  summary: string;
  coverageAssessment: string;
  evidenceStrength: string;
  anomalies: string[];
  missingItems: string[];
  recommendedNextAction: string;
  citations: AdvancedReviewCitation[];
  confidence: number;
  advisoryOnly: boolean;
  providerMode: string;
  framework: string;
}

const BASE_URL: string =
  (import.meta.env as unknown as Record<string, string>).VITE_INSURANCE_API_BASE_URL ??
  'http://localhost:5284';

type Status = 'idle' | 'loading' | 'error' | 'done';

export function AdvancedAiReviewPanel({ claimId }: { claimId: string }) {
  const [status, setStatus] = useState<Status>('idle');
  const [review, setReview] = useState<AdvancedReviewDto | null>(null);
  const [error, setError] = useState<string | null>(null);

  async function run() {
    setStatus('loading');
    setError(null);
    try {
      const res = await fetch(`${BASE_URL}/api/claims/${claimId}/advanced-ai-review`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ question: null }),
      });
      if (!res.ok) throw new Error(`HTTP ${res.status}`);
      setReview((await res.json()) as AdvancedReviewDto);
      setStatus('done');
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Невідома помилка');
      setStatus('error');
    }
  }

  const disabled = review && (review.providerMode === 'Disabled' || review.providerMode === 'Unavailable');

  return (
    <section
      className="rounded-xl border border-ai-200 bg-white p-4 mt-6"
      data-testid="advanced-ai-review-section"
    >
      <div className="flex items-center justify-between gap-2 mb-3">
        <div>
          <h3 className="text-sm font-semibold text-ink-900">Розширений AI-огляд (LangChain)</h3>
          <p className="text-[11px] text-ink-500">
            Додатковий структурований огляд поверх базового RAG. Рекомендаційно, не фінальне рішення.
          </p>
        </div>
        <button
          type="button"
          onClick={run}
          disabled={status === 'loading'}
          data-testid="advanced-ai-review-btn"
          className="btn-primary inline-flex items-center gap-1.5 px-3 py-1.5 text-sm disabled:opacity-50 shrink-0"
        >
          <Icon name="cpu" size={14} />
          {status === 'loading' ? 'Аналізуємо…' : 'Запустити розширений огляд'}
        </button>
      </div>

      {status === 'loading' && (
        <div className="rounded-lg border border-ai-200 bg-ai-50 px-4 py-3 text-sm text-ai-700 animate-pulse"
             data-testid="advanced-ai-review-loading">
          Виконується розширений аналіз через LangChain-сайдкар…
        </div>
      )}

      {status === 'error' && (
        <div className="rounded-lg border border-danger-200 bg-danger-50 px-4 py-3 text-sm text-danger-700"
             data-testid="advanced-ai-review-error">
          Помилка розширеного огляду: {error}. Базовий RAG-аналіз доступний вище.
        </div>
      )}

      {status === 'done' && review && (
        <div className="space-y-3" data-testid="advanced-ai-review-panel">
          <div className="flex items-center gap-2 rounded-lg border border-warn-200 bg-warn-50 px-4 py-2 text-sm font-semibold text-warn-800"
               data-testid="advanced-ai-review-advisory">
            <span className="text-warn-600 text-base">⚠</span>
            AI-аналіз має лише рекомендаційний характер — фінальне рішення приймає людина-адʼюстер.
          </div>

          {disabled ? (
            <p className="text-sm text-ink-600" data-testid="advanced-ai-review-fallback">{review.summary}</p>
          ) : (
            <>
              <div className="flex flex-wrap gap-3 text-xs">
                <span className="px-2 py-0.5 rounded-full bg-ai-100 text-ai-700 border border-ai-200">
                  Сила доказів: <b>{review.evidenceStrength}</b>
                </span>
                <span className="px-2 py-0.5 rounded-full bg-ink-100 text-ink-700 border border-ink-200"
                      data-testid="advanced-ai-review-confidence">
                  Впевненість: <b>{review.confidence}%</b>
                </span>
                <span className="px-2 py-0.5 rounded-full bg-ink-50 text-ink-500 border border-ink-200">
                  {review.providerMode} · {review.framework}
                </span>
              </div>

              <div>
                <div className="text-[10px] font-semibold text-ink-400 uppercase tracking-wide">Підсумок</div>
                <p className="text-sm text-ink-800">{review.summary}</p>
              </div>
              <div>
                <div className="text-[10px] font-semibold text-ink-400 uppercase tracking-wide">Оцінка покриття</div>
                <p className="text-sm text-ink-800">{review.coverageAssessment}</p>
              </div>

              {review.anomalies.length > 0 && (
                <div data-testid="advanced-ai-review-anomalies">
                  <div className="text-[10px] font-semibold text-ink-400 uppercase tracking-wide">Аномалії</div>
                  <ul className="list-disc pl-5 text-sm text-ink-800">
                    {review.anomalies.map((a, i) => <li key={i}>{a}</li>)}
                  </ul>
                </div>
              )}

              {review.missingItems.length > 0 && (
                <div>
                  <div className="text-[10px] font-semibold text-ink-400 uppercase tracking-wide">Відсутні матеріали</div>
                  <ul className="list-disc pl-5 text-sm text-ink-800">
                    {review.missingItems.map((m, i) => <li key={i}>{m}</li>)}
                  </ul>
                </div>
              )}

              <div>
                <div className="text-[10px] font-semibold text-ink-400 uppercase tracking-wide">Рекомендована дія</div>
                <p className="text-sm text-ink-800">{review.recommendedNextAction}</p>
              </div>

              {review.citations.length > 0 && (
                <div data-testid="advanced-ai-review-citations">
                  <div className="text-[10px] font-semibold text-ink-400 uppercase tracking-wide">
                    Цитати ({review.citations.length}) — лише ця справа
                  </div>
                  <ul className="text-xs font-mono text-ink-600 space-y-0.5">
                    {review.citations.map((c) => (
                      <li key={c.chunkId}>[{c.kind}] {c.chunkId}</li>
                    ))}
                  </ul>
                </div>
              )}
            </>
          )}
        </div>
      )}
    </section>
  );
}
