import useAuthStore from "../auth/authStore";
import { ROLES } from "../../utils/constants";
import ManagerDashboard from "./ManagerDashboard";
import AdminDashboard from "./AdminDashboard";
import WelcomeDashboard from "./WelcomeDashboard";

// Manager cần xem hiệu quả công việc tổng quan của cả đội, Admin cần xem
// tình trạng quản trị tài khoản/hệ thống — mỗi role có 1 dashboard riêng.
// Sale/Accountant giữ màn chào mừng đơn giản vì phạm vi dữ liệu của họ đã
// được phục vụ tốt hơn qua từng trang module riêng.
export default function DashboardPage() {
  const { user } = useAuthStore();
  if (user?.role === ROLES.Manager) return <ManagerDashboard />;
  if (user?.role === ROLES.Admin) return <AdminDashboard />;
  return <WelcomeDashboard />;
}
