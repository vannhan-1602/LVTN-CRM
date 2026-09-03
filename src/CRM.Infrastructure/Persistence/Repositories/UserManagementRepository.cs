using CRM.Application.Features.Users.DTOs;
using CRM.Application.Interfaces.Users;
using CRM.Infrastructure.Identity;
using CRM.Infrastructure.Persistence.Contexts;
using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Persistence.Repositories;

public class UserManagementRepository : IUserManagementRepository
{
    private readonly CrmDbContext _context;
    private readonly TokenVersionCache _tokenVersionCache;
    private readonly CRM.Application.Interfaces.Auth.IRefreshTokenService _refreshTokenService;

    public UserManagementRepository(
        CrmDbContext context,
        TokenVersionCache tokenVersionCache,
        CRM.Application.Interfaces.Auth.IRefreshTokenService refreshTokenService)
    {
        _context = context;
        _tokenVersionCache = tokenVersionCache;
        _refreshTokenService = refreshTokenService;
    }

    public async Task<List<UserDto>> GetAllAsync(CancellationToken ct = default) =>
        await BuildUserDtoQuery()
            .OrderBy(x => x.U.Username)
            .Select(ProjectToUserDto)
            .ToListAsync(ct);

    public async Task<UserDto?> GetByIdAsync(uint id, CancellationToken ct = default) =>
      
        await BuildUserDtoQuery()
            .Where(x => x.U.Id == id)
            .Select(ProjectToUserDto)
            .FirstOrDefaultAsync(ct);

 
    private sealed record UserJoinRow(HtUserEntity U, HtRoleEntity? Role, HtThongTinNhanSuEntity? Ns, HtPhongBanEntity? Pb, HtChucVuEntity? Cv);

    private IQueryable<UserJoinRow> BuildUserDtoQuery() =>
        from u in _context.HtUsers.AsNoTracking()
        join role in _context.HtRoles on u.RoleId equals role.Id into roleJoin
        from role in roleJoin.DefaultIfEmpty()
        join ns in _context.HtThongTinNhanSu on u.NhanSuId equals ns.Id into nsJoin
        from ns in nsJoin.DefaultIfEmpty()
        join pb in _context.HtPhongBans on ns.PhongBanId equals pb.Id into pbJoin
        from pb in pbJoin.DefaultIfEmpty()
        join cv in _context.HtChucVus on ns.ChucVuId equals cv.Id into cvJoin
        from cv in cvJoin.DefaultIfEmpty()
        select new UserJoinRow(u, role, ns, pb, cv);

    private static readonly System.Linq.Expressions.Expression<Func<UserJoinRow, UserDto>> ProjectToUserDto =
        x => new UserDto
        {
            Id = x.U.Id,
            Username = x.U.Username,
            TrangThai = x.U.TrangThai,
            RoleId = x.U.RoleId,
            RoleName = x.Role != null ? x.Role.TenRole : null,
            NhanSuId = x.U.NhanSuId,
            HoTen = x.Ns != null ? x.Ns.HoTen : null,
            Email = x.Ns != null ? x.Ns.Email : null,
            SoDienThoai = x.Ns != null ? x.Ns.SoDienThoai : null,
            PhongBanId = x.Ns != null ? x.Ns.PhongBanId : null,
            TenPhongBan = x.Pb != null ? x.Pb.TenPhongBan : null,
            ChucVuId = x.Ns != null ? x.Ns.ChucVuId : null,
            TenChucVu = x.Cv != null ? x.Cv.TenChucVu : null,
            CreatedAt = x.U.CreatedAt,
            UpdatedAt = x.U.UpdatedAt
        };

    public Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default) =>
        _context.HtUsers.AnyAsync(u => u.Username == username, ct);

    public Task<bool> EmailExistsAsync(string? email, uint? excludeNhanSuId = null, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email)) return Task.FromResult(false);
        return _context.HtThongTinNhanSu.AnyAsync(
            ns => ns.Email == email && (!excludeNhanSuId.HasValue || ns.Id != excludeNhanSuId.Value), ct);
    }

    public Task<bool> RoleExistsAsync(uint roleId, CancellationToken ct = default) =>
        _context.HtRoles.AnyAsync(r => r.Id == roleId, ct);

    public Task<bool> PhongBanExistsAsync(ushort id, CancellationToken ct = default) =>
        _context.HtPhongBans.AnyAsync(p => p.Id == id && p.IsActive, ct);

    public Task<bool> ChucVuExistsAsync(ushort id, CancellationToken ct = default) =>
        _context.HtChucVus.AnyAsync(c => c.Id == id && c.IsActive, ct);

    public async Task<uint> CreateAsync(
        string username, string passwordHash, uint roleId,
        string hoTen, string? email, string? soDienThoai,
        ushort? phongBanId, ushort? chucVuId,
        CancellationToken ct = default)
    {
        //  tạo nhân sự trước, user sau
        var nhanSu = new HtThongTinNhanSuEntity
        {
            HoTen = hoTen,
            Email = email,
            SoDienThoai = soDienThoai,
            PhongBanId = phongBanId,
            ChucVuId = chucVuId,
            TrangThai = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _context.HtThongTinNhanSu.AddAsync(nhanSu, ct);
        await _context.SaveChangesAsync(ct); // cần Id của nhân sự trước khi tạo user

        var user = new HtUserEntity
        {
            NhanSuId = nhanSu.Id,
            Username = username,
            Password = passwordHash,
            RoleId = roleId,
            TrangThai = "Active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _context.HtUsers.AddAsync(user, ct);
        await _context.SaveChangesAsync(ct);

        return user.Id;
    }

    public async Task UpdateAsync(
        uint userId, uint roleId,
        string hoTen, string? email, string? soDienThoai,
        ushort? phongBanId, ushort? chucVuId,
        CancellationToken ct = default)
    {
        var user = await _context.HtUsers.FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new InvalidOperationException("Không tìm thấy tài khoản.");

        user.RoleId = roleId;
        user.UpdatedAt = DateTime.UtcNow;

        if (user.NhanSuId.HasValue)
        {
            var nhanSu = await _context.HtThongTinNhanSu.FirstOrDefaultAsync(n => n.Id == user.NhanSuId.Value, ct);
            if (nhanSu is not null)
            {
                nhanSu.HoTen = hoTen;
                nhanSu.Email = email;
                nhanSu.SoDienThoai = soDienThoai;
                nhanSu.PhongBanId = phongBanId;
                nhanSu.ChucVuId = chucVuId;
                nhanSu.UpdatedAt = DateTime.UtcNow;
            }
        }
    }

    public async Task UpdatePasswordAsync(uint userId, string passwordHash, CancellationToken ct = default)
    {
        var user = await _context.HtUsers.FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new InvalidOperationException("Không tìm thấy tài khoản.");

        user.Password = passwordHash;
        user.UpdatedAt = DateTime.UtcNow;
    }

    public async Task UpdateStatusAsync(uint userId, string trangThai, CancellationToken ct = default)
    {
        var user = await _context.HtUsers.FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new InvalidOperationException("Không tìm thấy tài khoản.");

        user.TrangThai = trangThai;
        user.UpdatedAt = DateTime.UtcNow;

      
        _tokenVersionCache.Invalidate(userId);
    }

    
    public async Task IncrementTokenVersionAsync(uint userId, CancellationToken ct = default)
    {
        await _context.HtUsers
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.TokenVersion, u => u.TokenVersion + 1), ct);

        _tokenVersionCache.Invalidate(userId);

       
        await _refreshTokenService.RevokeAllForUserAsync(userId, ct: ct);
    }

    public async Task<bool> DeleteAsync(uint userId, CancellationToken ct = default)
    {
        var user = await _context.HtUsers.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null) return false;

        // Chỉ xóa tài khoản đăng nhập, giữ lại bản ghi nhân sự (lịch sử)
        _context.HtUsers.Remove(user);
        return true;
    }

    public async Task<UserLookupsDto> GetLookupsAsync(CancellationToken ct = default)
    {
        var roles = await _context.HtRoles.AsNoTracking()
            .Select(r => new RoleDto { Id = r.Id, TenRole = r.TenRole, MoTa = r.MoTa })
            .ToListAsync(ct);

        var phongBans = await _context.HtPhongBans.AsNoTracking()
            .Where(p => p.IsActive)
            .Select(p => new PhongBanDto { Id = p.Id, TenPhongBan = p.TenPhongBan })
            .ToListAsync(ct);

        var chucVus = await _context.HtChucVus.AsNoTracking()
            .Where(c => c.IsActive)
            .Select(c => new ChucVuDto { Id = c.Id, TenChucVu = c.TenChucVu })
            .ToListAsync(ct);

        return new UserLookupsDto { Roles = roles, PhongBans = phongBans, ChucVus = chucVus };
    }
}