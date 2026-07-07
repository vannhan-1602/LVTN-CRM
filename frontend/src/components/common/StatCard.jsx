import { ArrowUp, ArrowDown, Minus } from "lucide-react";

const TONES = {
  default: "text-ink-900",
  success: "text-success-700",
  warning: "text-warning-700",
  accent: "text-accent-700",
  info: "text-info-700",
};

// trend = số mới tháng này - số mới tháng trước (vd 3 khách hàng mới tháng này vs 1 tháng trước = +2).
// Không hiển thị gì nếu trend là undefined (tránh nhấp nháy trong lúc dữ liệu trend chưa load xong).
function TrendBadge({ trend }) {
  if (trend === undefined || trend === null) return null;
  if (trend === 0) {
    return (
      <span className="inline-flex items-center gap-0.5 text-xs text-ink-400 mt-1">
        <Minus size={12} /> Không đổi so với tháng trước
      </span>
    );
  }
  const isUp = trend > 0;
  return (
    <span className={`inline-flex items-center gap-0.5 text-xs mt-1 ${isUp ? "text-success-600" : "text-danger-600"}`}>
      {isUp ? <ArrowUp size={12} /> : <ArrowDown size={12} />}
      {isUp ? "+" : ""}{trend} so với tháng trước
    </span>
  );
}

export default function StatCard({ label, value, tone = "default", icon: Icon, trend }) {
  return (
    <div className="bg-surface border border-ink-100 rounded-card p-4 flex items-start justify-between">
      <div>
        <p className="text-xs text-ink-400 mb-1.5">{label}</p>
        <p className={`text-2xl font-semibold ${TONES[tone]}`}>{value}</p>
        <TrendBadge trend={trend} />
      </div>
      {Icon && (
        <div className="w-9 h-9 rounded-lg bg-ink-100 flex items-center justify-center text-ink-500">
          <Icon size={18} />
        </div>
      )}
    </div>
  );
}
