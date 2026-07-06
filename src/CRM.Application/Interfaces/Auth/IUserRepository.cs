using CRM.Application.Models.Auth;

namespace CRM.Application.Interfaces.Auth;

public interface IUserRepository
{
    Task<UserAccount?> GetByUsernameWithRoleAsync(string username, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserAccount>> GetAllWithRolesAsync(CancellationToken cancellationToken = default);

    /// <summary>Lấy nhanh TokenVersion + trạng thái hiện tại — dùng cho middleware xác thực mỗi request.</summary>
    Task<(int TokenVersion, string TrangThai)?> GetTokenVersionAsync(uint userId, CancellationToken cancellationToken = default);
}