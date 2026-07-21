import axiosClient from "./axiosClient";

const locationApi = {
  getTinhThanh: () => axiosClient.get("/Location/tinh-thanh"),
  getPhuongXa: (tinhThanhId) =>
    axiosClient.get(`/Location/tinh-thanh/${tinhThanhId}/phuong-xa`),
};

export default locationApi;
