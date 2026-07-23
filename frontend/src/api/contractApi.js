import axiosClient from "./axiosClient";

const contractApi = {
  getAll: (params) => axiosClient.get("/Contract", { params }),
  getById: (id) => axiosClient.get(`/Contract/${id}`),
  createFromQuote: (data) => axiosClient.post("/Contract/from-quote", data),
  getLichThanhToan: (id) => axiosClient.get(`/Contract/${id}/lich-thanh-toan`),
  updateStatus: (id, trangThai) =>
    axiosClient.put(`/Contract/${id}/status`, { trangThai }),
  delete: (id) => axiosClient.delete(`/Contract/${id}`),

  // ── Mốc triển khai (Đào tạo / Bàn giao / Nghiệm thu) ──────────────────
  getMocTrienKhai: (id) => axiosClient.get(`/Contract/${id}/moc-trien-khai`),
  createMocTrienKhai: (id, data) =>
    axiosClient.post(`/Contract/${id}/moc-trien-khai`, data),
  updateMocTrienKhai: (mocId, data) =>
    axiosClient.put(`/Contract/moc-trien-khai/${mocId}`, data),
  deleteMocTrienKhai: (mocId) =>
    axiosClient.delete(`/Contract/moc-trien-khai/${mocId}`),
};

export default contractApi;
