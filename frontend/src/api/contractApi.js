import axiosClient from "./axiosClient";

const contractApi = {
  getAll: (params) => axiosClient.get("/Contract", { params }),
  getById: (id) => axiosClient.get(`/Contract/${id}`),
  createFromQuote: (data) => axiosClient.post("/Contract/from-quote", data),
  getLichThanhToan: (id) => axiosClient.get(`/Contract/${id}/lich-thanh-toan`),
  updateStatus: (id, trangThai) =>
    axiosClient.put(`/Contract/${id}/status`, { trangThai }),
  delete: (id) => axiosClient.delete(`/Contract/${id}`),
};

export default contractApi;
