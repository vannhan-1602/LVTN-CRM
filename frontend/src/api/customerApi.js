import axiosClient from "./axiosClient";

const customerApi = {
  getAll: (params) => axiosClient.get("/Customer", { params }),
  getById: (id) => axiosClient.get(`/Customer/${id}`),
  create: (data) => axiosClient.post("/Customer", data),
  update: (id, data) => axiosClient.put(`/Customer/${id}`, data),
  delete: (id) => axiosClient.delete(`/Customer/${id}`),
  restore: (id) => axiosClient.post(`/Customer/${id}/restore`),
  getLoyaltyInfo: (id) => axiosClient.get(`/Customer/${id}/loyalty`),
};

export default customerApi;
