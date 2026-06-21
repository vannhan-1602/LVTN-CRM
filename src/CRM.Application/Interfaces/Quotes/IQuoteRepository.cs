using CRM.Application.Common.Models;
using CRM.Application.Features.Quotes.DTOs;
using CRM.Domain.Entities.Sales;

namespace CRM.Application.Interfaces.Quotes;

public interface IQuoteRepository
{
    Task<BaoGia?> GetByIdAsync(ulong id, CancellationToken ct = default);
    Task<QuoteDetailDto?> GetByIdEnrichedAsync(ulong id, CancellationToken ct = default);

    Task<PagedResult<QuoteDto>> GetPagedAsync(
        int pageNumber, int pageSize, string? search, string? trangThai,
        ulong? khachHangId, uint? ownerNhanSuId, CancellationToken ct = default);

    Task<BaoGia> AddAsync(BaoGia quote, List<BaoGiaChiTietInput> chiTiet, CancellationToken ct = default);
    Task UpdateAsync(BaoGia quote, List<BaoGiaChiTietInput> chiTiet, CancellationToken ct = default);
    Task UpdateStatusAsync(ulong id, string trangThai, CancellationToken ct = default);
    Task<bool> DeleteAsync(ulong id, CancellationToken ct = default);

    Task<string> GenerateMaBaoGiaAsync(CancellationToken ct = default);
    Task<List<QuoteDetailItemDto>> GetChiTietAsync(ulong baoGiaId, CancellationToken ct = default);
}

public record BaoGiaChiTietInput(uint SanPhamId, int SoLuong, decimal DonGia);
