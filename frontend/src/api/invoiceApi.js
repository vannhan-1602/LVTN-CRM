import axiosClient from "./axiosClient";

const invoiceApi = {
  getAll: (params) => axiosClient.get("/Invoice", { params }),
  getById: (id) => axiosClient.get(`/Invoice/${id}`),
  create: (data) => axiosClient.post("/Invoice", data),
};

export default invoiceApi;
