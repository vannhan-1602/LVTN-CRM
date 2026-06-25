using CRM.Application.Common.Constants;
using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Opportunities.DTOs;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Opportunities;
using CRM.Domain.Entities.Sales;
using MediatR;

namespace CRM.Application.Features.Opportunities.Queries.GetOpportunityById;

public class GetOpportunityByIdQueryHandler : IRequestHandler<GetOpportunityByIdQuery, OpportunityDto>
{
    private readonly IOpportunityRepository _repo;
    private readonly ICurrentUserService _currentUser;

    public GetOpportunityByIdQueryHandler(IOpportunityRepository repo, ICurrentUserService currentUser)
    { _repo = repo; _currentUser = currentUser; }

    public async Task<OpportunityDto> Handle(GetOpportunityByIdQuery req, CancellationToken ct)
    {
        var dto = await _repo.GetByIdEnrichedAsync(req.Id, ct)
            ?? throw new NotFoundException(nameof(CoHoiBanHang), req.Id);

        if (_currentUser.Role == Roles.Sale && dto.NhanVienPhuTrachId != _currentUser.UserId)
            throw new ForbiddenException("Bạn không có quyền xem cơ hội của nhân viên khác.");

        return dto;
    }
}