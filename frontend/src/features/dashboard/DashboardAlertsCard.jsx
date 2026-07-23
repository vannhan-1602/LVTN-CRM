import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { Bell, ArrowRight, CheckCircle2 } from "lucide-react";
import alertApi from "../../api/alertApi";
import Card from "../../components/common/Card";

// Route chi tiết theo từng loại thực thể — dùng khi bấm vào 1 cảnh báo.
const ENTITY_ROUTE = {
  Lead: (id) => `/leads/${id}`,
  Customer: (id) => `/customers/${id}`,
  Ticket: (id) => `/tickets/${id}`,
  Contract: (id) => `/contracts/${id}`,
  Invoice: (id) => `/invoices/${id}`,
};

// Màu theo mức độ nghiêm trọng (Info/Warning/Danger) — khớp bảng màu đã dùng ở ManagerDashboard.
const SEVERITY_STYLE = {
  Danger: {
    badge: "bg-danger-50 text-danger-700 border-danger-100",
    item: "bg-danger-50 border-danger-100 hover:bg-danger-50/70",
    text: "text-danger-700",
    icon: "text-danger-600",
  },
  Warning: {
    badge: "bg-warning-50 text-warning-700 border-warning-100",
    item: "bg-warning-50 border-warning-100 hover:bg-warning-50/70",
    text: "text-warning-700",
    icon: "text-warning-600",
  },
  Info: {
    badge: "bg-info-50 text-info-700 border-info-100",
    item: "bg-info-50 border-info-100 hover:bg-info-50/70",
    text: "text-info-700",
    icon: "text-info-600",
  },
};

export default function DashboardAlertsCard() {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [data, setData] = useState(null);

  useEffect(() => {
    let cancelled = false;

    (async () => {
      setLoading(true);
      try {
        const res = await alertApi.getDashboardAlerts();
        if (!cancelled) setData(res.data);
      } catch {
        // Không để lỗi tải cảnh báo làm hỏng cả Dashboard — coi như không có cảnh báo nào.
        if (!cancelled) setData(null);
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, []);

  const handleItemClick = (item) => {
    const buildRoute = ENTITY_ROUTE[item.entityType];
    if (buildRoute) navigate(buildRoute(item.entityId));
  };

  if (loading) {
    return (
      <Card title="Cảnh báo">
        <div className="text-sm text-ink-400 py-4 text-center">
          Đang tải cảnh báo...
        </div>
      </Card>
    );
  }

  const groups = data?.groups ?? [];
  const tongSoCanhBao = data?.tongSoCanhBao ?? 0;

  return (
    <Card
      title={
        <span className="flex items-center gap-1.5">
          <Bell size={15} />
          Cảnh báo
        </span>
      }
      action={
        tongSoCanhBao > 0 && (
          <span className="text-xs font-medium text-ink-500 bg-ink-100 rounded-full px-2 py-0.5">
            {tongSoCanhBao}
          </span>
        )
      }
    >
      {groups.length === 0 ? (
        <div className="flex items-center gap-2 text-success-700 bg-success-50 rounded-lg px-3 py-2.5">
          <CheckCircle2 size={16} />
          <span className="text-sm font-medium">
            Không có cảnh báo nào cần xử lý.
          </span>
        </div>
      ) : (
        <div className="space-y-4">
          {groups.map((group) => {
            const style =
              SEVERITY_STYLE[group.severity] ?? SEVERITY_STYLE.Warning;
            return (
              <div key={group.groupKey}>
                <div className="flex items-center justify-between mb-2">
                  <span className="text-xs font-semibold text-ink-500 uppercase tracking-wide">
                    {group.groupTitle}
                  </span>
                  <span
                    className={`text-xs font-medium rounded-full border px-2 py-0.5 ${style.badge}`}
                  >
                    {group.count}
                  </span>
                </div>
                <div className="space-y-1.5">
                  {group.items.map((item, idx) => (
                    <button
                      key={`${item.type}-${item.entityId}-${idx}`}
                      onClick={() => handleItemClick(item)}
                      className={`w-full flex items-center justify-between gap-3 border rounded-lg px-3 py-2 text-left transition-colors ${style.item}`}
                    >
                      <div className="min-w-0">
                        <p
                          className={`text-sm font-medium truncate ${style.text}`}
                        >
                          {item.title}
                        </p>
                        {item.description && (
                          <p className="text-xs text-ink-500 truncate">
                            {item.description}
                          </p>
                        )}
                      </div>
                      <ArrowRight
                        size={15}
                        className={`shrink-0 ${style.icon}`}
                      />
                    </button>
                  ))}
                </div>
                {group.count > group.items.length && (
                  <p className="text-xs text-ink-400 mt-1.5">
                    và {group.count - group.items.length} mục khác...
                  </p>
                )}
              </div>
            );
          })}
        </div>
      )}
    </Card>
  );
}
