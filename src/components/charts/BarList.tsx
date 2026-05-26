interface Bar {
  label: string;
  value: number;
  color: string;
}

export function BarList({ data }: { data: Bar[] }) {
  const max = Math.max(...data.map((d) => d.value), 1);
  return (
    <ul className="space-y-2.5">
      {data.map((d) => (
        <li key={d.label} className="flex items-center gap-3 text-sm">
          <span className="w-16 shrink-0 text-xs font-mono text-ink-500">{d.label}</span>
          <span className="flex-1 h-2.5 rounded-full bg-ink-100 overflow-hidden">
            <span
              className="block h-full rounded-full"
              style={{ width: `${(d.value / max) * 100}%`, backgroundColor: d.color }}
            />
          </span>
          <span className="w-6 text-right font-semibold text-ink-800 tabular-nums">{d.value}</span>
        </li>
      ))}
    </ul>
  );
}
