import { Outlet, NavLink, useNavigate } from "react-router-dom";
import useAuthStore from "../../features/auth/authStore";
import { ROLES } from "../../utils/constants";

function SidebarLink({ to, children }) {
  return (
    <NavLink
      to={to}
      className={({ isActive }) =>
        `block px-4 py-3 rounded-lg text-sm font-medium transition-colors ${
          isActive
            ? "bg-blue-600 text-white"
            : "text-gray-300 hover:bg-gray-800 hover:text-white"
        }`
      }
    >
      {children}
    </NavLink>
  );
}

export default function MainLayout() {
  const user = useAuthStore((s) => s.user);
  const logout = useAuthStore((s) => s.logout);
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  // ✅ Theo đúng bảng phân quyền trong docx:
  // - Sale: Lead, Customer, Opportunity, Quote, Contract, Ticket (phạm vi phụ trách)
  // - Manager: toàn bộ quyền Sale (toàn đội) + Dashboard/Report/AI Analysis
  // - Accountant: Invoice, Payment, Debt Tracking + xem (read-only) Customer/Contract
  // - Admin: CHỈ User Management, Role & Permission, Audit Log, nhân sự/phòng ban
  //          KHÔNG truy cập nghiệp vụ kinh doanh hay kế toán
  const isSalesTeam = [ROLES.Sale, ROLES.Manager].includes(user?.role);
  const isFinanceTeam = [ROLES.Accountant, ROLES.Manager].includes(user?.role);
  const isAdmin = user?.role === ROLES.Admin;

  return (
    <div className="flex h-screen bg-gray-100 overflow-hidden">
      <aside className="w-56 bg-gray-900 flex flex-col flex-shrink-0">
        <div className="px-5 py-5 border-b border-gray-800">
          <span className="text-white font-bold text-lg tracking-wide">
            CRM SYSTEM
          </span>
        </div>

        <nav className="flex-1 px-3 py-4 space-y-1 overflow-y-auto">
          <SidebarLink to="/">Dashboard</SidebarLink>

          {isSalesTeam && (
            <>
              <div className="pt-3 pb-1 px-4 text-xs font-semibold text-gray-500 uppercase tracking-wider">
                Kinh doanh
              </div>
              <SidebarLink to="/leads">Quản lý Lead</SidebarLink>
              <SidebarLink to="/customers">Khách hàng</SidebarLink>
              <SidebarLink to="/products">Sản phẩm / Dịch vụ</SidebarLink>
              <SidebarLink to="/quotes">Báo giá</SidebarLink>
              <SidebarLink to="/contracts">Hợp đồng</SidebarLink>
              <SidebarLink to="/tickets">Hỗ trợ (Ticket)</SidebarLink>
            </>
          )}

          {isFinanceTeam && (
            <>
              <div className="pt-3 pb-1 px-4 text-xs font-semibold text-gray-500 uppercase tracking-wider">
                Kế toán
              </div>
              <SidebarLink to="/invoices">Hóa đơn & Công nợ</SidebarLink>
              {/* Accountant xem Customer/Contract (chỉ đọc); Manager đã có 2 link này ở mục Kinh doanh nên chỉ thêm cho Accountant */}
              {user?.role === ROLES.Accountant && (
                <>
                  <SidebarLink to="/customers">Khách hàng (xem)</SidebarLink>
                  <SidebarLink to="/contracts">Hợp đồng (xem)</SidebarLink>
                </>
              )}
            </>
          )}

          {isAdmin && (
            <>
              <div className="pt-3 pb-1 px-4 text-xs font-semibold text-gray-500 uppercase tracking-wider">
                Quản trị hệ thống
              </div>
              <SidebarLink to="/users">Người dùng & Nhân sự</SidebarLink>
            </>
          )}
        </nav>

        <div className="px-4 py-3 border-t border-gray-800 text-xs text-gray-500">
          {isAdmin && "Phạm vi: Quản trị hệ thống"}
          {user?.role === ROLES.Manager && "Phạm vi: Toàn đội kinh doanh"}
          {user?.role === ROLES.Sale && "Phạm vi: Dữ liệu phụ trách"}
          {user?.role === ROLES.Accountant && "Phạm vi: Nghiệp vụ kế toán"}
        </div>
      </aside>

      <div className="flex-1 flex flex-col min-w-0">
        <header className="bg-white border-b px-6 py-3 flex items-center justify-between flex-shrink-0">
          <div className="flex items-center gap-3">
            <span className="font-semibold text-gray-800">
              {user?.hoTen || user?.username}
            </span>
            <span className="text-xs bg-gray-100 text-gray-600 px-2 py-0.5 rounded-full font-medium">
              {user?.role}
            </span>
          </div>
          <button
            onClick={handleLogout}
            className="text-sm text-red-600 border border-red-200 hover:bg-red-50 px-4 py-1.5 rounded-lg transition-colors"
          >
            Đăng xuất
          </button>
        </header>

        <main className="flex-1 overflow-y-auto bg-gray-50 p-6">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
