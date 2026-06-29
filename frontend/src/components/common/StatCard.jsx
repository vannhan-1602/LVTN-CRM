const TONES = {
  default: "text-ink-900",
  success: "text-success-700",
  warning: "text-warning-700",
  accent: "text-accent-700",
  info: "text-info-700",
};

export default function StatCard({ label, value, tone = "default", icon: Icon }) {
  return (
    <div className="bg-surface border border-ink-100 rounded-card p-4 flex items-start justify-between">
      <div>
        <p className="text-xs text-ink-400 mb-1.5">{label}</p>
        <p className={`text-2xl font-semibold ${TONES[tone]}`}>{value}</p>
      </div>
      {Icon && (
        <div className="w-9 h-9 rounded-lg bg-ink-100 flex items-center justify-center text-ink-500">
          <Icon size={18} />
        </div>
      )}
    </div>
  );
}
