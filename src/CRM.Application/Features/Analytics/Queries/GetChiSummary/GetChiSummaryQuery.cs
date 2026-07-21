using CRM.Application.Features.Analytics.DTOs;
using CRM.Application.Interfaces.Analytics;
using MediatR;

namespace CRM.Application.Features.Analytics.Queries.GetChiSummary;

public record GetChiSummaryQuery : IRequest<ChiSummaryDto>;

public class GetChiSummaryQueryHandler : IRequestHandler<GetChiSummaryQuery, ChiSummaryDto>
{
    private readonly IAnalyticsRepository _analyticsRepository;
    public GetChiSummaryQueryHandler(IAnalyticsRepository analyticsRepository) =>
        _analyticsRepository = analyticsRepository;

    public Task<ChiSummaryDto> Handle(GetChiSummaryQuery request, CancellationToken ct) =>
        _analyticsRepository.GetChiSummaryAsync(ct);
}
