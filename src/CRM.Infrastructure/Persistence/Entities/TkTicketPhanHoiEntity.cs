using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Infrastructure.Persistence.Entities;

[Table("TK_Ticket_PhanHoi")]
public class TkTicketPhanHoiEntity
{
    public ulong Id { get; set; }
    public ulong Ticket_Id { get; set; }
    public uint? NguoiPhanHoi_Id { get; set; }
    public string LoaiPhanHoi { get; set; } = string.Empty;
    public string NoiDung { get; set; } = string.Empty;
    public string? FileDinhKem { get; set; }
    public string? TrangThaiTruoc { get; set; }
    public string? TrangThaiSau { get; set; }
    public DateTime? CreatedAt { get; set; }
}