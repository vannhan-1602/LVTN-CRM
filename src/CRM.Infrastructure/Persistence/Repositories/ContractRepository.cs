using CRM.Application.Common.Models;
using CRM.Application.Features.Contracts.DTOs;
using CRM.Application.Interfaces.Contracts;
using CRM.Domain.Entities.Sales;
using CRM.Infrastructure.Persistence.Contexts;
using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Persistence.Repositories;

public class ContractRepository : IContractRepository
{
    private readonly CrmDbContext _context;
    public ContractRepository(CrmDbContext context) => _context = context;

    public async Task<HopDong?> GetByIdAsync(ulong id, CancellationToken ct = default)
    {
        var e = await _context.Set<HdHopDongEntity>().FirstOrDefaultAsync(x => x.Id == id, ct);
        return e is null ? null : MapToDomain(e);
    }

    public async Task<ContractDto?> GetByIdEnrichedAsync(ulong id, CancellationToken ct = default)
    {
        var result = await BuildEnrichedQuery().FirstOrDefaultAsync(x => x.HopDong.Id == id, ct);
        if (result is null) return null;

        return MapToDto(result.HopDong, result.TenKhachHang, result.MaBaoGia, result.GiaTri);
    }

    public async Task<PagedResult<ContractDto>> GetPagedAsync(
        int pageNumber, int pageSize, string? search, string? trangThai,
        ulong? khachHangId, CancellationToken ct = default)
    {
        var query = BuildEnrichedQuery();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.HopDong.MaHopDong.Contains(search));

        if (!string.IsNullOrWhiteSpace(trangThai))
            query = query.Where(x => x.HopDong.TrangThai == trangThai);

        if (khachHangId.HasValue)
            query = query.Where(x => x.HopDong.KhachHang_Id == khachHangId.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(x => x.HopDong.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var dtos = items.Select(x => MapToDto(x.HopDong, x.TenKhachHang, x.MaBaoGia, x.GiaTri)).ToList();

        return new PagedResult<ContractDto> { Items = dtos, PageNumber = pageNumber, PageSize = pageSize, TotalCount = total };
    }

    public async Task<HopDong> AddAsync(HopDong contract, CancellationToken ct = default)
    {
        var entity = MapToEntity(contract);
        _context.Set<HdHopDongEntity>().Add(entity);
        await _context.SaveChangesAsync(ct);
        contract.Id = entity.Id;
        return contract;
    }

    public async Task UpdateStatusAsync(ulong id, string trangThai, CancellationToken ct = default)
    {
        var entity = await _context.Set<HdHopDongEntity>().FindAsync([id], ct);
        if (entity is null) return;
        entity.TrangThai = trangThai;
        entity.UpdatedAt = DateTime.UtcNow;
    }

    public async Task<bool> DeleteAsync(ulong id, CancellationToken ct = default)
    {
        var entity = await _context.Set<HdHopDongEntity>().FindAsync([id], ct);
        if (entity is null) return false;
        _context.Set<HdHopDongEntity>().Remove(entity);
        return true;
    }

    public async Task<string> GenerateMaHopDongAsync(CancellationToken ct = default)
    {
        var last = await _context.Set<HdHopDongEntity>().OrderByDescending(x => x.Id).FirstOrDefaultAsync(ct);
        var next = (last?.Id ?? 0) + 1;
        return $"HD{next:D5}";
    }

    public Task<bool> ExistsForBaoGiaAsync(ulong baoGiaId, CancellationToken ct = default) =>
        _context.Set<HdHopDongEntity>().AnyAsync(x => x.BaoGia_Id == baoGiaId, ct);

    private IQueryable<ContractEnrichedRow> BuildEnrichedQuery() =>
        from hd in _context.Set<HdHopDongEntity>().AsNoTracking()
        join kh in _context.KhKhachHangs on hd.KhachHang_Id equals kh.Id into khJoin
        from kh in khJoin.DefaultIfEmpty()
        join bg in _context.Set<HdBaoGiaEntity>() on hd.BaoGia_Id equals (ulong?)bg.Id into bgJoin
        from bg in bgJoin.DefaultIfEmpty()
        select new ContractEnrichedRow
        {
            HopDong = hd,
            TenKhachHang = kh != null ? kh.TenKhachHang : null,
            MaBaoGia = bg != null ? bg.MaBaoGia : null,
            GiaTri = bg != null ? (decimal?)bg.TongTien : null
        };

    private class ContractEnrichedRow
    {
        public HdHopDongEntity HopDong { get; set; } = null!;
        public string? TenKhachHang { get; set; }
        public string? MaBaoGia { get; set; }
        public decimal? GiaTri { get; set; }
    }

    private static HopDong MapToDomain(HdHopDongEntity e) => new()
    {
        Id = e.Id, MaHopDong = e.MaHopDong, KhachHangId = e.KhachHang_Id,
        BaoGiaGocId = e.BaoGia_Id, NgayKy = e.NgayKy, ThoiHan = e.ThoiHan,
        TrangThai = e.TrangThai, CreatedAt = e.CreatedAt, UpdatedAt = e.UpdatedAt
    };

    private static HdHopDongEntity MapToEntity(HopDong d) => new()
    {
        MaHopDong = d.MaHopDong, KhachHang_Id = d.KhachHangId, BaoGia_Id = d.BaoGiaGocId,
        NgayKy = d.NgayKy, ThoiHan = d.ThoiHan, TrangThai = d.TrangThai,
        CreatedAt = d.CreatedAt, UpdatedAt = d.UpdatedAt
    };

    private static ContractDto MapToDto(HdHopDongEntity e, string? tenKhachHang, string? maBaoGia, decimal? giaTri) => new()
    {
        Id = e.Id, MaHopDong = e.MaHopDong, KhachHangId = e.KhachHang_Id, TenKhachHang = tenKhachHang,
        BaoGiaId = e.BaoGia_Id, MaBaoGia = maBaoGia, GiaTri = giaTri,
        NgayKy = e.NgayKy, ThoiHan = e.ThoiHan, TrangThai = e.TrangThai,
        CreatedAt = e.CreatedAt, UpdatedAt = e.UpdatedAt
    };
}
