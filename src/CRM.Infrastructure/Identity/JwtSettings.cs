namespace CRM.Infrastructure.Identity;

public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;

    // Access token sống ngắn (chuẩn OAuth2/JWT best practice: 15 phút) — vì không có cách
    // thu hồi tức thời theo thời gian thực nếu không check DB mỗi request (đã có TokenVersionCache
    // hỗ trợ phần đó), access token ngắn hạn giảm thiểu rủi ro khi token bị lộ.
    public int ExpirationInMinutes { get; set; } = 15;

    // Refresh token sống dài hơn nhiều, lưu trong HttpOnly cookie, dùng để lấy access token mới
    // mà không cần đăng nhập lại. Rotation: mỗi lần refresh sinh token mới, token cũ bị revoke.
    public int RefreshTokenExpirationInDays { get; set; } = 7;
}
