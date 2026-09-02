using System.Security.Cryptography;
using CRM.Application.Interfaces.Auth;
using CRM.Infrastructure.Persistence.Contexts;
using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Identity;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly CrmDbContext _context;
    private readonly JwtSettings _settings;

    public RefreshTokenService(CrmDbContext context, IOptions<JwtSettings> settings)
    {
        _context = context;
        _settings = settings.Value;
    }

    public async Task<RefreshTokenIssueResult> IssueAsync(uint userId, string? createdByIp, CancellationToken ct = default)
    {
        var plainToken = GeneratePlainToken();
        var expiresAt = DateTime.UtcNow.AddDays(Math.Max(1, _settings.RefreshTokenExpirationInDays));

        _context.HtRefreshTokens.Add(new HtRefreshTokenEntity
        {
            UserId = userId,
            TokenHash = Hash(plainToken),
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = createdByIp
        });

        await _context.SaveChangesAsync(ct);

        return new RefreshTokenIssueResult(plainToken, expiresAt);
    }

    public async Task<RefreshTokenRotateResult> RotateAsync(string plainToken, string? requestIp, CancellationToken ct = default)
    {
        var hash = Hash(plainToken);
        var existing = await _context.HtRefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == hash, ct);

        if (existing is null)
        {
            return new RefreshTokenRotateResult(RefreshTokenStatus.Invalid, null, null, null);
        }

        if (existing.RevokedAt is not null)
        {
            // Token đã bị revoke trước đó nhưng vẫn bị đem ra dùng lại -> nghi ngờ bị đánh cắp.
            // Thu hồi toàn bộ token còn sống của user này để buộc đăng nhập lại trên mọi thiết bị.
            await RevokeAllForUserAsync(existing.UserId, requestIp, ct);
            return new RefreshTokenRotateResult(RefreshTokenStatus.ReuseDetected, existing.UserId, null, null);
        }

        if (existing.ExpiresAt <= DateTime.UtcNow)
        {
            return new RefreshTokenRotateResult(RefreshTokenStatus.Expired, existing.UserId, null, null);
        }

        var newPlainToken = GeneratePlainToken();
        var newHash = Hash(newPlainToken);
        var newExpiresAt = DateTime.UtcNow.AddDays(Math.Max(1, _settings.RefreshTokenExpirationInDays));

        existing.RevokedAt = DateTime.UtcNow;
        existing.RevokedByIp = requestIp;
        existing.ReplacedByTokenHash = newHash;

        _context.HtRefreshTokens.Add(new HtRefreshTokenEntity
        {
            UserId = existing.UserId,
            TokenHash = newHash,
            ExpiresAt = newExpiresAt,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = requestIp
        });

        await _context.SaveChangesAsync(ct);

        return new RefreshTokenRotateResult(RefreshTokenStatus.Success, existing.UserId, newPlainToken, newExpiresAt);
    }

    public async Task RevokeAsync(string plainToken, string? revokedByIp, CancellationToken ct = default)
    {
        var hash = Hash(plainToken);
        var existing = await _context.HtRefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == hash && t.RevokedAt == null, ct);

        if (existing is null) return;

        existing.RevokedAt = DateTime.UtcNow;
        existing.RevokedByIp = revokedByIp;
        await _context.SaveChangesAsync(ct);
    }

    public async Task RevokeAllForUserAsync(uint userId, string? revokedByIp = null, CancellationToken ct = default)
    {
        await _context.HtRefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s
                .SetProperty(t => t.RevokedAt, DateTime.UtcNow)
                .SetProperty(t => t.RevokedByIp, revokedByIp), ct);
    }

    // 256-bit token ngẫu nhiên, mã hoá base64url (không dấu '+', '/', '=' để an toàn khi đặt trong cookie).
    private static string GeneratePlainToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

    private static string Hash(string plainToken)
    {
        var bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(plainToken));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
