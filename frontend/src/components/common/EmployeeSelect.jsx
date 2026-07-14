import { useEffect, useMemo, useRef, useState } from "react";
import { createPortal } from "react-dom";
import { ChevronDown, Search, Check } from "lucide-react";
import { USER_ROLE_COLOR } from "../../utils/constants";

const PANEL_WIDTH_MIN = 260;
const ITEM_HEIGHT = 44;
const PANEL_PADDING = 8;
const SEARCH_HEIGHT = 44;

function initialsOf(name) {
  return (name || "??")
    .split(" ")
    .filter(Boolean)
    .map((p) => p[0])
    .slice(-2)
    .join("")
    .toUpperCase();
}

// Vòng tròn avatar theo role, dùng cùng bảng màu USER_ROLE_COLOR để đồng bộ với
// badge role ở header/trang quản lý người dùng.
function RoleAvatar({ name, role }) {
  const colorClass = USER_ROLE_COLOR[role] || "bg-ink-100 text-ink-600";
  return (
    <span
      className={`w-8 h-8 shrink-0 rounded-full flex items-center justify-center text-xs font-semibold ${colorClass}`}
    >
      {initialsOf(name)}
    </span>
  );
}

function RoleBadge({ role }) {
  if (!role) return null;
  const colorClass = USER_ROLE_COLOR[role] || "bg-ink-100 text-ink-500";
  return (
    <span className={`text-[11px] font-medium px-2 py-0.5 rounded-full shrink-0 ${colorClass}`}>
      {role}
    </span>
  );
}

/**
 * Dropdown chọn nhân viên — thay cho <select> thường để hiện avatar + tên +
 * badge role màu sắc rõ ràng, có ô tìm kiếm khi danh sách dài.
 *
 * Props:
 *  - value: id đang chọn (string | number | "")
 *  - onChange(id): id dạng string, rỗng "" nghĩa là bỏ chọn
 *  - options: [{ id, hoTen, role }]
 *  - placeholder, emptyLabel, disabled
 */
export default function EmployeeSelect({
  value,
  onChange,
  options = [],
  placeholder = "-- Chọn nhân viên --",
  emptyLabel = "-- Chọn nhân viên --",
  disabled = false,
}) {
  const [open, setOpen] = useState(false);
  const [query, setQuery] = useState("");
  const [coords, setCoords] = useState(null);
  const btnRef = useRef(null);
  const panelRef = useRef(null);
  const searchRef = useRef(null);

  const selected = useMemo(
    () => options.find((o) => String(o.id) === String(value)) || null,
    [options, value],
  );

  const filtered = useMemo(() => {
    const q = query.trim().toLowerCase();
    if (!q) return options;
    return options.filter(
      (o) =>
        (o.hoTen || "").toLowerCase().includes(q) ||
        (o.role || "").toLowerCase().includes(q),
    );
  }, [options, query]);

  const computePosition = () => {
    const btn = btnRef.current;
    if (!btn) return;
    const rect = btn.getBoundingClientRect();
    const panelHeight =
      Math.min(filtered.length, 6) * ITEM_HEIGHT + SEARCH_HEIGHT + PANEL_PADDING;
    const spaceBelow = window.innerHeight - rect.bottom;
    const openUpward = spaceBelow < panelHeight + 12 && rect.top > panelHeight;

    setCoords({
      left: rect.left,
      width: Math.max(rect.width, PANEL_WIDTH_MIN),
      top: openUpward ? rect.top - panelHeight - 4 : rect.bottom + 4,
    });
  };

  const toggleOpen = () => {
    if (disabled) return;
    if (!open) {
      computePosition();
      setQuery("");
    }
    setOpen((v) => !v);
  };

  useEffect(() => {
    if (!open) return;
    // Focus ô tìm kiếm khi vừa mở
    const t = setTimeout(() => searchRef.current?.focus(), 0);

    const handleClickOutside = (e) => {
      if (
        btnRef.current &&
        !btnRef.current.contains(e.target) &&
        panelRef.current &&
        !panelRef.current.contains(e.target)
      ) {
        setOpen(false);
      }
    };
    const handleReposition = () => computePosition();
    const handleEsc = (e) => e.key === "Escape" && setOpen(false);

    document.addEventListener("mousedown", handleClickOutside);
    document.addEventListener("keydown", handleEsc);
    window.addEventListener("scroll", handleReposition, true);
    window.addEventListener("resize", handleReposition);
    return () => {
      clearTimeout(t);
      document.removeEventListener("mousedown", handleClickOutside);
      document.removeEventListener("keydown", handleEsc);
      window.removeEventListener("scroll", handleReposition, true);
      window.removeEventListener("resize", handleReposition);
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, filtered.length]);

  const handleSelect = (id) => {
    onChange(String(id));
    setOpen(false);
  };

  return (
    <>
      <button
        type="button"
        ref={btnRef}
        onClick={toggleOpen}
        disabled={disabled}
        className={`w-full flex items-center justify-between gap-2 border rounded-lg px-3 py-2 text-sm text-left transition-colors ${
          disabled
            ? "bg-surface-alt text-ink-400 border-ink-100 cursor-not-allowed"
            : open
              ? "border-accent-400 ring-2 ring-accent-400/40"
              : "border-ink-200 hover:border-ink-300"
        }`}
      >
        {selected ? (
          <span className="flex items-center gap-2 min-w-0">
            <RoleAvatar name={selected.hoTen} role={selected.role} />
            <span className="truncate text-ink-900 font-medium">{selected.hoTen}</span>
            <RoleBadge role={selected.role} />
          </span>
        ) : (
          <span className="text-ink-400">{placeholder}</span>
        )}
        {!disabled && <ChevronDown size={16} className="text-ink-400 shrink-0" />}
      </button>

      {open &&
        coords &&
        createPortal(
          <div
            ref={panelRef}
            style={{ position: "fixed", top: coords.top, left: coords.left, width: coords.width }}
            className="z-[60] bg-surface border border-ink-100 rounded-lg shadow-lg overflow-hidden flex flex-col"
          >
            <div className="flex items-center gap-2 px-3 border-b border-ink-100" style={{ height: SEARCH_HEIGHT }}>
              <Search size={15} className="text-ink-400 shrink-0" />
              <input
                ref={searchRef}
                value={query}
                onChange={(e) => setQuery(e.target.value)}
                placeholder="Tìm theo tên hoặc vai trò..."
                className="w-full text-sm outline-none bg-transparent"
              />
            </div>

            <div className="overflow-y-auto" style={{ maxHeight: ITEM_HEIGHT * 6 }}>
              <button
                type="button"
                onClick={() => handleSelect("")}
                className="w-full flex items-center px-3 py-2.5 text-sm text-ink-400 hover:bg-surface-alt transition-colors"
                style={{ height: ITEM_HEIGHT }}
              >
                {emptyLabel}
              </button>

              {filtered.length === 0 && (
                <div className="px-3 py-4 text-sm text-ink-400 text-center">
                  Không tìm thấy nhân viên phù hợp
                </div>
              )}

              {filtered.map((o) => {
                const isSelected = String(o.id) === String(value);
                return (
                  <button
                    type="button"
                    key={o.id}
                    onClick={() => handleSelect(o.id)}
                    className={`w-full flex items-center gap-2.5 px-3 py-2 text-sm transition-colors ${
                      isSelected ? "bg-accent-50" : "hover:bg-surface-alt"
                    }`}
                    style={{ height: ITEM_HEIGHT }}
                  >
                    <RoleAvatar name={o.hoTen} role={o.role} />
                    <span className="truncate text-ink-900 font-medium flex-1 text-left">
                      {o.hoTen}
                    </span>
                    <RoleBadge role={o.role} />
                    {isSelected && <Check size={15} className="text-accent-600 shrink-0" />}
                  </button>
                );
              })}
            </div>
          </div>,
          document.body,
        )}
    </>
  );
}
