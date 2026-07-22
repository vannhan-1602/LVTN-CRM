using CRM.Domain.Common;
using CRM.Domain.Enums;

namespace CRM.Domain.Entities.Tickets;

public class Ticket : SoftDeletableEntity<ulong>
{
    public string MaTicket { get; set; } = string.Empty;
    public string TieuDe { get; set; } = string.Empty;
    public string? MoTa { get; set; }
    public string? FileDinhKem { get; set; }
    public ushort? LoaiTicketId { get; set; }
    public ulong KhachHangId { get; set; }
    public ulong? HopDongId { get; set; }
    public uint? SanPhamId { get; set; }
    public TicketPriority MucDoUuTien { get; set; } = TicketPriority.TrungBinh;
    public TicketSource NguonTiepNhan { get; set; } = TicketSource.Phone;
    public TicketStatus TrangThai { get; set; } = TicketStatus.Moi;
    public uint? NhanVienTiepNhanId { get; set; }
    public uint? NhanVienXuLyId { get; set; }
    public DateTime? NgayHenXuLy { get; set; }
    public DateTime? ThoiHanSLA { get; set; }
    public uint SoLanEscalate { get; set; }
    public DateTime? NgayDong { get; set; }
    public string? LyDoDong { get; set; }
}
