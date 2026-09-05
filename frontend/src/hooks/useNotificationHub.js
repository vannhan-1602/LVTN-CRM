import { useEffect, useRef } from "react";
import * as signalR from "@microsoft/signalr";
import { API_ORIGIN } from "../api/axiosClient";
import useAuthStore from "../features/auth/authStore";

// Kết nối tới NotificationHub và đăng ký sẵn toàn bộ handler truyền vào qua eventHandlers
// (dạng { "customer:created": (payload) => {...}, ... }). Tự reconnect nếu rớt mạng, tự đóng
// khi logout/unmount, chỉ tạo lại kết nối khi trạng thái đăng nhập thật sự đổi — KHÔNG tạo lại
// mỗi khi access token tự làm mới sau 15 phút (accessTokenFactory đọc token trực tiếp từ store
// mỗi lần cần thay vì đóng cứng 1 giá trị lúc tạo kết nối).
//
// handlersRef giữ luôn bản MỚI NHẤT của eventHandlers mà không cần tạo lại connection mỗi khi
// component cha re-render với callback mới — .on() chỉ đăng ký 1 lần lúc connect, còn lại uỷ
// quyền qua handlersRef.current tại thời điểm sự kiện thực sự đến.
export default function useNotificationHub(eventHandlers = {}) {
  const isLoggedIn = useAuthStore((s) => Boolean(s.token));
  const connectionRef = useRef(null);
  const handlersRef = useRef(eventHandlers);

  // Cập nhật ref TRONG effect, không phải ngay lúc render — gán ref trực tiếp trong thân hàm
  // component (ngoài effect) bị React coi là tác dụng phụ không an toàn cho concurrent rendering.
  useEffect(() => {
    handlersRef.current = eventHandlers;
  });

  useEffect(() => {
    if (!isLoggedIn) return undefined;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_ORIGIN}/hubs/notifications`, {
        accessTokenFactory: () => useAuthStore.getState().token,
      })
      .withAutomaticReconnect()
      .build();

    Object.keys(handlersRef.current).forEach((eventName) => {
      connection.on(eventName, (...args) =>
        handlersRef.current[eventName]?.(...args),
      );
    });

    connection
      .start()
      .catch((err) => console.error("SignalR connect failed:", err));
    connectionRef.current = connection;

    return () => {
      connection.stop();
      connectionRef.current = null;
    };
  }, [isLoggedIn]);

  return connectionRef;
}
