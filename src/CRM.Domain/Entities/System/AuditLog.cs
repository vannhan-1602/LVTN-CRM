using CRM.Domain.Enums;

namespace CRM.Domain.Entities.System;

public class AuditLog
{
    public ulong Id { get; set; }
    public string TableName { get; set; } = string.Empty;
    public ulong RecordId { get; set; }
    // Action là string: "INSERT" | "UPDATE" | "DELETE" 
    public string Action { get; set; } = string.Empty;
    public string? OldData { get; set; }
    public string? NewData { get; set; }
    public uint? UserId { get; set; }
    public DateTime? ChangedAt { get; set; }
}