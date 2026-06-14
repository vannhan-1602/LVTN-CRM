import axiosClient from "./axiosClient";

const authApi = {
  login: (data) => axiosClient.post("/Auth/login", data),
  getProfile: () => axiosClient.get("/Auth/profile"),
};

export default authApi;
