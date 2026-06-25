using CRM.Domain.Common;

namespace CRM.Domain.Entities.Products;

public class LoaiSanPham : BaseEntity<uint>
{
    public string TenLoai { get; set; } = string.Empty;
    public string? MoTa { get; set; }
}