using CRM.Application.Common.Models;
using CRM.Application.Features.PhieuThuChi.DTOs;
using CRM.Application.Interfaces.PhieuThuChi;
using CRM.Domain.Enums;
using CRM.Infrastructure.Persistence.Contexts;
using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using DomainPhieuThuChi = CRM.Domain.Entities.Sales.PhieuThuChi;

namespace CRM.Infrastructure.Persistence.Repositories;

public class PhieuThuChiRepository : IPhieuThuChiRepository
{
    private readonly CrmDbContext _context;
    public PhieuThuChiRepository(CrmDbContext context) => _context = context;

    public async Task<DomainPhieuThuChi?> GetByIdAsync(ulong id, CancellationToken ct = default)
    {
        var entity = await _context.KtPhieuThuChis
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        return entity is null ? null : MapToDomain(entity);
    }

    public async Task<PhieuThuChiDto?> GetByIdEnrichedAsync(ulong id, CancellationToken ct = default)
    {
        var result = await BuildBaseQuery()
            .Where(x => x.Phieu.Id == id)
            .FirstOrDefaultAsync(ct);

        if (result is null) return null;

        var tenNguoiLap = await GetTenNguoiLapAsync(result.Phieu.NguoiLap_Id, ct);
        return MapToDto(result.Phieu, result.TenKhachHang, result.MaHoaDon, tenNguoiLap);
    }

    public async Task<PagedResult<PhieuThuChiDto>> GetPagedAsync(
        int pageNumber, int pageSize, ulong? khachHangId, ulong? hoaDonId,
        string? loaiPhieu, CancellationToken ct = default)
    {
        var query = BuildBaseQuery();

        if (khachHangId.HasValue)
            query = query.Where(x => x.Phieu.KhachHang_Id == khachHangId.Value);

        if (hoaDonId.HasValue)
            query = query.Where(x => x.Phieu.HoaDon_Id == hoaDonId.Value);

        if (!string.IsNullOrWhiteSpace(loaiPhieu))
            query = query.Where(x => x.Phieu.LoaiPhieu == loaiPhieu);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.Phieu.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        // Tra tên người lập theo lô — tránh N+1 query
        var nguoiLapIds = items
            .Where(x => x.Phieu.NguoiLap_Id.HasValue)
            .Select(x => x.Phieu.NguoiLap_Id!.Value)
            .Distinct()
            .ToList();
        var tenNguoiLapMap = await GetTenNguoiLapMapAsync(nguoiLapIds, ct);

        return new PagedResult<PhieuThuChiDto>
        {
            Items = items.Select(x => MapToDto(
                x.Phieu, x.TenKhachHang, x.MaHoaDon,
                x.Phieu.NguoiLap_Id.HasValue
                    && tenNguoiLapMap.TryGetValue(x.Phieu.NguoiLap_Id.Value, out var ten)
                    ? ten : null
            )).ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<DomainPhieuThuChi> AddAsync(DomainPhieuThuChi phieu, CancellationToken ct = default)
    {
        var entity = MapToEntity(phieu);
        _context.KtPhieuThuChis.Add(entity);
        await _context.SaveChangesAsync(ct);
        phieu.Id = entity.Id;
        return phieu;
    }

    public async Task<string> GenerateMaPhieuAsync(string loaiPhieu, CancellationToken ct = default)
    {
        // Dùng ngày + random suffix để tránh race condition khi nhiều request đến cùng lúc.
        // Format: PT-20260702-A3F9 (Phiếu Thu) hoặc PC-20260702-A3F9 (Phiếu Chi)
        var prefix = loaiPhieu == PaymentVoucherType.Chi ? "PC" : "PT";
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var randomPart = Guid.NewGuid().ToString("N")[..6].ToUpper();
        return $"{prefix}-{datePart}-{randomPart}";
    }

    public Task<decimal> GetTongDaThuByHoaDonAsync(ulong hoaDonId, CancellationToken ct = default) =>
        _context.KtPhieuThuChis
            .Where(x => x.HoaDon_Id == hoaDonId && x.LoaiPhieu == PaymentVoucherType.Thu)
            .SumAsync(x => x.SoTien, ct);

    // ── Query cơ sở: join Khách hàng + Hóa đơn (cùng kiểu ulong, an toàn) ─────
    private IQueryable<PhieuThuChiProjection> BuildBaseQuery() =>
        from p in _context.KtPhieuThuChis
        join kh in _context.KhKhachHangs
            on p.KhachHang_Id equals kh.Id into khJoin
        from kh in khJoin.DefaultIfEmpty()
        join hd in _context.KtHoaDons
            on p.HoaDon_Id equals hd.Id into hdJoin
        from hd in hdJoin.DefaultIfEmpty()
        select new PhieuThuChiProjection
        {
            Phieu = p,
            TenKhachHang = kh != null ? kh.TenKhachHang : null,
            MaHoaDon = hd != null ? hd.MaHoaDon : null
        };

    // Tra tên người lập (1 người) — dùng cho GetByIdEnrichedAsync
    private async Task<string?> GetTenNguoiLapAsync(uint? nguoiLapId, CancellationToken ct)
    {
        if (!nguoiLapId.HasValue) return null;
        var map = await GetTenNguoiLapMapAsync([nguoiLapId.Value], ct);
        return map.TryGetValue(nguoiLapId.Value, out var ten) ? ten : null;
    }

    // Tra tên người lập theo lô — dùng cho GetPagedAsync, tránh N+1
    private async Task<Dictionary<uint, string>> GetTenNguoiLapMapAsync(
        IReadOnlyList<uint> userIds, CancellationToken ct)
    {
        if (userIds.Count == 0) return [];

        return await (
            from u in _context.HtUsers
            where userIds.Contains(u.Id)
            join ns in _context.HtThongTinNhanSu on u.NhanSuId equals ns.Id into nsJoin
            from ns in nsJoin.DefaultIfEmpty()
            select new { u.Id, HoTen = ns != null ? ns.HoTen : u.Username }
        ).ToDictionaryAsync(x => x.Id, x => x.HoTen, ct);
    }

    private class PhieuThuChiProjection
    {
        public KtPhieuThuChiEntity Phieu { get; set; } = null!;
        public string? TenKhachHang { get; set; }
        public string? MaHoaDon { get; set; }
    }

    // ── Mappers ───────────────────────────────────────────────────────────────
    private static DomainPhieuThuChi MapToDomain(KtPhieuThuChiEntity e) => new()
    {
        Id = e.Id,
        MaPhieu = e.MaPhieu,
        LoaiPhieu = e.LoaiPhieu,
        KhachHangId = e.KhachHang_Id,
        HoaDonId = e.HoaDon_Id,
        SoTien = e.SoTien,
        NguoiLapId = e.NguoiLap_Id,          // uint? → uint? trực tiếp, không cần cast
        CreatedAt = e.NgayTao,
        UpdatedAt = e.UpdatedAt
    };

    private static KtPhieuThuChiEntity MapToEntity(DomainPhieuThuChi d) => new()
    {
        MaPhieu = d.MaPhieu,
        LoaiPhieu = d.LoaiPhieu,
        KhachHang_Id = d.KhachHangId,
        HoaDon_Id = d.HoaDonId,
        SoTien = d.SoTien,
        NguoiLap_Id = d.NguoiLapId,          // uint? → uint? trực tiếp
        NgayTao = d.CreatedAt,
        UpdatedAt = d.UpdatedAt
    };

    private static PhieuThuChiDto MapToDto(
        KtPhieuThuChiEntity e, string? tenKhachHang, string? maHoaDon, string? tenNguoiLap) => new()
    {
        Id = e.Id,
        MaPhieu = e.MaPhieu,
        LoaiPhieu = e.LoaiPhieu,
        KhachHangId = e.KhachHang_Id,
        TenKhachHang = tenKhachHang,
        HoaDonId = e.HoaDon_Id,
        MaHoaDon = maHoaDon,
        SoTien = e.SoTien,
        NguoiLapId = e.NguoiLap_Id,
        TenNguoiLap = tenNguoiLap,
        NgayTao = e.NgayTao,
        UpdatedAt = e.UpdatedAt
    };
}
