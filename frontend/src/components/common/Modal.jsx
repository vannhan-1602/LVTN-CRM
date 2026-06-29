import { useEffect } from "react";
import { X } from "lucide-react";

export default function Modal({
  isOpen,
  onClose,
  title,
  children,
  size = "md",
}) {
  useEffect(() => {
    if (isOpen) document.body.style.overflow = "hidden";
    else document.body.style.overflow = "";
    return () => {
      document.body.style.overflow = "";
    };
  }, [isOpen]);

  if (!isOpen) return null;

  const sizeClass =
    { sm: "max-w-md", md: "max-w-lg", lg: "max-w-2xl", xl: "max-w-4xl" }[
      size
    ] || "max-w-lg";

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      <div className="absolute inset-0 bg-brand-900/50" onClick={onClose} />
      <div
        className={`relative bg-surface rounded-2xl shadow-xl w-full ${sizeClass} mx-4 max-h-[90vh] flex flex-col`}
      >
        <div className="flex items-center justify-between px-6 py-4 border-b border-ink-100 shrink-0">
          <h3 className="text-base font-semibold text-ink-900">{title}</h3>
          <button
            onClick={onClose}
            className="text-ink-400 hover:text-ink-700 hover:bg-ink-100 rounded-lg p-1 transition-colors"
            aria-label="Đóng"
          >
            <X size={18} />
          </button>
        </div>
        <div className="overflow-y-auto flex-1 px-6 py-5">{children}</div>
      </div>
    </div>
  );
}
