import axios from "axios";

// Endpoint công khai — khách hàng bấm link khảo sát hài lòng (CSAT) trong email,
// KHÔNG cần đăng nhập. Dùng axios riêng (không qua axiosClient) để tránh phụ thuộc
// vào interceptor điều hướng /login khi gặp 401 — trang public không nên bị redirect
// kiểu đó. Cấu trúc giống hệt quotePublicApi.js.
const API_BASE_URL = "https://localhost:7071/api";

const publicClient = axios.create({
  baseURL: API_BASE_URL,
  headers: { "Content-Type": "application/json" },
});

publicClient.interceptors.response.use(
  (res) => res.data,
  (err) => Promise.reject(err.response?.data ?? { message: err.message }),
);

const ticketPublicApi = {
  getCsatByToken: (token) => publicClient.get(`/public/tickets/csat/${token}`),
  submitCsat: (token, diemDanhGia, nhanXet) =>
    publicClient.post(`/public/tickets/csat/${token}`, { diemDanhGia, nhanXet }),
};

export default ticketPublicApi;
