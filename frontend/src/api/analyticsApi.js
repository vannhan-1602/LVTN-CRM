import axiosClient from "./axiosClient";

const analyticsApi = {
  getAiSalesAnalysis: (soThang = 6) =>
    axiosClient.get("/analytics/ai-sales-analysis", { params: { soThang } }),
  getDashboardTrends: () => axiosClient.get("/analytics/dashboard-trends"),
};

export default analyticsApi;
