interface Segment {
  label: string;
  value: number;
  color: string;
}

export function DonutChart({
  data,
  size = 132,
  thickness = 16,
}: {
  data: Segment[];
  size?: number;
  thickness?: number;
}) {
  const total = data.reduce((s, d) => s + d.value, 0) || 1;
  const r = (size - thickness) / 2;
  const c = 2 * Math.PI * r;
  let offset = 0;

  return (
    <svg width={size} height={size} viewBox={`0 0 ${size} ${size}`} role="img">
      <g transform={`rotate(-90 ${size / 2} ${size / 2})`}>
        <circle cx={size / 2} cy={size / 2} r={r} fill="none" stroke="#eceff5" strokeWidth={thickness} />
        {data.map((d, i) => {
          const len = (d.value / total) * c;
          const seg = (
            <circle
              key={i}
              cx={size / 2}
              cy={size / 2}
              r={r}
              fill="none"
              stroke={d.color}
              strokeWidth={thickness}
              strokeDasharray={`${len} ${c - len}`}
              strokeDashoffset={-offset}
              strokeLinecap="butt"
            />
          );
          offset += len;
          return seg;
        })}
      </g>
      <text
        x="50%"
        y="48%"
        textAnchor="middle"
        className="fill-ink-900"
        style={{ fontSize: 22, fontWeight: 700 }}
      >
        {total}
      </text>
      <text
        x="50%"
        y="62%"
        textAnchor="middle"
        className="fill-ink-400"
        style={{ fontSize: 9, letterSpacing: 0.6 }}
      >
        ВИПАДКІВ
      </text>
    </svg>
  );
}
