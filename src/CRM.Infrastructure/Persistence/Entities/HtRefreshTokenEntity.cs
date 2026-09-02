using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Infrastructure.Persistence.Entities;

/// <summary>
/// Lưu trữ refresh token dạng rotation (mỗi lần dùng sinh token mới, token cũ bị revoke).
/// Chỉ lưu SHA-256 hash của token — token gốc (plain) chỉ tồn tại trong cookie phía client,
/// không bao giờ lưu plaintext trong DB, kể cả khi bị lộ DB cũng không dùng lại được token.
/// </summary>
[Table("HT_RefreshToken")]
public class HtRefreshTokenEntity
{
    public ulong Id { get; set; }
    public uint UserId { get; set; }

    /// SHA-256 (hex, 64 ký tự) của refresh token gốc.
    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedByIp { get; set; }

    public DateTime? RevokedAt { get; set; }
    public string? RevokedByIp { get; set; }

    /// Hash của token đã thay thế token này (dùng để dò chuỗi rotation khi cần audit).
    public string? ReplacedByTokenHash { get; set; }

    public HtUserEntity? User { get; set; }

    public bool IsActive => RevokedAt is null && ExpiresAt > DateTime.UtcNow;
}
