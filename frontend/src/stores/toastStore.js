import { create } from "zustand";

// Toast tự sinh id tăng dần, tự biến mất sau "durationMs". Không cần thư viện ngoài vì chỉ
// dùng cho thông báo real-time (số lượng ít, không cần animation/queue phức tạp).
let nextId = 1;

const useToastStore = create((set, get) => ({
  toasts: [],

  push: (toast) => {
    const id = nextId++;
    const durationMs = toast.durationMs ?? 5000;
    set((state) => ({ toasts: [...state.toasts, { ...toast, id }] }));
    setTimeout(() => get().dismiss(id), durationMs);
  },

  dismiss: (id) =>
    set((state) => ({ toasts: state.toasts.filter((t) => t.id !== id) })),
}));

export default useToastStore;
