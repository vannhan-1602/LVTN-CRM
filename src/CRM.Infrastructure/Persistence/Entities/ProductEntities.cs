using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Infrastructure.Persistence.Entities;

[Table("BH_LoaiSanPham")]
public class BhLoaiSanPhamEntity
{
    public uint Id { get; set; }
    public string TenLoai { get; set; } = string.Empty;
    public string? MoTa { get; set; }
}

[Table("BH_SanPham")]
public class BhSanPhamEntity
{
    public uint Id { get; set; }
    public uint? LoaiSanPham_Id { get; set; }
    public string MaSP { get; set; } = string.Empty;
    public string TenSP { get; set; } = string.Empty;
    public string? DonVi { get; set; }
    public decimal GiaBan { get; set; }
    public int SoLuongTon { get; set; }
    public byte TrangThai { get; set; } = 1; // tinyint 0/1
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

[Table("BH_SanPham_HinhAnh")]
public class BhSanPhamHinhAnhEntity
{
    public ulong Id { get; set; }
    public uint? SanPham_Id { get; set; }
    public string UrlHinhAnh { get; set; } = string.Empty;
    public bool IsMain { get; set; }
}

[Table("Kho_TheKho")]
public class KhoTheKhoEntity
{
    public ulong Id { get; set; }
    public uint SanPham_Id { get; set; }
    public string? MaChungTu { get; set; }
    public string LoaiGiaoDich { get; set; } = string.Empty;
    public int SoLuongThayDoi { get; set; }
    public int TonCuoi { get; set; }
    public DateTime NgayGiaoDich { get; set; }
    public uint? NguoiThucHien_Id { get; set; }
    public string? GhiChu { get; set; }
}
