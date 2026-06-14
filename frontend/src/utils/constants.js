export const TICKET_STATUS = {
  Moi: "Mới",
  DangXuLy: "Đang xử lý",
  ChoPhanHoi: "Chờ phản hồi",
  Dong: "Đóng",
};

export const TICKET_STATUS_OPTIONS = Object.entries(TICKET_STATUS).map(
  ([value, label]) => ({ value, label }),
);

export const TICKET_PRIORITY = {
  Thap: "Thấp",
  TrungBinh: "Trung bình",
  Cao: "Cao",
  KhanCap: "Khẩn cấp",
};

export const TICKET_PRIORITY_OPTIONS = Object.entries(TICKET_PRIORITY).map(
  ([value, label]) => ({ value, label }),
);

export const TICKET_SOURCE = {
  Email: "Email",
  Phone: "Điện thoại",
  Web: "Web",
  Zalo: "Zalo",
  TrucTiep: "Trực tiếp",
};

export const TICKET_SOURCE_OPTIONS = Object.entries(TICKET_SOURCE).map(
  ([value, label]) => ({ value, label }),
);

export const TICKET_PHAN_HOI_LOAI = {
  NoiBoXuLy: "Nội bộ xử lý",
  PhanHoiKhachHang: "Phản hồi khách hàng",
  YeuCauBoSung: "Yêu cầu bổ sung",
  DongTicket: "Đóng ticket",
};

export const TICKET_PHAN_HOI_LOAI_OPTIONS = Object.entries(
  TICKET_PHAN_HOI_LOAI,
).map(([value, label]) => ({ value, label }));

export const LEAD_TINH_TRANG = {
  Mới: "Mới",
  "Đang chăm sóc": "Đang chăm sóc",
  "Đã chuyển đổi": "Đã chuyển đổi",
  "Thất bại": "Thất bại",
};

export const LEAD_TINH_TRANG_OPTIONS = Object.entries(LEAD_TINH_TRANG).map(
  ([value, label]) => ({ value, label }),
);

export const ROLES = {
  Admin: "Admin",
  Manager: "Manager",
  Sale: "Sale",
  Accountant: "Accountant",
};

// Badge color mapping
export const TICKET_STATUS_COLOR = {
  Moi: "bg-blue-100 text-blue-700",
  DangXuLy: "bg-yellow-100 text-yellow-700",
  ChoPhanHoi: "bg-orange-100 text-orange-700",
  Dong: "bg-gray-100 text-gray-600",
};

export const TICKET_PRIORITY_COLOR = {
  Thap: "bg-green-100 text-green-700",
  TrungBinh: "bg-blue-100 text-blue-700",
  Cao: "bg-orange-100 text-orange-700",
  KhanCap: "bg-red-100 text-red-700",
};
