import { useEffect, useState } from 'react';
import { SectionHeader } from '@/components/ui/SectionHeader';
import { Icon } from '@/components/ui/Icon';
import { CreateCustomerModal } from '@/components/customers/CreateCustomerModal';
import { insuranceApi } from '@/api/insuranceApi';
import type {
  CustomerListResultDto,
  CustomerSummaryDto,
} from '@/api/insuranceApi.types';
import clsx from '@/utils/clsx';
import { useI18n } from '@/i18n/useI18n';

const PAGE_SIZE = 25;

/**
 * Customers directory — paginated list of all synthetic test customers
 * (IsSynthetic=true rows in customers_policies.SyntheticCustomers). Includes
 * a case-insensitive substring search over name / email / id, and a
 * "Create customer" action that opens CreateCustomerModal. After
 * a successful creation the list is reloaded.
 */
export default function CustomersDirectoryPage() {
  const { t } = useI18n();
  const [search, setSearch] = useState('');
  const [debouncedSearch, setDebouncedSearch] = useState('');
  const [page, setPage] = useState(1);
  const [result, setResult] = useState<CustomerListResultDto | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [createOpen, setCreateOpen] = useState(false);
  // Reload counter — bumped after a successful create so the useEffect that
  // fetches the list re-runs without changing search/page.
  const [reloadTick, setReloadTick] = useState(0);

  useEffect(() => {
    const timer = setTimeout(() => setDebouncedSearch(search), 250);
    return () => clearTimeout(timer);
  }, [search]);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    setError(null);
    insuranceApi
      .listCustomers(debouncedSearch || null, page, PAGE_SIZE)
      .then((r) => {
        if (!cancelled) setResult(r);
      })
      .catch((err: unknown) => {
        if (!cancelled) {
          setError(err instanceof Error ? err.message : String(err));
        }
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });
    return () => {
      cancelled = true;
    };
  }, [debouncedSearch, page, reloadTick]);

  // Reset to page 1 when the search query changes
  useEffect(() => {
    setPage(1);
  }, [debouncedSearch]);

  const totalPages = result ? Math.max(1, Math.ceil(result.total / PAGE_SIZE)) : 1;

  return (
    <div className="flex flex-col gap-5" data-testid="customers-directory-page">
      <SectionHeader
        title={t.customers.pageTitle}
        subtitle={t.customers.pageSubtitle}
        actions={
          <button
            type="button"
            data-testid="create-customer-open"
            onClick={() => setCreateOpen(true)}
            className="btn-primary inline-flex items-center gap-1.5"
            title={t.customers.createButtonTitle}
          >
            <Icon name="plus" size={14} />
            {t.customers.createButtonLabel}
          </button>
        }
      />

      <CreateCustomerModal
        open={createOpen}
        onClose={() => setCreateOpen(false)}
        onCreated={(created) => {
          // Reset search to the new id so the new row is immediately visible
          // and so the test can assert "I see the row I just created".
          setSearch(created.customerId);
          setPage(1);
          setReloadTick((n) => n + 1);
        }}
      />

      <section className="card card-pad">
        <div className="flex flex-wrap items-center justify-between gap-3 mb-3">
          <div className="flex-1 max-w-md relative">
            <span className="absolute left-3 top-1/2 -translate-y-1/2 text-ink-400">
              <Icon name="search" size={15} />
            </span>
            <input
              type="search"
              data-testid="customers-search"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder={t.customers.searchPlaceholder}
              className="w-full pl-9 pr-3 py-2 rounded-lg border border-ink-200 bg-white text-sm focus-ring"
            />
          </div>
          <div className="text-xs text-ink-500" data-testid="customers-meta">
            {loading
              ? t.customers.metaLoading
              : result
                ? `${result.total} ${t.customers.metaFoundPrefix} ${result.page}${t.customers.metaFoundSeparator}${totalPages}`
                : t.customers.metaReady}
          </div>
        </div>

        {error ? (
          <div role="alert" className="rounded-lg border border-danger-200 bg-danger-50 text-danger-700 text-xs px-3 py-2 mb-3">
            {error}
          </div>
        ) : null}

        <div className="overflow-x-auto">
          <table className="w-full" data-testid="customers-table">
            <thead className="bg-ink-50/80">
              <tr>
                <th className="table-th">{t.customers.colId}</th>
                <th className="table-th">{t.customers.colFullName}</th>
                <th className="table-th">{t.customers.colEmail}</th>
                <th className="table-th">{t.customers.colPhone}</th>
                <th className="table-th">{t.customers.colCustomerSince}</th>
                <th className="table-th text-right">{t.customers.colPriorCases}</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-ink-100">
              {(result?.items ?? []).map((c) => (
                <CustomerRow key={c.id} c={c} />
              ))}
              {result && result.items.length === 0 && (
                <tr>
                  <td colSpan={6} className="table-td text-center text-ink-500 py-8" data-testid="customers-empty">
                    {t.customers.emptyState}
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>

        {result && totalPages > 1 && (
          <div className="flex items-center justify-between pt-3 mt-3 border-t border-ink-100">
            <button
              type="button"
              onClick={() => setPage((p) => Math.max(1, p - 1))}
              disabled={page <= 1 || loading}
              className="btn-ghost px-3 py-1.5 text-xs disabled:opacity-40"
            >
              {t.customers.paginationBack}
            </button>
            <div className="text-xs text-ink-600">
              {t.customers.paginationPageLabel}{' '}
              <span className="font-mono font-semibold">{result.page}</span>{' '}
              {t.customers.paginationPageOf}{' '}
              <span className="font-mono font-semibold">{totalPages}</span>
            </div>
            <button
              type="button"
              onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
              disabled={page >= totalPages || loading}
              className="btn-ghost px-3 py-1.5 text-xs disabled:opacity-40"
            >
              {t.customers.paginationNext}
            </button>
          </div>
        )}
      </section>

      <section className="card card-pad text-xs text-ink-600 leading-snug">
        <div className="flex items-start gap-2">
          <span className="mt-0.5 shrink-0 text-ink-400">
            <Icon name="info" size={14} />
          </span>
          <p>{t.customers.syntheticNoteText}</p>
        </div>
      </section>
    </div>
  );
}

function CustomerRow({ c }: { c: CustomerSummaryDto }) {
  return (
    <tr className="hover:bg-ink-50 transition-colors" data-testid={`customer-row-${c.id}`}>
      <td className="table-td font-mono font-semibold text-brand-700">{c.id}</td>
      <td className="table-td text-ink-900">{c.fullName}</td>
      <td className="table-td text-ink-600 font-mono text-xs">{c.email}</td>
      <td className="table-td text-ink-600">{c.phone || '—'}</td>
      <td className="table-td text-ink-600">{c.customerSince}</td>
      <td className="table-td text-right">
        <span
          className={clsx(
            'inline-flex px-2 py-0.5 rounded text-xs font-mono font-semibold',
            c.previousClaimsCount === 0
              ? 'bg-ink-100 text-ink-500'
              : c.previousClaimsCount > 3
                ? 'bg-warn-100 text-warn-700'
                : 'bg-good-100 text-good-700',
          )}
        >
          {c.previousClaimsCount}
        </span>
      </td>
    </tr>
  );
}
