import useAuthStore from "../auth/authStore";
import { ROLES } from "../../utils/constants";
import ManagerDashboard from "./ManagerDashboard";
import WelcomeDashboard from "./WelcomeDashboard";

// Chỉ Manager cần xem hiệu quả công việc tổng quan của cả đội.
// Các role khác (Sale, Accountant, Admin) giữ màn chào mừng đơn giản,
// vì phạm vi dữ liệu của họ đã được phục vụ tốt hơn qua từng trang module riêng.
export default function DashboardPage() {
  const { user } = useAuthStore();
  if (user?.role === ROLES.Manager) return <ManagerDashboard />;
  return <WelcomeDashboard />;
}
