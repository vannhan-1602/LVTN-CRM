using CRM.Domain.Common;

namespace CRM.Domain.Entities.Sales;

public class BaoGiaChiTiet : BaseEntity<ulong>
{
    public ulong BaoGiaId { get; set; }
    public uint SanPhamId { get; set; }
    public int SoLuong { get; set; }
    public decimal DonGia { get; set; }

    public decimal ThanhTien => SoLuong * DonGia;
}