using CRM.Domain.Enums;

namespace CRM.Domain.Entities.System;

public class AuditLog
{
    public ulong Id { get; set; }
    public string TableName { get; set; } = string.Empty;
    public ulong RecordId { get; set; }
    public AuditAction Action { get; set; }
    public string? OldData { get; set; }
    public string? NewData { get; set; }
    public uint? UserId { get; set; }
    public DateTime? ChangedAt { get; set; }
}
