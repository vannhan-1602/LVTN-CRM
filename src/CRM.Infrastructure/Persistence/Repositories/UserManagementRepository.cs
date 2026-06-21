using CRM.Application.Features.Users.DTOs;
using CRM.Application.Interfaces.Users;
using CRM.Infrastructure.Persistence.Contexts;
using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Persistence.Repositories;

public class UserManagementRepository : IUserManagementRepository
{
    private readonly CrmDbContext _context;

    public UserManagementRepository(CrmDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserDto>> GetAllAsync(CancellationToken ct = default)
    {
        var query =
            from u in _context.HtUsers.AsNoTracking()
            join role in _context.HtRoles on u.RoleId equals role.Id into roleJoin
            from role in roleJoin.DefaultIfEmpty()
            join ns in _context.HtThongTinNhanSu on u.NhanSuId equals ns.Id into nsJoin
            from ns in nsJoin.DefaultIfEmpty()
            join pb in _context.HtPhongBans on ns.PhongBanId equals pb.Id into pbJoin
            from pb in pbJoin.DefaultIfEmpty()
            join cv in _context.HtChucVus on ns.ChucVuId equals cv.Id into cvJoin
            from cv in cvJoin.DefaultIfEmpty()
            orderby u.Username
            select new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                TrangThai = u.TrangThai,
                RoleId = u.RoleId,
                RoleName = role != null ? role.TenRole : null,
                NhanSuId = u.NhanSuId,
                HoTen = ns != null ? ns.HoTen : null,
                Email = ns != null ? ns.Email : null,
                SoDienThoai = ns != null ? ns.SoDienThoai : null,
                PhongBanId = ns != null ? ns.PhongBanId : null,
                TenPhongBan = pb != null ? pb.TenPhongBan : null,
                ChucVuId = ns != null ? ns.ChucVuId : null,
                TenChucVu = cv != null ? cv.TenChucVu : null,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            };

        return await query.ToListAsync(ct);
    }

    public async Task<UserDto?> GetByIdAsync(uint id, CancellationToken ct = default)
    {
        var users = await GetAllAsync(ct);
        return users.FirstOrDefault(u => u.Id == id);
    }

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
        var nhanSu = new HtThongTinNhanSu
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

        var user = new HtUser
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
