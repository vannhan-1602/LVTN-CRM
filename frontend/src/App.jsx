import { BrowserRouter } from "react-router-dom";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { useEffect } from "react";
import AppRoutes from "./routes/AppRoutes";
import useAuthStore from "./features/auth/authStore";
import useDanhMucStore from "./stores/danhMucStore";

const queryClient = new QueryClient({
  defaultOptions: { queries: { retry: 1, staleTime: 30_000 } },
});

function AppInit() {
  const token = useAuthStore((s) => s.token);
  const loadDanhMuc = useDanhMucStore((s) => s.load);
  const reloadDanhMuc = useDanhMucStore((s) => s.reload);

  // Load danh mục ngay sau khi đăng nhập
  useEffect(() => {
    if (token) {
      reloadDanhMuc();
    }
  }, [token]);

  return <AppRoutes />;
}

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <AppInit />
      </BrowserRouter>
    </QueryClientProvider>
  );
}
