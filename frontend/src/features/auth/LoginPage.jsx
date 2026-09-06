import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Eye, EyeOff, Lock, User } from "lucide-react";
import authApi from "../../api/authApi";
import useAuthStore from "./authStore";
import { getApiErrorMessage } from "../../utils/formatters";

export default function LoginPage() {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const login = useAuthStore((s) => s.login);
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    setLoading(true);
    try {
      // axiosClient unwraps → res = ApiResponse<LoginResponseDto>
      // res.data = LoginResponseDto { accessToken, userId, role, hoTen, ... }
      const res = await authApi.login(username, password);
      if (!res.success || !res.data) {
        setError(res.message || "Đăng nhập thất bại");
        return;
      }
      login(res.data); // lưu token + user vào zustand + localStorage
      navigate("/");
    } catch (err) {
      setError(
        getApiErrorMessage(err, "Tên đăng nhập hoặc mật khẩu không đúng"),
      );
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-canvas flex items-center justify-center p-4">
      <div className="w-full max-w-sm">
        {/* Logo — cùng kiểu mark với sidebar (MainLayout) để nhất quán nhận diện */}
        <div className="flex items-center justify-center gap-2.5 mb-8">
          <div className="w-9 h-9 rounded-lg bg-accent-500 flex items-center justify-center text-white text-base font-semibold">
            C
          </div>
          <span className="text-ink-900 font-semibold text-lg tracking-wide">
            CRM System
          </span>
        </div>

        <div className="bg-surface rounded-card shadow-sm border border-ink-100 overflow-hidden">
          {/* 1 điểm nhấn duy nhất — dải màu accent mỏng phía trên, không lặp lại decoration ở nơi khác */}
          <div className="h-1 bg-accent-500" />

          <div className="p-8">
            <h1 className="text-lg font-semibold text-ink-900">Đăng nhập</h1>
            <p className="text-sm text-ink-500 mt-1 mb-6">
              Nhập tài khoản để tiếp tục vào hệ thống
            </p>

            <form onSubmit={handleSubmit} className="space-y-4">
              {error && (
                <div className="bg-danger-50 border border-danger-100 text-danger-700 text-sm rounded-lg px-4 py-3">
                  {error}
                </div>
              )}

              <div>
                <label className="block text-sm font-medium text-ink-700 mb-1.5">
                  Tên đăng nhập
                </label>
                <div className="relative">
                  <User className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-ink-400" />
                  <input
                    type="text"
                    value={username}
                    onChange={(e) => setUsername(e.target.value)}
                    required
                    autoFocus
                    autoComplete="username"
                    className="w-full border border-ink-200 rounded-lg pl-10 pr-4 py-2.5 text-sm text-ink-900 placeholder:text-ink-400 focus:outline-none focus:ring-2 focus:ring-accent-500/40 focus:border-accent-500 transition-colors"
                  />
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-ink-700 mb-1.5">
                  Mật khẩu
                </label>
                <div className="relative">
                  <Lock className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-ink-400" />
                  <input
                    type={showPassword ? "text" : "password"}
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    required
                    autoComplete="current-password"
                    className="w-full border border-ink-200 rounded-lg pl-10 pr-10 py-2.5 text-sm text-ink-900 placeholder:text-ink-400 focus:outline-none focus:ring-2 focus:ring-accent-500/40 focus:border-accent-500 transition-colors"
                  />
                  <button
                    type="button"
                    onClick={() => setShowPassword((v) => !v)}
                    tabIndex={-1}
                    aria-label={showPassword ? "Ẩn mật khẩu" : "Hiện mật khẩu"}
                    className="absolute right-3 top-1/2 -translate-y-1/2 text-ink-400 hover:text-ink-600"
                  >
                    {showPassword ? (
                      <EyeOff className="w-4 h-4" />
                    ) : (
                      <Eye className="w-4 h-4" />
                    )}
                  </button>
                </div>
              </div>

              <button
                type="submit"
                disabled={loading}
                className="w-full bg-accent-500 hover:bg-accent-600 disabled:opacity-60 text-white font-semibold py-2.5 rounded-lg text-sm transition-colors mt-2"
              >
                {loading ? "Đang đăng nhập..." : "Đăng nhập"}
              </button>
            </form>
          </div>
        </div>

        <p className="text-center text-xs text-ink-400 mt-6">
          Hệ thống quản lý khách hàng nội bộ — liên hệ Admin nếu quên mật khẩu
        </p>
      </div>
    </div>
  );
}
