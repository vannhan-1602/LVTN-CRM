import { useEffect, useState } from "react";
import authApi from "../../api/authApi";
import { formatDateTime } from "../../utils/formatters";

function Badge({ label, colorClass }) {
  return (
    <span
      className={`px-2 py-0.5 rounded-full text-xs font-semibold ${colorClass}`}
    >
      {label}
    </span>
  );
}

const ROLE_COLOR = {
  Admin: "bg-red-100 text-red-700",
  Manager: "bg-purple-100 text-purple-700",
  Sale: "bg-blue-100 text-blue-700",
  Accountant: "bg-green-100 text-green-700",
};

const TRANG_THAI_COLOR = {
  Active: "bg-emerald-100 text-emerald-700",
  Inactive: "bg-gray-100 text-gray-500",
  Locked: "bg-red-100 text-red-600",
};

export default function UserListPage() {
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [search, setSearch] = useState("");

  const loadUsers = async () => {
    setLoading(true);
    setError("");
    try {
      const res = await authApi.getUsers();
      setUsers(res.data ?? []);
    } catch (err) {
      setError(err?.message || "Không thể tải danh sách người dùng");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadUsers();
  }, []);

  const filtered = users.filter((u) => {
    const q = search.toLowerCase();
    return (
      !q ||
      u.username?.toLowerCase().includes(q) ||
      u.hoTen?.toLowerCase().includes(q) ||
      u.email?.toLowerCase().includes(q)
    );
  });

  const stats = {
    total: users.length,
    active: users.filter((u) => u.trangThai === "Active").length,
    byRole: users.reduce((acc, u) => {
      acc[u.role] = (acc[u.role] ?? 0) + 1;
      return acc;
    }, {}),
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <p className="text-xs text-gray-500 uppercase tracking-wide">
            CRM / Admin
          </p>
          <h1 className="text-2xl font-bold text-gray-800">
            Quản lý người dùng
          </h1>
        </div>
        <div className="flex gap-2">
          <input
            type="search"
            placeholder="Tìm theo tên, username, email..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="border rounded-lg px-3 py-2 text-sm w-64 focus:outline-none focus:ring-2 focus:ring-blue-400"
          />
          <button
            onClick={loadUsers}
            className="border rounded-lg px-4 py-2 text-sm hover:bg-gray-50"
          >
            Tải lại
          </button>
        </div>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-2 sm:grid-cols-5 gap-4">
        <div className="bg-white rounded-xl border p-4 shadow-sm">
          <p className="text-xs text-gray-500">Tổng người dùng</p>
          <p className="text-2xl font-bold text-gray-800 mt-1">{stats.total}</p>
        </div>
        <div className="bg-white rounded-xl border p-4 shadow-sm">
          <p className="text-xs text-gray-500">Đang hoạt động</p>
          <p className="text-2xl font-bold text-emerald-600 mt-1">
            {stats.active}
          </p>
        </div>
        {["Admin", "Manager", "Sale", "Accountant"].map((role) => (
          <div key={role} className="bg-white rounded-xl border p-4 shadow-sm">
            <p className="text-xs text-gray-500">{role}</p>
            <p className="text-2xl font-bold text-gray-800 mt-1">
              {stats.byRole[role] ?? 0}
            </p>
          </div>
        ))}
      </div>

      {error && (
        <div className="text-sm text-red-600 bg-red-50 rounded-lg p-3">
          {error}
        </div>
      )}

      {/* Table */}
      <div className="bg-white rounded-xl border shadow-sm overflow-hidden">
        <div className="px-6 py-4 border-b flex items-center justify-between">
          <div>
            <h2 className="font-semibold text-gray-800">Danh sách tài khoản</h2>
            <p className="text-xs text-gray-400">
              {filtered.length} người dùng
            </p>
          </div>
          {loading && (
            <span className="text-xs text-gray-400 animate-pulse">
              Đang tải...
            </span>
          )}
        </div>
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 text-xs text-gray-500 uppercase">
              <tr>
                <th className="px-4 py-3 text-left">Username</th>
                <th className="px-4 py-3 text-left">Họ tên</th>
                <th className="px-4 py-3 text-left">Email</th>
                <th className="px-4 py-3 text-left">Vai trò</th>
                <th className="px-4 py-3 text-left">Trạng thái</th>
                <th className="px-4 py-3 text-left">Ngày tạo</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {filtered.length === 0 ? (
                <tr>
                  <td colSpan="6" className="text-center py-10 text-gray-400">
                    {loading ? "Đang tải..." : "Không có dữ liệu"}
                  </td>
                </tr>
              ) : (
                filtered.map((u) => (
                  <tr key={u.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3 font-mono font-semibold text-gray-800">
                      {u.username}
                    </td>
                    <td className="px-4 py-3 font-medium text-gray-900">
                      {u.hoTen ?? "—"}
                    </td>
                    <td className="px-4 py-3 text-gray-600">
                      {u.email ?? "—"}
                    </td>
                    <td className="px-4 py-3">
                      <Badge
                        label={u.role}
                        colorClass={
                          ROLE_COLOR[u.role] ?? "bg-gray-100 text-gray-600"
                        }
                      />
                    </td>
                    <td className="px-4 py-3">
                      <Badge
                        label={u.trangThai}
                        colorClass={
                          TRANG_THAI_COLOR[u.trangThai] ??
                          "bg-gray-100 text-gray-600"
                        }
                      />
                    </td>
                    <td className="px-4 py-3 text-xs text-gray-400">
                      {formatDateTime(u.createdAt)}
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
