using CRM.Domain.Common;

namespace CRM.Domain.Entities.Identity;

public class NhanSu : AuditableEntity<uint>
{
    public string HoTen { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? SoDienThoai { get; set; }
    public ushort? PhongBanId { get; set; }
    public ushort? ChucVuId { get; set; }
    public bool TrangThai { get; set; } = true;
}