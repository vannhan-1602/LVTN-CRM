using CRM.Domain.Common;
using CRM.Domain.Enums;

namespace CRM.Domain.Entities.Identity;

public class Role : BaseEntity<uint>
{
    public string TenRole { get; set; } = string.Empty;
    public string? MoTa { get; set; }
}

public class User : AuditableEntity<uint>
{
    public uint? NhanSuId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public uint? RoleId { get; set; }
    public UserStatus TrangThai { get; set; } = UserStatus.Active;

    public Role? Role { get; set; }
}

public class NhanSu : AuditableEntity<uint>
{
    public string HoTen { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? SoDienThoai { get; set; }
    public ushort? PhongBanId { get; set; }
    public ushort? ChucVuId { get; set; }
    public bool TrangThai { get; set; } = true;
}
