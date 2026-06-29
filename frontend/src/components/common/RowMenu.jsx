import { useEffect, useRef, useState } from "react";
import { MoreHorizontal } from "lucide-react";

// Menu "..." cho mỗi dòng bảng. items: [{ label, icon: LucideIcon, onClick, danger }]
export default function RowMenu({ items }) {
  const [open, setOpen] = useState(false);
  const ref = useRef(null);

  useEffect(() => {
    if (!open) return;
    const handler = (e) => {
      if (ref.current && !ref.current.contains(e.target)) setOpen(false);
    };
    document.addEventListener("mousedown", handler);
    return () => document.removeEventListener("mousedown", handler);
  }, [open]);

  return (
    <div className="relative inline-block" ref={ref}>
      <button
        onClick={(e) => {
          e.stopPropagation();
          setOpen((v) => !v);
        }}
        className={`w-8 h-8 inline-flex items-center justify-center rounded-lg transition-colors ${
          open ? "bg-ink-100 text-ink-900" : "text-ink-400 hover:bg-ink-100 hover:text-ink-700"
        }`}
        aria-label="Tùy chọn"
      >
        <MoreHorizontal size={17} />
      </button>

      {open && (
        <div
          className="absolute right-0 top-9 z-20 w-44 bg-surface border border-ink-100 rounded-lg shadow-lg py-1 text-left"
          onClick={(e) => e.stopPropagation()}
        >
          {items.map((item, i) => (
            <button
              key={i}
              onClick={() => {
                setOpen(false);
                item.onClick();
              }}
              className={`w-full flex items-center gap-2.5 px-3 py-2 text-sm transition-colors ${
                item.danger
                  ? "text-danger-600 hover:bg-danger-50"
                  : "text-ink-700 hover:bg-ink-100"
              }`}
            >
              {item.icon && <item.icon size={15} />}
              {item.label}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
