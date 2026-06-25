using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Infrastructure.Persistence.Entities;

[Table("KH_HoatDong")]
public class KhHoatDongEntity
{
    public ulong Id { get; set; }
    public ulong? KhachHang_Id { get; set; }
    public ulong? Lead_Id { get; set; }
    public string? LoaiHoatDong { get; set; }
    public string? NoiDung { get; set; }
    public DateTime? ThoiGianThucHien { get; set; }
    public uint? NhanVien_Id { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}