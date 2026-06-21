import { useEffect, useMemo, useState } from "react";
import userManagementApi from "../../api/userManagementApi";
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

const STATUS_COLOR = {
  Active: "bg-emerald-100 text-emerald-700",
  Locked: "bg-red-100 text-red-600",
  Inactive: "bg-gray-100 text-gray-500",
};

const STATUS_LABEL = {
  Active: "Đang hoạt động",
  Locked: "Đã khóa",
  Inactive: "Ngừng hoạt động",
};

const emptyForm = {
  username: "",
  password: "",
  roleId: "",
  hoTen: "",
  email: "",
  soDienThoai: "",
  phongBanId: "",
  chucVuId: "",
};

// ── Modal: tạo / sửa user ────────────────────────────────────────────────────
function UserFormModal({ mode, initial, lookups, onClose, onSaved }) {
  const isEdit = mode === "edit";
  const [form, setForm] = useState(
    isEdit
      ? {
          username: initial.username,
          password: "",
          roleId: initial.roleId ?? "",
          hoTen: initial.hoTen ?? "",
          email: initial.email ?? "",
          soDienThoai: initial.soDienThoai ?? "",
          phongBanId: initial.phongBanId ?? "",
          chucVuId: initial.chucVuId ?? "",
        }
      : emptyForm,
  );
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

  const handleChange = (e) =>
    setForm((f) => ({ ...f, [e.target.name]: e.target.value }));

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!isEdit && (!form.username.trim() || !form.password.trim())) {
      setError("Tên đăng nhập và mật khẩu không được để trống");
      return;
    }
    if (!form.hoTen.trim() || !form.roleId) {
      setError("Họ tên và vai trò là bắt buộc");
      return;
    }
    setSubmitting(true);
    setError("");
    const toInt = (v) => (v === "" || v == null ? null : Number(v));
    try {
      if (isEdit) {
        await userManagementApi.update(initial.id, {
          roleId: Number(form.roleId),
          hoTen: form.hoTen.trim(),
          email: form.email.trim() || null,
          soDienThoai: form.soDienThoai.trim() || null,
          phongBanId: toInt(form.phongBanId),
          chucVuId: toInt(form.chucVuId),
        });
      } else {
        await userManagementApi.create({
          username: form.username.trim(),
          password: form.password,
          roleId: Number(form.roleId),
          hoTen: form.hoTen.trim(),
          email: form.email.trim() || null,
          soDienThoai: form.soDienThoai.trim() || null,
          phongBanId: toInt(form.phongBanId),
          chucVuId: toInt(form.chucVuId),
        });
      }
      onSaved();
    } catch (err) {
      setError(err?.message || "Không thể lưu tài khoản");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
      <form
        onSubmit={handleSubmit}
        className="bg-white rounded-xl shadow-xl w-full max-w-lg p-6 space-y-4 max-h-[90vh] overflow-y-auto"
      >
        <h3 className="font-bold text-lg text-gray-800">
          {isEdit ? `Sửa tài khoản: ${initial.username}` : "Tạo tài khoản mới"}
        </h3>

        {!isEdit && (
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-sm font-medium mb-1">
                Tên đăng nhập <span className="text-red-500">*</span>
              </label>
              <input
                name="username"
                value={form.username}
                onChange={handleChange}
                placeholder="vd: nv_kinhdoanh1"
                className="w-full border rounded-lg px-3 py-2 text-sm"
              />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">
                Mật khẩu <span className="text-red-500">*</span>
              </label>
              <input
                name="password"
                type="password"
                value={form.password}
                onChange={handleChange}
                placeholder="Tối thiểu 6 ký tự"
                className="w-full border rounded-lg px-3 py-2 text-sm"
              />
            </div>
          </div>
        )}

        <div>
          <label className="block text-sm font-medium mb-1">
            Vai trò <span className="text-red-500">*</span>
          </label>
          <select
            name="roleId"
            value={form.roleId}
            onChange={handleChange}
            className="w-full border rounded-lg px-3 py-2 text-sm"
          >
            <option value="">-- Chọn vai trò --</option>
            {lookups.roles.map((r) => (
              <option key={r.id} value={r.id}>
                {r.tenRole}
              </option>
            ))}
          </select>
        </div>

        <div>
          <label className="block text-sm font-medium mb-1">
            Họ tên <span className="text-red-500">*</span>
          </label>
          <input
            name="hoTen"
            value={form.hoTen}
            onChange={handleChange}
            placeholder="Nguyễn Văn A"
            className="w-full border rounded-lg px-3 py-2 text-sm"
          />
        </div>

        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="block text-sm font-medium mb-1">Email</label>
            <input
              name="email"
              type="email"
              value={form.email}
              onChange={handleChange}
              placeholder="email@crm.vn"
              className="w-full border rounded-lg px-3 py-2 text-sm"
            />
          </div>
          <div>
            <label className="block text-sm font-medium mb-1">
              Số điện thoại
            </label>
            <input
              name="soDienThoai"
              value={form.soDienThoai}
              onChange={handleChange}
              placeholder="0901234567"
              className="w-full border rounded-lg px-3 py-2 text-sm"
            />
          </div>
        </div>

        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="block text-sm font-medium mb-1">Phòng ban</label>
            <select
              name="phongBanId"
              value={form.phongBanId}
              onChange={handleChange}
              className="w-full border rounded-lg px-3 py-2 text-sm"
            >
              <option value="">-- Chọn --</option>
              {lookups.phongBans.map((p) => (
                <option key={p.id} value={p.id}>
                  {p.tenPhongBan}
                </option>
              ))}
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium mb-1">Chức vụ</label>
            <select
              name="chucVuId"
              value={form.chucVuId}
              onChange={handleChange}
              className="w-full border rounded-lg px-3 py-2 text-sm"
            >
              <option value="">-- Chọn --</option>
              {lookups.chucVus.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.tenChucVu}
                </option>
              ))}
            </select>
          </div>
        </div>

        {error && (
          <div className="text-sm text-red-600 bg-red-50 rounded p-2">
            {error}
          </div>
        )}

        <div className="flex gap-2 pt-2">
          <button
            type="submit"
            disabled={submitting}
            className="flex-1 bg-blue-600 text-white rounded-lg py-2 text-sm font-medium hover:bg-blue-700 disabled:opacity-50"
          >
            {submitting ? "Đang lưu..." : isEdit ? "Cập nhật" : "Tạo tài khoản"}
          </button>
          <button
            type="button"
            onClick={onClose}
            className="px-4 border rounded-lg py-2 text-sm hover:bg-gray-50"
          >
            Hủy
          </button>
        </div>
      </form>
    </div>
  );
}

// ── Modal: đặt lại mật khẩu ──────────────────────────────────────────────────
function ResetPasswordModal({ user, onClose, onDone }) {
  const [password, setPassword] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (password.length < 6) {
      setError("Mật khẩu tối thiểu 6 ký tự");
      return;
    }
    setSubmitting(true);
    setError("");
    try {
      await userManagementApi.resetPassword(user.id, password);
      onDone();
    } catch (err) {
      setError(err?.message || "Đặt lại mật khẩu thất bại");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
      <form
        onSubmit={handleSubmit}
        className="bg-white rounded-xl shadow-xl w-full max-w-sm p-6 space-y-4"
      >
        <h3 className="font-bold text-lg text-gray-800">Đặt lại mật khẩu</h3>
        <p className="text-sm text-gray-500">
          Tài khoản: <strong>{user.username}</strong>
        </p>
        <input
          type="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          placeholder="Mật khẩu mới (tối thiểu 6 ký tự)"
          className="w-full border rounded-lg px-3 py-2 text-sm"
          autoFocus
        />
        {error && (
          <div className="text-sm text-red-600 bg-red-50 rounded p-2">
            {error}
          </div>
        )}
        <div className="flex gap-2">
          <button
            type="submit"
            disabled={submitting}
            className="flex-1 bg-blue-600 text-white rounded-lg py-2 text-sm font-medium hover:bg-blue-700 disabled:opacity-50"
          >
            {submitting ? "Đang lưu..." : "Đặt lại mật khẩu"}
          </button>
          <button
            type="button"
            onClick={onClose}
            className="px-4 border rounded-lg py-2 text-sm hover:bg-gray-50"
          >
            Hủy
          </button>
        </div>
      </form>
    </div>
  );
}

export default function UserManagementPage() {
  const [users, setUsers] = useState([]);
  const [lookups, setLookups] = useState({
    roles: [],
    phongBans: [],
    chucVus: [],
  });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [search, setSearch] = useState("");

  const [formModal, setFormModal] = useState(null); // { mode: 'create'|'edit', user? }
  const [resetModalUser, setResetModalUser] = useState(null);

  const loadAll = async () => {
    setLoading(true);
    setError("");
    try {
      const [usersRes, lookupsRes] = await Promise.all([
        userManagementApi.getAll(),
        userManagementApi.getLookups(),
      ]);
      setUsers(usersRes.data ?? []);
      setLookups(lookupsRes.data ?? { roles: [], phongBans: [], chucVus: [] });
    } catch (err) {
      setError(err?.message || "Không thể tải dữ liệu");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadAll();
  }, []);

  const filtered = useMemo(() => {
    const q = search.toLowerCase();
    return users.filter(
      (u) =>
        !q ||
        u.username?.toLowerCase().includes(q) ||
        u.hoTen?.toLowerCase().includes(q) ||
        u.email?.toLowerCase().includes(q),
    );
  }, [users, search]);

  const stats = useMemo(
    () => ({
      total: users.length,
      active: users.filter((u) => u.trangThai === "Active").length,
      byRole: users.reduce((acc, u) => {
        acc[u.roleName] = (acc[u.roleName] ?? 0) + 1;
        return acc;
      }, {}),
    }),
    [users],
  );

  const handleToggleStatus = async (user) => {
    const next = user.trangThai === "Active" ? "Locked" : "Active";
    const verb = next === "Locked" ? "khóa" : "mở khóa";
    if (
      !window.confirm(`Bạn có chắc muốn ${verb} tài khoản "${user.username}"?`)
    )
      return;
    try {
      await userManagementApi.toggleStatus(user.id, next);
      setSuccess(`Đã ${verb} tài khoản ${user.username}`);
      await loadAll();
    } catch (err) {
      setError(err?.message || "Không thể đổi trạng thái");
    }
  };

  const handleDelete = async (user) => {
    if (
      !window.confirm(
        `Xóa vĩnh viễn tài khoản "${user.username}"? Hành động này không thể hoàn tác.`,
      )
    )
      return;
    try {
      await userManagementApi.delete(user.id);
      setSuccess("Xóa tài khoản thành công");
      await loadAll();
    } catch (err) {
      setError(err?.message || "Không thể xóa tài khoản");
    }
  };

  return (
    <div className="space-y-6">
      {formModal && (
        <UserFormModal
          mode={formModal.mode}
          initial={formModal.user}
          lookups={lookups}
          onClose={() => setFormModal(null)}
          onSaved={() => {
            setFormModal(null);
            setSuccess(
              formModal.mode === "edit"
                ? "Cập nhật thành công"
                : "Tạo tài khoản thành công",
            );
            loadAll();
          }}
        />
      )}
      {resetModalUser && (
        <ResetPasswordModal
          user={resetModalUser}
          onClose={() => setResetModalUser(null)}
          onDone={() => {
            setResetModalUser(null);
            setSuccess("Đặt lại mật khẩu thành công");
          }}
        />
      )}

      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <p className="text-xs text-gray-500 uppercase tracking-wide">
            CRM / Admin
          </p>
          <h1 className="text-2xl font-bold text-gray-800">
            Quản lý người dùng & nhân sự
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
            onClick={() => setFormModal({ mode: "create" })}
            className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm font-medium hover:bg-blue-700"
          >
            + Tạo tài khoản
          </button>
        </div>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-2 sm:grid-cols-6 gap-4">
        <div className="bg-white rounded-xl border p-4 shadow-sm">
          <p className="text-xs text-gray-500">Tổng tài khoản</p>
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
      {success && (
        <div className="text-sm text-green-700 bg-green-50 rounded-lg p-3">
          {success}
        </div>
      )}

      {/* Table */}
      <div className="bg-white rounded-xl border shadow-sm overflow-hidden">
        <div className="px-6 py-4 border-b flex items-center justify-between">
          <div>
            <h2 className="font-semibold text-gray-800">Danh sách tài khoản</h2>
            <p className="text-xs text-gray-400">{filtered.length} tài khoản</p>
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
                <th className="px-4 py-3 text-left">Liên hệ</th>
                <th className="px-4 py-3 text-left">Phòng ban / Chức vụ</th>
                <th className="px-4 py-3 text-left">Vai trò</th>
                <th className="px-4 py-3 text-left">Trạng thái</th>
                <th className="px-4 py-3 text-left">Hành động</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {filtered.length === 0 ? (
                <tr>
                  <td colSpan="7" className="text-center py-10 text-gray-400">
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
                    <td className="px-4 py-3">
                      <div className="text-gray-600">{u.email || "—"}</div>
                      {u.soDienThoai && (
                        <div className="text-xs text-gray-400">
                          {u.soDienThoai}
                        </div>
                      )}
                    </td>
                    <td className="px-4 py-3 text-gray-600">
                      <div>{u.tenPhongBan || "—"}</div>
                      {u.tenChucVu && (
                        <div className="text-xs text-gray-400">
                          {u.tenChucVu}
                        </div>
                      )}
                    </td>
                    <td className="px-4 py-3">
                      <Badge
                        label={u.roleName ?? "—"}
                        colorClass={
                          ROLE_COLOR[u.roleName] ?? "bg-gray-100 text-gray-600"
                        }
                      />
                    </td>
                    <td className="px-4 py-3">
                      <Badge
                        label={STATUS_LABEL[u.trangThai] ?? u.trangThai}
                        colorClass={
                          STATUS_COLOR[u.trangThai] ??
                          "bg-gray-100 text-gray-600"
                        }
                      />
                    </td>
                    <td className="px-4 py-3">
                      <div className="flex gap-2 flex-wrap">
                        <button
                          onClick={() =>
                            setFormModal({ mode: "edit", user: u })
                          }
                          className="text-blue-600 hover:underline text-xs font-medium"
                        >
                          Sửa
                        </button>
                        <button
                          onClick={() => setResetModalUser(u)}
                          className="text-amber-600 hover:underline text-xs font-medium"
                        >
                          Đổi MK
                        </button>
                        <button
                          onClick={() => handleToggleStatus(u)}
                          className="text-orange-600 hover:underline text-xs font-medium"
                        >
                          {u.trangThai === "Active" ? "Khóa" : "Mở khóa"}
                        </button>
                        <button
                          onClick={() => handleDelete(u)}
                          className="text-red-500 hover:underline text-xs font-medium"
                        >
                          Xóa
                        </button>
                      </div>
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
