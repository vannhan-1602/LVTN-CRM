const TONES = {
  success: "bg-success-50 text-success-700",
  warning: "bg-warning-50 text-warning-700",
  danger: "bg-danger-50 text-danger-600",
  info: "bg-info-50 text-info-700",
  neutral: "bg-ink-100 text-ink-700",
};

// colorClass: cho phép truyền class Tailwind cũ (bg-x-100 text-x-700) để không phải sửa hết constants.js cùng lúc
export default function Badge({ label, tone, colorClass }) {
  const cls = colorClass || TONES[tone] || TONES.neutral;
  return (
    <span
      className={`inline-block text-xs font-medium px-2.5 py-1 rounded-full ${cls}`}
    >
      {label}
    </span>
  );
}
