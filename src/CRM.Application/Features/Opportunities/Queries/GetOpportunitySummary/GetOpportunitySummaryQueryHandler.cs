using CRM.Application.Features.Opportunities.DTOs;
using CRM.Application.Interfaces.Opportunities;
using MediatR;

namespace CRM.Application.Features.Opportunities.Queries.GetOpportunitySummary;

public class GetOpportunitySummaryQueryHandler : IRequestHandler<GetOpportunitySummaryQuery, OpportunitySummaryDto>
{
    private readonly IOpportunityRepository _repo;

    public GetOpportunitySummaryQueryHandler(IOpportunityRepository repo)
    {
        _repo = repo;
    }

    public Task<OpportunitySummaryDto> Handle(GetOpportunitySummaryQuery request, CancellationToken ct) =>
        _repo.GetSummaryAsync(ct);
}