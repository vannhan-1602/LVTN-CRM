import { create } from "zustand";
import { persist } from "zustand/middleware";

// LoginResponseDto từ backend:
// { accessToken, expiresAt, userId, username, role, hoTen, email }
const useAuthStore = create(
  persist(
    (set) => ({
      token: null,
      user: null,

      // Gọi sau khi login thành công: login(res.data) với res.data = LoginResponseDto
      login: (loginResponse) => {
        const { accessToken, ...userInfo } = loginResponse;
        set({ token: accessToken, user: userInfo });
      },

      logout: () => set({ token: null, user: null }),
    }),
    { name: "auth-storage" },
  ),
);

export default useAuthStore;
