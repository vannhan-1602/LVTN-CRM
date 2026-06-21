import axiosClient from "./axiosClient";

const productApi = {
  getAll: (params) => axiosClient.get("/Product", { params }),
  getById: (id) => axiosClient.get(`/Product/${id}`),
  getTypes: () => axiosClient.get("/Product/types"),
  create: (data) => axiosClient.post("/Product", data),
  update: (id, data) => axiosClient.put(`/Product/${id}`, data),
  delete: (id) => axiosClient.delete(`/Product/${id}`),
  adjustStock: (id, data) => axiosClient.post(`/Product/${id}/stock`, data),
};

export default productApi;
