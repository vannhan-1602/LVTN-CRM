using CRM.Application.Common.Constants;
using CRM.Application.Common.Exceptions;
using CRM.Application.Features.PhieuThuChi.DTOs;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.PhieuThuChi;
using MediatR;

namespace CRM.Application.Features.PhieuThuChi.Queries.GetPhieuThuChiById;

public record GetPhieuThuChiByIdQuery(ulong Id) : IRequest<PhieuThuChiDto>;

public class GetPhieuThuChiByIdQueryHandler : IRequestHandler<GetPhieuThuChiByIdQuery, PhieuThuChiDto>
{
    private readonly IPhieuThuChiRepository _repo;
    private readonly ICurrentUserService _currentUser;

    public GetPhieuThuChiByIdQueryHandler(IPhieuThuChiRepository repo, ICurrentUserService currentUser)
    {
        _repo = repo;
        _currentUser = currentUser;
    }

    public async Task<PhieuThuChiDto> Handle(GetPhieuThuChiByIdQuery request, CancellationToken ct)
    {
        var phieu = await _repo.GetByIdEnrichedAsync(request.Id, ct)
            ?? throw new NotFoundException("Phiếu thu/chi", request.Id);

        // Chặn Sale xem phiếu thu/chi của khách hàng không phải mình phụ trách.
        if (_currentUser.Role == Roles.Sale && phieu.NhanVienPhuTrachId != _currentUser.UserId)
            throw new ForbiddenException("Bạn không có quyền xem phiếu thu/chi của khách hàng khác.");

        return phieu;
    }
}
