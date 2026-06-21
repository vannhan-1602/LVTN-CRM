import axiosClient from "./axiosClient";

const userManagementApi = {
  getAll: () => axiosClient.get("/UserManagement"),
  getById: (id) => axiosClient.get(`/UserManagement/${id}`),
  getLookups: () => axiosClient.get("/UserManagement/lookups"),
  create: (data) => axiosClient.post("/UserManagement", data),
  update: (id, data) => axiosClient.put(`/UserManagement/${id}`, data),
  resetPassword: (id, newPassword) =>
    axiosClient.post(`/UserManagement/${id}/reset-password`, { newPassword }),
  toggleStatus: (id, trangThai) =>
    axiosClient.post(`/UserManagement/${id}/status`, { trangThai }),
  delete: (id) => axiosClient.delete(`/UserManagement/${id}`),
};

export default userManagementApi;
