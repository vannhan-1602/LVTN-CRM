import axiosClient from "./axiosClient";

const danhMucApi = {
  // Loại khách hàng
  getLoaiKhachHang: () => axiosClient.get("/danh-muc/loai-khach-hang"),
  createLoaiKhachHang: (data) =>
    axiosClient.post("/danh-muc/loai-khach-hang", data),
  updateLoaiKhachHang: (id, data) =>
    axiosClient.put(`/danh-muc/loai-khach-hang/${id}`, data),
  deleteLoaiKhachHang: (id) =>
    axiosClient.delete(`/danh-muc/loai-khach-hang/${id}`),

  // Tình trạng khách hàng
  getTinhTrang: () => axiosClient.get("/danh-muc/tinh-trang-khach-hang"),
  createTinhTrang: (data) =>
    axiosClient.post("/danh-muc/tinh-trang-khach-hang", data),
  updateTinhTrang: (id, data) =>
    axiosClient.put(`/danh-muc/tinh-trang-khach-hang/${id}`, data),
  deleteTinhTrang: (id) =>
    axiosClient.delete(`/danh-muc/tinh-trang-khach-hang/${id}`),

  // Loại ticket
  getLoaiTicket: () => axiosClient.get("/danh-muc/loai-ticket"),
  createLoaiTicket: (data) => axiosClient.post("/danh-muc/loai-ticket", data),
  updateLoaiTicket: (id, data) =>
    axiosClient.put(`/danh-muc/loai-ticket/${id}`, data),
  deleteLoaiTicket: (id) => axiosClient.delete(`/danh-muc/loai-ticket/${id}`),

  // Loại sản phẩm
  getLoaiSanPham: () => axiosClient.get("/danh-muc/loai-san-pham"),
  createLoaiSanPham: (data) =>
    axiosClient.post("/danh-muc/loai-san-pham", data),
  updateLoaiSanPham: (id, data) =>
    axiosClient.put(`/danh-muc/loai-san-pham/${id}`, data),
  deleteLoaiSanPham: (id) =>
    axiosClient.delete(`/danh-muc/loai-san-pham/${id}`),

  // Xếp hạng (chỉ update)
  getXepHang: () => axiosClient.get("/danh-muc/xep-hang"),
  updateXepHang: (id, data) =>
    axiosClient.put(`/danh-muc/xep-hang/${id}`, data),

  // Ngày lễ
  getNgayLe: () => axiosClient.get("/danh-muc/ngay-le"),
  createNgayLe: (data) => axiosClient.post("/danh-muc/ngay-le", data),
  updateNgayLe: (id, data) => axiosClient.put(`/danh-muc/ngay-le/${id}`, data),
  deleteNgayLe: (id) => axiosClient.delete(`/danh-muc/ngay-le/${id}`),
};

export default danhMucApi;
