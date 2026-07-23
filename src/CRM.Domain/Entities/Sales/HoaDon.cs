using CRM.Domain.Common;

namespace CRM.Domain.Entities.Sales;

public class HoaDon : AuditableEntity<ulong>
{
    public string MaHoaDon { get; set; } = string.Empty;
    public ulong? HopDongId { get; set; }
    public ulong? LichThanhToanId { get; set; }
    public ulong KhachHangId { get; set; }
    public decimal TongTien { get; set; }
    public decimal SoTienDaThu { get; set; }
    public string TrangThaiThanhToan { get; set; } = Enums.InvoiceStatus.ChuaThanhToan;
}
