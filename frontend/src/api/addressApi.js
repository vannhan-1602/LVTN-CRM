import axiosClient from "./axiosClient";

const addressApi = {
  getByCustomer: (khachHangId) =>
    axiosClient.get(`/Address/customer/${khachHangId}`),
  create: (khachHangId, data) =>
    axiosClient.post(`/Address/customer/${khachHangId}`, data),
  update: (id, data) => axiosClient.put(`/Address/${id}`, data),
  delete: (id) => axiosClient.delete(`/Address/${id}`),
};

export default addressApi;
