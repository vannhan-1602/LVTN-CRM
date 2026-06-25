using CRM.Domain.Common;
using CRM.Domain.Enums;

namespace CRM.Domain.Entities.Identity;

public class User : AuditableEntity<uint>
{
    public uint? NhanSuId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public uint? RoleId { get; set; }
    public UserStatus TrangThai { get; set; } = UserStatus.Active;

    public Role? Role { get; set; }
}