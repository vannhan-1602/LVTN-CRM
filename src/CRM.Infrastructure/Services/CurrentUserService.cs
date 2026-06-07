using System.Security.Claims;
using CRM.Application.Interfaces.Common;
using Microsoft.AspNetCore.Http;

namespace CRM.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public uint? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User
                .FindFirstValue(ClaimTypes.NameIdentifier);

            return uint.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }

    public string? Username =>
        _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name);

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}
