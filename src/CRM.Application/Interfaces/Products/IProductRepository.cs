using CRM.Application.Common.Models;
using CRM.Application.Features.Products.DTOs;
using CRM.Domain.Entities.Products;

namespace CRM.Application.Interfaces.Products;

public interface IProductRepository
{
    Task<SanPham?> GetByIdAsync(uint id, CancellationToken ct = default);
    Task<ProductDto?> GetByIdEnrichedAsync(uint id, CancellationToken ct = default);

    Task<PagedResult<ProductDto>> GetPagedAsync(
        int pageNumber, int pageSize, string? search,
        uint? loaiSanPhamId, bool? dangKinhDoanh, CancellationToken ct = default);

    Task<SanPham> AddAsync(SanPham product, CancellationToken ct = default);
    Task UpdateAsync(SanPham product, CancellationToken ct = default);

    Task<bool> DeactivateAsync(uint id, CancellationToken ct = default);

    Task<bool> ExistsMaSPAsync(string maSP, uint? excludeId = null, CancellationToken ct = default);
    Task<bool> LoaiSanPhamExistsAsync(uint id, CancellationToken ct = default);
    Task<bool> IsReferencedAsync(uint id, CancellationToken ct = default);

    Task<List<LoaiSanPham>> GetAllLoaiSanPhamAsync(CancellationToken ct = default);

    // Quản lý kho
    Task<int> GetCurrentStockAsync(uint sanPhamId, CancellationToken ct = default);
    Task<StockTransactionResultDto> AdjustStockAsync(
        uint sanPhamId, string loaiGiaoDich, int soLuongThayDoi,
        string? maChungTu, string? ghiChu, uint? nguoiThucHienId,
        CancellationToken ct = default);
    Task<List<StockTransactionDto>> GetStockHistoryAsync(uint sanPhamId, CancellationToken ct = default);
}
