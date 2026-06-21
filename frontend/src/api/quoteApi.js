import axiosClient from "./axiosClient";

const quoteApi = {
  getAll: (params) => axiosClient.get("/Quote", { params }),
  getById: (id) => axiosClient.get(`/Quote/${id}`),
  create: (data) => axiosClient.post("/Quote", data),
  update: (id, data) => axiosClient.put(`/Quote/${id}`, data),
  delete: (id) => axiosClient.delete(`/Quote/${id}`),
  send: (id) => axiosClient.post(`/Quote/${id}/send`),
  accept: (id) => axiosClient.post(`/Quote/${id}/accept`),
  reject: (id, lyDo) => axiosClient.post(`/Quote/${id}/reject`, { lyDo }),
};

export default quoteApi;
