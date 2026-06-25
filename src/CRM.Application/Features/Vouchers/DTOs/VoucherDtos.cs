namespace CRM.Application.Features.Vouchers.DTOs;

public class VoucherDto
{
    public ulong Id { get; set; }
    public string MaPhieu { get; set; } = string.Empty;
    public ulong HoaDonId { get; set; }
    public string? MaHoaDon { get; set; }
    public string LoaiPhieu { get; set; } = string.Empty;
    public decimal SoTien { get; set; }
    public string? GhiChu { get; set; }
    public DateTime? NgayThucHien { get; set; }
    public int? NguoiThucHienId { get; set; }
    public string? TenNguoiThucHien { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class CreateVoucherRequestDto
{
    public ulong HoaDonId { get; set; }
    public string LoaiPhieu { get; set; } = "Thu";
    public decimal SoTien { get; set; }
    public string? GhiChu { get; set; }
    public DateTime? NgayThucHien { get; set; }
}