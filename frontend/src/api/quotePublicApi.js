import axios from "axios";

// Endpoint công khai — khách hàng bấm link trong email, KHÔNG cần đăng nhập.
// Dùng axios riêng (không qua axiosClient) để tránh phụ thuộc vào interceptor
// điều hướng /login khi gặp 401 — trang public không nên bị redirect kiểu đó.
const API_BASE_URL = "https://localhost:7071/api";

const publicClient = axios.create({
  baseURL: API_BASE_URL,
  headers: { "Content-Type": "application/json" },
});

publicClient.interceptors.response.use(
  (res) => res.data,
  (err) => Promise.reject(err.response?.data ?? { message: err.message }),
);

const quotePublicApi = {
  getByToken: (token) => publicClient.get(`/public/quotes/${token}`),
  accept: (token) => publicClient.post(`/public/quotes/${token}/accept`),
  reject: (token, lyDo) => publicClient.post(`/public/quotes/${token}/reject`, { lyDo }),
};

export default quotePublicApi;
