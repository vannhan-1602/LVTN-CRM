using CRM.Application.Common.Constants;
using CRM.Application.Common.Models;
using CRM.Application.Features.PhieuThuChi.DTOs;
using CRM.Application.Interfaces.Common;
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
    private readonly ICurrentUserService _currentUser;

    public GetAllPhieuThuChiQueryHandler(IPhieuThuChiRepository repo, ICurrentUserService currentUser)
    {
        _repo = repo;
        _currentUser = currentUser;
    }

    public Task<PagedResult<PhieuThuChiDto>> Handle(GetAllPhieuThuChiQuery request, CancellationToken ct)
    {
        // Sale chỉ xem phiếu thu/chi của khách hàng mình phụ trách. Manager/Accountant xem toàn bộ.
        uint? ownerUserId = _currentUser.Role == Roles.Sale ? _currentUser.UserId : null;

        return _repo.GetPagedAsync(request.PageNumber, request.PageSize,
            request.KhachHangId, request.HoaDonId, request.LoaiPhieu, ownerUserId, ct);
    }
}
