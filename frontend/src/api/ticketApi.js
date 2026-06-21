import axiosClient from "./axiosClient";

const ticketApi = {
  getAll: (params) => axiosClient.get("/Ticket", { params }),
  getById: (id) => axiosClient.get(`/Ticket/${id}`),
  create: (data) => axiosClient.post("/Ticket", data),
  update: (id, data) => axiosClient.put(`/Ticket/${id}`, data),
  delete: (id) => axiosClient.delete(`/Ticket/${id}`),
  assign: (id, data) => axiosClient.post(`/Ticket/${id}/assign`, data),
  close: (id, data) => axiosClient.post(`/Ticket/${id}/close`, data),
  getPhanHois: (id) => axiosClient.get(`/Ticket/${id}/phanhoi`),
  addPhanHoi: (id, data) => axiosClient.post(`/Ticket/${id}/phanhoi`, data),
};

export default ticketApi;
