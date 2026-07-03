import axiosClient from "./axiosClient";

const phieuThuChiApi = {
  getAll: (params) => axiosClient.get("/PhieuThuChi", { params }),
  getById: (id) => axiosClient.get(`/PhieuThuChi/${id}`),
  create: (data) => axiosClient.post("/PhieuThuChi", data),
};

export default phieuThuChiApi;
