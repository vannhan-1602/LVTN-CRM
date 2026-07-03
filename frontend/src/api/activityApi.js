import axiosClient from "./axiosClient";

const activityApi = {
  getByCustomer: (khachHangId) =>
    axiosClient.get(`/Activity/customer/${khachHangId}`),
  getByLead: (leadId) => axiosClient.get(`/Activity/lead/${leadId}`),
  create: (data) => axiosClient.post("/Activity", data),
  update: (id, data) => axiosClient.put(`/Activity/${id}`, data),
  delete: (id) => axiosClient.delete(`/Activity/${id}`),
};

export default activityApi;
