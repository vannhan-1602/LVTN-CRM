export default function Badge({
  label,
  colorClass = "bg-gray-100 text-gray-600",
}) {
  return (
    <span
      className={`inline-block text-xs font-medium px-2 py-0.5 rounded-full ${colorClass}`}
    >
      {label}
    </span>
  );
}
