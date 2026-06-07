using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CRM.Application.Interfaces.Auth;
using CRM.Application.Models.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CRM.Infrastructure.Identity;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _settings;

    public JwtTokenService(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }

    public AuthTokenResult GenerateToken(AuthUser authUser)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_settings.ExpirationInMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, authUser.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, authUser.Username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, authUser.Id.ToString()),
            new(ClaimTypes.Name, authUser.Username),
            new(ClaimTypes.Role, authUser.RoleName)
        };

        if (!string.IsNullOrWhiteSpace(authUser.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, authUser.Email));
        }

        if (!string.IsNullOrWhiteSpace(authUser.HoTen))
        {
            claims.Add(new Claim("hoTen", authUser.HoTen));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new AuthTokenResult
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expiresAt
        };
    }
}
