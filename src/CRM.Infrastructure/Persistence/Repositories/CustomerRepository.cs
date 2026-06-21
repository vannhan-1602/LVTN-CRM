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
            where kh.Id == id && !kh.IsDeleted
            join loai in _context.KhLoaiKhachHangs on kh.LoaiKhachHangId equals loai.Id into loaiJoin
            from loai in loaiJoin.DefaultIfEmpty()
            join tinh in _context.KhTinhTrangKhachHangs on kh.TinhTrangId equals tinh.Id into tinhJoin
            from tinh in tinhJoin.DefaultIfEmpty()
            join ns in _context.HtThongTinNhanSu on (uint?)kh.NhanVienPhuTrachId equals (uint?)ns.Id into nsJoin
            from ns in nsJoin.DefaultIfEmpty()
            select new
            {
                KhachHang = kh,
                TenLoai = loai != null ? loai.TenLoai : null,
                TenTinhTrang = tinh != null ? tinh.TenTinhTrang : null,
                TenNhanVien = ns != null ? ns.HoTen : null
            }
        ).FirstOrDefaultAsync(cancellationToken);

        if (result is null) return null;

        return CustomerMapper.ToDto(MapToDomain(result.KhachHang),
            result.TenLoai, result.TenTinhTrang, result.TenNhanVien);
    }

    public async Task<PagedResult<CustomerDto>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? search,
        ushort? loaiKhachHangId,
        ushort? tinhTrangId,
        uint? ownerNhanSuId,
        CancellationToken cancellationToken = default)
    {
        var query =
            from kh in _context.KhKhachHangs.AsNoTracking()
            where !kh.IsDeleted
            join loai in _context.KhLoaiKhachHangs on kh.LoaiKhachHangId equals loai.Id into loaiJoin
            from loai in loaiJoin.DefaultIfEmpty()
            join tinh in _context.KhTinhTrangKhachHangs on kh.TinhTrangId equals tinh.Id into tinhJoin
            from tinh in tinhJoin.DefaultIfEmpty()
            join ns in _context.HtThongTinNhanSu on (uint?)kh.NhanVienPhuTrachId equals (uint?)ns.Id into nsJoin
            from ns in nsJoin.DefaultIfEmpty()
            select new
            {
                KhachHang = kh,
                TenLoai = loai != null ? loai.TenLoai : null,
                TenTinhTrang = tinh != null ? tinh.TenTinhTrang : null,
                TenNhanVien = ns != null ? ns.HoTen : null
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
        if (ownerNhanSuId.HasValue)
            query = query.Where(x => x.KhachHang.NhanVienPhuTrachId == ownerNhanSuId.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.KhachHang.CreatedAt)
            .ThenByDescending(x => x.KhachHang.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(x =>
            CustomerMapper.ToDto(MapToDomain(x.KhachHang),
                x.TenLoai, x.TenTinhTrang, x.TenNhanVien))
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

    public async Task<KhachHang?> GetByMaKhachHangAsync(string maKhachHang, CancellationToken cancellationToken = default)
    {
        var entity = await _context.KhKhachHangs
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.MaKhachHang == maKhachHang && !c.IsDeleted, cancellationToken);
        return entity is null ? null : MapToDomain(entity);
    }

    private static KhachHang MapToDomain(KhKhachHang e) => new()
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
        IsDeleted = e.IsDeleted,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };

    private static KhKhachHang MapToEntity(KhachHang d) => new()
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
        IsDeleted = d.IsDeleted,
        CreatedAt = d.CreatedAt,
        UpdatedAt = d.UpdatedAt
    };
}
