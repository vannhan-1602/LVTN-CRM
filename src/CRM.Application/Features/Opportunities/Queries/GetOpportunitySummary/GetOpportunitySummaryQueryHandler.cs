using CRM.Application.Common.Constants;
using CRM.Application.Features.Opportunities.DTOs;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Opportunities;
using MediatR;

namespace CRM.Application.Features.Opportunities.Queries.GetOpportunitySummary;

public class GetOpportunitySummaryQueryHandler : IRequestHandler<GetOpportunitySummaryQuery, OpportunitySummaryDto>
{
    private readonly IOpportunityRepository _repo;
    private readonly ICurrentUserService _currentUser;

    public GetOpportunitySummaryQueryHandler(IOpportunityRepository repo, ICurrentUserService currentUser)
    {
        _repo = repo;
        _currentUser = currentUser;
    }

    public Task<OpportunitySummaryDto> Handle(GetOpportunitySummaryQuery request, CancellationToken ct)
    {
        // Sale chỉ thấy thống kê của chính mình — đồng nhất với GetAllOpportunitiesQueryHandler.
        uint? ownerFilter = _currentUser.Role == Roles.Sale ? _currentUser.UserId : null;
        return _repo.GetSummaryAsync(ownerFilter, ct);
    }
}