using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Infrastructure.Persistence.Entities;

[Table("KH_KhachHang")]
public class KhKhachHangEntity
{
    public ulong Id { get; set; }
    public string MaKhachHang { get; set; } = string.Empty;
    public string TenKhachHang { get; set; } = string.Empty;
    public ushort? LoaiKhachHangId { get; set; }
    public ushort? TinhTrangId { get; set; }
    public string? Email { get; set; }
    public string? SoDienThoai { get; set; }
    public string? MaSoThue { get; set; }
    public uint? NhanVienPhuTrachId { get; set; }
    public ushort? HangKhachHang_Id { get; set; }   // FK → KH_XepHang
    public DateOnly? NgaySinh { get; set; }           // B2C: sinh nhật cá nhân
    public DateOnly? NgayThanhLap { get; set; }       // B2B: ngày thành lập công ty
    public bool IsDeleted { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}