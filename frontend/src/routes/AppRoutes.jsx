import { Routes, Route, Navigate } from "react-router-dom";
import LoginPage from "../features/auth/LoginPage";
import MainLayout from "../components/layout/MainLayout";
import ProtectedRoute from "./ProtectedRoute";
import { ROLES } from "../utils/constants";
import CustomerListPage from "../features/customers/CustomerListPage";
// Các Component giả lập để test Layout
function Dashboard() {
  return (
    <div className="p-6 bg-white rounded-xl shadow-sm border border-gray-100">
      <h3 className="text-xl font-bold text-gray-800 mb-2">Dashboard</h3>
      <p className="text-gray-500">Đang tải dữ liệu tổng quan...</p>
    </div>
  );
}

function LeadListPage() {
  return (
    <div className="p-4">
      <h3>Danh sách Lead</h3>
    </div>
  );
}
function TicketListPage() {
  return (
    <div className="p-4">
      <h3>Danh sách Ticket</h3>
    </div>
  );
}
function InvoiceListPage() {
  return (
    <div className="p-4">
      <h3>Hóa đơn & Công nợ</h3>
    </div>
  );
}
function UserListPage() {
  return (
    <div className="p-4">
      <h3>Quản lý Người dùng</h3>
    </div>
  );
}
function UnauthorizedPage() {
  return (
    <div className="p-4 text-red-600">
      <h3>403 - Không có quyền truy cập</h3>
      <p>Bạn không có quyền truy cập trang này.</p>
    </div>
  );
}

export default function AppRoutes() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/unauthorized" element={<UnauthorizedPage />} />

      {/* Main Layout bọc các trang cần đăng nhập */}
      <Route element={<MainLayout />}>
        {/* Cho phép tất cả user đã đăng nhập vào Dashboard */}
        <Route element={<ProtectedRoute />}>
          <Route path="/" element={<Dashboard />} />
        </Route>

        {/* Khách hàng & Lead (Đã sửa lại đúng chữ hoa/thường: Sale, Manager, Admin) */}
        <Route
          element={
            <ProtectedRoute
              allowedRoles={[ROLES.Sale, ROLES.Manager, ROLES.Admin]}
            />
          }
        >
          <Route path="/customers" element={<CustomerListPage />} />
          <Route path="/leads" element={<LeadListPage />} />
          <Route path="/tickets" element={<TicketListPage />} />
        </Route>

        {/* Kế toán */}
        <Route
          element={
            <ProtectedRoute
              allowedRoles={[ROLES.Accountant, ROLES.Manager, ROLES.Admin]}
            />
          }
        >
          <Route path="/invoices" element={<InvoiceListPage />} />
        </Route>

        {/* Admin */}
        <Route element={<ProtectedRoute allowedRoles={[ROLES.Admin]} />}>
          <Route path="/users" element={<UserListPage />} />
        </Route>
      </Route>

      {/* Catch all */}
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
