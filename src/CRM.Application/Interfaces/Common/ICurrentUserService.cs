namespace CRM.Application.Interfaces.Common;

public interface ICurrentUserService
{
    uint? UserId { get; }
    string? Username { get; }
    bool IsAuthenticated { get; }
}
