import { API_ORIGIN } from "../api/axiosClient";

export function formatDate(dateStr) {
  if (!dateStr) return "—";
  return new Date(dateStr).toLocaleDateString("vi-VN", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  });
}

export function formatDateTime(dateStr) {
  if (!dateStr) return "—";
  return new Date(dateStr).toLocaleString("vi-VN", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}

export function formatCurrency(amount) {
  if (amount == null) return "—";
  return new Intl.NumberFormat("vi-VN", {
    style: "currency",
    currency: "VND",
  }).format(amount);
}

// Danh mục lấy từ DB (LoaiKhachHang, TinhTrang...) không có cột màu,
// và id có thể đổi bất cứ lúc nào qua CRUD ở Settings, nên không map màu cứng theo id.
// Thay vào đó xoay vòng bảng màu cố định của Badge để màu ổn định theo id nhưng không phụ thuộc thứ tự tạo.
const BADGE_TONE_CYCLE = ["info", "success", "warning", "danger", "neutral"];
export function badgeToneForId(id) {
  if (id == null) return "neutral";
  return BADGE_TONE_CYCLE[Number(id) % BADGE_TONE_CYCLE.length];
}

// Ảnh sản phẩm được backend trả về dạng đường dẫn tương đối (vd: "/uploads/products/xxx.jpg"),
// cần ghép với gốc server (không phải gốc /api) mới load được trên <img>.
export function getImageUrl(relativeOrAbsoluteUrl) {
  if (!relativeOrAbsoluteUrl) return null;
  if (/^https?:\/\//i.test(relativeOrAbsoluteUrl)) return relativeOrAbsoluteUrl;
  return `${API_ORIGIN}${relativeOrAbsoluteUrl}`;
}
