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
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

[Table("HD_BaoGia_ChiTiet")]
public class HdBaoGiaChiTietEntity
{
    public ulong Id { get; set; }
    public ulong BaoGia_Id { get; set; }
    public uint SanPham_Id { get; set; }
    public int SoLuong { get; set; }
    public decimal DonGia { get; set; }
}
