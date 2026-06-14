import { Outlet, Link, useNavigate } from "react-router-dom";
import useAuthStore from "../../features/auth/authStore";
import { ROLES } from "../../utils/constants";

export default function MainLayout() {
  // Lấy state từ Zustand chuẩn xác
  const user = useAuthStore((state) => state.user);
  const logout = useAuthStore((state) => state.logout);
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  return (
    <div className="flex h-screen bg-gray-100">
      {/* Sidebar - Đổi sang Tailwind: bg-gray-900 (màu tối) */}
      <div className="w-64 bg-gray-900 text-white flex flex-col shadow-xl">
        <div className="p-5 text-2xl font-bold border-b border-gray-800 text-center tracking-wide">
          CRM SYSTEM
        </div>

        <nav className="flex-1 px-4 py-6 space-y-2 overflow-y-auto">
          <Link
            to="/"
            className="block px-4 py-3 rounded-lg hover:bg-gray-800 transition-colors font-medium"
          >
            Dashboard
          </Link>

          {/* Menu cho khối Kinh doanh & Quản lý */}
          {[ROLES.Sale, ROLES.Manager, ROLES.Admin].includes(user?.role) && (
            <>
              <Link
                to="/leads"
                className="block px-4 py-3 rounded-lg hover:bg-gray-800 transition-colors font-medium"
              >
                Quản lý Lead
              </Link>
              <Link
                to="/customers"
                className="block px-4 py-3 rounded-lg hover:bg-gray-800 transition-colors font-medium"
              >
                Khách hàng
              </Link>
              <Link
                to="/tickets"
                className="block px-4 py-3 rounded-lg hover:bg-gray-800 transition-colors font-medium"
              >
                Hỗ trợ (Tickets)
              </Link>
            </>
          )}

          {/* Menu cho Khối Kế toán */}
          {[ROLES.Accountant, ROLES.Manager, ROLES.Admin].includes(
            user?.role,
          ) && (
            <Link
              to="/invoices"
              className="block px-4 py-3 rounded-lg hover:bg-gray-800 transition-colors font-medium"
            >
              Hóa đơn & Công nợ
            </Link>
          )}

          {/* Menu cho Admin */}
          {user?.role === ROLES.Admin && (
            <Link
              to="/users"
              className="block px-4 py-3 rounded-lg hover:bg-gray-800 transition-colors font-medium"
            >
              Quản lý Người dùng
            </Link>
          )}
        </nav>
      </div>

      {/* Main Content Area */}
      <div className="flex-1 flex flex-col overflow-hidden">
        {/* Header */}
        <header className="bg-white shadow-sm px-8 py-4 flex justify-between items-center z-10">
          <h5 className="text-lg font-semibold text-gray-800 m-0">
            Xin chào, {user?.hoTen}{" "}
            <span className="text-sm font-normal text-gray-500 bg-gray-100 px-2 py-1 rounded ml-2">
              {user?.role}
            </span>
          </h5>
          <button
            className="px-4 py-2 text-sm font-medium text-red-600 border border-red-200 hover:bg-red-50 rounded-lg transition-colors"
            onClick={handleLogout}
          >
            Đăng xuất
          </button>
        </header>

        {/* Render nội dung các trang con */}
        <main className="flex-1 overflow-x-hidden overflow-y-auto bg-gray-50 p-8">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
