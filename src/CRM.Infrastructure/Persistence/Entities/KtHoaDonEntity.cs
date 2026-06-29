using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Infrastructure.Persistence.Entities;

[Table("KT_HoaDon")]
public class KtHoaDonEntity
{
    public ulong Id { get; set; }
    public string MaHoaDon { get; set; } = string.Empty;
    public ulong? HopDongId { get; set; }
    public ulong KhachHang_Id { get; set; }
    public decimal TongTien { get; set; }
    public decimal? SoTienDaThu { get; set; }
    public string TrangThaiThanhToan { get; set; } = "ChuaThanhToan";
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}