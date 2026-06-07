using CRM.Application.Common.Models;
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
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        return entity is null ? null : MapToDomain(entity);
    }

    public async Task<PagedResult<KhachHang>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var query = _context.KhKhachHangs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();
            query = query.Where(c =>
                c.TenKhachHang.Contains(keyword) ||
                c.MaKhachHang.Contains(keyword) ||
                (c.Email != null && c.Email.Contains(keyword)) ||
                (c.SoDienThoai != null && c.SoDienThoai.Contains(keyword)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .ThenByDescending(c => c.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<KhachHang>
        {
            Items = items.Select(MapToDomain).ToList(),
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
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (entity is null)
        {
            return false;
        }

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        _context.KhKhachHangs.Update(entity);
        return true;
    }

    public Task<bool> ExistsMaKhachHangAsync(
        string maKhachHang,
        ulong? excludeId = null,
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

    public async Task<KhachHang?> GetByMaKhachHangAsync(string maKhachHang, CancellationToken cancellationToken = default)
    {
        var entity = await _context.KhKhachHangs
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.MaKhachHang == maKhachHang, cancellationToken);

        return entity is null ? null : MapToDomain(entity);
    }

    private static KhachHang MapToDomain(KhKhachHang entity) =>
        new()
        {
            Id = entity.Id,
            MaKhachHang = entity.MaKhachHang,
            TenKhachHang = entity.TenKhachHang,
            LoaiKhachHangId = entity.LoaiKhachHangId,
            TinhTrangId = entity.TinhTrangId,
            Email = entity.Email,
            SoDienThoai = entity.SoDienThoai,
            MaSoThue = entity.MaSoThue,
            NhanVienPhuTrachId = entity.NhanVienPhuTrachId,
            IsDeleted = entity.IsDeleted,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };

    private static KhKhachHang MapToEntity(KhachHang customer) =>
        new()
        {
            Id = customer.Id,
            MaKhachHang = customer.MaKhachHang,
            TenKhachHang = customer.TenKhachHang,
            LoaiKhachHangId = customer.LoaiKhachHangId,
            TinhTrangId = customer.TinhTrangId,
            Email = customer.Email,
            SoDienThoai = customer.SoDienThoai,
            MaSoThue = customer.MaSoThue,
            NhanVienPhuTrachId = customer.NhanVienPhuTrachId,
            IsDeleted = customer.IsDeleted,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt
        };
}
