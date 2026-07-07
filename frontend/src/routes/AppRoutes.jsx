import { Routes, Route, Navigate } from "react-router-dom";
import LoginPage from "../features/auth/LoginPage";
import MainLayout from "../components/layout/MainLayout";
import ProtectedRoute from "./ProtectedRoute";
import { ROLES } from "../utils/constants";

import CustomerListPage from "../features/customers/CustomerListPage";
import CustomerDetailPage from "../features/customers/CustomerDetailPage";
import LeadListPage from "../features/leads/LeadListPage";
import LeadDetailPage from "../features/leads/LeadDetailPage";
import TicketListPage from "../features/tickets/TicketListPage";
import TicketDetailPage from "../features/tickets/TicketDetailPage";
import UserManagementPage from "../features/users/UserManagementPage";
import UserDetailPage from "../features/users/UserDetailPage";
import ProductListPage from "../features/products/ProductListPage";
import ProductDetailPage from "../features/products/ProductDetailPage";
import QuoteListPage from "../features/quotes/QuoteListPage";
import QuoteDetailPage from "../features/quotes/QuoteDetailPage";
import ContractListPage from "../features/contracts/ContractListPage";
import ContractDetailPage from "../features/contracts/ContractDetailPage";
import OpportunityListPage from "../features/opportunities/OpportunityListPage";
import OpportunityDetailPage from "../features/opportunities/OpportunityDetailPage";
import InvoiceListPage from "../features/invoices/InvoiceListPage";
import InvoiceDetailPage from "../features/invoices/InvoiceDetailPage";
import SettingsPage from "../features/settings/SettingsPage";
import DashboardPage from "../features/dashboard/DashboardPage";

function UnauthorizedPage() {
  return (
    <div className="flex flex-col items-center justify-center h-64 text-center">
      <div className="text-5xl mb-4">🚫</div>
      <h3 className="text-xl font-bold text-danger-600 mb-2">
        403 — Không có quyền truy cập
      </h3>
      <p className="text-ink-500 text-sm">
        Tài khoản của bạn không có quyền xem trang này.
      </p>
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
          <Route path="/" element={<DashboardPage />} />
        </Route>

        {/* Sale + Manager */}
        <Route
          element={
            <ProtectedRoute allowedRoles={[ROLES.Sale, ROLES.Manager]} />
          }
        >
          <Route path="/leads" element={<LeadListPage />} />
          <Route path="/leads/:id" element={<LeadDetailPage />} />
          <Route path="/tickets" element={<TicketListPage />} />
          <Route path="/tickets/:id" element={<TicketDetailPage />} />
          <Route path="/opportunities" element={<OpportunityListPage />} />
          <Route
            path="/opportunities/:id"
            element={<OpportunityDetailPage />}
          />
          <Route path="/quotes" element={<QuoteListPage />} />
          <Route path="/quotes/:id" element={<QuoteDetailPage />} />
          <Route path="/products" element={<ProductListPage />} />
          <Route path="/products/:id" element={<ProductDetailPage />} />
        </Route>

        {/* Customer + Contract: Sale + Manager (đọc/ghi) + Accountant (chỉ đọc) */}
        <Route
          element={
            <ProtectedRoute
              allowedRoles={[ROLES.Sale, ROLES.Manager, ROLES.Accountant]}
            />
          }
        >
          <Route path="/customers" element={<CustomerListPage />} />
          <Route path="/customers/:id" element={<CustomerDetailPage />} />
          <Route path="/contracts" element={<ContractListPage />} />
          <Route path="/contracts/:id" element={<ContractDetailPage />} />
        </Route>

        {/* Hóa đơn: Accountant + Manager */}
        <Route
          element={
            <ProtectedRoute allowedRoles={[ROLES.Accountant, ROLES.Manager]} />
          }
        >
          <Route path="/invoices" element={<InvoiceListPage />} />
          <Route path="/invoices/:id" element={<InvoiceDetailPage />} />
        </Route>

        {/* Admin ONLY */}
        <Route element={<ProtectedRoute allowedRoles={[ROLES.Admin]} />}>
          <Route path="/users" element={<UserManagementPage />} />
          <Route path="/users/:id" element={<UserDetailPage />} />
          <Route path="/settings" element={<SettingsPage />} />
        </Route>
      </Route>

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
