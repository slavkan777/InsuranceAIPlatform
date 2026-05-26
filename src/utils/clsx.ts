export default function clsx(
  ...values: Array<string | undefined | null | false | Record<string, unknown>>
): string {
  const out: string[] = [];
  for (const v of values) {
    if (!v) continue;
    if (typeof v === 'string') out.push(v);
    else {
      for (const [k, val] of Object.entries(v)) if (val) out.push(k);
    }
  }
  return out.join(' ');
}
