import { useState } from "react";
import userManagementApi from "../../api/userManagementApi";
import Modal from "../../components/common/Modal";
import Button from "../../components/common/Button";

export default function ResetPasswordModal({ user, onClose, onDone }) {
  const [password, setPassword] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (password.length < 6) { setError("Mật khẩu tối thiểu 6 ký tự"); return; }
    setSubmitting(true); setError("");
    try { await userManagementApi.resetPassword(user.id, password); onDone(); }
    catch (err) { setError(err?.message || "Đặt lại mật khẩu thất bại"); }
    finally { setSubmitting(false); }
  };

  return (
    <Modal isOpen onClose={onClose} title="Đặt lại mật khẩu" size="sm">
      <form onSubmit={handleSubmit} className="space-y-4">
        <p className="text-sm text-ink-500">
          Tài khoản: <strong className="text-ink-900">{user.username}</strong>
        </p>
        <input
          type="password" value={password} onChange={(e) => setPassword(e.target.value)}
          placeholder="Mật khẩu mới (tối thiểu 6 ký tự)" autoFocus
          className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
        />
        {error && <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-2.5">{error}</div>}
        <div className="flex gap-2">
          <Button type="submit" disabled={submitting} className="flex-1">
            {submitting ? "Đang lưu..." : "Đặt lại mật khẩu"}
          </Button>
          <Button type="button" variant="secondary" onClick={onClose}>Hủy</Button>
        </div>
      </form>
    </Modal>
  );
}
