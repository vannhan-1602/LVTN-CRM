import { ArrowLeft } from "lucide-react";

// breadcrumb: "CRM / Kinh doanh" ; title: "Quản lý hợp đồng"
// onBack: nếu có, hiện nút quay lại (dùng cho trang chi tiết)
export default function PageHeader({ breadcrumb, title, badge, onBack, actions }) {
  return (
    <div className="flex items-center justify-between gap-4 flex-wrap">
      <div className="flex items-center gap-3">
        {onBack && (
          <button
            onClick={onBack}
            className="w-9 h-9 rounded-lg border border-ink-200 bg-surface flex items-center justify-center text-ink-500 hover:bg-ink-100 shrink-0"
            aria-label="Quay lại"
          >
            <ArrowLeft size={16} />
          </button>
        )}
        <div>
          {breadcrumb && (
            <p className="text-xs text-ink-400 uppercase tracking-wide mb-0.5">{breadcrumb}</p>
          )}
          <div className="flex items-center gap-2.5">
            <h1 className="text-xl font-semibold text-ink-900">{title}</h1>
            {badge}
          </div>
        </div>
      </div>
      {actions && <div className="flex flex-wrap gap-2">{actions}</div>}
    </div>
  );
}
