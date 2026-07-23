using CRM.Application.Common.Models;
using CRM.Application.Features.PhieuThuChi.DTOs;
using DomainPhieuThuChi = CRM.Domain.Entities.Sales.PhieuThuChi;

namespace CRM.Application.Interfaces.PhieuThuChi;

public interface IPhieuThuChiRepository
{
    Task<DomainPhieuThuChi?> GetByIdAsync(ulong id, CancellationToken ct = default);
    Task<PhieuThuChiDto?> GetByIdEnrichedAsync(ulong id, CancellationToken ct = default);

    Task<PagedResult<PhieuThuChiDto>> GetPagedAsync(
        int pageNumber, int pageSize, ulong? khachHangId, ulong? hoaDonId,
        string? loaiPhieu, uint? ownerUserId, CancellationToken ct = default);

    Task<DomainPhieuThuChi> AddAsync(DomainPhieuThuChi phieu, CancellationToken ct = default);

    Task<string> GenerateMaPhieuAsync(string loaiPhieu, CancellationToken ct = default);

    /// Tổng số tiền đã thu (loại 'Thu') của 1 hóa đơn, dùng để kiểm tra không thu vượt quá TongTien.
    Task<decimal> GetTongDaThuByHoaDonAsync(ulong hoaDonId, CancellationToken ct = default);
}
