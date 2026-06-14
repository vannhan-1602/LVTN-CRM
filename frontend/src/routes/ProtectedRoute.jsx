
import { Navigate, Outlet, useLocation } from "react-router-dom";
import useAuthStore from "../features/auth/authStore";

export default function ProtectedRoute({ allowedRoles }) {
  const { token, user } = useAuthStore();
  const location = useLocation();

  // Chưa đăng nhập -> Về trang Login
  if (!token || !user) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  // Đã đăng nhập nhưng không có quyền (role không khớp) -> Về trang chủ 
  if (allowedRoles && allowedRoles.length > 0 && !allowedRoles.includes(user.role)) {
    return <Navigate to="/unauthorized" replace />; 
  }

  // Hợp lệ -> Render component con
  return <Outlet />;
}