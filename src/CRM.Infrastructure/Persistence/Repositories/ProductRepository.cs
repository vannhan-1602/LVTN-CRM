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
            Items = dtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = total
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
        // Cập nhật tồn kho bằng 1 câu UPDATE cộng dồn atomic (SET SoLuongTon = SoLuongTon + @delta),
        // kèm điều kiện SoLuongTon + @delta >= 0 ngay trong WHERE.
        // Nhờ đó DB tự khoá dòng (row lock) khi thực thi UPDATE, hai giao dịch xuất/nhập kho đồng
        // thời trên cùng 1 sản phẩm sẽ tuần tự hoá ở mức DB thay vì đọc-sửa-ghi ở tầng ứng dụng
        // (kiểu cũ có thể làm mất phần trừ của giao dịch trước → tồn kho âm thực tế dù có check).
        // Toàn bộ handler chạy trong 1 transaction (TransactionBehavior), nên row lock của UPDATE
        // này được giữ tới khi commit, đảm bảo SELECT + INSERT bên dưới nhất quán với giá trị vừa ghi.
        var rowsAffected = await _context.Set<BhSanPhamEntity>()
            .Where(x => x.Id == sanPhamId && x.SoLuongTon + soLuongThayDoi >= 0)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.SoLuongTon, x => x.SoLuongTon + soLuongThayDoi)
                .SetProperty(x => x.UpdatedAt, DateTime.UtcNow), ct);

        if (rowsAffected == 0)
        {
            var exists = await _context.Set<BhSanPhamEntity>().AnyAsync(x => x.Id == sanPhamId, ct);
            if (!exists)
                throw new NotFoundException(nameof(SanPham), sanPhamId);

            throw new BusinessRuleException(
                "Không đủ tồn kho để thực hiện giao dịch này (tồn kho có thể vừa được thay đổi bởi giao dịch khác).");
        }

        var tonSau = await _context.Set<BhSanPhamEntity>()
            .Where(x => x.Id == sanPhamId)
            .Select(x => x.SoLuongTon)
            .FirstAsync(ct);
        var tonTruoc = tonSau - soLuongThayDoi;

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

    // ── Quản lý hình ảnh sản phẩm (1 ảnh chính + nhiều ảnh phụ) ──────────────────
    public async Task<List<ProductImageDto>> GetImagesAsync(uint sanPhamId, CancellationToken ct = default) =>
        await _context.Set<BhSanPhamHinhAnhEntity>()
            .AsNoTracking()
            .Where(h => h.SanPham_Id == sanPhamId)
            .OrderByDescending(h => h.IsMain)
            .ThenBy(h => h.Id)
            .Select(h => MapImageToDto(h))
            .ToListAsync(ct);

    public async Task<ProductImageDto> AddImageAsync(uint sanPhamId, string urlHinhAnh, bool isMain, CancellationToken ct = default)
    {
        // Nếu đây là ảnh chính, hoặc sản phẩm chưa có ảnh chính nào, đảm bảo chỉ 1 ảnh IsMain=true.
        var hasMain = await _context.Set<BhSanPhamHinhAnhEntity>()
            .AnyAsync(h => h.SanPham_Id == sanPhamId && h.IsMain, ct);

        var shouldBeMain = isMain || !hasMain;

        if (shouldBeMain && hasMain)
        {
            await _context.Set<BhSanPhamHinhAnhEntity>()
                .Where(h => h.SanPham_Id == sanPhamId && h.IsMain)
                .ExecuteUpdateAsync(s => s.SetProperty(h => h.IsMain, false), ct);
        }

        var entity = new BhSanPhamHinhAnhEntity
        {
            SanPham_Id = sanPhamId,
            UrlHinhAnh = urlHinhAnh,
            IsMain = shouldBeMain
        };
        await _context.Set<BhSanPhamHinhAnhEntity>().AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);

        return MapImageToDto(entity);
    }

    public async Task<bool> SetMainImageAsync(uint sanPhamId, ulong imageId, CancellationToken ct = default)
    {
        var target = await _context.Set<BhSanPhamHinhAnhEntity>()
            .FirstOrDefaultAsync(h => h.Id == imageId && h.SanPham_Id == sanPhamId, ct);
        if (target is null) return false;

        // Bỏ cờ IsMain của ảnh chính cũ (nếu khác ảnh đang chọn) rồi gán cho ảnh mới.
        await _context.Set<BhSanPhamHinhAnhEntity>()
            .Where(h => h.SanPham_Id == sanPhamId && h.IsMain && h.Id != imageId)
            .ExecuteUpdateAsync(s => s.SetProperty(h => h.IsMain, false), ct);

        target.IsMain = true;
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteImageAsync(ulong imageId, CancellationToken ct = default)
    {
        var entity = await _context.Set<BhSanPhamHinhAnhEntity>().FirstOrDefaultAsync(h => h.Id == imageId, ct);
        if (entity is null) return false;

        var wasMain = entity.IsMain;
        var sanPhamId = entity.SanPham_Id;
        _context.Set<BhSanPhamHinhAnhEntity>().Remove(entity);
        await _context.SaveChangesAsync(ct);

        // Nếu vừa xóa ảnh chính, tự động chọn 1 ảnh phụ còn lại (nếu có) làm ảnh chính mới,
        // để sản phẩm không bị "mất ảnh đại diện" đột ngột.
        if (wasMain && sanPhamId.HasValue)
        {
            var next = await _context.Set<BhSanPhamHinhAnhEntity>()
                .Where(h => h.SanPham_Id == sanPhamId.Value)
                .OrderBy(h => h.Id)
                .FirstOrDefaultAsync(ct);
            if (next is not null)
            {
                next.IsMain = true;
                await _context.SaveChangesAsync(ct);
            }
        }
        return true;
    }

    public async Task<ProductImageDto?> GetImageByIdAsync(ulong imageId, CancellationToken ct = default) =>
        await _context.Set<BhSanPhamHinhAnhEntity>()
            .AsNoTracking()
            .Where(h => h.Id == imageId)
            .Select(h => MapImageToDto(h))
            .FirstOrDefaultAsync(ct);

    private static ProductImageDto MapImageToDto(BhSanPhamHinhAnhEntity e) => new()
    {
        Id = e.Id,
        SanPhamId = e.SanPham_Id ?? 0,
        UrlHinhAnh = e.UrlHinhAnh,
        IsMain = e.IsMain
    };
}