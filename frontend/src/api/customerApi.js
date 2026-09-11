import axiosClient from "./axiosClient";

const customerApi = {
  getAll: (params) => axiosClient.get("/Customer", { params }),
  getById: (id) => axiosClient.get(`/Customer/${id}`),
  create: (data) => axiosClient.post("/Customer", data),
  update: (id, data) => axiosClient.put(`/Customer/${id}`, data),
  delete: (id) => axiosClient.delete(`/Customer/${id}`),
  restore: (id) => axiosClient.post(`/Customer/${id}/restore`),
  getLoyaltyInfo: (id) => axiosClient.get(`/Customer/${id}/loyalty`),
  // Lấy TOÀN BỘ khách hàng khớp bộ lọc hiện tại (không phân trang) để xuất Excel.
  getForExport: (params) => axiosClient.get("/Customer/export", { params }),
};

export default customerApi;
