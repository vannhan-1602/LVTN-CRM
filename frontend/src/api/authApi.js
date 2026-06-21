import axiosClient from "./axiosClient";

const authApi = {
  login: (username, password) =>
    axiosClient.post("/Auth/login", { username, password }),

  // Admin only — danh sách tài khoản đầy đủ
  getUsers: () => axiosClient.get("/Auth/users"),

  //  mọi role đã đăng nhập, dùng cho dropdown "Nhân viên phụ trách/xử lý"
  getStaffList: () => axiosClient.get("/Auth/staff-list"),
};

export default authApi;
