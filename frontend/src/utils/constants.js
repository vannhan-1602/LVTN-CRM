// ── Roles
export const ROLES = {
  Admin: "Admin",
  Manager: "Manager",
  Sale: "Sale",
  Accountant: "Accountant",
};

// Loại khách hàng & Tình trạng khách hàng: KHÔNG hardcode ở đây.
// Đây là danh mục DB-driven (bảng KH_LoaiKhachHang, KH_TinhTrangKhachHang), quản lý CRUD ở trang Settings.
// Dùng useDanhMucStore (stores/danhMucStore.js) để lấy danh sách + tên; dùng badgeToneForId() (utils/formatters.js) cho màu badge.

export const LEAD_TINH_TRANG_OPTIONS = [
  { value: "Moi", label: "Mới" },
  { value: "DangChamSoc", label: "Đang chăm sóc" },
  { value: "DaChuyenDoi", label: "Đã chuyển đổi" },
  { value: "ThatBai", label: "Thất bại" },
];
export const LEAD_TINH_TRANG_LABEL = Object.fromEntries(
  LEAD_TINH_TRANG_OPTIONS.map((o) => [o.value, o.label]),
);
export const LEAD_TINH_TRANG_COLOR = {
  Moi: "bg-info-50 text-info-700",
  DangChamSoc: "bg-warning-50 text-warning-700",
  DaChuyenDoi: "bg-success-50 text-success-700",
  ThatBai: "bg-danger-50 text-danger-600",
};

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
  Moi: "bg-info-50 text-info-700",
  DangXuLy: "bg-warning-50 text-warning-700",
  ChoPhanHoi: "bg-accent-50 text-accent-700",
  Dong: "bg-ink-100 text-ink-500",
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
  Thap: "bg-success-50 text-success-700",
  TrungBinh: "bg-info-50 text-info-700",
  Cao: "bg-warning-50 text-warning-700",
  KhanCap: "bg-danger-50 text-danger-600",
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
  { value: "PhanHoiKhachHang", label: "Đã trả lời khách hàng" },
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
  ([value, label]) => ({ value, label }),
);

export const QUOTE_STATUS_COLOR = {
  Nhap: "bg-ink-100 text-ink-500",
  DaGui: "bg-info-50 text-info-700",
  TuChoi: "bg-danger-50 text-danger-600",
  ChapNhan: "bg-success-50 text-success-700",
};

export const CONTRACT_STATUS = {
  DangThucHien: "Đang thực hiện",
  TamDung: "Tạm dừng",
  ThanhLy: "Đã thanh lý",
};

export const CONTRACT_STATUS_OPTIONS = Object.entries(CONTRACT_STATUS).map(
  ([value, label]) => ({ value, label }),
);

export const CONTRACT_STATUS_COLOR = {
  DangThucHien: "bg-success-50 text-success-700",
  TamDung: "bg-warning-50 text-warning-700",
  ThanhLy: "bg-ink-100 text-ink-500",
};

// HD_MocTrienKhai.LoaiMoc
export const MOC_LOAI_OPTIONS = [
  { value: "DaoTao", label: "Đào tạo" },
  { value: "BanGiao", label: "Bàn giao" },
  { value: "NghiemThu", label: "Nghiệm thu" },
];
export const MOC_LOAI_LABEL = Object.fromEntries(
  MOC_LOAI_OPTIONS.map((o) => [o.value, o.label]),
);

// HD_MocTrienKhai.TrangThai
export const MOC_TRANG_THAI_OPTIONS = [
  { value: "ChuaThucHien", label: "Chưa thực hiện" },
  { value: "DaThucHien", label: "Đã thực hiện" },
  { value: "DaXacNhan", label: "Đã xác nhận" },
];
export const MOC_TRANG_THAI_LABEL = Object.fromEntries(
  MOC_TRANG_THAI_OPTIONS.map((o) => [o.value, o.label]),
);
export const MOC_TRANG_THAI_COLOR = {
  ChuaThucHien: "bg-ink-100 text-ink-500",
  DaThucHien: "bg-info-50 text-info-700",
  DaXacNhan: "bg-success-50 text-success-700",
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
  STOCK_TRANSACTION_TYPE_OPTIONS.map((o) => [o.value, o.label]),
);

// ── Biểu ngữ nghiệp vụ theo loại sản phẩm (HinhThuc) ────────────────────────
// Cơ chế điều chỉnh tồn kho ở tầng dữ liệu dùng CHUNG 1 bộ giá trị LoaiGiaoDich
// (NhapMua/XuatBan/NhapTraKhach/XuatTraNCC/XuatHuy/KiemKe) cho mọi loại sản phẩm —
// KHÔNG đổi giá trị này. Chỉ đổi NHÃN hiển thị ở giao diện cho đúng thuật ngữ
// nghiệp vụ của từng loại: Phần cứng (VatLy) dùng ngôn ngữ kho vật lý, Phần mềm
// (License/Subscription) dùng ngôn ngữ cấp phát license, Dịch vụ (DichVu) dùng
// ngôn ngữ phân bổ khả dụng.
export const STOCK_TRANSACTION_LABELS_BY_HINHTHUC = {
  VatLy: {
    NhapMua: "Nhập mua",
    XuatBan: "Xuất bán",
    NhapTraKhach: "Nhận trả từ khách",
    XuatTraNCC: "Trả nhà cung cấp",
    XuatHuy: "Xuất hủy",
    KiemKe: "Kiểm kê",
  },
  License: {
    NhapMua: "Bổ sung License",
    XuatBan: "Cấp License",
    NhapTraKhach: "Thu hồi License",
    XuatTraNCC: "Hoàn trả License cho nhà cung cấp",
    XuatHuy: "Hủy License",
    KiemKe: "Đối soát License",
  },
  // Subscription dùng chung ngôn ngữ với License (cùng bản chất là cấp phát quyền dùng
  // phần mềm, không phải hàng tồn kho vật lý) — chưa có yêu cầu tách riêng thuật ngữ.
  Subscription: {
    NhapMua: "Bổ sung License",
    XuatBan: "Cấp License",
    NhapTraKhach: "Thu hồi License",
    XuatTraNCC: "Hoàn trả License cho nhà cung cấp",
    XuatHuy: "Hủy License",
    KiemKe: "Đối soát License",
  },
  DichVu: {
    NhapMua: "Bổ sung khả dụng",
    XuatBan: "Phân bổ dịch vụ",
    NhapTraKhach: "Hoàn tác phân bổ",
    XuatTraNCC: "Giảm khả dụng",
    XuatHuy: "Hủy khả dụng",
    KiemKe: "Đối soát khả dụng",
  },
};

/** Nhãn hiển thị đúng nghiệp vụ cho 1 giao dịch kho, theo HinhThuc của sản phẩm. */
export function getStockTransactionLabel(loaiGiaoDich, hinhThuc) {
  const table =
    STOCK_TRANSACTION_LABELS_BY_HINHTHUC[hinhThuc] ??
    STOCK_TRANSACTION_LABELS_BY_HINHTHUC.VatLy;
  return (
    table[loaiGiaoDich] ??
    STOCK_TRANSACTION_TYPE_LABEL[loaiGiaoDich] ??
    loaiGiaoDich
  );
}

/** Danh sách option cho dropdown "Loại giao dịch", nhãn đổi theo HinhThuc, value giữ nguyên. */
export function getStockTransactionOptions(hinhThuc) {
  const table =
    STOCK_TRANSACTION_LABELS_BY_HINHTHUC[hinhThuc] ??
    STOCK_TRANSACTION_LABELS_BY_HINHTHUC.VatLy;
  return STOCK_TRANSACTION_TYPE_OPTIONS.map((o) => ({
    value: o.value,
    label: table[o.value] ?? o.label,
  }));
}

// CH_CoHoiBanHang.GiaiDoan
export const GIAI_DOAN_LIST = [
  "KhaoSat",
  "DeXuat",
  "ThuongLuong",
  "ThanhCong",
  "ThatBai",
];

export const GIAI_DOAN_LABEL = {
  KhaoSat: "Khảo sát",
  DeXuat: "Đề xuất",
  ThuongLuong: "Thương lượng",
  ThanhCong: "Thành công",
  ThatBai: "Thất bại",
};

export const GIAI_DOAN_COLOR = {
  KhaoSat: "bg-info-50 text-info-700",
  DeXuat: "bg-accent-50 text-accent-700",
  ThuongLuong: "bg-warning-50 text-warning-700",
  ThanhCong: "bg-success-50 text-success-700",
  ThatBai: "bg-danger-50 text-danger-600",
};

// Màu header cột kanban — đặc (solid), không dùng token -50 nhạt
export const GIAI_DOAN_HEADER_COLOR = {
  KhaoSat: "bg-info-600",
  DeXuat: "bg-accent-600",
  ThuongLuong: "bg-warning-600",
  ThanhCong: "bg-success-600",
  ThatBai: "bg-danger-600",
};

export const NEXT_STAGE = {
  KhaoSat: "DeXuat",
  DeXuat: "ThuongLuong",
  ThuongLuong: "ThanhCong",
};

// HT_User — màu badge vai trò và trạng thái tài khoản
export const USER_ROLE_COLOR = {
  Admin: "bg-danger-50 text-danger-600",
  Manager: "bg-accent-50 text-accent-700",
  Sale: "bg-info-50 text-info-700",
  Accountant: "bg-success-50 text-success-700",
};

export const USER_STATUS_LABEL = {
  Active: "Đang hoạt động",
  Locked: "Đã khóa",
  Inactive: "Ngừng hoạt động",
};

export const USER_STATUS_COLOR = {
  Active: "bg-success-50 text-success-700",
  Locked: "bg-danger-50 text-danger-600",
  Inactive: "bg-ink-100 text-ink-500",
};
