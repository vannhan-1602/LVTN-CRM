using CRM.Application.Common.Exceptions;
using CRM.Application.Common.Models;
using CRM.Application.Features.Products.DTOs;
using CRM.Application.Interfaces.Products;
using CRM.Domain.Entities.Products;
using CRM.Infrastructure.Persistence.Contexts;
using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Persistence.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly CrmDbContext _context;
    public ProductRepository(CrmDbContext context) => _context = context;

    public async Task<SanPham?> GetByIdAsync(uint id, CancellationToken ct = default)
    {
        var e = await _context.Set<BhSanPhamEntity>().FirstOrDefaultAsync(x => x.Id == id, ct);
        return e is null ? null : MapToDomain(e);
    }

    public async Task<ProductDto?> GetByIdEnrichedAsync(uint id, CancellationToken ct = default)
    {
        var result = await (
            from sp in _context.Set<BhSanPhamEntity>()
            where sp.Id == id
            join loai in _context.Set<BhLoaiSanPhamEntity>() on sp.LoaiSanPham_Id equals loai.Id into loaiJoin
            from loai in loaiJoin.DefaultIfEmpty()
            select new { SanPham = sp, TenLoai = loai != null ? loai.TenLoai : null }
        ).FirstOrDefaultAsync(ct);

        if (result is null) return null;

        var anhDaiDien = await _context.Set<BhSanPhamHinhAnhEntity>()
            .Where(h => h.SanPham_Id == id && h.IsMain)
            .Select(h => h.UrlHinhAnh)
            .FirstOrDefaultAsync(ct);

        return MapToDto(result.SanPham, result.TenLoai, anhDaiDien);
    }

    public async Task<PagedResult<ProductDto>> GetPagedAsync(
        int pageNumber, int pageSize, string? search,
        uint? loaiSanPhamId, bool? dangKinhDoanh, CancellationToken ct = default)
    {
        var query =
            from sp in _context.Set<BhSanPhamEntity>().AsNoTracking()
            join loai in _context.Set<BhLoaiSanPhamEntity>() on sp.LoaiSanPham_Id equals loai.Id into loaiJoin
            from loai in loaiJoin.DefaultIfEmpty()
            select new { SanPham = sp, TenLoai = loai != null ? loai.TenLoai : null };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();
            query = query.Where(x =>
                x.SanPham.TenSP.Contains(keyword) || x.SanPham.MaSP.Contains(keyword));
        }

        if (loaiSanPhamId.HasValue)
            query = query.Where(x => x.SanPham.LoaiSanPham_Id == loaiSanPhamId.Value);

        if (dangKinhDoanh.HasValue)
        {
            var flag = (byte)(dangKinhDoanh.Value ? 1 : 0);
            query = query.Where(x => x.SanPham.TrangThai == flag);
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(x => x.SanPham.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var ids = items.Select(x => x.SanPham.Id).ToList();
        var anhMap = await _context.Set<BhSanPhamHinhAnhEntity>()
            .Where(h => h.SanPham_Id != null && ids.Contains(h.SanPham_Id.Value) && h.IsMain)
            .ToDictionaryAsync(h => h.SanPham_Id!.Value, h => h.UrlHinhAnh, ct);

        var dtos = items.Select(x => MapToDto(
            x.SanPham, x.TenLoai,
            anhMap.TryGetValue(x.SanPham.Id, out var url) ? url : null)).ToList();

        return new PagedResult<ProductDto>
        {
            Items = dtos, PageNumber = pageNumber, PageSize = pageSize, TotalCount = total
        };
    }

    public async Task<SanPham> AddAsync(SanPham product, CancellationToken ct = default)
    {
        var entity = MapToEntity(product);
        _context.Set<BhSanPhamEntity>().Add(entity);
        await _context.SaveChangesAsync(ct);
        product.Id = entity.Id;
        return product;
    }

    public async Task UpdateAsync(SanPham product, CancellationToken ct = default)
    {
        var entity = await _context.Set<BhSanPhamEntity>().FindAsync([product.Id], ct);
        if (entity is null) return;

        entity.LoaiSanPham_Id = product.LoaiSanPhamId;
        entity.TenSP = product.TenSP;
        entity.DonVi = product.DonVi;
        entity.GiaBan = product.GiaBan;
        entity.TrangThai = (byte)(product.DangKinhDoanh ? 1 : 0);
        entity.UpdatedAt = product.UpdatedAt;
    }

    public async Task<bool> DeactivateAsync(uint id, CancellationToken ct = default)
    {
        var entity = await _context.Set<BhSanPhamEntity>().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return false;

        entity.TrangThai = 0;
        entity.UpdatedAt = DateTime.UtcNow;
        return true;
    }

    public Task<bool> ExistsMaSPAsync(string maSP, uint? excludeId = null, CancellationToken ct = default) =>
        _context.Set<BhSanPhamEntity>().AnyAsync(
            x => x.MaSP == maSP && (!excludeId.HasValue || x.Id != excludeId.Value), ct);

    public Task<bool> LoaiSanPhamExistsAsync(uint id, CancellationToken ct = default) =>
        _context.Set<BhLoaiSanPhamEntity>().AnyAsync(x => x.Id == id, ct);

    public async Task<bool> IsReferencedAsync(uint id, CancellationToken ct = default)
    {
        var inQuote = await _context.Set<HdBaoGiaChiTietEntity>().AnyAsync(x => x.SanPham_Id == id, ct);
        var inStock = await _context.Set<KhoTheKhoEntity>().AnyAsync(x => x.SanPham_Id == id, ct);
        return inQuote || inStock;
    }

    public Task<List<LoaiSanPham>> GetAllLoaiSanPhamAsync(CancellationToken ct = default) =>
        _context.Set<BhLoaiSanPhamEntity>()
            .AsNoTracking()
            .OrderBy(x => x.TenLoai)
            .Select(x => new LoaiSanPham { Id = x.Id, TenLoai = x.TenLoai, MoTa = x.MoTa })
            .ToListAsync(ct);

    public async Task<int> GetCurrentStockAsync(uint sanPhamId, CancellationToken ct = default)
    {
        var product = await _context.Set<BhSanPhamEntity>()
            .Where(x => x.Id == sanPhamId)
            .Select(x => (int?)x.SoLuongTon)
            .FirstOrDefaultAsync(ct);

        return product ?? 0;
    }

    public async Task<StockTransactionResultDto> AdjustStockAsync(
        uint sanPhamId, string loaiGiaoDich, int soLuongThayDoi,
        string? maChungTu, string? ghiChu, uint? nguoiThucHienId,
        CancellationToken ct = default)
    {
        var product = await _context.Set<BhSanPhamEntity>().FirstOrDefaultAsync(x => x.Id == sanPhamId, ct)
            ?? throw new NotFoundException(nameof(SanPham), sanPhamId);

        var tonTruoc = product.SoLuongTon;
        var tonSau = tonTruoc + soLuongThayDoi;

        // Cập nhật tồn kho lũy kế ngay trên bảng sản phẩm 
        product.SoLuongTon = tonSau;
        product.UpdatedAt = DateTime.UtcNow;

        var transaction = new KhoTheKhoEntity
        {
            SanPham_Id = sanPhamId,
            MaChungTu = maChungTu,
            LoaiGiaoDich = loaiGiaoDich,
            SoLuongThayDoi = soLuongThayDoi,
            TonCuoi = tonSau,
            NgayGiaoDich = DateTime.UtcNow,
            NguoiThucHien_Id = nguoiThucHienId,
            GhiChu = ghiChu
        };
        _context.Set<KhoTheKhoEntity>().Add(transaction);
        await _context.SaveChangesAsync(ct);

        return new StockTransactionResultDto
        {
            TonTruoc = tonTruoc,
            TonSau = tonSau,
            Transaction = MapTransactionToDto(transaction, null)
        };
    }

    public async Task<List<StockTransactionDto>> GetStockHistoryAsync(uint sanPhamId, CancellationToken ct = default)
    {
        var query =
            from gd in _context.Set<KhoTheKhoEntity>().AsNoTracking()
            where gd.SanPham_Id == sanPhamId
            join ns in _context.HtThongTinNhanSu on (uint?)gd.NguoiThucHien_Id equals (uint?)ns.Id into nsJoin
            from ns in nsJoin.DefaultIfEmpty()
            orderby gd.NgayGiaoDich descending
            select new { Transaction = gd, TenNguoiThucHien = ns != null ? ns.HoTen : null };

        var results = await query.ToListAsync(ct);
        return results.Select(x => MapTransactionToDto(x.Transaction, x.TenNguoiThucHien)).ToList();
    }


    private static SanPham MapToDomain(BhSanPhamEntity e) => new()
    {
        Id = e.Id,
        LoaiSanPhamId = e.LoaiSanPham_Id,
        MaSP = e.MaSP,
        TenSP = e.TenSP,
        DonVi = e.DonVi,
        GiaBan = e.GiaBan,
        SoLuongTon = e.SoLuongTon,
        DangKinhDoanh = e.TrangThai == 1,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };

    private static BhSanPhamEntity MapToEntity(SanPham d) => new()
    {
        LoaiSanPham_Id = d.LoaiSanPhamId,
        MaSP = d.MaSP,
        TenSP = d.TenSP,
        DonVi = d.DonVi,
        GiaBan = d.GiaBan,
        SoLuongTon = d.SoLuongTon,
        TrangThai = (byte)(d.DangKinhDoanh ? 1 : 0),
        CreatedAt = d.CreatedAt,
        UpdatedAt = d.UpdatedAt
    };

    private static ProductDto MapToDto(BhSanPhamEntity e, string? tenLoai, string? anhDaiDien) => new()
    {
        Id = e.Id,
        LoaiSanPhamId = e.LoaiSanPham_Id,
        TenLoai = tenLoai,
        MaSP = e.MaSP,
        TenSP = e.TenSP,
        DonVi = e.DonVi,
        GiaBan = e.GiaBan,
        SoLuongTon = e.SoLuongTon,
        DangKinhDoanh = e.TrangThai == 1,
        AnhDaiDien = anhDaiDien,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };

    private static StockTransactionDto MapTransactionToDto(KhoTheKhoEntity e, string? tenNguoiThucHien) => new()
    {
        Id = e.Id,
        SanPhamId = e.SanPham_Id,
        MaChungTu = e.MaChungTu,
        LoaiGiaoDich = e.LoaiGiaoDich,
        SoLuongThayDoi = e.SoLuongThayDoi,
        TonCuoi = e.TonCuoi,
        NgayGiaoDich = e.NgayGiaoDich,
        NguoiThucHienId = e.NguoiThucHien_Id,
        TenNguoiThucHien = tenNguoiThucHien,
        GhiChu = e.GhiChu
    };
}
