using CRM.Application.Common.Models;
using CRM.Application.Features.Invoices.DTOs;
using CRM.Domain.Entities.Sales;

namespace CRM.Application.Interfaces.Invoices;

public interface IInvoiceRepository
{
    Task<HoaDon?> GetByIdAsync(ulong id, CancellationToken ct = default);
    Task<InvoiceDto?> GetByIdEnrichedAsync(ulong id, CancellationToken ct = default);

    Task<PagedResult<InvoiceDto>> GetPagedAsync(
        int pageNumber, int pageSize, string? search, string? trangThaiThanhToan,
        ulong? khachHangId, CancellationToken ct = default);

    Task<HoaDon> AddAsync(HoaDon invoice, CancellationToken ct = default);

    /// Cộng dồn SoTienDaThu và tự cập nhật TrangThaiThanhToan tương ứng,
    /// dùng khi có phiếu thu mới được tạo cho hóa đơn này.
    Task<(decimal SoTienDaThu, decimal TongTien)> UpdateSoTienDaThuAsync(
        ulong hoaDonId, decimal soTienCong, CancellationToken ct = default);

    Task<string> GenerateMaHoaDonAsync(CancellationToken ct = default);

    Task<bool> ExistsForHopDongAsync(ulong hopDongId, CancellationToken ct = default);
}
