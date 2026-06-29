export default function EmptyState({ icon: Icon, title, description, action }) {
  return (
    <div className="flex flex-col items-center justify-center py-14 text-center px-6">
      {Icon && (
        <div className="w-12 h-12 rounded-full bg-ink-100 flex items-center justify-center text-ink-400 mb-3">
          <Icon size={22} />
        </div>
      )}
      <p className="text-sm font-medium text-ink-700">{title}</p>
      {description && <p className="text-xs text-ink-400 mt-1 max-w-xs">{description}</p>}
      {action && <div className="mt-4">{action}</div>}
    </div>
  );
}
