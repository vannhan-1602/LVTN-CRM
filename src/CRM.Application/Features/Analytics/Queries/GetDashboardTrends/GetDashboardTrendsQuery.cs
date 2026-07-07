using CRM.Application.Features.Analytics.DTOs;
using CRM.Application.Interfaces.Analytics;
using MediatR;

namespace CRM.Application.Features.Analytics.Queries.GetDashboardTrends;

public record GetDashboardTrendsQuery : IRequest<DashboardTrendsDto>;

public class GetDashboardTrendsQueryHandler : IRequestHandler<GetDashboardTrendsQuery, DashboardTrendsDto>
{
    private readonly IAnalyticsRepository _analyticsRepository;
    public GetDashboardTrendsQueryHandler(IAnalyticsRepository analyticsRepository) =>
        _analyticsRepository = analyticsRepository;

    public Task<DashboardTrendsDto> Handle(GetDashboardTrendsQuery request, CancellationToken ct) =>
        _analyticsRepository.GetDashboardTrendsAsync(ct);
}
