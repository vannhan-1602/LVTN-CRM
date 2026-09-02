namespace CRM.Application.Interfaces.Auth;

public enum RefreshTokenStatus
{
    Success,
    Invalid,
    Expired,

    /// <summary>
    /// Token hợp lệ về mặt hash nhưng ĐÃ bị revoke trước đó (đã được rotate 1 lần) —
    /// nghĩa là ai đó đang cố dùng lại một refresh token cũ đã bị thay thế.
    /// Đây là dấu hiệu token bị đánh cắp (stolen token reuse). Khi gặp trường hợp này,
    /// toàn bộ refresh token còn hiệu lực của user bị thu hồi ngay lập tức.
    /// </summary>
    ReuseDetected
}

public record RefreshTokenIssueResult(string PlainToken, DateTime ExpiresAt);

public record RefreshTokenRotateResult(RefreshTokenStatus Status, uint? UserId, string? NewPlainToken, DateTime? NewExpiresAt);

/// <summary>
/// Quản lý vòng đời refresh token theo mô hình rotation: mỗi lần refresh sinh token mới,
/// token cũ bị revoke ngay. Chỉ hash (SHA-256) được lưu DB, token gốc chỉ tồn tại trong
/// cookie HttpOnly phía client.
/// </summary>
public interface IRefreshTokenService
{
    Task<RefreshTokenIssueResult> IssueAsync(uint userId, string? createdByIp, CancellationToken ct = default);

    Task<RefreshTokenRotateResult> RotateAsync(string plainToken, string? requestIp, CancellationToken ct = default);

    Task RevokeAsync(string plainToken, string? revokedByIp, CancellationToken ct = default);

    Task RevokeAllForUserAsync(uint userId, string? revokedByIp = null, CancellationToken ct = default);
}
