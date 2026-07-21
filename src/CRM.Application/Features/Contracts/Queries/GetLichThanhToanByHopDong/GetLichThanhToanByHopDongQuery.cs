using CRM.Application.Features.Contracts.DTOs;
using CRM.Application.Interfaces.Contracts;
using MediatR;

namespace CRM.Application.Features.Contracts.Queries.GetLichThanhToanByHopDong;

public record GetLichThanhToanByHopDongQuery(ulong HopDongId) : IRequest<List<LichThanhToanDto>>;

public class GetLichThanhToanByHopDongQueryHandler
    : IRequestHandler<GetLichThanhToanByHopDongQuery, List<LichThanhToanDto>>
{
    private readonly IContractRepository _contractRepository;

    public GetLichThanhToanByHopDongQueryHandler(IContractRepository contractRepository)
    {
        _contractRepository = contractRepository;
    }

    public Task<List<LichThanhToanDto>> Handle(GetLichThanhToanByHopDongQuery request, CancellationToken ct) =>
        _contractRepository.GetLichThanhToanByHopDongAsync(request.HopDongId, ct);
}