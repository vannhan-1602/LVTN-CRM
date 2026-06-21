using CRM.Application.Common.Models;
using CRM.Application.Features.Quotes.DTOs;
using CRM.Application.Interfaces.Quotes;
using CRM.Domain.Entities.Sales;
using CRM.Infrastructure.Persistence.Contexts;
using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Persistence.Repositories;

public class QuoteRepository : IQuoteRepository
{
    private readonly CrmDbContext _context;
    public QuoteRepository(CrmDbContext context) => _context = context;

    public async Task<BaoGia?> GetByIdAsync(ulong id, CancellationToken ct = default)
    {
        var e = await _context.Set<HdBaoGiaEntity>().FirstOrDefaultAsync(x => x.Id == id, ct);
        return e is null ? null : MapToDomain(e);
    }

    public async Task<QuoteDetailDto?> GetByIdEnrichedAsync(ulong id, CancellationToken ct = default)
    {
        var result = await (
            from bg in _context.Set<HdBaoGiaEntity>()
            where bg.Id == id
            join kh in _context.KhKhachHangs on bg.KhachHang_Id equals kh.Id into khJoin
            from kh in khJoin.DefaultIfEmpty()
            join u in _context.HtUsers on bg.NhanVien_Id equals (uint?)u.Id into uJoin
            from u in uJoin.DefaultIfEmpty()
            select new { BaoGia = bg, TenKhachHang = kh != null ? kh.TenKhachHang : null, TenNhanVien = u != null ? u.Username : null }
        ).FirstOrDefaultAsync(ct);

        if (result is null) return null;

        var chiTiet = await GetChiTietAsync(id, ct);

        var dto = MapToDto(result.BaoGia, result.TenKhachHang, result.TenNhanVien);
        return new QuoteDetailDto
        {
            Id = dto.Id, MaBaoGia = dto.MaBaoGia, KhachHangId = dto.KhachHangId,
            TenKhachHang = dto.TenKhachHang, TongTien = dto.TongTien, TrangThai = dto.TrangThai,
            NhanVienId = dto.NhanVienId, TenNhanVien = dto.TenNhanVien,
            CreatedAt = dto.CreatedAt, UpdatedAt = dto.UpdatedAt, ChiTiet = chiTiet
        };
    }

    public async Task<PagedResult<QuoteDto>> GetPagedAsync(
        int pageNumber, int pageSize, string? search, string? trangThai,
        ulong? khachHangId, uint? ownerUserId, CancellationToken ct = default)
    {
        var query =
            from bg in _context.Set<HdBaoGiaEntity>().AsNoTracking()
            join kh in _context.KhKhachHangs on bg.KhachHang_Id equals kh.Id into khJoin
            from kh in khJoin.DefaultIfEmpty()
            join u in _context.HtUsers on bg.NhanVien_Id equals (uint?)u.Id into uJoin
            from u in uJoin.DefaultIfEmpty()
            select new { BaoGia = bg, TenKhachHang = kh != null ? kh.TenKhachHang : null, TenNhanVien = u != null ? u.Username : null };

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.BaoGia.MaBaoGia.Contains(search));

        if (!string.IsNullOrWhiteSpace(trangThai))
            query = query.Where(x => x.BaoGia.TrangThai == trangThai);

        if (khachHangId.HasValue)
            query = query.Where(x => x.BaoGia.KhachHang_Id == khachHangId.Value);

        if (ownerUserId.HasValue)
            query = query.Where(x => x.BaoGia.NhanVien_Id == ownerUserId.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(x => x.BaoGia.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var dtos = items.Select(x => MapToDto(x.BaoGia, x.TenKhachHang, x.TenNhanVien)).ToList();

        return new PagedResult<QuoteDto> { Items = dtos, PageNumber = pageNumber, PageSize = pageSize, TotalCount = total };
    }

    public async Task<BaoGia> AddAsync(BaoGia quote, List<BaoGiaChiTietInput> chiTiet, CancellationToken ct = default)
    {
        var entity = MapToEntity(quote);
        _context.Set<HdBaoGiaEntity>().Add(entity);
        await _context.SaveChangesAsync(ct);

        foreach (var item in chiTiet)
        {
            _context.Set<HdBaoGiaChiTietEntity>().Add(new HdBaoGiaChiTietEntity
            {
                BaoGia_Id = entity.Id,
                SanPham_Id = item.SanPhamId,
                SoLuong = item.SoLuong,
                DonGia = item.DonGia
            });
        }
        await _context.SaveChangesAsync(ct);

        quote.Id = entity.Id;
        return quote;
    }

    public async Task UpdateAsync(BaoGia quote, List<BaoGiaChiTietInput> chiTiet, CancellationToken ct = default)
    {
        var entity = await _context.Set<HdBaoGiaEntity>().FindAsync([quote.Id], ct);
        if (entity is null) return;

        entity.TongTien = quote.TongTien;
        entity.UpdatedAt = quote.UpdatedAt;

        // Thay thế toàn bộ chi tiết cũ bằng danh sách mới (đơn giản, an toàn cho
        // trường hợp sửa số lượng/đơn giá/thêm-bớt sản phẩm trong báo giá nháp)
        var oldItems = await _context.Set<HdBaoGiaChiTietEntity>()
            .Where(x => x.BaoGia_Id == quote.Id).ToListAsync(ct);
        _context.Set<HdBaoGiaChiTietEntity>().RemoveRange(oldItems);

        foreach (var item in chiTiet)
        {
            _context.Set<HdBaoGiaChiTietEntity>().Add(new HdBaoGiaChiTietEntity
            {
                BaoGia_Id = quote.Id,
                SanPham_Id = item.SanPhamId,
                SoLuong = item.SoLuong,
                DonGia = item.DonGia
            });
        }
    }

    public async Task UpdateStatusAsync(ulong id, string trangThai, CancellationToken ct = default)
    {
        var entity = await _context.Set<HdBaoGiaEntity>().FindAsync([id], ct);
        if (entity is null) return;
        entity.TrangThai = trangThai;
        entity.UpdatedAt = DateTime.UtcNow;
    }

    public async Task<bool> DeleteAsync(ulong id, CancellationToken ct = default)
    {
        var entity = await _context.Set<HdBaoGiaEntity>().FindAsync([id], ct);
        if (entity is null) return false;

        var items = await _context.Set<HdBaoGiaChiTietEntity>().Where(x => x.BaoGia_Id == id).ToListAsync(ct);
        _context.Set<HdBaoGiaChiTietEntity>().RemoveRange(items);
        _context.Set<HdBaoGiaEntity>().Remove(entity);
        return true;
    }

    public async Task<string> GenerateMaBaoGiaAsync(CancellationToken ct = default)
    {
        var last = await _context.Set<HdBaoGiaEntity>().OrderByDescending(x => x.Id).FirstOrDefaultAsync(ct);
        var next = (last?.Id ?? 0) + 1;
        return $"BG{next:D5}";
    }

    public async Task<List<QuoteDetailItemDto>> GetChiTietAsync(ulong baoGiaId, CancellationToken ct = default)
    {
        var query =
            from ct1 in _context.Set<HdBaoGiaChiTietEntity>().AsNoTracking()
            where ct1.BaoGia_Id == baoGiaId
            join sp in _context.Set<BhSanPhamEntity>() on ct1.SanPham_Id equals sp.Id into spJoin
            from sp in spJoin.DefaultIfEmpty()
            select new QuoteDetailItemDto
            {
                Id = ct1.Id,
                SanPhamId = ct1.SanPham_Id,
                TenSP = sp != null ? sp.TenSP : null,
                MaSP = sp != null ? sp.MaSP : null,
                DonVi = sp != null ? sp.DonVi : null,
                SoLuong = ct1.SoLuong,
                DonGia = ct1.DonGia
            };

        return await query.ToListAsync(ct);
    }

    private static BaoGia MapToDomain(HdBaoGiaEntity e) => new()
    {
        Id = e.Id, MaBaoGia = e.MaBaoGia, KhachHangId = e.KhachHang_Id,
        TongTien = e.TongTien, TrangThai = e.TrangThai, NhanVienId = e.NhanVien_Id,
        CreatedAt = e.CreatedAt, UpdatedAt = e.UpdatedAt
    };

    private static HdBaoGiaEntity MapToEntity(BaoGia d) => new()
    {
        MaBaoGia = d.MaBaoGia, KhachHang_Id = d.KhachHangId, TongTien = d.TongTien,
        TrangThai = d.TrangThai, NhanVien_Id = d.NhanVienId,
        CreatedAt = d.CreatedAt, UpdatedAt = d.UpdatedAt
    };

    private static QuoteDto MapToDto(HdBaoGiaEntity e, string? tenKhachHang, string? tenNhanVien) => new()
    {
        Id = e.Id, MaBaoGia = e.MaBaoGia, KhachHangId = e.KhachHang_Id, TenKhachHang = tenKhachHang,
        TongTien = e.TongTien, TrangThai = e.TrangThai, NhanVienId = e.NhanVien_Id, TenNhanVien = tenNhanVien,
        CreatedAt = e.CreatedAt, UpdatedAt = e.UpdatedAt
    };
}
