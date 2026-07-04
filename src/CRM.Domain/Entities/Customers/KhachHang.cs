using CRM.Domain.Common;

namespace CRM.Domain.Entities.Customers;

public class KhachHang : SoftDeletableEntity<ulong>
{
    public string MaKhachHang { get; set; } = string.Empty;
    public string TenKhachHang { get; set; } = string.Empty;
    public ushort? LoaiKhachHangId { get; set; }
    public ushort? TinhTrangId { get; set; }
    public string? Email { get; set; }
    public string? SoDienThoai { get; set; }
    public string? MaSoThue { get; set; }
    public uint? NhanVienPhuTrachId { get; set; }
    public DateOnly? NgaySinh { get; set; }
    public DateOnly? NgayThanhLap { get; set; }
    public ushort? HangKhachHangId { get; set; }
}

