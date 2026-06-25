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
    public bool IsDeleted { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}