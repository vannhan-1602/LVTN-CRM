using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Infrastructure.Persistence.Entities;

[Table("BH_LoaiSanPham")]
public class BhLoaiSanPhamEntity
{
    public uint Id { get; set; }
    public string TenLoai { get; set; } = string.Empty;

    /// <summary>VatLy | DichVu | License | Subscription — chỉ loại VatLy mới áp dụng SoLuongTon trên BH_SanPham.</summary>
    public string HinhThuc { get; set; } = "VatLy";
    public string? MoTa { get; set; }
}