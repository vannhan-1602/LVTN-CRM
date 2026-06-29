import { useState } from "react";
import userManagementApi from "../../api/userManagementApi";
import Modal from "../../components/common/Modal";
import Button from "../../components/common/Button";

const emptyForm = {
  username: "", password: "", roleId: "", hoTen: "", email: "",
  soDienThoai: "", phongBanId: "", chucVuId: "",
};

const toInt = (v) => (v === "" || v == null ? null : Number(v));

// Modal Tạo/Sửa tài khoản người dùng. Có prop `user` => chế độ Sửa (không đổi username/password ở đây).
export default function UserFormModal({ user, lookups, onClose, onSaved }) {
  const isEdit = Boolean(user);
  const [form, setForm] = useState(
    isEdit
      ? {
          username: user.username, password: "", roleId: user.roleId ?? "",
          hoTen: user.hoTen ?? "", email: user.email ?? "", soDienThoai: user.soDienThoai ?? "",
          phongBanId: user.phongBanId ?? "", chucVuId: user.chucVuId ?? "",
        }
      : emptyForm,
  );
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

  const handleChange = (e) => setForm((f) => ({ ...f, [e.target.name]: e.target.value }));

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!isEdit && (!form.username.trim() || !form.password.trim())) {
      setError("Tên đăng nhập và mật khẩu không được để trống"); return;
    }
    if (!form.hoTen.trim() || !form.roleId) { setError("Họ tên và vai trò là bắt buộc"); return; }

    setSubmitting(true); setError("");
    try {
      if (isEdit) {
        await userManagementApi.update(user.id, {
          roleId: Number(form.roleId), hoTen: form.hoTen.trim(),
          email: form.email.trim() || null, soDienThoai: form.soDienThoai.trim() || null,
          phongBanId: toInt(form.phongBanId), chucVuId: toInt(form.chucVuId),
        });
      } else {
        await userManagementApi.create({
          username: form.username.trim(), password: form.password,
          roleId: Number(form.roleId), hoTen: form.hoTen.trim(),
          email: form.email.trim() || null, soDienThoai: form.soDienThoai.trim() || null,
          phongBanId: toInt(form.phongBanId), chucVuId: toInt(form.chucVuId),
        });
      }
      onSaved();
    } catch (err) { setError(err?.message || "Không thể lưu tài khoản"); }
    finally { setSubmitting(false); }
  };

  return (
    <Modal isOpen onClose={onClose} title={isEdit ? `Sửa tài khoản: ${user.username}` : "Tạo tài khoản mới"} size="md">
      <form onSubmit={handleSubmit} className="space-y-4">
        {!isEdit && (
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-sm font-medium text-ink-700 mb-1.5">
                Tên đăng nhập <span className="text-danger-500">*</span>
              </label>
              <input name="username" value={form.username} onChange={handleChange} placeholder="vd: nv_kinhdoanh1"
                className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400" />
            </div>
            <div>
              <label className="block text-sm font-medium text-ink-700 mb-1.5">
                Mật khẩu <span className="text-danger-500">*</span>
              </label>
              <input name="password" type="password" value={form.password} onChange={handleChange} placeholder="Tối thiểu 6 ký tự"
                className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400" />
            </div>
          </div>
        )}

        <div>
          <label className="block text-sm font-medium text-ink-700 mb-1.5">
            Vai trò <span className="text-danger-500">*</span>
          </label>
          <select name="roleId" value={form.roleId} onChange={handleChange}
            className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400">
            <option value="">-- Chọn vai trò --</option>
            {lookups.roles.map((r) => <option key={r.id} value={r.id}>{r.tenRole}</option>)}
          </select>
        </div>

        <div>
          <label className="block text-sm font-medium text-ink-700 mb-1.5">
            Họ tên <span className="text-danger-500">*</span>
          </label>
          <input name="hoTen" value={form.hoTen} onChange={handleChange} placeholder="Nguyễn Văn A"
            className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400" />
        </div>

        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="block text-sm font-medium text-ink-700 mb-1.5">Email</label>
            <input name="email" type="email" value={form.email} onChange={handleChange} placeholder="email@crm.vn"
              className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400" />
          </div>
          <div>
            <label className="block text-sm font-medium text-ink-700 mb-1.5">Số điện thoại</label>
            <input name="soDienThoai" value={form.soDienThoai} onChange={handleChange} placeholder="0901234567"
              className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400" />
          </div>
        </div>

        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="block text-sm font-medium text-ink-700 mb-1.5">Phòng ban</label>
            <select name="phongBanId" value={form.phongBanId} onChange={handleChange}
              className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400">
              <option value="">-- Chọn --</option>
              {lookups.phongBans.map((p) => <option key={p.id} value={p.id}>{p.tenPhongBan}</option>)}
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium text-ink-700 mb-1.5">Chức vụ</label>
            <select name="chucVuId" value={form.chucVuId} onChange={handleChange}
              className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400">
              <option value="">-- Chọn --</option>
              {lookups.chucVus.map((c) => <option key={c.id} value={c.id}>{c.tenChucVu}</option>)}
            </select>
          </div>
        </div>

        {error && <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-2.5">{error}</div>}

        <div className="flex gap-2 pt-1">
          <Button type="submit" disabled={submitting} className="flex-1">
            {submitting ? "Đang lưu..." : isEdit ? "Cập nhật" : "Tạo tài khoản"}
          </Button>
          <Button type="button" variant="secondary" onClick={onClose}>Hủy</Button>
        </div>
      </form>
    </Modal>
  );
}
