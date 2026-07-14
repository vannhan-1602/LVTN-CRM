using System.Security.Cryptography;
using System.Text;
using CRM.Application.Interfaces.Quotes;
using Microsoft.Extensions.Configuration;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Token dạng "{quoteId}.{chữ ký HMAC-SHA256 base64url}". Dùng chung khóa bí mật
/// với JwtSettings:Secret (đã có sẵn trong appsettings, không cần thêm cấu hình mới).
/// Không lưu token vào DB — xác thực bằng cách tính lại chữ ký từ quoteId và so khớp.
/// </summary>
public class QuotePublicTokenService : IQuotePublicTokenService
{
    private readonly string _secret;

    public QuotePublicTokenService(IConfiguration config)
    {
        _secret = config["JwtSettings:Secret"] ?? "crm-quote-public-fallback-secret";
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
