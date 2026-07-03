namespace CRM.Application.Features.DanhMuc.DTOs;

// ── LoaiKhachHang ─────────────────────────────────────────────────────────────
public class LoaiKhachHangDto
{
    public ushort Id { get; set; }
    public string TenLoai { get; set; } = string.Empty;
    public string? MoTa { get; set; }
    public bool IsActive { get; set; }
}
public class UpsertLoaiKhachHangDto
{
    public string TenLoai { get; set; } = string.Empty;
    public string? MoTa { get; set; }
    public bool IsActive { get; set; } = true;
}

// ── TinhTrangKhachHang ────────────────────────────────────────────────────────
public class TinhTrangKhachHangDto
{
    public ushort Id { get; set; }
    public string TenTinhTrang { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
public class UpsertTinhTrangKhachHangDto
{
    public string TenTinhTrang { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

// ── LoaiTicket ────────────────────────────────────────────────────────────────
public class LoaiTicketDto
{
    public ushort Id { get; set; }
    public string TenLoai { get; set; } = string.Empty;
    public string? MoTa { get; set; }
    public bool IsActive { get; set; }
}
public class UpsertLoaiTicketDto
{
    public string TenLoai { get; set; } = string.Empty;
    public string? MoTa { get; set; }
    public bool IsActive { get; set; } = true;
}

// ── LoaiSanPham ───────────────────────────────────────────────────────────────
public class LoaiSanPhamDto
{
    public uint Id { get; set; }
    public string TenLoai { get; set; } = string.Empty;
    public string? MoTa { get; set; }
}
public class UpsertLoaiSanPhamDto
{
    public string TenLoai { get; set; } = string.Empty;
    public string? MoTa { get; set; }
}

// ── XepHang ───────────────────────────────────────────────────────────────────
public class XepHangDto
{
    public ushort Id { get; set; }
    public string MaHang { get; set; } = string.Empty;
    public string TenHang { get; set; } = string.Empty;
    public byte ThuTu { get; set; }
    public int DiemToiThieu { get; set; }
    public int SoLanThuToiThieu { get; set; }
    public decimal PhanTramGiamVoucher { get; set; }
    public string? MoTaQuyenLoi { get; set; }
    public bool IsActive { get; set; }
}
public class UpdateXepHangDto
{
    /// <summary>Mốc điểm tối thiểu trong 12 tháng để đạt hạng này</summary>
    public int DiemToiThieu { get; set; }
    /// <summary>Số lần thu tiền tối thiểu trong 12 tháng</summary>
    public int SoLanThuToiThieu { get; set; }
    /// <summary>% giảm giá voucher tự động khi khách thăng lên hạng này (0 = không phát)</summary>
    public decimal PhanTramGiamVoucher { get; set; }
    /// <summary>Mô tả quyền lợi, chèn vào nội dung email thông báo</summary>
    public string? MoTaQuyenLoi { get; set; }
    public bool IsActive { get; set; } = true;
}

// ── NgayLe ────────────────────────────────────────────────────────────────────
public class NgayLeDto
{
    public ushort Id { get; set; }
    public string TenNgayLe { get; set; } = string.Empty;
    public byte Thang { get; set; }
    public byte Ngay { get; set; }
    public byte SoNgayGuiTruoc { get; set; }
    public string ApDungChoLoaiKH { get; set; } = string.Empty;
    public ushort? HangToiThieuApDung { get; set; }
    public bool IsActive { get; set; }
}
public class UpsertNgayLeDto
{
    public string TenNgayLe { get; set; } = string.Empty;
    public byte Thang { get; set; }
    public byte Ngay { get; set; }
    public byte SoNgayGuiTruoc { get; set; } = 5;
    /// <summary>B2C | B2B | TatCa</summary>
    public string ApDungChoLoaiKH { get; set; } = "TatCa";
    /// <summary>Id của KH_XepHang. NULL = áp dụng mọi hạng</summary>
    public ushort? HangToiThieuApDung { get; set; }
    public bool IsActive { get; set; } = true;
}
