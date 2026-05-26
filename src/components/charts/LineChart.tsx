interface Series {
  name: string;
  color: string;
  points: number[];
}

export function LineChart({
  labels,
  series,
  height = 150,
}: {
  labels: string[];
  series: Series[];
  height?: number;
}) {
  const w = 340;
  const h = height;
  const padX = 10;
  const padTop = 12;
  const padBottom = 22;
  const n = labels.length;
  const max = Math.max(...series.flatMap((s) => s.points), 1);
  const x = (i: number) => padX + (i * (w - 2 * padX)) / Math.max(n - 1, 1);
  const y = (v: number) => padTop + (1 - v / max) * (h - padTop - padBottom);

  return (
    <svg viewBox={`0 0 ${w} ${h}`} width="100%" preserveAspectRatio="xMidYMid meet" role="img">
      {[0.25, 0.5, 0.75, 1].map((g) => (
        <line
          key={g}
          x1={padX}
          x2={w - padX}
          y1={padTop + g * (h - padTop - padBottom)}
          y2={padTop + g * (h - padTop - padBottom)}
          stroke="#eceff5"
          strokeWidth={1}
        />
      ))}
      {series.map((s) => (
        <g key={s.name}>
          <polyline
            points={s.points.map((v, i) => `${x(i)},${y(v)}`).join(' ')}
            fill="none"
            stroke={s.color}
            strokeWidth={2}
            strokeLinecap="round"
            strokeLinejoin="round"
          />
          {s.points.map((v, i) => (
            <circle key={i} cx={x(i)} cy={y(v)} r={2.4} fill={s.color} />
          ))}
        </g>
      ))}
      {labels.map((l, i) => (
        <text
          key={l}
          x={x(i)}
          y={h - 6}
          textAnchor="middle"
          className="fill-ink-400"
          style={{ fontSize: 8 }}
        >
          {l}
        </text>
      ))}
    </svg>
  );
}
