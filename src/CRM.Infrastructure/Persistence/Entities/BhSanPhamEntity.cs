using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Infrastructure.Persistence.Entities;

[Table("BH_SanPham")]
public class BhSanPhamEntity
{
    public uint Id { get; set; }
    public uint? LoaiSanPham_Id { get; set; }
    public string MaSP { get; set; } = string.Empty;
    public string TenSP { get; set; } = string.Empty;
    public string? DonVi { get; set; }
    public decimal? GiaBan { get; set; }
    public int SoLuongTon { get; set; }
    public byte TrangThai { get; set; } = 1; // tinyint 0/1
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}