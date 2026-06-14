import axiosClient from "./axiosClient";

const leadApi = {
  getAll: (params) => axiosClient.get("/Lead", { params }),
  getById: (id) => axiosClient.get(`/Lead/${id}`),
  create: (data) => axiosClient.post("/Lead", data),
  update: (id, data) => axiosClient.put(`/Lead/${id}`, data),
  delete: (id) => axiosClient.delete(`/Lead/${id}`),
  convert: (id, data) => axiosClient.post(`/Lead/${id}/convert`, data),
};

export default leadApi;
