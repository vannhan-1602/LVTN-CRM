import axiosClient from "./axiosClient";

const authApi = {
  login: (username, password) =>
    axiosClient.post("/Auth/login", { username, password }),

  // Admin only — danh sách tài khoản đầy đủ
  getUsers: () => axiosClient.get("/Auth/users"),

  //  mọi role đã đăng nhập, dùng cho dropdown "Nhân viên phụ trách/xử lý"
  getStaffList: () => axiosClient.get("/Auth/staff-list"),

  // UC-AUTH-03 — tự đổi mật khẩu
  changePassword: (currentPassword, newPassword, confirmNewPassword) =>
    axiosClient.post("/Auth/change-password", {
      currentPassword,
      newPassword,
      confirmNewPassword,
    }),
};

export default authApi;
