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

    // Khi Lead được convert thành Customer, gán lại các Cơ hội đang gắn LeadId đó sang KhachHangId mới
    // để không bị "mồ côi" khỏi trang chi tiết Khách hàng. Không tự SaveChanges — theo UnitOfWork của caller.
    Task ReassignLeadOpportunitiesToCustomerAsync(ulong leadId, ulong customerId, CancellationToken ct = default);

    //Dữ liệu thống kê để AI phân tích — số cơ hội theo giai đoạn, tỷ lệ, doanh thu.
    // ownerUserId: null = toàn công ty (Manager/Admin); có giá trị = chỉ tính cơ hội của nhân viên đó (Sale).
    Task<OpportunitySummaryDto> GetSummaryAsync(uint? ownerUserId, CancellationToken ct = default);
}