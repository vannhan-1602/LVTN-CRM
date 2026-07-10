import axiosClient from "./axiosClient";

const loyaltyApi = {
  runDailyJob: () => axiosClient.post("/Loyalty/run-daily-job"),
};

export default loyaltyApi;
