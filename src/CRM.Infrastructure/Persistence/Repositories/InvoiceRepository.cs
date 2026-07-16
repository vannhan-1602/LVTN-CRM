using CRM.Application.Common.Models;
using CRM.Application.Features.Invoices.DTOs;
using CRM.Application.Interfaces.Invoices;
using CRM.Domain.Entities.Sales;
using CRM.Domain.Enums;
using CRM.Infrastructure.Persistence.Contexts;
using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Persistence.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly CrmDbContext _context;
    public InvoiceRepository(CrmDbContext context) => _context = context;

    public async Task<HoaDon?> GetByIdAsync(ulong id, CancellationToken ct = default)
    {
        var entity = await _context.KtHoaDons
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        return entity is null ? null : MapToDomain(entity);
    }

    public async Task<InvoiceDto?> GetByIdEnrichedAsync(ulong id, CancellationToken ct = default)
    {
        var result = await BuildEnrichedQuery()
            .Where(x => x.HoaDon.Id == id)
            .FirstOrDefaultAsync(ct);

        return result is null ? null
            : MapToDto(result.HoaDon, result.TenKhachHang, result.MaHopDong);
    }

    public async Task<PagedResult<InvoiceDto>> GetPagedAsync(
        int pageNumber, int pageSize, string? search,
        string? trangThaiThanhToan, ulong? khachHangId,
        CancellationToken ct = default)
    {
        var query = BuildEnrichedQuery();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x =>
                x.HoaDon.MaHoaDon.Contains(search) ||
                (x.TenKhachHang != null && x.TenKhachHang.Contains(search)));

        if (!string.IsNullOrWhiteSpace(trangThaiThanhToan))
            query = query.Where(x => x.HoaDon.TrangThaiThanhToan == trangThaiThanhToan);

        if (khachHangId.HasValue)
            query = query.Where(x => x.HoaDon.KhachHang_Id == khachHangId.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(x => x.HoaDon.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<InvoiceDto>
        {
            Items = items.Select(x => MapToDto(x.HoaDon, x.TenKhachHang, x.MaHopDong)).ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<HoaDon> AddAsync(HoaDon invoice, CancellationToken ct = default)
    {
        var entity = MapToEntity(invoice);
        _context.KtHoaDons.Add(entity);
        await _context.SaveChangesAsync(ct);
        invoice.Id = entity.Id;
        return invoice;
    }

    /// <summary>
    /// Cộng dồn SoTienDaThu và tự cập nhật TrangThaiThanhToan tương ứng — TRONG CÙNG 1 câu
    /// UPDATE (biểu thức SQL tham chiếu chính giá trị SoTienDaThu/TongTien của DB tại thời
    /// điểm ghi, không đọc lại bằng round-trip riêng). Trả về (SoTienDaThu, TongTien) SAU khi
    /// cộng để caller tự kiểm tra có bị vượt tổng tiền hay không (do 2 phiếu thu tạo đồng thời
    /// cùng qua được bước validate "còn lại" trước khi ghi).
    /// </summary>
    public async Task<(decimal SoTienDaThu, decimal TongTien)> UpdateSoTienDaThuAsync(
        ulong hoaDonId, decimal soTienCong, CancellationToken ct = default)
    {
        await _context.KtHoaDons
            .Where(x => x.Id == hoaDonId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.SoTienDaThu, x => (x.SoTienDaThu ?? 0m) + soTienCong)
                .SetProperty(x => x.TrangThaiThanhToan, x =>
                    (x.SoTienDaThu ?? 0m) + soTienCong >= x.TongTien
                        ? InvoiceStatus.HoanTat
                        : (x.SoTienDaThu ?? 0m) + soTienCong > 0
                            ? InvoiceStatus.ThanhToan1Phan
                            : InvoiceStatus.ChuaThanhToan)
                .SetProperty(x => x.UpdatedAt, DateTime.UtcNow),
            ct);

        var after = await _context.KtHoaDons
            .AsNoTracking()
            .Where(x => x.Id == hoaDonId)
            .Select(x => new { x.SoTienDaThu, x.TongTien })
            .FirstAsync(ct);

        return (after.SoTienDaThu ?? 0m, after.TongTien);
    }

    public async Task<string> GenerateMaHoaDonAsync(CancellationToken ct = default)
    {
        // Format: INV-20260702-A3F9 — tránh race condition so với MAX(Id)+1
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var randomPart = Guid.NewGuid().ToString("N")[..6].ToUpper();
        return $"INV-{datePart}-{randomPart}";
    }

    public Task<bool> ExistsForHopDongAsync(ulong hopDongId, CancellationToken ct = default) =>
        _context.KtHoaDons.AnyAsync(x => x.HopDongId == hopDongId, ct);

    // ── Query enriched dùng chung ─────────────────────────────────────────────
    private IQueryable<InvoiceProjection> BuildEnrichedQuery() =>
        from hd in _context.KtHoaDons
        join kh in _context.KhKhachHangs
            on hd.KhachHang_Id equals kh.Id into khJoin
        from kh in khJoin.DefaultIfEmpty()
        join hdong in _context.HdHopDongs
            on hd.HopDongId equals hdong.Id into hdongJoin
        from hdong in hdongJoin.DefaultIfEmpty()
        select new InvoiceProjection
        {
            HoaDon = hd,
            TenKhachHang = kh != null ? kh.TenKhachHang : null,
            MaHopDong = hdong != null ? hdong.MaHopDong : null
        };

    private class InvoiceProjection
    {
        public KtHoaDonEntity HoaDon { get; set; } = null!;
        public string? TenKhachHang { get; set; }
        public string? MaHopDong { get; set; }
    }

    // ── Mappers ───────────────────────────────────────────────────────────────
    private static HoaDon MapToDomain(KtHoaDonEntity e) => new()
    {
        Id = e.Id,
        MaHoaDon = e.MaHoaDon,
        HopDongId = e.HopDongId,
        KhachHangId = e.KhachHang_Id,
        TongTien = e.TongTien,
        SoTienDaThu = e.SoTienDaThu ?? 0m,
        TrangThaiThanhToan = e.TrangThaiThanhToan,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };

    private static KtHoaDonEntity MapToEntity(HoaDon d) => new()
    {
        MaHoaDon = d.MaHoaDon,
        HopDongId = d.HopDongId,
        KhachHang_Id = d.KhachHangId,
        TongTien = d.TongTien,
        SoTienDaThu = d.SoTienDaThu,
        TrangThaiThanhToan = d.TrangThaiThanhToan,
        CreatedAt = d.CreatedAt,
        UpdatedAt = d.UpdatedAt
    };

    private static InvoiceDto MapToDto(
        KtHoaDonEntity e, string? tenKhachHang, string? maHopDong) => new()
    {
        Id = e.Id,
        MaHoaDon = e.MaHoaDon,
        HopDongId = e.HopDongId,
        MaHopDong = maHopDong,
        KhachHangId = e.KhachHang_Id,
        TenKhachHang = tenKhachHang,
        TongTien = e.TongTien,
        SoTienDaThu = e.SoTienDaThu ?? 0m,
        TrangThaiThanhToan = e.TrangThaiThanhToan,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };
}
