// ── Roles (khớp với backend Roles.cs) ────────────────────────────────────────
export const ROLES = {
  Admin: "Admin",
  Manager: "Manager",
  Sale: "Sale",
  Accountant: "Accountant",
};

// ── Khách hàng — dữ liệu thực từ DB ─────────────────────────────────────────
// KH_LoaiKhachHang: 1=VIP, 2=B2B, 3=B2C
export const LOAI_KHACH_HANG_OPTIONS = [
  { value: 1, label: "VIP" },
  { value: 2, label: "B2B (Doanh nghiệp)" },
  { value: 3, label: "B2C (Cá nhân)" },
];
export const LOAI_KHACH_HANG_MAP = { 1: "VIP", 2: "B2B", 3: "B2C" };
export const LOAI_BADGE_COLOR = {
  1: "bg-yellow-100 text-yellow-800",
  2: "bg-blue-100 text-blue-800",
  3: "bg-green-100 text-green-800",
};

// KH_TinhTrangKhachHang: 1=Đang giao dịch, 2=Tiềm năng, 3=Ngừng giao dịch
export const TINH_TRANG_KHACH_HANG_OPTIONS = [
  { value: 1, label: "Đang giao dịch" },
  { value: 2, label: "Tiềm năng" },
  { value: 3, label: "Ngừng giao dịch" },
];
export const TINH_TRANG_BADGE_COLOR = {
  1: "bg-emerald-100 text-emerald-800",
  2: "bg-purple-100 text-purple-800",
  3: "bg-gray-100 text-gray-600",
};

// ── Lead ─────────────────────────────────────────────────────────────────────
export const LEAD_TINH_TRANG_OPTIONS = [
  { value: "Mới", label: "Mới" },
  { value: "Đang chăm sóc", label: "Đang chăm sóc" },
  { value: "Đã chuyển đổi", label: "Đã chuyển đổi" },
  { value: "Thất bại", label: "Thất bại" },
];
export const LEAD_TINH_TRANG_COLOR = {
  Mới: "bg-blue-100 text-blue-700",
  "Đang chăm sóc": "bg-yellow-100 text-yellow-700",
  "Đã chuyển đổi": "bg-green-100 text-green-700",
  "Thất bại": "bg-red-100 text-red-600",
};

// ── Ticket — enum strings từ backend DomainEnums.cs ──────────────────────────
export const TICKET_STATUS = {
  Moi: "Mới",
  DangXuLy: "Đang xử lý",
  ChoPhanHoi: "Chờ phản hồi",
  Dong: "Đóng",
};
export const TICKET_STATUS_OPTIONS = Object.entries(TICKET_STATUS).map(
  ([value, label]) => ({ value, label }),
);
export const TICKET_STATUS_COLOR = {
  Moi: "bg-blue-100 text-blue-700",
  DangXuLy: "bg-yellow-100 text-yellow-700",
  ChoPhanHoi: "bg-orange-100 text-orange-700",
  Dong: "bg-gray-100 text-gray-600",
};

export const TICKET_PRIORITY = {
  Thap: "Thấp",
  TrungBinh: "Trung bình",
  Cao: "Cao",
  KhanCap: "Khẩn cấp",
};
export const TICKET_PRIORITY_OPTIONS = Object.entries(TICKET_PRIORITY).map(
  ([value, label]) => ({ value, label }),
);
export const TICKET_PRIORITY_COLOR = {
  Thap: "bg-green-100 text-green-700",
  TrungBinh: "bg-blue-100 text-blue-700",
  Cao: "bg-orange-100 text-orange-700",
  KhanCap: "bg-red-100 text-red-700",
};

export const TICKET_SOURCE_OPTIONS = [
  { value: "Email", label: "Email" },
  { value: "Phone", label: "Điện thoại" },
  { value: "Web", label: "Web" },
  { value: "Zalo", label: "Zalo" },
  { value: "TrucTiep", label: "Trực tiếp" },
];

export const TICKET_PHAN_HOI_LOAI_OPTIONS = [
  { value: "NoiBoXuLy", label: "Nội bộ xử lý" },
  { value: "PhanHoiKhachHang", label: "Phản hồi khách hàng" },
  { value: "YeuCauBoSung", label: "Yêu cầu bổ sung" },
  { value: "DongTicket", label: "Đóng ticket" },
];

export const TICKET_PHAN_HOI_LABEL = Object.fromEntries(
  TICKET_PHAN_HOI_LOAI_OPTIONS.map((o) => [o.value, o.label]),
);

export const QUOTE_STATUS = {
  Nhap: "Nháp",
  DaGui: "Đã gửi",
  TuChoi: "Từ chối",
  ChapNhan: "Đã chấp nhận",
};

export const QUOTE_STATUS_OPTIONS = Object.entries(QUOTE_STATUS).map(
  ([value, label]) => ({ value, label })
);

export const QUOTE_STATUS_COLOR = {
  Nhap: "bg-gray-100 text-gray-600",
  DaGui: "bg-blue-100 text-blue-700",
  TuChoi: "bg-red-100 text-red-600",
  ChapNhan: "bg-green-100 text-green-700",
};

export const CONTRACT_STATUS = {
  DangThucHien: "Đang thực hiện",
  TamDung: "Tạm dừng",
  ThanhLy: "Đã thanh lý",
};

export const CONTRACT_STATUS_OPTIONS = Object.entries(CONTRACT_STATUS).map(
  ([value, label]) => ({ value, label })
);

export const CONTRACT_STATUS_COLOR = {
  DangThucHien: "bg-green-100 text-green-700",
  TamDung: "bg-yellow-100 text-yellow-700",
  ThanhLy: "bg-gray-100 text-gray-600",
};

export const STOCK_TRANSACTION_TYPE_OPTIONS = [
  { value: "NhapMua", label: "Nhập mua" },
  { value: "XuatBan", label: "Xuất bán" },
  { value: "NhapTraKhach", label: "Nhập trả (khách trả lại)" },
  { value: "XuatTraNCC", label: "Xuất trả (trả NCC)" },
  { value: "XuatHuy", label: "Xuất hủy" },
  { value: "KiemKe", label: "Kiểm kê" },
];

export const STOCK_TRANSACTION_TYPE_LABEL = Object.fromEntries(
  STOCK_TRANSACTION_TYPE_OPTIONS.map((o) => [o.value, o.label])
);
