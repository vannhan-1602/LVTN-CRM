const VARIANTS = {
  primary: "bg-accent-500 text-white hover:bg-accent-600 shadow-sm",
  secondary: "bg-surface border border-ink-200 text-ink-700 hover:bg-ink-100",
  ghost: "text-ink-500 hover:bg-ink-100 hover:text-ink-900",
  danger:
    "bg-surface border border-danger-100 text-danger-600 hover:bg-danger-50",
  dangerSolid: "bg-danger-500 text-white hover:bg-danger-600 shadow-sm",
};

const SIZES = {
  sm: "px-3 py-1.5 text-xs gap-1.5",
  md: "px-4 py-2 text-sm gap-2",
  lg: "px-5 py-2.5 text-sm gap-2",
};

export default function Button({
  children,
  onClick,
  type = "button",
  variant = "primary",
  size = "md",
  disabled = false,
  icon: Icon,
  className = "",
}) {
  const base =
    "inline-flex items-center justify-center font-medium rounded-lg transition-colors disabled:opacity-50 disabled:cursor-not-allowed active:scale-[0.98]";

  return (
    <button
      type={type}
      onClick={onClick}
      disabled={disabled}
      className={`${base} ${SIZES[size]} ${VARIANTS[variant] || VARIANTS.primary} ${className}`}
    >
      {Icon && <Icon size={size === "sm" ? 14 : 16} strokeWidth={2} />}
      {children}
    </button>
  );
}
