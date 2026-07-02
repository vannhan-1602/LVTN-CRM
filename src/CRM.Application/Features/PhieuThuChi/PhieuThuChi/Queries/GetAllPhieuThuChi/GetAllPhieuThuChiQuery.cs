using CRM.Application.Common.Models;
using CRM.Application.Features.PhieuThuChi.DTOs;
using CRM.Application.Interfaces.PhieuThuChi;
using MediatR;

namespace CRM.Application.Features.PhieuThuChi.Queries.GetAllPhieuThuChi;

public record GetAllPhieuThuChiQuery(
    int PageNumber,
    int PageSize,
    ulong? KhachHangId,
    ulong? HoaDonId,
    string? LoaiPhieu
) : IRequest<PagedResult<PhieuThuChiDto>>;

public class GetAllPhieuThuChiQueryHandler : IRequestHandler<GetAllPhieuThuChiQuery, PagedResult<PhieuThuChiDto>>
{
    private readonly IPhieuThuChiRepository _repo;
    public GetAllPhieuThuChiQueryHandler(IPhieuThuChiRepository repo) => _repo = repo;

    public Task<PagedResult<PhieuThuChiDto>> Handle(GetAllPhieuThuChiQuery request, CancellationToken ct) =>
        _repo.GetPagedAsync(request.PageNumber, request.PageSize,
            request.KhachHangId, request.HoaDonId, request.LoaiPhieu, ct);
}
