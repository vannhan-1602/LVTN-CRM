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
    public ulong? HoaDonId { get; set; }
    public ulong? KhachHangId { get; set; }
    public string LoaiPhieu { get; set; } = "Thu";
    public decimal SoTien { get; set; }
}
