using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Infrastructure.Persistence.Entities;

/// <summary>Khảo sát mức độ hài lòng của khách sau khi ticket được đóng. PK = Ticket_Id (1-1).</summary>
[Table("TK_DanhGiaHaiLong")]
public class TkDanhGiaHaiLongEntity
{
    public ulong Ticket_Id { get; set; }

    /// <summary>Token public để khách đánh giá qua link email không cần đăng nhập.</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>Thang điểm 1-5, NULL nếu khách chưa đánh giá.</summary>
    public byte? DiemDanhGia { get; set; }
    public string? NhanXet { get; set; }
    public bool DaGuiEmail { get; set; }
    public DateTime? NgayGuiEmail { get; set; }
    public DateTime? NgayDanhGia { get; set; }

    [ForeignKey("Ticket_Id")]
    public TkTicketEntity? Ticket { get; set; }
}
