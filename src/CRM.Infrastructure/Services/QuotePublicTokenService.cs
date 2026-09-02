using System.Security.Cryptography;
using System.Text;
using CRM.Application.Interfaces.Quotes;
using CRM.Infrastructure.Identity;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Token dạng "{quoteId}.{chữ ký HMAC-SHA256 base64url}". Dùng chung khóa bí mật
/// với JwtSettings:Secret (đã có sẵn trong appsettings, không cần thêm cấu hình mới).
/// Không lưu token vào DB — xác thực bằng cách tính lại chữ ký từ quoteId và so khớp.
/// </summary>
public class QuotePublicTokenService : IQuotePublicTokenService
{
    private readonly string _secret;

    // Trước đây đọc trực tiếp IConfiguration["JwtSettings:Secret"] và fallback về một chuỗi
    // hardcode ("crm-quote-public-fallback-secret") nếu thiếu cấu hình — nghĩa là nếu deploy
    // quên set secret, hệ thống vẫn "chạy được" nhưng dùng secret CÔNG KHAI (nằm ngay trong
    // mã nguồn) để ký token truy cập báo giá công khai, ai đọc được source là tự tạo được
    // token hợp lệ cho BẤT KỲ quoteId nào. Đổi sang IOptions<JwtSettings> để dùng chung giá
    // trị đã được validate độ dài tối thiểu 256-bit ở DependencyInjection.AddInfrastructure —
    // không còn đường nào để service này chạy với secret yếu/rỗng.
    public QuotePublicTokenService(IOptions<JwtSettings> jwtSettings)
    {
        _secret = jwtSettings.Value.Secret;
    }

    public string GenerateToken(ulong quoteId)
    {
        var sig = Sign(quoteId);
        return $"{quoteId}.{sig}";
    }

    public ulong? ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;

        var parts = token.Split('.', 2);
        if (parts.Length != 2) return null;
        if (!ulong.TryParse(parts[0], out var quoteId)) return null;

        var expectedSig = Sign(quoteId);
        var providedSigBytes = Base64UrlDecodeSafe(parts[1]);
        var expectedSigBytes = Base64UrlDecodeSafe(expectedSig);
        if (providedSigBytes is null || expectedSigBytes is null) return null;

        // So sánh dạng constant-time để tránh timing attack
        return CryptographicOperations.FixedTimeEquals(providedSigBytes, expectedSigBytes)
            ? quoteId
            : null;
    }

    private string Sign(ulong quoteId)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(quoteId.ToString()));
        return Base64UrlEncode(hash);
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static byte[]? Base64UrlDecodeSafe(string s)
    {
        try
        {
            var padded = s.Replace('-', '+').Replace('_', '/');
            switch (padded.Length % 4)
            {
                case 2: padded += "=="; break;
                case 3: padded += "="; break;
            }
            return Convert.FromBase64String(padded);
        }
        catch { return null; }
    }
}