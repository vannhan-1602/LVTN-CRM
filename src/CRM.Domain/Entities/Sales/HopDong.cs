using CRM.Domain.Common;

namespace CRM.Domain.Entities.Sales;

public class HopDong : AuditableEntity<ulong>
{
    public string MaHopDong { get; set; } = string.Empty;
    public ulong KhachHangId { get; set; }
    public DateOnly? NgayKy { get; set; }
    public int? ThoiHan { get; set; } // số tháng hiệu lực
    public string TrangThai { get; set; } = Enums.ContractStatus.DangThucHien;

    public ulong? BaoGiaGocId { get; set; }
}