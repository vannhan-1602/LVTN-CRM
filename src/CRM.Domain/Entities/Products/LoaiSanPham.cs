using CRM.Domain.Common;

namespace CRM.Domain.Entities.Products;

public class LoaiSanPham : BaseEntity<uint>
{
    public string TenLoai { get; set; } = string.Empty;
    public string? MoTa { get; set; }

    /// <summary>VatLy | DichVu | License | Subscription — cột này đã có sẵn trong DB
    /// (BH_LoaiSanPham.HinhThuc), trước giờ chỉ chưa được đưa lên tầng domain/DTO.</summary>
    public string HinhThuc { get; set; } = "VatLy";
}