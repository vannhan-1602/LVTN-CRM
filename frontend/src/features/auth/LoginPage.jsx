import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useForm } from "react-hook-form";
import authApi from "../../api/authApi";
import useAuthStore from "./authStore";

export default function LoginPage() {
  const navigate = useNavigate();

  // Lấy dữ liệu Store
  const setAuth = useAuthStore((s) => s.setAuth);
  const token = useAuthStore((s) => s.token);
  const user = useAuthStore((s) => s.user);

  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  // Khởi tạo React Hook Form
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm({
    defaultValues: { username: "", password: "" },
  });

  // Chặn vòng lặp đăng nhập
  useEffect(() => {
    if (token && user) {
      navigate("/");
    }
  }, [token, user, navigate]);

  // Hàm xử lý API đăng nhập
  const onSubmit = async (data) => {
    setError("");
    setLoading(true);
    try {
      const res = await authApi.login(data);
      // Lấy đúng tên trường từ API C#
      const { accessToken, ...userData } = res.data.data;

      setAuth(accessToken, userData);
      navigate("/");
    } catch (err) {
      // Bắt lỗi nếu sai pass, sai user
      setError(
        err.response?.data?.message || "Tài khoản hoặc mật khẩu không đúng.",
      );
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gray-100 flex items-center justify-center">
      <div className="bg-white rounded-2xl shadow-md w-full max-w-md p-8">
        <h1 className="text-2xl font-bold text-gray-800 mb-1">CRM Online</h1>
        <p className="text-sm text-gray-500 mb-6">Đăng nhập để tiếp tục</p>

        {/* Khung hiển thị lỗi từ API */}
        {error && (
          <div className="mb-4 text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg px-4 py-2">
            {error}
          </div>
        )}

        {/* Thẻ Form */}
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          {/* Input Username */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Tên đăng nhập
            </label>
            <input
              {...register("username", {
                required: "Vui lòng nhập tên đăng nhập",
              })}
              className={`w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 ${
                errors.username ? "border-red-500" : "border-gray-300"
              }`}
              placeholder="Nhập tên đăng nhập (VD: admin)"
            />
            {errors.username && (
              <p className="text-xs text-red-500 mt-1">
                {errors.username.message}
              </p>
            )}
          </div>

          {/* Input Password */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Mật khẩu
            </label>
            <input
              type="password"
              {...register("password", {
                required: "Vui lòng nhập mật khẩu",
              })}
              className={`w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 ${
                errors.password ? "border-red-500" : "border-gray-300"
              }`}
              placeholder="Nhập mật khẩu (VD: 123456)"
            />
            {errors.password && (
              <p className="text-xs text-red-500 mt-1">
                {errors.password.message}
              </p>
            )}
          </div>

          {/* Nút Submit */}
          <button
            type="submit"
            disabled={loading}
            className="w-full bg-blue-600 hover:bg-blue-700 text-white font-medium py-2 rounded-lg text-sm transition-colors disabled:opacity-60"
          >
            {loading ? "Đang xử lý..." : "Đăng nhập"}
          </button>
        </form>
      </div>
    </div>
  );
}
