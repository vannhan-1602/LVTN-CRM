using CRM.Application.Common.Models;
using CRM.Application.Features.Contracts.DTOs;
using CRM.Application.Interfaces.Contracts;
using MediatR;

namespace CRM.Application.Features.Contracts.Queries.GetAllContracts;

public record GetAllContractsQuery(
    int PageNumber, int PageSize, string? Search, string? TrangThai, ulong? KhachHangId
) : IRequest<PagedResult<ContractDto>>;

public class GetAllContractsQueryHandler : IRequestHandler<GetAllContractsQuery, PagedResult<ContractDto>>
{
    private readonly IContractRepository _contractRepository;
    public GetAllContractsQueryHandler(IContractRepository contractRepository) => _contractRepository = contractRepository;

    // Sale + Manager xem được toàn bộ hợp đồng của tất cả khách hàng
    // Accountant cũng xem được toàn bộ hợp đồng nhưng chỉ đọc
   
    public Task<PagedResult<ContractDto>> Handle(GetAllContractsQuery request, CancellationToken ct) =>
        _contractRepository.GetPagedAsync(
            request.PageNumber, request.PageSize, request.Search, request.TrangThai, request.KhachHangId, ct);
}
