using CRM.Domain.Common;

namespace CRM.Domain.Entities.Loyalty;

public class LichSuHang : BaseEntity<ulong>
{
    public ulong KhachHangId { get; set; }
    public ushort? HangCuId { get; set; }
    public ushort HangMoiId { get; set; }
    public string LyDo { get; set; } = "TuDongDuDieuKien";
    public string? GhiChu { get; set; }
    public uint? NguoiThucHienId { get; set; }
    public DateTime? CreatedAt { get; set; }
}
