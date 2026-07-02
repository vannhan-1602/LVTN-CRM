using CRM.Domain.Common;

namespace CRM.Domain.Entities.Sales;

public class PhieuThuChi : AuditableEntity<ulong>
{
    public string MaPhieu { get; set; } = string.Empty;
    public string LoaiPhieu { get; set; } = Enums.PaymentVoucherType.Thu;
    public ulong? KhachHangId { get; set; }
    public ulong? HoaDonId { get; set; }
    public decimal SoTien { get; set; }
    public uint? NguoiLapId { get; set; }   // uint? khớp với DB int unsigned + HtUser.Id (uint)
}
