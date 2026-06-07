using CRM.Application.Models.Auth;

namespace CRM.Application.Interfaces.Auth;

public interface IJwtTokenService
{
    AuthTokenResult GenerateToken(AuthUser authUser);
}
