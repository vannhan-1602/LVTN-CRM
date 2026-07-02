namespace CRM.Application.Features.PhieuThuChi.DTOs;

public class PhieuThuChiDto
{
    public ulong Id { get; set; }
    public string MaPhieu { get; set; } = string.Empty;
    public string LoaiPhieu { get; set; } = string.Empty;
    public ulong? KhachHangId { get; set; }
    public string? TenKhachHang { get; set; }
    public ulong? HoaDonId { get; set; }
    public string? MaHoaDon { get; set; }
    public decimal SoTien { get; set; }
    public uint? NguoiLapId { get; set; }
    public string? TenNguoiLap { get; set; }
    public DateTime? NgayTao { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreatePhieuThuChiRequestDto
{
    /// <summary>
    /// Hóa đơn liên kết. Bắt buộc nếu LoaiPhieu = "Thu".
    /// </summary>
    public ulong? HoaDonId { get; set; }

    /// <summary>
    /// Khách hàng. Nếu không truyền và có HoaDonId, hệ thống tự lấy từ hóa đơn.
    /// Bắt buộc nếu LoaiPhieu = "Chi" và không có HoaDonId.
    /// </summary>
    public ulong? KhachHangId { get; set; }

    /// <summary>Thu hoặc Chi</summary>
    public string LoaiPhieu { get; set; } = "Thu";

    public decimal SoTien { get; set; }
}
