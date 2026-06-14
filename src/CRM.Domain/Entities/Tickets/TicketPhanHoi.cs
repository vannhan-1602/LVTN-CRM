using CRM.Domain.Common;
using CRM.Domain.Enums;

namespace CRM.Domain.Entities.Tickets;

public class TicketPhanHoi : BaseEntity<ulong>
{
    public ulong TicketId { get; set; }
    public uint? NguoiPhanHoiId { get; set; }
    public TicketPhanHoiLoai LoaiPhanHoi { get; set; }
    public string NoiDung { get; set; } = string.Empty;
    public string? FileDinhKem { get; set; }
    public TicketStatus? TrangThaiTruoc { get; set; }
    public TicketStatus? TrangThaiSau { get; set; }
    public DateTime? CreatedAt { get; set; }
}