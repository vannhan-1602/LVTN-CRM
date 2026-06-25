using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Infrastructure.Persistence.Entities;

[Table("SYS_AuditLog")]
public class SysAuditLogEntity
{
    public ulong Id { get; set; }
    public string TableName { get; set; } = string.Empty;
    public ulong RecordId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldData { get; set; }
    public string? NewData { get; set; }
    public uint? UserId { get; set; }
    public DateTime? ChangedAt { get; set; }
}