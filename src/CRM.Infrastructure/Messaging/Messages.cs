namespace CRM.Infrastructure.Messaging;

public class AuditLogMessage
{
    public string TableName { get; set; } = string.Empty;
    public ulong RecordId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldData { get; set; }
    public string? NewData { get; set; }
    public uint? UserId { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}

public class NotificationMessage
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public uint? RecipientUserId { get; set; }
    public string? Type { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
