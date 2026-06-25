using CRM.Domain.Common;

namespace CRM.Domain.Entities.Products;

public class SanPham : AuditableEntity<uint>
{
    public uint? LoaiSanPhamId { get; set; }
    public string MaSP { get; set; } = string.Empty;
    public string TenSP { get; set; } = string.Empty;
    public string? DonVi { get; set; }
    public decimal? GiaBan { get; set; }
    public int SoLuongTon { get; set; }
    public bool DangKinhDoanh { get; set; } = true;
}
