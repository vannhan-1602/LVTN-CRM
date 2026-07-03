import { create } from "zustand";
import danhMucApi from "../api/danhMucApi";

/**
 * Store danh mục toàn cục — load 1 lần khi app khởi động sau khi đăng nhập.
 * Các component dùng store này thay vì hardcode constants.
 *
 * Dữ liệu: LoaiKhachHang, TinhTrang, LoaiTicket, LoaiSanPham, XepHang
 */
const useDanhMucStore = create((set, get) => ({
  loaiKhachHang: [],
  tinhTrang: [],
  loaiTicket: [],
  loaiSanPham: [],
  xepHang: [],
  loaded: false,
  loading: false,

  load: async () => {
    if (get().loaded || get().loading) return;
    set({ loading: true });
    try {
      const [lkh, tt, ltk, lsp, xh] = await Promise.allSettled([
        danhMucApi.getLoaiKhachHang(),
        danhMucApi.getTinhTrang(),
        danhMucApi.getLoaiTicket(),
        danhMucApi.getLoaiSanPham(),
        danhMucApi.getXepHang(),
      ]);
      set({
        loaiKhachHang: lkh.status === "fulfilled" ? (lkh.value.data ?? []) : [],
        tinhTrang:     tt.status  === "fulfilled" ? (tt.value.data  ?? []) : [],
        loaiTicket:    ltk.status === "fulfilled" ? (ltk.value.data ?? []) : [],
        loaiSanPham:   lsp.status === "fulfilled" ? (lsp.value.data ?? []) : [],
        xepHang:       xh.status  === "fulfilled" ? (xh.value.data  ?? []) : [],
        loaded: true,
      });
    } catch {
      // fail silently — component fallback về []
    } finally {
      set({ loading: false });
    }
  },

  reload: () => { set({ loaded: false }); get().load(); },

  // Helpers tìm tên theo id
  getTenLoaiKH:     (id) => get().loaiKhachHang.find(x => x.id === id)?.tenLoai      ?? "—",
  getTenTinhTrang:  (id) => get().tinhTrang.find(x => x.id === id)?.tenTinhTrang     ?? "—",
  getTenLoaiTicket: (id) => get().loaiTicket.find(x => x.id === id)?.tenLoai         ?? "—",
  getTenXepHang:    (id) => get().xepHang.find(x => x.id === id)?.tenHang            ?? "—",
}));

export default useDanhMucStore;
