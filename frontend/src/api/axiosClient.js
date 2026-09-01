import axios from "axios";

export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "https://localhost:7071/api";
// Gốc server (không có /api) — dùng để build URL đầy đủ cho ảnh tĩnh trong wwwroot/uploads/...
export const API_ORIGIN = API_BASE_URL.replace(/\/api\/?$/, "");

const axiosClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

axiosClient.interceptors.request.use((config) => {
  try {
    const raw = localStorage.getItem("auth-storage");
    if (raw) {
      const token = JSON.parse(raw)?.state?.token;
      if (token) config.headers.Authorization = `Bearer ${token}`;
    }
  } catch {
    // localStorage có thể lỗi (VD JSON hỏng, đang ở chế độ private browsing chặn storage) —
    // bỏ qua, coi như chưa đăng nhập, không chặn request tiếp tục chạy.
  }
  return config;
});

axiosClient.interceptors.response.use(
  (res) => res.data,
  (err) => {
    if (err.response?.status === 401) {
      localStorage.removeItem("auth-storage");
      if (window.location.pathname !== "/login") {
        window.location.href = "/login";
      }
    }
    return Promise.reject(err.response?.data ?? { message: err.message });
  },
);

export default axiosClient;