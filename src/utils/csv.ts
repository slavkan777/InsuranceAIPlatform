/**
 * Local-only CSV export utilities. Runs entirely in the browser.
 * No server upload, no external service, no analytics ping.
 */

/** Escapes a single CSV cell per RFC 4180 — wraps in quotes only when needed. */
function escapeCell(value: unknown): string {
  if (value === null || value === undefined) return '';
  const s = String(value);
  if (s.includes('"') || s.includes(',') || s.includes('\n') || s.includes('\r')) {
    return `"${s.replace(/"/g, '""')}"`;
  }
  return s;
}

export interface CsvColumn<T> {
  header: string;
  /** Pull the cell value from a row. */
  accessor: (row: T) => unknown;
}

/** Build a CSV blob from rows + column definitions. UTF-8 BOM included for Excel. */
export function buildCsv<T>(rows: T[], columns: CsvColumn<T>[]): Blob {
  const headerLine = columns.map((c) => escapeCell(c.header)).join(',');
  const lines = rows.map((row) =>
    columns.map((c) => escapeCell(c.accessor(row))).join(','),
  );
  const csv = '﻿' + [headerLine, ...lines].join('\r\n');
  return new Blob([csv], { type: 'text/csv;charset=utf-8' });
}

/** Trigger a browser download for the given blob with the given filename. */
export function downloadBlob(blob: Blob, filename: string) {
  if (typeof window === 'undefined') return;
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = filename;
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  // Defer URL revoke so Safari doesn't cancel the download
  setTimeout(() => URL.revokeObjectURL(url), 1000);
}

/** Returns a YYYY-MM-DD string for the current local date, useful in filenames. */
export function localDateStamp(): string {
  const d = new Date();
  const y = d.getFullYear();
  const m = String(d.getMonth() + 1).padStart(2, '0');
  const day = String(d.getDate()).padStart(2, '0');
  return `${y}-${m}-${day}`;
}
