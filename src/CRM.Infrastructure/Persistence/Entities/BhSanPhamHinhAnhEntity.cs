using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Infrastructure.Persistence.Entities;

[Table("BH_SanPham_HinhAnh")]
public class BhSanPhamHinhAnhEntity
{
    public ulong Id { get; set; }
    public uint? SanPham_Id { get; set; }
    public string UrlHinhAnh { get; set; } = string.Empty;
    public bool IsMain { get; set; }
}