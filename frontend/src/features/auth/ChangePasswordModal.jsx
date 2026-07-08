import { useState } from "react";
import { useNavigate } from "react-router-dom";
import Modal from "../../components/common/Modal";
import Button from "../../components/common/Button";
import authApi from "../../api/authApi";
import useAuthStore from "./authStore";

const emptyForm = { currentPassword: "", newPassword: "", confirmNewPassword: "" };

export default function ChangePasswordModal({ onClose }) {
  const logout = useAuthStore((s) => s.logout);
  const navigate = useNavigate();

  const [form, setForm] = useState(emptyForm);
  const [error, setError] = useState("");
  const [submitting, setSubmitting] = useState(false);

  const set = (k, v) => setForm((f) => ({ ...f, [k]: v }));

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");

    if (form.newPassword !== form.confirmNewPassword) {
      setError("Mật khẩu xác nhận không khớp với mật khẩu mới.");
      return;
    }
    if (form.newPassword.length < 8) {
      setError("Mật khẩu mới phải có tối thiểu 8 ký tự.");
      return;
    }
    if (!/[0-9]/.test(form.newPassword) || !/[^a-zA-Z0-9]/.test(form.newPassword)) {
      setError("Mật khẩu mới phải chứa ít nhất 1 chữ số và 1 ký tự đặc biệt.");
      return;
    }
    if (form.newPassword === form.currentPassword) {
      setError("Mật khẩu mới phải khác mật khẩu hiện tại.");
      return;
    }

    setSubmitting(true);
    try {
      await authApi.changePassword(
        form.currentPassword,
        form.newPassword,
        form.confirmNewPassword,
      );
      // Backend đã thu hồi JWT hiện tại (TokenVersion) — buộc đăng nhập lại ngay.
      logout();
      navigate("/login", {
        state: { message: "Đổi mật khẩu thành công. Vui lòng đăng nhập lại." },
      });
    } catch (err) {
      setError(err?.message || "Đổi mật khẩu thất bại.");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Modal isOpen onClose={onClose} title="Đổi mật khẩu" size="sm">
      <form onSubmit={handleSubmit} className="space-y-4">
        {error && (
          <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-3">
            {error}
          </div>
        )}

        <div>
          <label className="block text-xs font-medium text-ink-700 mb-1">
            Mật khẩu hiện tại
          </label>
          <input
            type="password"
            required
            value={form.currentPassword}
            onChange={(e) => set("currentPassword", e.target.value)}
            className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
          />
        </div>

        <div>
          <label className="block text-xs font-medium text-ink-700 mb-1">
            Mật khẩu mới
          </label>
          <input
            type="password"
            required
            value={form.newPassword}
            onChange={(e) => set("newPassword", e.target.value)}
            className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
          />
          <p className="text-[11px] text-ink-400 mt-1">
            Tối thiểu 8 ký tự, có ít nhất 1 số và 1 ký tự đặc biệt.
          </p>
        </div>

        <div>
          <label className="block text-xs font-medium text-ink-700 mb-1">
            Xác nhận mật khẩu mới
          </label>
          <input
            type="password"
            required
            value={form.confirmNewPassword}
            onChange={(e) => set("confirmNewPassword", e.target.value)}
            className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
          />
        </div>

        <div className="flex justify-end gap-2 pt-2">
          <Button type="button" variant="secondary" onClick={onClose} disabled={submitting}>
            Hủy
          </Button>
          <Button type="submit" disabled={submitting}>
            {submitting ? "Đang lưu..." : "Cập nhật mật khẩu"}
          </Button>
        </div>
      </form>
    </Modal>
  );
}
