using CRM.Domain.Common;

namespace CRM.Domain.Entities.Sales;

public class BaoGia : AuditableEntity<ulong>
{
    public string MaBaoGia { get; set; } = string.Empty;
    public ulong KhachHangId { get; set; }
    public decimal TongTien { get; set; }
    public string TrangThai { get; set; } = Enums.QuoteStatus.Nhap;
    public uint? NhanVienId { get; set; }
}

public class BaoGiaChiTiet : BaseEntity<ulong>
{
    public ulong BaoGiaId { get; set; }
    public uint SanPhamId { get; set; }
    public int SoLuong { get; set; }
    public decimal DonGia { get; set; }

    public decimal ThanhTien => SoLuong * DonGia;
}

public class HopDong : AuditableEntity<ulong>
{
    public string MaHopDong { get; set; } = string.Empty;
    public ulong KhachHangId { get; set; }
    public DateOnly? NgayKy { get; set; }
    public int? ThoiHan { get; set; } // số tháng hiệu lực
    public string TrangThai { get; set; } = Enums.ContractStatus.DangThucHien;

    public ulong? BaoGiaGocId { get; set; }
}
