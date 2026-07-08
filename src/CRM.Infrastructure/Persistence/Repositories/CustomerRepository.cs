using CRM.Application.Common.Models;
using CRM.Application.Features.Customers.DTOs;
using CRM.Application.Features.Customers.Mappings;
using CRM.Application.Interfaces.Customers;
using CRM.Domain.Entities.Customers;
using CRM.Infrastructure.Persistence.Contexts;
using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Persistence.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly CrmDbContext _context;

    public CustomerRepository(CrmDbContext context)
    {
        _context = context;
    }

    public async Task<KhachHang?> GetByIdAsync(ulong id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.KhKhachHangs
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);

        return entity is null ? null : MapToDomain(entity);
    }

    public async Task<CustomerDto?> GetByIdEnrichedAsync(ulong id, CancellationToken cancellationToken = default)
    {
        var result = await (
            from kh in _context.KhKhachHangs
            where kh.Id == id
            join loai in _context.KhLoaiKhachHangs on kh.LoaiKhachHangId equals loai.Id into loaiJoin
            from loai in loaiJoin.DefaultIfEmpty()
            join tinh in _context.KhTinhTrangKhachHangs on kh.TinhTrangId equals tinh.Id into tinhJoin
            from tinh in tinhJoin.DefaultIfEmpty()
            join ns in _context.HtThongTinNhanSu on (uint?)kh.NhanVienPhuTrachId equals (uint?)ns.Id into nsJoin
            from ns in nsJoin.DefaultIfEmpty()
            join hang in _context.Set<KhXepHangEntity>() on kh.HangKhachHang_Id equals hang.Id into hangJoin
            from hang in hangJoin.DefaultIfEmpty()
            select new
            {
                KhachHang = kh,
                TenLoai = loai != null ? loai.TenLoai : null,
                TenTinhTrang = tinh != null ? tinh.TenTinhTrang : null,
                TenNhanVien = ns != null ? ns.HoTen : null,
                TenHang = hang != null ? hang.TenHang : null
            }
        ).FirstOrDefaultAsync(cancellationToken);

        if (result is null) return null;

        return CustomerMapper.ToDto(MapToDomain(result.KhachHang),
            result.TenLoai, result.TenTinhTrang, result.TenNhanVien, result.TenHang);
    }

    public async Task<PagedResult<CustomerDto>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? search,
        ushort? loaiKhachHangId,
        ushort? tinhTrangId,
        uint? ownerUserId,
        bool? isDeleted = null,
        CancellationToken cancellationToken = default)
    {
        var query =
            from kh in _context.KhKhachHangs.AsNoTracking()
            where kh.IsDeleted == (isDeleted ?? false)
            join loai in _context.KhLoaiKhachHangs on kh.LoaiKhachHangId equals loai.Id into loaiJoin
            from loai in loaiJoin.DefaultIfEmpty()
            join tinh in _context.KhTinhTrangKhachHangs on kh.TinhTrangId equals tinh.Id into tinhJoin
            from tinh in tinhJoin.DefaultIfEmpty()
            join ns in _context.HtThongTinNhanSu on (uint?)kh.NhanVienPhuTrachId equals (uint?)ns.Id into nsJoin
            from ns in nsJoin.DefaultIfEmpty()
            join hang in _context.Set<KhXepHangEntity>() on kh.HangKhachHang_Id equals hang.Id into hangJoin
            from hang in hangJoin.DefaultIfEmpty()
            select new
            {
                KhachHang = kh,
                TenLoai = loai != null ? loai.TenLoai : null,
                TenTinhTrang = tinh != null ? tinh.TenTinhTrang : null,
                TenNhanVien = ns != null ? ns.HoTen : null,
                TenHang = hang != null ? hang.TenHang : null
            };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();
            query = query.Where(x =>
                x.KhachHang.TenKhachHang.Contains(keyword) ||
                x.KhachHang.MaKhachHang.Contains(keyword) ||
                (x.KhachHang.Email != null && x.KhachHang.Email.Contains(keyword)) ||
                (x.KhachHang.SoDienThoai != null && x.KhachHang.SoDienThoai.Contains(keyword)));
        }

        if (loaiKhachHangId.HasValue)
            query = query.Where(x => x.KhachHang.LoaiKhachHangId == loaiKhachHangId.Value);

        if (tinhTrangId.HasValue)
            query = query.Where(x => x.KhachHang.TinhTrangId == tinhTrangId.Value);

        //  Sale chỉ thấy Customer mình phụ trách 
        if (ownerUserId.HasValue)
            query = query.Where(x => x.KhachHang.NhanVienPhuTrachId == ownerUserId.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.KhachHang.CreatedAt)
            .ThenByDescending(x => x.KhachHang.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(x =>
            CustomerMapper.ToDto(MapToDomain(x.KhachHang),
                x.TenLoai, x.TenTinhTrang, x.TenNhanVien, x.TenHang))
            .ToList();

        return new PagedResult<CustomerDto>
        {
            Items = dtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<KhachHang> AddAsync(KhachHang customer, CancellationToken cancellationToken = default)
    {
        var entity = MapToEntity(customer);
        await _context.KhKhachHangs.AddAsync(entity, cancellationToken);
        customer.Id = entity.Id;
        return MapToDomain(entity);
    }

    public Task UpdateAsync(KhachHang customer, CancellationToken cancellationToken = default)
    {
        var entity = MapToEntity(customer);
        _context.KhKhachHangs.Update(entity);
        return Task.CompletedTask;
    }

    public async Task<bool> SoftDeleteAsync(ulong id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.KhKhachHangs
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);

        if (entity is null) return false;

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        _context.KhKhachHangs.Update(entity);
        return true;
    }

    public async Task<bool> RestoreAsync(ulong id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.KhKhachHangs
            .FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted, cancellationToken);

        if (entity is null) return false;

        entity.IsDeleted = false;
        entity.UpdatedAt = DateTime.UtcNow;
        _context.KhKhachHangs.Update(entity);
        return true;
    }

    public Task<bool> ExistsMaKhachHangAsync(string maKhachHang, ulong? excludeId = null,
        CancellationToken cancellationToken = default) =>
        _context.KhKhachHangs.AnyAsync(
            c => c.MaKhachHang == maKhachHang && (!excludeId.HasValue || c.Id != excludeId.Value),
            cancellationToken);

    public async Task<string> GenerateMaKhachHangAsync(CancellationToken cancellationToken = default)
    {
        var lastCustomer = await _context.KhKhachHangs
            .IgnoreQueryFilters()
            .OrderByDescending(c => c.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var nextNumber = (lastCustomer?.Id ?? 0) + 1;
        return $"KH{nextNumber:D4}";
    }

    public Task<bool> LoaiKhachHangExistsAsync(ushort id, CancellationToken cancellationToken = default) =>
        _context.KhLoaiKhachHangs.AnyAsync(l => l.Id == id && l.IsActive, cancellationToken);

    public Task<bool> TinhTrangKhachHangExistsAsync(ushort id, CancellationToken cancellationToken = default) =>
        _context.KhTinhTrangKhachHangs.AnyAsync(t => t.Id == id && t.IsActive, cancellationToken);

    public Task<bool> HangKhachHangExistsAsync(ushort id, CancellationToken cancellationToken = default) =>
        _context.Set<KhXepHangEntity>().AnyAsync(h => h.Id == id && h.IsActive, cancellationToken);

    public async Task<KhachHang?> GetByMaKhachHangAsync(string maKhachHang, CancellationToken cancellationToken = default)
    {
        var entity = await _context.KhKhachHangs
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.MaKhachHang == maKhachHang && !c.IsDeleted, cancellationToken);
        return entity is null ? null : MapToDomain(entity);
    }

    public async Task<List<(ulong Id, string MaKhachHang, string TenKhachHang, string TrungTruong)>> FindDuplicatesAsync(
        string? email, string? soDienThoai, string? maSoThue, ulong? excludeId,
        CancellationToken cancellationToken = default)
    {
        var result = new List<(ulong, string, string, string)>();
        if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(soDienThoai) && string.IsNullOrWhiteSpace(maSoThue))
            return result;

        var query = _context.KhKhachHangs.AsNoTracking()
            .Where(c => !c.IsDeleted && (!excludeId.HasValue || c.Id != excludeId.Value));

        if (!string.IsNullOrWhiteSpace(email))
        {
            var matches = await query.Where(c => c.Email == email)
                .Select(c => new { c.Id, c.MaKhachHang, c.TenKhachHang }).ToListAsync(cancellationToken);
            result.AddRange(matches.Select(m => (m.Id, m.MaKhachHang, m.TenKhachHang, "Email")));
        }

        if (!string.IsNullOrWhiteSpace(soDienThoai))
        {
            var matches = await query.Where(c => c.SoDienThoai == soDienThoai)
                .Select(c => new { c.Id, c.MaKhachHang, c.TenKhachHang }).ToListAsync(cancellationToken);
            result.AddRange(matches.Select(m => (m.Id, m.MaKhachHang, m.TenKhachHang, "SoDienThoai")));
        }

        if (!string.IsNullOrWhiteSpace(maSoThue))
        {
            var matches = await query.Where(c => c.MaSoThue == maSoThue)
                .Select(c => new { c.Id, c.MaKhachHang, c.TenKhachHang }).ToListAsync(cancellationToken);
            result.AddRange(matches.Select(m => (m.Id, m.MaKhachHang, m.TenKhachHang, "MaSoThue")));
        }

        return result;
    }

    private static KhachHang MapToDomain(KhKhachHangEntity e) => new()
    {
        Id = e.Id,
        MaKhachHang = e.MaKhachHang,
        TenKhachHang = e.TenKhachHang,
        LoaiKhachHangId = e.LoaiKhachHangId,
        TinhTrangId = e.TinhTrangId,
        Email = e.Email,
        SoDienThoai = e.SoDienThoai,
        MaSoThue = e.MaSoThue,
        NhanVienPhuTrachId = e.NhanVienPhuTrachId,
        NgaySinh           = e.NgaySinh,
        NgayThanhLap       = e.NgayThanhLap,
        HangKhachHangId    = e.HangKhachHang_Id,
        IsDeleted = e.IsDeleted,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };

    private static KhKhachHangEntity MapToEntity(KhachHang d) => new()
    {
        Id = d.Id,
        MaKhachHang = d.MaKhachHang,
        TenKhachHang = d.TenKhachHang,
        LoaiKhachHangId = d.LoaiKhachHangId,
        TinhTrangId = d.TinhTrangId,
        Email = d.Email,
        SoDienThoai = d.SoDienThoai,
        MaSoThue = d.MaSoThue,
        NhanVienPhuTrachId = d.NhanVienPhuTrachId,
        NgaySinh = d.NgaySinh,
        NgayThanhLap = d.NgayThanhLap,
        HangKhachHang_Id = d.HangKhachHangId,
        IsDeleted = d.IsDeleted,
        CreatedAt = d.CreatedAt,
        UpdatedAt = d.UpdatedAt
    };
}