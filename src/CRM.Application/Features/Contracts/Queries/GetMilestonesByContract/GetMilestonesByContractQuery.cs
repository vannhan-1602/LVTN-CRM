using CRM.Application.Features.Contracts.DTOs;
using CRM.Application.Interfaces.Contracts;
using MediatR;

namespace CRM.Application.Features.Contracts.Queries.GetMilestonesByContract;

public record GetMilestonesByContractQuery(ulong HopDongId) : IRequest<List<MocTrienKhaiDto>>;

public class GetMilestonesByContractQueryHandler
    : IRequestHandler<GetMilestonesByContractQuery, List<MocTrienKhaiDto>>
{
    private readonly IContractMilestoneRepository _milestoneRepository;
    public GetMilestonesByContractQueryHandler(IContractMilestoneRepository milestoneRepository) =>
        _milestoneRepository = milestoneRepository;

    public Task<List<MocTrienKhaiDto>> Handle(GetMilestonesByContractQuery request, CancellationToken ct) =>
        _milestoneRepository.GetByHopDongAsync(request.HopDongId, ct);
}
