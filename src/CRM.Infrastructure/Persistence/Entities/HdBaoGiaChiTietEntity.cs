using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Infrastructure.Persistence.Entities;

[Table("HD_BaoGia_ChiTiet")]
public class HdBaoGiaChiTietEntity
{
    public ulong Id { get; set; }
    public ulong BaoGia_Id { get; set; }
    public uint SanPham_Id { get; set; }
    public int SoLuong { get; set; }
    public decimal DonGia { get; set; }
}