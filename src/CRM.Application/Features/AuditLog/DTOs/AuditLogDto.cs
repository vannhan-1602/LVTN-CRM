namespace CRM.Application.Features.AuditLog.DTOs;

public class AuditLogDto
{
    public ulong Id { get; set; }
    public string TableName { get; set; } = string.Empty;
    public ulong RecordId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldData { get; set; }
    public string? NewData { get; set; }
    public uint? UserId { get; set; }
    public string? TenNguoiThucHien { get; set; }
    public string? UsernameNguoiThucHien { get; set; }
    public DateTime? ChangedAt { get; set; }
}
