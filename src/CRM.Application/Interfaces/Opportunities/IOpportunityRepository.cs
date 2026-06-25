using CRM.Application.Common.Models;
using CRM.Application.Features.Opportunities.DTOs;
using CRM.Domain.Entities.Sales;

namespace CRM.Application.Interfaces.Opportunities;

public interface IOpportunityRepository
{
    Task<CoHoiBanHang?> GetByIdAsync(ulong id, CancellationToken ct = default);
    Task<OpportunityDto?> GetByIdEnrichedAsync(ulong id, CancellationToken ct = default);

    Task<PagedResult<OpportunityDto>> GetPagedAsync(
        int pageNumber, int pageSize,
        string? search, string? giaiDoan,
        ulong? khachHangId, ulong? leadId,
        uint? ownerUserId,
        CancellationToken ct = default);

    Task<CoHoiBanHang> AddAsync(CoHoiBanHang entity, CancellationToken ct = default);
    Task UpdateAsync(CoHoiBanHang entity, CancellationToken ct = default);
    Task<bool> SoftDeleteAsync(ulong id, CancellationToken ct = default);

    //Dữ liệu thống kê để AI phân tích — số cơ hội theo giai đoạn, tỷ lệ, doanh thu.
    Task<OpportunitySummaryDto> GetSummaryAsync(CancellationToken ct = default);
}