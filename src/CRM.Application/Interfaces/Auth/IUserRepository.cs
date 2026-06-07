using CRM.Application.Models.Auth;

namespace CRM.Application.Interfaces.Auth;

public interface IUserRepository
{
    Task<UserAccount?> GetByUsernameWithRoleAsync(string username, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserAccount>> GetAllWithRolesAsync(CancellationToken cancellationToken = default);
}
