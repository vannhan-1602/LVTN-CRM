import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { Users, Plus, Search, Eye, KeyRound, Lock, Unlock, Trash2 } from "lucide-react";
import userManagementApi from "../../api/userManagementApi";
import PageHeader from "../../components/common/PageHeader";
import StatCard from "../../components/common/StatCard";
import EmptyState from "../../components/common/EmptyState";
import RowMenu from "../../components/common/RowMenu";
import Badge from "../../components/common/Badge";
import Button from "../../components/common/Button";
import UserFormModal from "./UserFormModal";
import ResetPasswordModal from "./ResetPasswordModal";
import { USER_ROLE_COLOR, USER_STATUS_LABEL, USER_STATUS_COLOR } from "../../utils/constants";

export default function UserManagementPage() {
  const navigate = useNavigate();
  const [users, setUsers] = useState([]);
  const [lookups, setLookups] = useState({ roles: [], phongBans: [], chucVus: [] });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [search, setSearch] = useState("");

  const [showCreateModal, setShowCreateModal] = useState(false);
  const [editingUser, setEditingUser] = useState(null);
  const [resetModalUser, setResetModalUser] = useState(null);

  const loadAll = async () => {
    setLoading(true); setError("");
    try {
      const [usersRes, lookupsRes] = await Promise.all([userManagementApi.getAll(), userManagementApi.getLookups()]);
      setUsers(usersRes.data ?? []);
      setLookups(lookupsRes.data ?? { roles: [], phongBans: [], chucVus: [] });
    } catch (err) { setError(err?.message || "Không thể tải dữ liệu"); }
    finally { setLoading(false); }
  };

  useEffect(() => { loadAll(); }, []);

  const filtered = useMemo(() => {
    const q = search.toLowerCase();
    return users.filter((u) =>
      !q || u.username?.toLowerCase().includes(q) || u.hoTen?.toLowerCase().includes(q) || u.email?.toLowerCase().includes(q),
    );
  }, [users, search]);

  const stats = useMemo(
    () => ({
      total: users.length,
      active: users.filter((u) => u.trangThai === "Active").length,
      locked: users.filter((u) => u.trangThai === "Locked").length,
    }),
    [users],
  );

  const handleToggleStatus = async (user) => {
    const next = user.trangThai === "Active" ? "Locked" : "Active";
    const verb = next === "Locked" ? "khóa" : "mở khóa";
    if (!window.confirm(`Bạn có chắc muốn ${verb} tài khoản "${user.username}"?`)) return;
    try { await userManagementApi.toggleStatus(user.id, next); setSuccess(`Đã ${verb} tài khoản ${user.username}`); await loadAll(); }
    catch (err) { setError(err?.message || "Không thể đổi trạng thái"); }
  };

  const handleDelete = async (user) => {
    if (!window.confirm(`Xóa vĩnh viễn tài khoản "${user.username}"? Hành động này không thể hoàn tác.`)) return;
    try { await userManagementApi.delete(user.id); setSuccess("Xóa tài khoản thành công"); await loadAll(); }
    catch (err) { setError(err?.message || "Không thể xóa tài khoản"); }
  };

  return (
    <div className="space-y-5">
      {showCreateModal && (
        <UserFormModal lookups={lookups} onClose={() => setShowCreateModal(false)}
          onSaved={() => { setShowCreateModal(false); setSuccess("Tạo tài khoản thành công"); loadAll(); }} />
      )}
      {editingUser && (
        <UserFormModal user={editingUser} lookups={lookups} onClose={() => setEditingUser(null)}
          onSaved={() => { setEditingUser(null); setSuccess("Cập nhật thành công"); loadAll(); }} />
      )}
      {resetModalUser && (
        <ResetPasswordModal user={resetModalUser} onClose={() => setResetModalUser(null)}
          onDone={() => { setResetModalUser(null); setSuccess("Đặt lại mật khẩu thành công"); }} />
      )}

      <PageHeader
        breadcrumb="CRM / Admin"
        title="Quản lý người dùng & nhân sự"
        actions={<Button icon={Plus} onClick={() => setShowCreateModal(true)}>Tạo tài khoản</Button>}
      />

      <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
        <StatCard label="Tổng tài khoản" value={stats.total} icon={Users} />
        <StatCard label="Đang hoạt động" value={stats.active} tone="success" icon={Unlock} />
        <StatCard label="Đã khóa" value={stats.locked} tone="warning" icon={Lock} />
      </div>

      {error && <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-3">{error}</div>}
      {success && <div className="text-sm text-success-700 bg-success-50 rounded-lg p-3">{success}</div>}

      <div className="bg-surface rounded-card border border-ink-100 overflow-hidden">
        <div className="px-5 py-3.5 border-b border-ink-100">
          <div className="relative w-72">
            <Search size={15} className="absolute left-3 top-1/2 -translate-y-1/2 text-ink-400" />
            <input
              type="search" placeholder="Tìm theo tên, username, email..."
              value={search} onChange={(e) => setSearch(e.target.value)}
              className="border border-ink-200 rounded-lg pl-9 pr-3 py-2 text-sm w-full focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
            />
          </div>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-surface-alt">
                <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">Username</th>
                <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">Họ tên</th>
                <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">Liên hệ</th>
                <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">Phòng ban / Chức vụ</th>
                <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">Vai trò</th>
                <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">Trạng thái</th>
                <th className="w-12"></th>
              </tr>
            </thead>
            <tbody className="divide-y divide-ink-100">
              {filtered.length === 0 ? (
                <tr>
                  <td colSpan={7}>
                    <EmptyState icon={Users} title={loading ? "Đang tải..." : "Không có dữ liệu"} />
                  </td>
                </tr>
              ) : filtered.map((u) => (
                <tr key={u.id} className="hover:bg-surface-alt cursor-pointer transition-colors"
                  onClick={() => navigate(`/users/${u.id}`)}>
                  <td className="px-5 py-3.5 font-mono font-semibold text-ink-900">{u.username}</td>
                  <td className="px-5 py-3.5 font-medium text-ink-900">{u.hoTen ?? "—"}</td>
                  <td className="px-5 py-3.5 text-ink-700">
                    <div>{u.email || "—"}</div>
                    {u.soDienThoai && <div className="text-xs text-ink-400">{u.soDienThoai}</div>}
                  </td>
                  <td className="px-5 py-3.5 text-ink-700">
                    <div>{u.tenPhongBan || "—"}</div>
                    {u.tenChucVu && <div className="text-xs text-ink-400">{u.tenChucVu}</div>}
                  </td>
                  <td className="px-5 py-3.5"><Badge label={u.roleName ?? "—"} colorClass={USER_ROLE_COLOR[u.roleName]} /></td>
                  <td className="px-5 py-3.5">
                    <Badge label={USER_STATUS_LABEL[u.trangThai] ?? u.trangThai} colorClass={USER_STATUS_COLOR[u.trangThai]} />
                  </td>
                  <td className="px-3 py-3.5 text-center" onClick={(e) => e.stopPropagation()}>
                    <RowMenu
                      items={[
                        { label: "Xem chi tiết", icon: Eye, onClick: () => navigate(`/users/${u.id}`) },
                        { label: "Đặt lại mật khẩu", icon: KeyRound, onClick: () => setResetModalUser(u) },
                        {
                          label: u.trangThai === "Active" ? "Khóa tài khoản" : "Mở khóa",
                          icon: u.trangThai === "Active" ? Lock : Unlock,
                          onClick: () => handleToggleStatus(u),
                        },
                        { label: "Xóa", icon: Trash2, danger: true, onClick: () => handleDelete(u) },
                      ]}
                    />
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        <div className="px-5 py-3.5 border-t border-ink-100">
          <p className="text-xs text-ink-400">{filtered.length} tài khoản</p>
        </div>
      </div>
    </div>
  );
}
