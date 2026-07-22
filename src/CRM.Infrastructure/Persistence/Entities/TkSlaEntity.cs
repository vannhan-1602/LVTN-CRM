using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Infrastructure.Persistence.Entities;

/// <summary>Cấu hình SLA theo mức độ ưu tiên ticket. PK là MucDoUuTien (Thap/TrungBinh/Cao/KhanCap).</summary>
[Table("TK_SLA")]
public class TkSlaEntity
{
    public string MucDoUuTien { get; set; } = string.Empty;

    /// <summary>Số giờ tối đa phải phản hồi lần đầu.</summary>
    public int SoGioPhanHoi { get; set; }

    /// <summary>Số giờ tối đa phải xử lý xong (dùng tính TK_Ticket.ThoiHanSLA).</summary>
    public int SoGioXuLy { get; set; }
}
