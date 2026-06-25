using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Infrastructure.Persistence.Entities;

[Table("HT_Role")]
public class HtRoleEntity
{
    public uint Id { get; set; }
    public string TenRole { get; set; } = string.Empty;
    public string? MoTa { get; set; }

    public ICollection<HtUserEntity> Users { get; set; } = [];
}