using CRM.Application.Common.Exceptions;
using CRM.Application.Features.PhieuThuChi.DTOs;
using CRM.Application.Interfaces.PhieuThuChi;
using MediatR;

namespace CRM.Application.Features.PhieuThuChi.Queries.GetPhieuThuChiById;

public record GetPhieuThuChiByIdQuery(ulong Id) : IRequest<PhieuThuChiDto>;

public class GetPhieuThuChiByIdQueryHandler : IRequestHandler<GetPhieuThuChiByIdQuery, PhieuThuChiDto>
{
    private readonly IPhieuThuChiRepository _repo;
    public GetPhieuThuChiByIdQueryHandler(IPhieuThuChiRepository repo) => _repo = repo;

    public async Task<PhieuThuChiDto> Handle(GetPhieuThuChiByIdQuery request, CancellationToken ct) =>
        await _repo.GetByIdEnrichedAsync(request.Id, ct)
            ?? throw new NotFoundException("Phiếu thu/chi", request.Id);
}
