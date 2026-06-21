import { Routes, Route, Navigate } from "react-router-dom";
import LoginPage from "../features/auth/LoginPage";
import MainLayout from "../components/layout/MainLayout";
import ProtectedRoute from "./ProtectedRoute";
import { ROLES } from "../utils/constants";

import CustomerListPage from "../features/customers/CustomerListPage";
import LeadListPage from "../features/leads/LeadListPage";
import TicketListPage from "../features/tickets/TicketListPage";
import UserManagementPage from "../features/users/UserManagementPage";
import ProductListPage from "../features/products/ProductListPage"; // ✅ MỚI
import QuoteListPage from "../features/quotes/QuoteListPage"; // ✅ MỚI
import ContractListPage from "../features/contracts/ContractListPage"; // ✅ MỚI

function Dashboard() {
  return (
    <div className="bg-white rounded-xl border shadow-sm p-8">
      <h2 className="text-xl font-bold text-gray-800 mb-2">Dashboard</h2>
      <p className="text-gray-500">
        Chào mừng đến với CRM System. Chọn module từ menu bên trái.
      </p>
    </div>
  );
}

function UnauthorizedPage() {
  return (
    <div className="flex flex-col items-center justify-center h-64 text-center">
      <div className="text-5xl mb-4">🚫</div>
      <h3 className="text-xl font-bold text-red-600 mb-2">
        403 — Không có quyền truy cập
      </h3>
      <p className="text-gray-500 text-sm">
        Tài khoản của bạn không có quyền xem trang này.
      </p>
    </div>
  );
}

function InvoicePlaceholder() {
  return (
    <div className="bg-white rounded-xl border shadow-sm p-8 text-center">
      <div className="text-4xl mb-4">🧾</div>
      <h3 className="text-xl font-bold text-gray-700">Hóa đơn & Công nợ</h3>
      <p className="text-gray-400 mt-2 text-sm">Module đang được phát triển.</p>
    </div>
  );
}

export default function AppRoutes() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/unauthorized" element={<UnauthorizedPage />} />

      <Route element={<MainLayout />}>
        <Route element={<ProtectedRoute />}>
          <Route path="/" element={<Dashboard />} />
        </Route>

        {/* Sale + Manager: Lead, Ticket, Báo giá (Admin KHÔNG có quyền theo docx) */}
        <Route
          element={
            <ProtectedRoute allowedRoles={[ROLES.Sale, ROLES.Manager]} />
          }
        >
          <Route path="/leads" element={<LeadListPage />} />
          <Route path="/tickets" element={<TicketListPage />} />
          <Route path="/quotes" element={<QuoteListPage />} /> {/* ✅ MỚI */}
          <Route path="/products" element={<ProductListPage />} />{" "}
          {/* ✅ MỚI — Sale xem để lập báo giá, chỉ Manager mới sửa được (component tự ẩn form) */}
        </Route>

        {/* Customer: Sale + Manager (đọc/ghi) + Accountant (chỉ đọc) */}
        <Route
          element={
            <ProtectedRoute
              allowedRoles={[ROLES.Sale, ROLES.Manager, ROLES.Accountant]}
            />
          }
        >
          <Route path="/customers" element={<CustomerListPage />} />
        </Route>

        {/* Hợp đồng: Sale + Manager (đọc/ghi) + Accountant (chỉ đọc — theo docx) */}
        <Route
          element={
            <ProtectedRoute
              allowedRoles={[ROLES.Sale, ROLES.Manager, ROLES.Accountant]}
            />
          }
        >
          <Route path="/contracts" element={<ContractListPage />} />{" "}
          {/* ✅ MỚI */}
        </Route>

        {/* Accountant + Manager (xem) */}
        <Route
          element={
            <ProtectedRoute allowedRoles={[ROLES.Accountant, ROLES.Manager]} />
          }
        >
          <Route path="/invoices" element={<InvoicePlaceholder />} />
        </Route>

        {/* Admin ONLY — User Management, theo đúng phạm vi quyền hạn trong docx */}
        <Route element={<ProtectedRoute allowedRoles={[ROLES.Admin]} />}>
          <Route path="/users" element={<UserManagementPage />} />
        </Route>
      </Route>

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
