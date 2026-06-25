using CRM.Application.Common.Constants;
using CRM.Application.Common.Models;
using CRM.Application.Features.Opportunities.DTOs;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Opportunities;
using MediatR;

namespace CRM.Application.Features.Opportunities.Queries.GetAllOpportunities;

public class GetAllOpportunitiesQueryHandler
    : IRequestHandler<GetAllOpportunitiesQuery, PagedResult<OpportunityDto>>
{
    private readonly IOpportunityRepository _repo;
    private readonly ICurrentUserService _currentUser;

    public GetAllOpportunitiesQueryHandler(IOpportunityRepository repo, ICurrentUserService currentUser)
    { _repo = repo; _currentUser = currentUser; }

    public Task<PagedResult<OpportunityDto>> Handle(GetAllOpportunitiesQuery req, CancellationToken ct)
    {
        // Sale chỉ thấy cơ hội của chính mình
        uint? ownerFilter = _currentUser.Role == Roles.Sale ? _currentUser.UserId : null;

        return _repo.GetPagedAsync(
            req.PageNumber, req.PageSize,
            req.Search, req.GiaiDoan,
            req.KhachHangId, req.LeadId,
            ownerFilter, ct);
    }
}