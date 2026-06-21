using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Infrastructure.Persistence.Entities;

[Table("HD_HopDong")]
public class HdHopDongEntity
{
    public ulong Id { get; set; }
    public string MaHopDong { get; set; } = string.Empty;
    public ulong KhachHang_Id { get; set; }

    public ulong? BaoGia_Id { get; set; }

    public DateOnly? NgayKy { get; set; }
    public int? ThoiHan { get; set; }
    public string TrangThai { get; set; } = "DangThucHien";
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
