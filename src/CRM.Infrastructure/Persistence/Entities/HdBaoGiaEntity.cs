using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Infrastructure.Persistence.Entities;

[Table("HD_BaoGia")]
public class HdBaoGiaEntity
{
    public ulong Id { get; set; }
    public string MaBaoGia { get; set; } = string.Empty;
    public ulong KhachHang_Id { get; set; }
    public decimal TongTien { get; set; }
    public string TrangThai { get; set; } = "Nhap";
    public uint? NhanVien_Id { get; set; }
    public string? LyDoTuChoi { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}