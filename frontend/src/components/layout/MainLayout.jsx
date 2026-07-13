import { useState } from "react";
import { Outlet, NavLink, useNavigate } from "react-router-dom";
import {
  LayoutDashboard,
  Users,
  Target,
  Package,
  Receipt,
  FileText,
  Headset,
  Wallet,
  UserCog,
  LogOut,
  Settings,
  TrendingUp,
  KeyRound,
  ScrollText,
} from "lucide-react";
import useAuthStore from "../../features/auth/authStore";
import ChangePasswordModal from "../../features/auth/ChangePasswordModal";
import { ROLES } from "../../utils/constants";

function SidebarLink({ to, icon: Icon, children }) {
  return (
    <NavLink
      to={to}
      className={({ isActive }) =>
        `flex items-center gap-2.5 px-3 py-2.5 rounded-lg text-sm transition-colors mb-0.5 ${
          isActive
            ? "bg-accent-500/15 text-white font-medium"
            : "text-brand-100/70 hover:bg-white/5 hover:text-white"
        }`
      }
    >
      {({ isActive }) => (
        <>
          <Icon size={17} className={isActive ? "text-accent-400" : ""} />
          {children}
        </>
      )}
    </NavLink>
  );
}

function SidebarSection({ children }) {
  return (
    <div className="pt-4 pb-1.5 px-3 text-[11px] font-semibold text-brand-100/40 uppercase tracking-wider">
      {children}
    </div>
  );
}

function SidebarDivider() {
  return <div className="my-1 border-t border-white/10" />;
}

export default function MainLayout() {
  const user = useAuthStore((s) => s.user);
  const logout = useAuthStore((s) => s.logout);
  const navigate = useNavigate();
  const [showChangePassword, setShowChangePassword] = useState(false);

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  const isSalesTeam = [ROLES.Sale, ROLES.Manager].includes(user?.role);
  const isFinanceTeam = [ROLES.Accountant, ROLES.Manager].includes(user?.role);
  const isAdmin = user?.role === ROLES.Admin;

  const initials = (user?.hoTen || user?.username || "??")
    .split(" ")
    .map((p) => p[0])
    .slice(-2)
    .join("")
    .toUpperCase();

  const scopeLabel =
    (isAdmin && "Phạm vi: Quản trị hệ thống") ||
    (user?.role === ROLES.Manager && "Phạm vi: Toàn đội kinh doanh") ||
    (user?.role === ROLES.Sale && "Phạm vi: Dữ liệu phụ trách") ||
    (user?.role === ROLES.Accountant && "Phạm vi: Nghiệp vụ kế toán");

  return (
    <div className="flex h-screen bg-canvas overflow-hidden">
      <aside className="w-60 bg-brand-700 flex flex-col flex-shrink-0">
        {/* Logo */}
        <div className="px-5 py-5 border-b border-white/10">
          <div className="flex items-center gap-2.5">
            <div className="w-8 h-8 rounded-lg bg-accent-500 flex items-center justify-center text-white text-sm font-semibold">
              C
            </div>
            <span className="text-white font-semibold text-[15px] tracking-wide">
              CRM System
            </span>
          </div>
        </div>

        <nav className="flex-1 px-3 py-3 overflow-y-auto">
          <SidebarLink to="/" icon={LayoutDashboard}>
            Dashboard
          </SidebarLink>

          {/* KINH DOANH: Sale + Manager */}
          {isSalesTeam && (
            <>
              <SidebarSection>Kinh doanh</SidebarSection>
              <SidebarLink to="/leads" icon={Target}>
                Quản lý Lead
              </SidebarLink>
              <SidebarLink to="/customers" icon={Users}>
                Khách hàng
              </SidebarLink>
              <SidebarLink to="/opportunities" icon={TrendingUp}>
                Cơ hội bán hàng
              </SidebarLink>
              <SidebarLink to="/products" icon={Package}>
                Sản phẩm / Dịch vụ
              </SidebarLink>
              <SidebarLink to="/quotes" icon={Receipt}>
                Báo giá
              </SidebarLink>
              <SidebarLink to="/contracts" icon={FileText}>
                Hợp đồng
              </SidebarLink>
              <SidebarLink to="/tickets" icon={Headset}>
                Hỗ trợ (Ticket)
              </SidebarLink>
            </>
          )}

          {/* KẾ TOÁN: Accountant + Manager */}
          {isFinanceTeam && (
            <>
              <SidebarSection>Kế toán</SidebarSection>
              <SidebarLink to="/invoices" icon={Wallet}>
                Hóa đơn & Công nợ
              </SidebarLink>
              <SidebarLink to="/phieu-thu-chi" icon={Receipt}>
                Phiếu thu / Phiếu chi
              </SidebarLink>
              {/* Accountant chỉ xem Customer + Contract + Quote (read-only) */}
              {user?.role === ROLES.Accountant && (
                <>
                  <SidebarLink to="/customers" icon={Users}>
                    Khách hàng (xem)
                  </SidebarLink>
                  <SidebarLink to="/quotes" icon={Receipt}>
                    Báo giá (xem)
                  </SidebarLink>
                  <SidebarLink to="/contracts" icon={FileText}>
                    Hợp đồng (xem)
                  </SidebarLink>
                </>
              )}
            </>
          )}

          {/* ADMIN: chỉ quản trị hệ thống */}
          {isAdmin && (
            <>
              <SidebarSection>Quản trị hệ thống</SidebarSection>
              <SidebarLink to="/users" icon={UserCog}>
                Người dùng & Nhân sự
              </SidebarLink>
              <SidebarLink to="/settings" icon={Settings}>
                Cài đặt danh mục
              </SidebarLink>
              <SidebarLink to="/audit-log" icon={ScrollText}>
                Nhật ký hệ thống
              </SidebarLink>
            </>
          )}
        </nav>

        <div className="px-4 py-3.5 border-t border-white/10 text-[11px] text-brand-100/40">
          {scopeLabel}
        </div>
      </aside>

      <div className="flex-1 flex flex-col min-w-0">
        <header className="bg-surface border-b border-ink-100 px-6 py-3 flex items-center justify-between flex-shrink-0">
          <div className="flex items-center gap-3">
            <div className="w-8 h-8 rounded-full bg-info-50 flex items-center justify-center text-xs font-semibold text-info-700">
              {initials}
            </div>
            <div>
              <p className="text-sm font-medium text-ink-900 leading-tight">
                {user?.hoTen || user?.username}
              </p>
            </div>
            <span className="text-xs bg-success-50 text-success-700 px-2.5 py-1 rounded-full font-medium">
              {user?.role}
            </span>
          </div>
          <div className="flex items-center gap-2">
            <button
              onClick={() => setShowChangePassword(true)}
              className="inline-flex items-center gap-1.5 text-sm text-ink-500 hover:text-ink-900 hover:bg-ink-100 px-3 py-1.5 rounded-lg transition-colors"
            >
              <KeyRound size={15} />
              Đổi mật khẩu
            </button>
            <button
              onClick={handleLogout}
              className="inline-flex items-center gap-1.5 text-sm text-ink-500 hover:text-danger-600 hover:bg-danger-50 px-3 py-1.5 rounded-lg transition-colors"
            >
              <LogOut size={15} />
              Đăng xuất
            </button>
          </div>
        </header>

        <main className="flex-1 overflow-y-auto bg-canvas p-6">
          <Outlet />
        </main>
      </div>

      {showChangePassword && (
        <ChangePasswordModal onClose={() => setShowChangePassword(false)} />
      )}
    </div>
  );
}
