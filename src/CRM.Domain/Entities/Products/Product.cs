using CRM.Domain.Common;

namespace CRM.Domain.Entities.Products;

public class LoaiSanPham : BaseEntity<uint>
{
    public string TenLoai { get; set; } = string.Empty;
    public string? MoTa { get; set; }
}

public class SanPham : AuditableEntity<uint>
{
    public uint? LoaiSanPhamId { get; set; }
    public string MaSP { get; set; } = string.Empty;
    public string TenSP { get; set; } = string.Empty;
    public string? DonVi { get; set; }
    public decimal GiaBan { get; set; }
    public int SoLuongTon { get; set; }
    public bool DangKinhDoanh { get; set; } = true; 
}

public class SanPhamHinhAnh : BaseEntity<ulong>
{
    public uint? SanPhamId { get; set; }
    public string UrlHinhAnh { get; set; } = string.Empty;
    public bool IsMain { get; set; }
}
