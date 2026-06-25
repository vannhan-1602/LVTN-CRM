using CRM.Domain.Common;

namespace CRM.Domain.Entities.Identity;

public class Role : BaseEntity<uint>
{
    public string TenRole { get; set; } = string.Empty;
    public string? MoTa { get; set; }
}