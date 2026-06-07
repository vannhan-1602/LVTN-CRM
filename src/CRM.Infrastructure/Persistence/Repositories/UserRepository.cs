using CRM.Application.Interfaces.Auth;
using CRM.Application.Models.Auth;
using CRM.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly CrmDbContext _context;

    public UserRepository(CrmDbContext context)
    {
        _context = context;
    }

    public async Task<UserAccount?> GetByUsernameWithRoleAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        var user = await _context.HtUsers
            .AsNoTracking()
            .Include(u => u.Role)
            .Include(u => u.NhanSu)
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);

        return user is null ? null : MapToUserAccount(user);
    }

    public async Task<IReadOnlyList<UserAccount>> GetAllWithRolesAsync(
        CancellationToken cancellationToken = default)
    {
        var users = await _context.HtUsers
            .AsNoTracking()
            .Include(u => u.Role)
            .Include(u => u.NhanSu)
            .OrderBy(u => u.Username)
            .ToListAsync(cancellationToken);

        return users.Select(MapToUserAccount).ToList();
    }

    private static UserAccount MapToUserAccount(Persistence.Entities.HtUser user) =>
        new()
        {
            Id = user.Id,
            Username = user.Username,
            Password = user.Password,
            TrangThai = user.TrangThai,
            RoleId = user.RoleId,
            RoleName = user.Role?.TenRole ?? string.Empty,
            HoTen = user.NhanSu?.HoTen,
            Email = user.NhanSu?.Email,
            CreatedAt = user.CreatedAt
        };
}
