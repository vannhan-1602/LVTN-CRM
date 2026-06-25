using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Infrastructure.Persistence.Entities;

[Table("HT_ThongTinNhanSu")]
public class HtThongTinNhanSuEntity
{
    public uint Id { get; set; }
    public string HoTen { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? SoDienThoai { get; set; }
    public ushort? PhongBanId { get; set; }
    public ushort? ChucVuId { get; set; }
    public bool TrangThai { get; set; } = true;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public HtUserEntity? User { get; set; }

    public HtPhongBanEntity? PhongBan { get; set; }
    public HtChucVuEntity? ChucVu { get; set; }
}