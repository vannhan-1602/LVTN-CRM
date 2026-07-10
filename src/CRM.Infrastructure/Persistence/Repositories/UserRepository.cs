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

    public async Task<(int TokenVersion, string TrangThai)?> GetTokenVersionAsync(
        uint userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.HtUsers
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new { u.TokenVersion, u.TrangThai })
            .FirstOrDefaultAsync(cancellationToken);

        return user is null ? null : (user.TokenVersion, user.TrangThai);
    }

    public async Task<UserAccount?> GetByIdWithPasswordAsync(
        uint userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.HtUsers
            .AsNoTracking()
            .Include(u => u.Role)
            .Include(u => u.NhanSu)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        return user is null ? null : MapToUserAccount(user);
    }

    private static UserAccount MapToUserAccount(Persistence.Entities.HtUserEntity user) =>
        new()
        {
            Id = user.Id,
            Username = user.Username,
            Password = user.Password,
            TrangThai = user.TrangThai,
            RoleId = user.RoleId,
            RoleName = user.Role?.TenRole ?? string.Empty,
            NhanSuId = user.NhanSuId,
            HoTen = user.NhanSu?.HoTen,
            Email = user.NhanSu?.Email,
            TokenVersion = user.TokenVersion,
            CreatedAt = user.CreatedAt
        };
}