using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Contracts.DTOs;
using CRM.Application.Interfaces.Contracts;
using CRM.Application.Interfaces.Loyalty;
using CRM.Domain.Entities.Sales;
using MediatR;

namespace CRM.Application.Features.Contracts.Queries.GetContractEmailHistory;

// Trả về lịch sử các lần gửi email hợp đồng cho khách — suy ra từ KH_EmailLog
// (lọc theo KhachHangId + LoaiEmail="HopDong" + Tiêu đề chứa mã hợp đồng), KHÔNG
// cần thêm cột trạng thái riêng trong HD_HopDong.
public record GetContractEmailHistoryQuery(ulong HopDongId) : IRequest<List<ContractEmailHistoryItemDto>>;

public class GetContractEmailHistoryQueryHandler
    : IRequestHandler<GetContractEmailHistoryQuery, List<ContractEmailHistoryItemDto>>
{
    private readonly IContractRepository _contractRepository;
    private readonly ILoyaltyRepository _loyaltyRepository;

    public GetContractEmailHistoryQueryHandler(
        IContractRepository contractRepository, ILoyaltyRepository loyaltyRepository)
    {
        _contractRepository = contractRepository;
        _loyaltyRepository = loyaltyRepository;
    }

    public async Task<List<ContractEmailHistoryItemDto>> Handle(GetContractEmailHistoryQuery request, CancellationToken ct)
    {
        var contract = await _contractRepository.GetByIdEnrichedAsync(request.HopDongId, ct)
            ?? throw new NotFoundException(nameof(HopDong), request.HopDongId);

        var rows = await _loyaltyRepository.LayLichSuGuiEmailAsync(
            contract.KhachHangId, "HopDong", contract.MaHopDong, ct);

        return rows.Select(r => new ContractEmailHistoryItemDto
        {
            CreatedAt = r.CreatedAt,
            ThanhCong = r.ThanhCong,
            EmailDen = r.EmailDen,
            LoiChiTiet = r.LoiChiTiet,
        }).ToList();
    }
}
