export default function Card({ title, action, children, className = "" }) {
  return (
    <div className={`bg-surface border border-ink-100 rounded-card p-5 ${className}`}>
      {(title || action) && (
        <div className="flex items-center justify-between mb-4">
          {title && <h3 className="text-sm font-semibold text-ink-900">{title}</h3>}
          {action}
        </div>
      )}
      {children}
    </div>
  );
}

export function Field({ label, value }) {
  return (
    <div>
      <p className="text-xs text-ink-400 mb-1">{label}</p>
      <p className="text-sm text-ink-900">{value ?? "—"}</p>
    </div>
  );
}
