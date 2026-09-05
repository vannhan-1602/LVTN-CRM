import { create } from "zustand";

// Mỗi khi RealtimeProvider nhận sự kiện từ backend (VD "customer:created"), nó gọi
// touch("customer") ở đây. Trang danh sách chỉ cần đọc lastUpdated["customer"] và thêm giá trị
// đó vào dependency array của useEffect đang fetch dữ liệu sẵn có — React tự chạy lại fetch mỗi
// khi giá trị đổi, không cần biết chi tiết logic fetch của từng trang.
const useRealtimeStore = create((set) => ({
  lastUpdated: {},

  touch: (module) =>
    set((state) => ({
      lastUpdated: { ...state.lastUpdated, [module]: Date.now() },
    })),
}));

export default useRealtimeStore;