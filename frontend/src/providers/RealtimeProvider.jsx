import { useMemo } from "react";
import { useQueryClient } from "@tanstack/react-query";
import useNotificationHub from "../hooks/useNotificationHub";
import useRealtimeStore from "../stores/realtimeStore";
import useToastStore from "../stores/toastStore";

// Tên sự kiện luôn theo quy ước "module:action" (VD "customer:created", "ticket:assigned") —
// tách phần trước dấu ":" ra làm tên module. Nhờ quy ước này, backend thêm sự kiện MỚI theo
// đúng quy ước là tự động hoạt động, không cần sửa danh sách này — chỉ cần thêm tên sự kiện
// vào đây để hook biết mà lắng nghe.
const KNOWN_EVENTS = [
  "customer:created",
  "customer:updated",
  "customer:deleted",
  "customer:restored",
  "lead:new",
  "lead:created",
  "lead:updated",
  "lead:deleted",
  "lead:restored",
  "lead:assigned",
  "lead:converted",
  "opportunity:stage_changed",
  "opportunity:deleted",
  "quote:created",
  "quote:updated",
  "quote:deleted",
  "quote:sent",
  "quote:accepted",
  "quote:rejected",
  "contract:created",
  "contract:status_changed",
  "contract:deleted",
  "contract:milestone_changed",
  "contract:license_changed",
  "invoice:created",
  "phieuthuchi:created",
  "product:created",
  "product:updated",
  "product:deleted",
  "product:stock_changed",
  "ticket:created",
  "ticket:updated",
  "ticket:deleted",
  "ticket:assigned",
  "ticket:closed",
  "ticket:reply_added",
  "loyalty:voucher_redeemed",
  "user:created",
  "user:updated",
  "user:deleted",
  "user:status_changed",
];

// Đặt <RealtimeProvider> ngay trong <QueryClientProvider> ở main.jsx — chỉ 1 kết nối
// SignalR duy nhất cho toàn bộ app, mọi module đều tự động nhận cập nhật.
export default function RealtimeProvider({ children }) {
  const touch = useRealtimeStore((s) => s.touch);
  const pushToast = useToastStore((s) => s.push);
  const queryClient = useQueryClient();

  // Build 1 lần: mỗi sự kiện -> cùng 1 hành động (touch module cho trang dùng state thủ công +
  // invalidate React Query cho trang dùng useQuery). useMemo tránh tạo lại object này mỗi lần
  // RealtimeProvider re-render (touch/queryClient/pushToast là tham chiếu ổn định giữa các lần render).
  const eventHandlers = useMemo(() => {
    const handlers = {};
    KNOWN_EVENTS.forEach((eventName) => {
      const moduleName = eventName.split(":")[0];
      handlers[eventName] = () => {
        touch(moduleName);
        queryClient.invalidateQueries({ queryKey: [moduleName] });
      };
    });

    // 2 sự kiện có payload phong phú (xem CreatePublicLeadCommandHandler/CreateTicketCommandHandler
    // ở backend) — hiện thêm toast trực quan thay vì chỉ âm thầm refetch như các sự kiện khác.
    const defaultLeadHandler = handlers["lead:new"];
    handlers["lead:new"] = (payload) => {
      defaultLeadHandler();
      pushToast({
        tone: "info",
        title: "Lead mới từ website",
        message: payload?.tenLead
          ? `${payload.tenLead} — ${payload.soDienThoai ?? ""}`
          : undefined,
      });
    };

    const defaultTicketAssignedHandler = handlers["ticket:assigned"];
    handlers["ticket:assigned"] = (payload) => {
      defaultTicketAssignedHandler();
      pushToast({
        tone: "warning",
        title: "Bạn vừa được giao ticket mới",
        message: payload?.maTicket
          ? `${payload.maTicket} — ${payload.tieuDe ?? ""}`
          : undefined,
      });
    };

    return handlers;
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useNotificationHub(eventHandlers);

  return children;
}
