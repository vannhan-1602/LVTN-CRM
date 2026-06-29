import axiosClient from "./axiosClient";

const opportunityApi = {
  getAll: (params) => axiosClient.get("/Opportunity", { params }),
  getSummary: () => axiosClient.get("/Opportunity/summary"),
  getById: (id) => axiosClient.get(`/Opportunity/${id}`),
  create: (data) => axiosClient.post("/Opportunity", data),
  update: (id, data) => axiosClient.put(`/Opportunity/${id}`, data),
  changeStage: (id, giaiDoan, ghiChu) =>
    axiosClient.post(`/Opportunity/${id}/stage`, { giaiDoan, ghiChu }),
  delete: (id) => axiosClient.delete(`/Opportunity/${id}`),
};

export default opportunityApi;
