using CRM.Domain.Common;

namespace CRM.Domain.Entities.Products;

public class SanPhamHinhAnh : BaseEntity<ulong>
{
    public uint? SanPhamId { get; set; }
    public string UrlHinhAnh { get; set; } = string.Empty;
    public bool IsMain { get; set; }
}