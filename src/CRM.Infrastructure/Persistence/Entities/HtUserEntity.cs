using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Infrastructure.Persistence.Entities;

[Table("HT_User")]
public class HtUserEntity
{
    public uint Id { get; set; }
    public uint? NhanSuId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public uint? RoleId { get; set; }
    public string TrangThai { get; set; } = "Active";
    public int TokenVersion { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public HtThongTinNhanSuEntity? NhanSu { get; set; }
    public HtRoleEntity? Role { get; set; }
}