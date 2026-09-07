import { useEffect, useState } from "react";
import { User, Mail, Phone, KeyRound, Save } from "lucide-react";
import authApi from "../../api/authApi";
import useAuthStore from "./authStore";
import ChangePasswordModal from "./ChangePasswordModal";
import Button from "../../components/common/Button";
import { getApiErrorMessage } from "../../utils/formatters";

export default function ProfilePage() {
  const user = useAuthStore((s) => s.user);

  const [form, setForm] = useState({ hoTen: "", email: "", soDienThoai: "" });
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [showChangePassword, setShowChangePassword] = useState(false);

  useEffect(() => {
    (async () => {
      try {
        const res = await authApi.getMyProfile();
        const profile = res.data;
        setForm({
          hoTen: profile.hoTen || "",
          email: profile.email || "",
          soDienThoai: profile.soDienThoai || "",
        });
      } catch (err) {
        setError(getApiErrorMessage(err, "Không thể tải thông tin cá nhân"));
      } finally {
        setLoading(false);
      }
    })();
  }, []);

  const set = (k, v) => setForm((f) => ({ ...f, [k]: v }));

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    setSuccess("");
    setSaving(true);
    try {
      await authApi.updateMyProfile(
        form.hoTen,
        form.email || null,
        form.soDienThoai || null,
      );
      setSuccess("Đã lưu thông tin thành công.");
    } catch (err) {
      setError(getApiErrorMessage(err, "Không thể lưu thông tin"));
    } finally {
      setSaving(false);
    }
  };

  const initials = (form.hoTen || user?.username || "??")
    .split(" ")
    .filter(Boolean)
    .slice(-2)
    .map((w) => w[0])
    .join("")
    .toUpperCase();

  if (loading) {
    return <div className="p-6 text-sm text-ink-500">Đang tải...</div>;
  }

  return (
    <div className="max-w-2xl mx-auto space-y-6">
      <div>
        <h1 className="text-lg font-semibold text-ink-900">
          Thông tin cá nhân
        </h1>
        <p className="text-sm text-ink-500 mt-0.5">
          Xem và cập nhật thông tin liên hệ của bạn
        </p>
      </div>

      {/* Header nhỏ: avatar + username (không đổi được) + vai trò */}
      <div className="bg-surface rounded-card border border-ink-100 p-6 flex items-center gap-4">
        <div className="w-14 h-14 rounded-full bg-info-50 flex items-center justify-center text-lg font-semibold text-info-700 flex-shrink-0">
          {initials}
        </div>
        <div className="min-w-0">
          <p className="text-base font-semibold text-ink-900 truncate">
            {form.hoTen || user?.username}
          </p>
          <div className="flex items-center gap-2 mt-1 flex-wrap">
            <span className="text-xs text-ink-500">@{user?.username}</span>
            <span className="text-xs bg-success-50 text-success-700 px-2 py-0.5 rounded-full font-medium">
              {user?.role}
            </span>
          </div>
        </div>
      </div>

      {/* Form sửa thông tin */}
      <form
        onSubmit={handleSubmit}
        className="bg-surface rounded-card border border-ink-100 p-6 space-y-4"
      >
        <h2 className="text-sm font-semibold text-ink-900">
          Cập nhật thông tin
        </h2>

        {error && (
          <div className="bg-danger-50 border border-danger-100 text-danger-700 text-sm rounded-lg px-4 py-3">
            {error}
          </div>
        )}
        {success && (
          <div className="bg-success-50 border border-success-100 text-success-700 text-sm rounded-lg px-4 py-3">
            {success}
          </div>
        )}

        <div>
          <label className="block text-sm font-medium text-ink-700 mb-1.5">
            Tên đăng nhập
          </label>
          <div className="relative">
            <User className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-ink-300" />
            <input
              type="text"
              value={user?.username || ""}
              disabled
              className="w-full border border-ink-100 bg-ink-50 rounded-lg pl-10 pr-4 py-2.5 text-sm text-ink-400 cursor-not-allowed"
            />
          </div>
          <p className="text-xs text-ink-400 mt-1">
            Tên đăng nhập không thể thay đổi.
          </p>
        </div>

        <div>
          <label className="block text-sm font-medium text-ink-700 mb-1.5">
            Họ và tên
          </label>
          <div className="relative">
            <User className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-ink-400" />
            <input
              type="text"
              value={form.hoTen}
              onChange={(e) => set("hoTen", e.target.value)}
              required
              className="w-full border border-ink-200 rounded-lg pl-10 pr-4 py-2.5 text-sm text-ink-900 focus:outline-none focus:ring-2 focus:ring-accent-500/40 focus:border-accent-500 transition-colors"
            />
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium text-ink-700 mb-1.5">
            Email
          </label>
          <div className="relative">
            <Mail className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-ink-400" />
            <input
              type="email"
              value={form.email}
              onChange={(e) => set("email", e.target.value)}
              className="w-full border border-ink-200 rounded-lg pl-10 pr-4 py-2.5 text-sm text-ink-900 focus:outline-none focus:ring-2 focus:ring-accent-500/40 focus:border-accent-500 transition-colors"
            />
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium text-ink-700 mb-1.5">
            Số điện thoại
          </label>
          <div className="relative">
            <Phone className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-ink-400" />
            <input
              type="text"
              value={form.soDienThoai}
              onChange={(e) => set("soDienThoai", e.target.value)}
              className="w-full border border-ink-200 rounded-lg pl-10 pr-4 py-2.5 text-sm text-ink-900 focus:outline-none focus:ring-2 focus:ring-accent-500/40 focus:border-accent-500 transition-colors"
            />
          </div>
        </div>

        <div className="flex items-center justify-between pt-2">
          <button
            type="button"
            onClick={() => setShowChangePassword(true)}
            className="inline-flex items-center gap-1.5 text-sm text-ink-500 hover:text-ink-900 transition-colors"
          >
            <KeyRound size={15} />
            Đổi mật khẩu
          </button>

          <Button type="submit" disabled={saving} icon={Save}>
            {saving ? "Đang lưu..." : "Lưu thay đổi"}
          </Button>
        </div>
      </form>

      {showChangePassword && (
        <ChangePasswordModal onClose={() => setShowChangePassword(false)} />
      )}
    </div>
  );
}
