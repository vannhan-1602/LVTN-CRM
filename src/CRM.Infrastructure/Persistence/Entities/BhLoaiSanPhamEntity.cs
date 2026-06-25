using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Infrastructure.Persistence.Entities;

[Table("BH_LoaiSanPham")]
public class BhLoaiSanPhamEntity
{
    public uint Id { get; set; }
    public string TenLoai { get; set; } = string.Empty;
    public string? MoTa { get; set; }
}