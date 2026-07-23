import axiosClient from "./axiosClient";

const alertApi = {
  getDashboardAlerts: () => axiosClient.get("/alerts/dashboard"),
};

export default alertApi;
