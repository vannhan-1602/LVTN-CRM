import axios from "axios";

export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "https://localhost:7071/api";
// Gốc server (không có /api) — dùng để build URL đầy đủ cho ảnh tĩnh trong wwwroot/uploads/...
export const API_ORIGIN = API_BASE_URL.replace(/\/api\/?$/, "");

const axiosClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    "Content-Type": "application/json",
  },
  // Bắt buộc để trình duyệt đính kèm cookie HttpOnly "refreshToken" (đặt bởi
  // /Auth/login, /Auth/refresh) trên các request cross-site tới API domain riêng.
  withCredentials: true,
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

// Access token sống ngắn (15 phút). Khi hết hạn, backend trả 401 — thay vì đá thẳng người
// dùng về trang login, tự động gọi /Auth/refresh (dùng refresh token trong cookie HttpOnly)
// để lấy access token mới rồi lặp lại request gốc, cho trải nghiệm liền mạch.
//
// Nhiều request có thể cùng lúc nhận 401 (VD trang gọi song song vài API) — dùng 1 Promise
// refreshPromise dùng chung để chỉ có ĐÚNG 1 lệnh gọi /Auth/refresh thực sự được gửi đi,
// các request khác "xếp hàng" chờ chung kết quả thay vì mỗi request tự refresh riêng
// (refresh token dùng cơ chế rotation — gọi refresh 2 lần cùng lúc sẽ khiến 1 trong 2 bị
// từ chối vì token đã bị token kia "dùng" và revoke trước).
let refreshPromise = null;

async function refreshAccessToken() {
  if (!refreshPromise) {
    refreshPromise = axios
      .post(
        `${API_BASE_URL}/Auth/refresh`,
        {},
        { withCredentials: true },
      )
      .then((res) => {
        const newToken = res.data?.data?.accessToken;
        if (!newToken) throw new Error("Refresh không trả về accessToken.");

        try {
          const raw = localStorage.getItem("auth-storage");
          const parsed = raw ? JSON.parse(raw) : { state: {}, version: 0 };
          parsed.state = { ...parsed.state, token: newToken };
          localStorage.setItem("auth-storage", JSON.stringify(parsed));
        } catch {
          // Không chặn luồng nếu ghi localStorage lỗi — request vẫn được retry với token mới
          // trong bộ nhớ, chỉ là lần load trang sau sẽ không còn nhớ (như một session ngắn).
        }

        return newToken;
      })
      .finally(() => {
        refreshPromise = null;
      });
  }
  return refreshPromise;
}

axiosClient.interceptors.response.use(
  (res) => res.data,
  async (err) => {
    const originalRequest = err.config;
    const status = err.response?.status;
    const isAuthEndpoint =
      originalRequest?.url?.includes("/Auth/login") ||
      originalRequest?.url?.includes("/Auth/refresh");

    if (status === 401 && !isAuthEndpoint && !originalRequest?._retried) {
      originalRequest._retried = true;
      try {
        const newToken = await refreshAccessToken();
        originalRequest.headers.Authorization = `Bearer ${newToken}`;
        return axiosClient(originalRequest);
      } catch {
        // Refresh token cũng hết hạn/không hợp lệ — không còn cách nào khác ngoài đăng nhập lại.
        localStorage.removeItem("auth-storage");
        if (window.location.pathname !== "/login") {
          window.location.href = "/login";
        }
        return Promise.reject(err.response?.data ?? { message: err.message });
      }
    }

    if (status === 401 && (isAuthEndpoint || originalRequest?._retried)) {
      localStorage.removeItem("auth-storage");
      if (window.location.pathname !== "/login") {
        window.location.href = "/login";
      }
    }

    return Promise.reject(err.response?.data ?? { message: err.message });
  },
);

export default axiosClient;
