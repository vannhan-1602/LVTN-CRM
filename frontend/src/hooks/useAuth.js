import useAuthStore from "../features/auth/authStore";

export default function useAuth() {
  const { token, user, setAuth, logout } = useAuthStore();

  const hasRole = (...roles) => roles.includes(user?.role);

  const isAdmin = () => hasRole("Admin");
  const isManager = () => hasRole("Manager", "Admin");
  const isSale = () => hasRole("Sale");

  return { token, user, setAuth, logout, hasRole, isAdmin, isManager, isSale };
}
