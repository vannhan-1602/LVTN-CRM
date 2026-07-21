import axios from "axios";
import { API_BASE_URL } from "./axiosClient";

const publicClient = axios.create({
  baseURL: API_BASE_URL,
  headers: { "Content-Type": "application/json" },
});

publicClient.interceptors.response.use(
  (res) => res.data,
  (err) => Promise.reject(err.response?.data ?? { message: err.message }),
);

const leadPublicApi = {
  submitLandingPageLead: (data) => publicClient.post("/public/leads", data),
};

export default leadPublicApi;
