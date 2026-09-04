import { useEffect, useRef } from "react";
import * as signalR from "@microsoft/signalr";
import { API_ORIGIN } from "../api/axiosClient";
import useAuthStore from "../features/auth/authStore";

// Kết nối tới NotificationHub 1 lần khi user đã đăng nhập, tự reconnect nếu rớt mạng, tự
// đóng kết nối khi logout hoặc component unmount. Trả về connection ref để component gọi
// connection.on("event:name", handler) đăng ký lắng nghe sự kiện cụ thể của module mình.
export default function useNotificationHub() {
  const token = useAuthStore((s) => s.token);
  const connectionRef = useRef(null);

  useEffect(() => {
    if (!token) return undefined;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_ORIGIN}/hubs/notifications`, { accessTokenFactory: () => token })
      .withAutomaticReconnect()
      .build();

    connection.start().catch((err) => console.error("SignalR connect failed:", err));
    connectionRef.current = connection;

    return () => {
      connection.stop();
      connectionRef.current = null;
    };
  }, [token]);

  return connectionRef;
}