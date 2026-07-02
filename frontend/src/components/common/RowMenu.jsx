import { useEffect, useRef, useState } from "react";
import { createPortal } from "react-dom";
import { MoreHorizontal } from "lucide-react";

const MENU_WIDTH = 176; // w-44
const ITEM_HEIGHT = 38;
const MENU_PADDING = 8;

// Menu "..." cho mỗi dòng bảng. items: [{ label, icon: LucideIcon, onClick, danger }]
// Dùng portal ra document.body để dropdown không bị clip bởi overflow của bảng/card cha.
export default function RowMenu({ items }) {
  const [open, setOpen] = useState(false);
  const [coords, setCoords] = useState(null);
  const btnRef = useRef(null);
  const menuRef = useRef(null);

  const computePosition = () => {
    const btn = btnRef.current;
    if (!btn) return;
    const rect = btn.getBoundingClientRect();
    const menuHeight = items.length * ITEM_HEIGHT + MENU_PADDING;
    const spaceBelow = window.innerHeight - rect.bottom;
    const openUpward = spaceBelow < menuHeight + 12 && rect.top > menuHeight;

    let left = rect.right - MENU_WIDTH;
    left = Math.max(8, Math.min(left, window.innerWidth - MENU_WIDTH - 8));

    setCoords({
      left,
      top: openUpward ? rect.top - menuHeight - 4 : rect.bottom + 4,
    });
  };

  const toggleOpen = (e) => {
    e.stopPropagation();
    if (!open) computePosition();
    setOpen((v) => !v);
  };

  useEffect(() => {
    if (!open) return;

    const handleClickOutside = (e) => {
      if (
        btnRef.current &&
        !btnRef.current.contains(e.target) &&
        menuRef.current &&
        !menuRef.current.contains(e.target)
      ) {
        setOpen(false);
      }
    };
    const handleReposition = () => computePosition();

    document.addEventListener("mousedown", handleClickOutside);
    window.addEventListener("scroll", handleReposition, true);
    window.addEventListener("resize", handleReposition);
    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
      window.removeEventListener("scroll", handleReposition, true);
      window.removeEventListener("resize", handleReposition);
    };
  }, [open]);

  return (
    <>
      <button
        ref={btnRef}
        onClick={toggleOpen}
        className={`w-8 h-8 inline-flex items-center justify-center rounded-lg transition-colors ${
          open
            ? "bg-ink-100 text-ink-900"
            : "text-ink-400 hover:bg-ink-100 hover:text-ink-700"
        }`}
        aria-label="Tùy chọn"
      >
        <MoreHorizontal size={17} />
      </button>

      {open &&
        coords &&
        createPortal(
          <div
            ref={menuRef}
            style={{
              position: "fixed",
              top: coords.top,
              left: coords.left,
              width: MENU_WIDTH,
            }}
            className="z-50 bg-surface border border-ink-100 rounded-lg shadow-lg py-1 text-left"
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
          </div>,
          document.body,
        )}
    </>
  );
}
